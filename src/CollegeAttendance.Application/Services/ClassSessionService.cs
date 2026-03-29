using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class ClassSessionService : IClassSessionService
{
    private readonly IRepository<ClassSession> _sessionRepo;
    private readonly IRepository<CourseEnrollment> _enrollmentRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ClassSessionService(
        IRepository<ClassSession> sessionRepo,
        IRepository<CourseEnrollment> enrollmentRepo,
        IUnitOfWork unitOfWork)
    {
        _sessionRepo = sessionRepo;
        _enrollmentRepo = enrollmentRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ClassSessionDto>> GetByCourseAsync(Guid courseId) =>
        await MapSessionsAsync(_sessionRepo.Query()
            .Include(s => s.Course).Include(s => s.Faculty).Include(s => s.AttendanceRecords)
            .Where(s => s.CourseId == courseId && !s.IsDeleted));

    public async Task<ClassSessionDto?> GetByIdAsync(Guid id)
    {
        var results = await MapSessionsAsync(_sessionRepo.Query()
            .Include(s => s.Course).Include(s => s.Faculty).Include(s => s.AttendanceRecords)
            .Where(s => s.Id == id && !s.IsDeleted));
        return results.FirstOrDefault();
    }

    public async Task<List<ClassSessionDto>> GetTodaySessionsAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;
        return await MapSessionsAsync(_sessionRepo.Query()
            .Include(s => s.Course).Include(s => s.Faculty).Include(s => s.AttendanceRecords)
            .Where(s => s.ScheduledDate.Date == today && s.FacultyId == userId && !s.IsDeleted));
    }

    public async Task<ClassSessionDto> CreateAsync(Guid facultyId, CreateClassSessionRequest request)
    {
        var session = new ClassSession
        {
            Title = request.Title,
            ScheduledDate = request.ScheduledDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Room = request.Room,
            CourseId = request.CourseId,
            FacultyId = facultyId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            GeofenceRadiusMeters = request.GeofenceRadiusMeters
        };

        await _sessionRepo.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();
        return (await GetByIdAsync(session.Id))!;
    }

    public async Task StartSessionAsync(Guid sessionId, Guid facultyId)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId) ?? throw new KeyNotFoundException("Session not found");
        if (session.FacultyId != facultyId) throw new UnauthorizedAccessException("Not your session");
        session.Status = SessionStatus.Active;
        session.UpdatedAt = DateTime.UtcNow;
        _sessionRepo.Update(session);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task EndSessionAsync(Guid sessionId, Guid facultyId)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId) ?? throw new KeyNotFoundException("Session not found");
        if (session.FacultyId != facultyId) throw new UnauthorizedAccessException("Not your session");
        session.Status = SessionStatus.Completed;
        session.UpdatedAt = DateTime.UtcNow;
        _sessionRepo.Update(session);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<List<ClassSessionDto>> MapSessionsAsync(IQueryable<ClassSession> query)
    {
        var sessions = await query.OrderByDescending(s => s.ScheduledDate).ThenBy(s => s.StartTime).ToListAsync();
        var result = new List<ClassSessionDto>();

        foreach (var s in sessions)
        {
            var totalStudents = await _enrollmentRepo.CountAsync(e => e.CourseId == s.CourseId && e.IsActive);
            var presentCount = s.AttendanceRecords.Count(a =>
                a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late);

            result.Add(new ClassSessionDto(
                s.Id, s.Title, s.ScheduledDate, s.StartTime, s.EndTime, s.Room,
                s.Status, s.CourseId, s.Course.Name, s.FacultyId, s.Faculty.FullName,
                s.Latitude, s.Longitude, s.GeofenceRadiusMeters,
                presentCount, totalStudents));
        }

        return result;
    }
}
