using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CollegeAttendance.Application.Services;

public class FraudDetectionService : IFraudDetectionService
{
    private readonly IRepository<FraudLog> _fraudRepo;
    private readonly IRepository<DeviceBinding> _deviceRepo;
    private readonly IRepository<AttendanceRecord> _attendanceRepo;
    private readonly IRepository<User> _userRepo;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public FraudDetectionService(
        IRepository<FraudLog> fraudRepo,
        IRepository<DeviceBinding> deviceRepo,
        IRepository<AttendanceRecord> attendanceRepo,
        IRepository<User> userRepo,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _fraudRepo = fraudRepo;
        _deviceRepo = deviceRepo;
        _attendanceRepo = attendanceRepo;
        _userRepo = userRepo;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<FraudLogDto?> AnalyzeAttendanceScanAsync(Guid studentId, Guid sessionId, double latitude, double longitude, string? deviceId)
    {
        // Check 1: Rapid scan spike (multiple scans in short time from same user)
        var recentScans = await _attendanceRepo.Query()
            .Where(a => a.StudentId == studentId && a.MarkedAt > DateTime.UtcNow.AddMinutes(-5))
            .CountAsync();

        if (recentScans > 3)
        {
            return await CreateFraudLogAsync(studentId, sessionId, FraudType.RapidScanSpike,
                FraudSeverity.Medium, "Multiple scans detected within 5 minutes",
                JsonSerializer.Serialize(new { recentScans, latitude, longitude }));
        }

        // Check 2: Device mismatch
        if (!string.IsNullOrEmpty(deviceId))
        {
            var isValid = await ValidateDeviceAsync(studentId, deviceId);
            if (!isValid)
            {
                var boundDevices = await _deviceRepo.CountAsync(db => db.UserId == studentId && db.IsActive);
                if (boundDevices > 0)
                {
                    return await CreateFraudLogAsync(studentId, sessionId, FraudType.DeviceMismatch,
                        FraudSeverity.High, "Attendance marked from unregistered device",
                        JsonSerializer.Serialize(new { deviceId, latitude, longitude }));
                }
            }
        }

        // Check 3: Multi-location in short time
        var lastAttendance = await _attendanceRepo.Query()
            .Where(a => a.StudentId == studentId && a.MarkedAt > DateTime.UtcNow.AddMinutes(-30))
            .OrderByDescending(a => a.MarkedAt)
            .FirstOrDefaultAsync();

        if (lastAttendance != null && lastAttendance.ClassSessionId != sessionId)
        {
            // Flag if attending two sessions within 30 min (potential proxy)
            return await CreateFraudLogAsync(studentId, sessionId, FraudType.MultiLocationScan,
                FraudSeverity.Medium, "Attendance at multiple sessions within 30 minutes",
                JsonSerializer.Serialize(new { previousSessionId = lastAttendance.ClassSessionId, timeDiff = "< 30min" }));
        }

        return null;
    }

    private async Task<FraudLogDto> CreateFraudLogAsync(Guid studentId, Guid sessionId, FraudType type, FraudSeverity severity, string description, string? evidence)
    {
        var log = new FraudLog
        {
            UserId = studentId,
            FraudType = type,
            Severity = severity,
            Description = description,
            Evidence = evidence,
            ClassSessionId = sessionId
        };

        await _fraudRepo.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();

        // Notify admins
        var admins = await _userRepo.Query()
            .Where(u => u.Role == UserRole.Admin && u.IsActive)
            .Select(u => u.Id).ToListAsync();

        var student = await _userRepo.GetByIdAsync(studentId);
        await _notificationService.SendBulkNotificationAsync(admins,
            "⚠️ Fraud Alert", $"Suspicious activity by {student?.FullName}: {description}",
            NotificationType.FraudAlert);

        return new FraudLogDto(log.Id, studentId, student?.FullName ?? "Unknown",
            type, severity, description, evidence, sessionId, null, false, null, null, log.CreatedAt);
    }

    public async Task<List<FraudLogDto>> GetFraudLogsAsync(Guid? userId, bool unresolvedOnly, int page, int pageSize)
    {
        var query = _fraudRepo.Query().AsQueryable();
        if (userId.HasValue) query = query.Where(f => f.UserId == userId);
        if (unresolvedOnly) query = query.Where(f => !f.IsResolved);

        return await query
            .Include(f => f.User)
            .Include(f => f.ClassSession)
            .Include(f => f.ResolvedBy)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(f => new FraudLogDto(f.Id, f.UserId, f.User.FullName,
                f.FraudType, f.Severity, f.Description, f.Evidence,
                f.ClassSessionId, f.ClassSession != null ? f.ClassSession.Title : null,
                f.IsResolved, f.ResolvedBy != null ? f.ResolvedBy.FullName : null,
                f.ResolvedAt, f.CreatedAt))
            .ToListAsync();
    }

    public async Task ResolveAsync(Guid fraudLogId, Guid resolvedById, string resolutionNotes)
    {
        var log = await _fraudRepo.GetByIdAsync(fraudLogId)
            ?? throw new KeyNotFoundException("Fraud log not found");

        log.IsResolved = true;
        log.ResolvedById = resolvedById;
        log.ResolvedAt = DateTime.UtcNow;
        log.ResolutionNotes = resolutionNotes;
        log.UpdatedAt = DateTime.UtcNow;
        _fraudRepo.Update(log);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<DeviceBindingDto>> GetUserDevicesAsync(Guid userId)
    {
        return await _deviceRepo.Query()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.BoundAt)
            .Select(d => new DeviceBindingDto(d.Id, d.DeviceFingerprint, d.DeviceName, d.Platform, d.BoundAt, d.IsActive, d.LastUsedAt))
            .ToListAsync();
    }

    public async Task<DeviceBindingDto> BindDeviceAsync(Guid userId, BindDeviceRequest request)
    {
        // Check max device limit
        var activeCount = await _deviceRepo.CountAsync(d => d.UserId == userId && d.IsActive);
        if (activeCount >= 2)
            throw new InvalidOperationException("Maximum device limit reached. Deactivate an existing device first.");

        var existing = await _deviceRepo.FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceFingerprint == request.DeviceFingerprint);
        if (existing != null)
        {
            existing.IsActive = true;
            existing.LastUsedAt = DateTime.UtcNow;
            _deviceRepo.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return new DeviceBindingDto(existing.Id, existing.DeviceFingerprint, existing.DeviceName, existing.Platform, existing.BoundAt, existing.IsActive, existing.LastUsedAt);
        }

        var binding = new DeviceBinding
        {
            UserId = userId,
            DeviceFingerprint = request.DeviceFingerprint,
            DeviceName = request.DeviceName,
            Platform = request.Platform,
            IsActive = true
        };

        await _deviceRepo.AddAsync(binding);
        await _unitOfWork.SaveChangesAsync();

        return new DeviceBindingDto(binding.Id, binding.DeviceFingerprint, binding.DeviceName, binding.Platform, binding.BoundAt, binding.IsActive, binding.LastUsedAt);
    }

    public async Task DeactivateDeviceAsync(Guid bindingId)
    {
        var binding = await _deviceRepo.GetByIdAsync(bindingId)
            ?? throw new KeyNotFoundException("Device binding not found");

        binding.IsActive = false;
        binding.UpdatedAt = DateTime.UtcNow;
        _deviceRepo.Update(binding);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ValidateDeviceAsync(Guid userId, string deviceFingerprint)
    {
        var binding = await _deviceRepo.FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceFingerprint == deviceFingerprint && d.IsActive);
        if (binding != null)
        {
            binding.LastUsedAt = DateTime.UtcNow;
            _deviceRepo.Update(binding);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        return false;
    }
}
