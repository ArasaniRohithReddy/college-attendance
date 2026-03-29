using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class EmergencyService : IEmergencyService
{
    private readonly IRepository<EmergencySOS> _sosRepo;
    private readonly IRepository<User> _userRepo;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public EmergencyService(
        IRepository<EmergencySOS> sosRepo,
        IRepository<User> userRepo,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _sosRepo = sosRepo;
        _userRepo = userRepo;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmergencySOSDto> CreateSOSAsync(Guid studentId, CreateSOSRequest request)
    {
        var student = await _userRepo.GetByIdAsync(studentId)
            ?? throw new KeyNotFoundException("Student not found");

        var sos = new EmergencySOS
        {
            StudentId = studentId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Message = request.Message,
            Priority = request.Priority,
            Status = SOSStatus.Active
        };

        await _sosRepo.AddAsync(sos);
        await _unitOfWork.SaveChangesAsync();

        // Notify all wardens and admins
        var wardens = await _userRepo.Query()
            .Where(u => (u.Role == UserRole.Warden || u.Role == UserRole.Admin) && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync();

        await _notificationService.SendBulkNotificationAsync(wardens,
            "🚨 Emergency SOS Alert",
            $"SOS from {student.FullName}: {request.Message ?? "Emergency help needed"}",
            NotificationType.EmergencySOS);

        return MapToDto(sos, student.FullName, null);
    }

    public async Task<EmergencySOSDto?> GetByIdAsync(Guid sosId)
    {
        var sos = await _sosRepo.Query()
            .Include(s => s.Student)
            .Include(s => s.RespondedBy)
            .FirstOrDefaultAsync(s => s.Id == sosId);

        return sos == null ? null : MapToDto(sos, sos.Student.FullName, sos.RespondedBy?.FullName);
    }

    public async Task<List<EmergencySOSDto>> GetActiveSOSAsync()
    {
        return await _sosRepo.Query()
            .Where(s => s.Status == SOSStatus.Active || s.Status == SOSStatus.Acknowledged || s.Status == SOSStatus.Responding)
            .Include(s => s.Student)
            .Include(s => s.RespondedBy)
            .OrderByDescending(s => s.Priority).ThenByDescending(s => s.CreatedAt)
            .Select(s => MapToDto(s, s.Student.FullName, s.RespondedBy != null ? s.RespondedBy.FullName : null))
            .ToListAsync();
    }

    public async Task AcknowledgeAsync(Guid sosId, Guid responderId)
    {
        var sos = await _sosRepo.GetByIdAsync(sosId)
            ?? throw new KeyNotFoundException("SOS alert not found");

        sos.Status = SOSStatus.Responding;
        sos.RespondedById = responderId;
        sos.RespondedAt = DateTime.UtcNow;
        sos.UpdatedAt = DateTime.UtcNow;
        _sosRepo.Update(sos);
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(sos.StudentId,
            "SOS Acknowledged", "Help is on the way. Stay calm.", NotificationType.EmergencySOS);
    }

    public async Task ResolveAsync(Guid sosId, Guid responderId, string resolutionNotes)
    {
        var sos = await _sosRepo.GetByIdAsync(sosId)
            ?? throw new KeyNotFoundException("SOS alert not found");

        sos.Status = SOSStatus.Resolved;
        sos.ResolvedAt = DateTime.UtcNow;
        sos.ResolutionNotes = resolutionNotes;
        if (sos.RespondedById == null) sos.RespondedById = responderId;
        sos.UpdatedAt = DateTime.UtcNow;
        _sosRepo.Update(sos);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<EmergencySOSDto>> GetHistoryAsync(Guid? studentId, int page, int pageSize)
    {
        var query = _sosRepo.Query().AsQueryable();
        if (studentId.HasValue) query = query.Where(s => s.StudentId == studentId);

        return await query
            .Include(s => s.Student)
            .Include(s => s.RespondedBy)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(s => MapToDto(s, s.Student.FullName, s.RespondedBy != null ? s.RespondedBy.FullName : null))
            .ToListAsync();
    }

    private static EmergencySOSDto MapToDto(EmergencySOS s, string studentName, string? respondedByName) =>
        new(s.Id, s.StudentId, studentName, s.Latitude, s.Longitude, s.Message,
            s.Status, s.Priority, respondedByName, s.RespondedAt, s.ResolvedAt,
            s.ResolutionNotes, s.CreatedAt);
}
