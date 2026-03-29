using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class CurfewService : ICurfewService
{
    private readonly IRepository<CurfewLog> _curfewRepo;
    private readonly IRepository<HostelLog> _hostelLogRepo;
    private readonly IRepository<User> _userRepo;
    private readonly ISystemConfigService _configService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public CurfewService(
        IRepository<CurfewLog> curfewRepo,
        IRepository<HostelLog> hostelLogRepo,
        IRepository<User> userRepo,
        ISystemConfigService configService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _curfewRepo = curfewRepo;
        _hostelLogRepo = hostelLogRepo;
        _userRepo = userRepo;
        _configService = configService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task CheckCurfewViolationsAsync()
    {
        var curfewTimeStr = await _configService.GetValueAsync("hostel.curfew_time", "22:00");
        var graceMinutes = await _configService.GetValueAsync("hostel.curfew_grace_minutes", 15);

        if (!TimeSpan.TryParse(curfewTimeStr, out var curfewTime)) return;
        var now = DateTime.UtcNow;
        var today = now.Date;
        var cutoffTime = today.Add(curfewTime).AddMinutes(graceMinutes);

        if (now < cutoffTime) return;

        // Find students who checked out today but haven't checked back in
        var outStudents = await _hostelLogRepo.Query()
            .Where(l => l.LogType == HostelLogType.Exit && l.Timestamp.Date == today)
            .Select(l => new { l.StudentId, l.HostelId })
            .ToListAsync();

        var returnedStudents = await _hostelLogRepo.Query()
            .Where(l => l.LogType == HostelLogType.Entry && l.Timestamp.Date == today && l.Timestamp > today.Add(curfewTime))
            .Select(l => l.StudentId)
            .ToListAsync();

        foreach (var student in outStudents)
        {
            // Check if already logged as curfew violation today
            var alreadyLogged = await _curfewRepo.AnyAsync(c =>
                c.StudentId == student.StudentId && c.CurfewTime.Date == today);
            if (alreadyLogged) continue;

            var returned = returnedStudents.Contains(student.StudentId);
            var minutesLate = returned ? 0 : (int)(now - today.Add(curfewTime)).TotalMinutes;

            var log = new CurfewLog
            {
                StudentId = student.StudentId,
                HostelId = student.HostelId,
                CurfewTime = today.Add(curfewTime),
                MinutesLate = minutesLate,
                ParentNotified = false
            };

            await _curfewRepo.AddAsync(log);

            await _notificationService.SendNotificationAsync(student.StudentId,
                "Curfew Violation", $"You were not back in the hostel by {curfewTimeStr}. This has been recorded.",
                NotificationType.CurfewViolation);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<CurfewLogDto>> GetCurfewLogsAsync(Guid? hostelId, DateTime from, DateTime to)
    {
        var query = _curfewRepo.Query()
            .Include(c => c.Student)
            .Include(c => c.Hostel)
            .Where(c => c.CurfewTime >= from && c.CurfewTime <= to);

        if (hostelId.HasValue)
            query = query.Where(c => c.HostelId == hostelId.Value);

        return await query.OrderByDescending(c => c.CurfewTime)
            .Select(c => new CurfewLogDto(
                c.Id, c.StudentId, c.Student.FullName,
                c.Hostel.Name, c.CurfewTime, c.ReturnTime,
                c.MinutesLate, c.ParentNotified, c.ParentNotifiedAt))
            .ToListAsync();
    }

    public async Task<List<CurfewLogDto>> GetStudentCurfewLogsAsync(Guid studentId)
    {
        return await _curfewRepo.Query()
            .Include(c => c.Student)
            .Include(c => c.Hostel)
            .Where(c => c.StudentId == studentId)
            .OrderByDescending(c => c.CurfewTime)
            .Select(c => new CurfewLogDto(
                c.Id, c.StudentId, c.Student.FullName,
                c.Hostel.Name, c.CurfewTime, c.ReturnTime,
                c.MinutesLate, c.ParentNotified, c.ParentNotifiedAt))
            .ToListAsync();
    }

    public async Task<CurfewConfigDto> GetCurfewConfigAsync()
    {
        var curfewTimeStr = await _configService.GetValueAsync("hostel.curfew_time", "22:00");
        var graceMinutes = await _configService.GetValueAsync("hostel.curfew_grace_minutes", 15);
        var autoNotify = await _configService.GetValueAsync("hostel.auto_notify_parent", true);

        TimeSpan.TryParse(curfewTimeStr, out var curfewTime);
        return new CurfewConfigDto(curfewTime, graceMinutes, autoNotify);
    }

    public async Task UpdateCurfewConfigAsync(Guid userId, CurfewConfigDto config)
    {
        await _configService.UpdateAsync("hostel.curfew_time", userId, new UpdateConfigRequest($"{config.CurfewTime:hh\\:mm}"));
        await _configService.UpdateAsync("hostel.curfew_grace_minutes", userId, new UpdateConfigRequest(config.GracePeriodMinutes.ToString()));
    }
}
