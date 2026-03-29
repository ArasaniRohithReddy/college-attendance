using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IRepository<AttendanceRecord> _attendanceRepo;
    private readonly IRepository<ClassSession> _sessionRepo;
    private readonly IRepository<CourseEnrollment> _enrollmentRepo;
    private readonly IRepository<QRSession> _qrRepo;
    private readonly IQRService _qrService;
    private readonly IGeofenceService _geofenceService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceService(
        IRepository<AttendanceRecord> attendanceRepo,
        IRepository<ClassSession> sessionRepo,
        IRepository<CourseEnrollment> enrollmentRepo,
        IRepository<QRSession> qrRepo,
        IQRService qrService,
        IGeofenceService geofenceService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _attendanceRepo = attendanceRepo;
        _sessionRepo = sessionRepo;
        _enrollmentRepo = enrollmentRepo;
        _qrRepo = qrRepo;
        _qrService = qrService;
        _geofenceService = geofenceService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AttendanceDto> MarkAttendanceByQRAsync(Guid studentId, MarkAttendanceRequest request)
    {
        // Validate QR token
        var qrSession = await _qrService.ValidateQRTokenAsync(request.QRToken)
            ?? throw new InvalidOperationException("Invalid or expired QR code");

        // Get the QR session entity
        var qrEntity = await _qrRepo.Query()
            .Include(q => q.ClassSession)
            .FirstOrDefaultAsync(q => q.QRToken == request.QRToken && q.IsActive)
            ?? throw new InvalidOperationException("QR session not found");

        var classSession = qrEntity.ClassSession;

        // Check duplicate attendance
        var existing = await _attendanceRepo.AnyAsync(a =>
            a.StudentId == studentId &&
            a.ClassSessionId == classSession.Id &&
            !a.IsDeleted);

        if (existing)
            throw new InvalidOperationException("Attendance already marked for this session");

        // Validate geofence
        bool isGeofenceValid = true;
        if (classSession.Latitude.HasValue && classSession.Longitude.HasValue)
        {
            isGeofenceValid = _geofenceService.IsWithinGeofence(
                request.Latitude, request.Longitude,
                classSession.Latitude.Value, classSession.Longitude.Value,
                classSession.GeofenceRadiusMeters);
        }

        // Detect potential fraud (same device used by different student)
        bool isFraudSuspected = false;
        if (!string.IsNullOrEmpty(request.DeviceId))
        {
            isFraudSuspected = await _attendanceRepo.AnyAsync(a =>
                a.DeviceId == request.DeviceId &&
                a.ClassSessionId == classSession.Id &&
                a.StudentId != studentId &&
                !a.IsDeleted);
        }

        var record = new AttendanceRecord
        {
            StudentId = studentId,
            ClassSessionId = classSession.Id,
            QRSessionId = qrEntity.Id,
            Status = isGeofenceValid ? AttendanceStatus.Present : AttendanceStatus.Late,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsGeofenceValid = isGeofenceValid,
            IsManualEntry = false,
            DeviceId = request.DeviceId,
            IsFraudSuspected = isFraudSuspected,
            MarkedAt = DateTime.UtcNow
        };

        await _attendanceRepo.AddAsync(record);
        qrEntity.ScanCount++;
        await _unitOfWork.SaveChangesAsync();

        if (isFraudSuspected)
        {
            await _notificationService.SendNotificationAsync(
                classSession.FacultyId,
                "Fraud Alert",
                $"Potential duplicate device detected in session: {classSession.Title}",
                NotificationType.FraudAlert);
        }

        return MapToDto(record);
    }

    public async Task<List<AttendanceDto>> MarkManualAttendanceAsync(Guid facultyId, ManualAttendanceRequest request)
    {
        var session = await _sessionRepo.GetByIdAsync(request.ClassSessionId)
            ?? throw new KeyNotFoundException("Session not found");

        var results = new List<AttendanceDto>();

        foreach (var entry in request.Entries)
        {
            var existing = await _attendanceRepo.FirstOrDefaultAsync(a =>
                a.StudentId == entry.StudentId &&
                a.ClassSessionId == request.ClassSessionId &&
                !a.IsDeleted);

            if (existing != null)
            {
                existing.Status = entry.Status;
                existing.Remarks = entry.Remarks;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = facultyId.ToString();
                _attendanceRepo.Update(existing);
                results.Add(MapToDto(existing));
            }
            else
            {
                var record = new AttendanceRecord
                {
                    StudentId = entry.StudentId,
                    ClassSessionId = request.ClassSessionId,
                    Status = entry.Status,
                    IsManualEntry = true,
                    IsGeofenceValid = true,
                    MarkedById = facultyId,
                    Remarks = entry.Remarks,
                    MarkedAt = DateTime.UtcNow
                };
                await _attendanceRepo.AddAsync(record);
                results.Add(MapToDto(record));
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return results;
    }

    public async Task<List<AttendanceDto>> GetSessionAttendanceAsync(Guid sessionId)
    {
        var records = await _attendanceRepo.Query()
            .Include(a => a.Student)
            .Include(a => a.ClassSession)
            .Where(a => a.ClassSessionId == sessionId && !a.IsDeleted)
            .OrderBy(a => a.Student.FullName)
            .ToListAsync();

        return records.Select(MapToDto).ToList();
    }

    public async Task<List<AttendanceReportDto>> GetStudentAttendanceReportAsync(Guid studentId)
    {
        var enrollments = await _enrollmentRepo.Query()
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId && e.IsActive)
            .ToListAsync();

        var reports = new List<AttendanceReportDto>();

        foreach (var enrollment in enrollments)
        {
            var totalSessions = await _sessionRepo.CountAsync(s =>
                s.CourseId == enrollment.CourseId &&
                s.Status == SessionStatus.Completed);

            var records = await _attendanceRepo.Query()
                .Include(a => a.ClassSession)
                .Where(a => a.StudentId == studentId &&
                    a.ClassSession.CourseId == enrollment.CourseId &&
                    !a.IsDeleted)
                .ToListAsync();

            var present = records.Count(r => r.Status == AttendanceStatus.Present);
            var absent = totalSessions - records.Count(r => r.Status != AttendanceStatus.Absent);
            var late = records.Count(r => r.Status == AttendanceStatus.Late);
            var excused = records.Count(r => r.Status == AttendanceStatus.Excused);
            var percentage = totalSessions > 0 ? (present + late + excused) * 100.0 / totalSessions : 0;

            var student = await _attendanceRepo.Query()
                .Include(a => a.Student)
                .Where(a => a.StudentId == studentId)
                .Select(a => a.Student)
                .FirstOrDefaultAsync();

            reports.Add(new AttendanceReportDto(
                studentId, student?.FullName ?? "", student?.StudentId,
                enrollment.Course.Name, enrollment.Course.Code,
                totalSessions, present, absent, late, excused,
                Math.Round(percentage, 2), percentage < 75
            ));
        }

        return reports;
    }

    public async Task<List<AttendanceReportDto>> GetCourseAttendanceReportAsync(Guid courseId)
    {
        var enrollments = await _enrollmentRepo.Query()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => e.CourseId == courseId && e.IsActive)
            .ToListAsync();

        var totalSessions = await _sessionRepo.CountAsync(s =>
            s.CourseId == courseId && s.Status == SessionStatus.Completed);

        var reports = new List<AttendanceReportDto>();

        foreach (var enrollment in enrollments)
        {
            var records = await _attendanceRepo.Query()
                .Include(a => a.ClassSession)
                .Where(a => a.StudentId == enrollment.StudentId &&
                    a.ClassSession.CourseId == courseId && !a.IsDeleted)
                .ToListAsync();

            var present = records.Count(r => r.Status == AttendanceStatus.Present);
            var late = records.Count(r => r.Status == AttendanceStatus.Late);
            var excused = records.Count(r => r.Status == AttendanceStatus.Excused);
            var absent = totalSessions - (present + late + excused);
            var percentage = totalSessions > 0 ? (present + late + excused) * 100.0 / totalSessions : 0;

            reports.Add(new AttendanceReportDto(
                enrollment.StudentId, enrollment.Student.FullName, enrollment.Student.StudentId,
                enrollment.Course.Name, enrollment.Course.Code,
                totalSessions, present, absent, late, excused,
                Math.Round(percentage, 2), percentage < 75
            ));
        }

        return reports.OrderBy(r => r.AttendancePercentage).ToList();
    }

    public async Task<PagedResult<AttendanceDto>> GetAttendanceHistoryAsync(
        Guid? studentId, Guid? courseId, DateTime? from, DateTime? to, int page, int pageSize)
    {
        var query = _attendanceRepo.Query()
            .Include(a => a.Student)
            .Include(a => a.ClassSession)
            .Where(a => !a.IsDeleted);

        if (studentId.HasValue) query = query.Where(a => a.StudentId == studentId.Value);
        if (courseId.HasValue) query = query.Where(a => a.ClassSession.CourseId == courseId.Value);
        if (from.HasValue) query = query.Where(a => a.MarkedAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.MarkedAt <= to.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.MarkedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AttendanceDto>(
            items.Select(MapToDto).ToList(), total, page, pageSize);
    }

    public async Task<AttendanceDto> UpdateAttendanceAsync(Guid recordId, Guid facultyId, AttendanceDto update)
    {
        var record = await _attendanceRepo.GetByIdAsync(recordId)
            ?? throw new KeyNotFoundException("Attendance record not found");

        record.Status = update.Status;
        record.Remarks = update.Remarks;
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = facultyId.ToString();

        _attendanceRepo.Update(record);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(record);
    }

    public async Task<List<AttendanceReportDto>> GetDefaultersAsync(Guid? courseId, double threshold = 75.0)
    {
        List<AttendanceReportDto> allReports;

        if (courseId.HasValue)
        {
            allReports = await GetCourseAttendanceReportAsync(courseId.Value);
        }
        else
        {
            var enrollments = await _enrollmentRepo.Query()
                .Select(e => e.CourseId)
                .Distinct()
                .ToListAsync();

            allReports = new List<AttendanceReportDto>();
            foreach (var cId in enrollments)
            {
                allReports.AddRange(await GetCourseAttendanceReportAsync(cId));
            }
        }

        return allReports.Where(r => r.AttendancePercentage < threshold).ToList();
    }

    public Task<byte[]> ExportAttendanceReportAsync(Guid courseId, string format)
    {
        // CSV export implementation
        throw new NotImplementedException("Export functionality - implement with ClosedXML or similar");
    }

    private static AttendanceDto MapToDto(AttendanceRecord a) => new(
        a.Id, a.StudentId, a.Student?.FullName ?? "", a.Student?.StudentId,
        a.ClassSessionId, a.ClassSession?.Title ?? "",
        a.Status, a.MarkedAt,
        a.IsManualEntry, a.IsGeofenceValid, a.IsFraudSuspected,
        a.Remarks
    );
}
