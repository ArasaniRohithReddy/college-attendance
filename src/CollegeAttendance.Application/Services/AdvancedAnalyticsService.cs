using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class AdvancedAnalyticsService : IAdvancedAnalyticsService
{
    private readonly IRepository<AttendanceRecord> _attendanceRepo;
    private readonly IRepository<ClassSession> _sessionRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<CourseEnrollment> _enrollmentRepo;
    private readonly IRepository<Course> _courseRepo;
    private readonly IRepository<EmergencySOS> _sosRepo;
    private readonly IRepository<LeaveRequest> _leaveRepo;
    private readonly IRepository<FraudLog> _fraudRepo;
    private readonly IRepository<CurfewLog> _curfewRepo;

    public AdvancedAnalyticsService(
        IRepository<AttendanceRecord> attendanceRepo,
        IRepository<ClassSession> sessionRepo,
        IRepository<User> userRepo,
        IRepository<CourseEnrollment> enrollmentRepo,
        IRepository<Course> courseRepo,
        IRepository<EmergencySOS> sosRepo,
        IRepository<LeaveRequest> leaveRepo,
        IRepository<FraudLog> fraudRepo,
        IRepository<CurfewLog> curfewRepo)
    {
        _attendanceRepo = attendanceRepo;
        _sessionRepo = sessionRepo;
        _userRepo = userRepo;
        _enrollmentRepo = enrollmentRepo;
        _courseRepo = courseRepo;
        _sosRepo = sosRepo;
        _leaveRepo = leaveRepo;
        _fraudRepo = fraudRepo;
        _curfewRepo = curfewRepo;
    }

    public async Task<List<AttendanceHeatmapDto>> GetAttendanceHeatmapAsync(Guid? departmentId, DateTime from, DateTime to)
    {
        var query = _attendanceRepo.Query()
            .Include(a => a.ClassSession)
            .Where(a => a.MarkedAt >= from && a.MarkedAt <= to);

        if (departmentId.HasValue)
        {
            var courseIds = await _courseRepo.Query()
                .Where(c => c.DepartmentId == departmentId.Value)
                .Select(c => c.Id).ToListAsync();

            var sessionIds = await _sessionRepo.Query()
                .Where(s => courseIds.Contains(s.CourseId))
                .Select(s => s.Id).ToListAsync();

            query = query.Where(a => sessionIds.Contains(a.ClassSessionId));
        }

        var records = await query.Select(a => new
        {
            DayOfWeek = a.ClassSession.ScheduledDate.DayOfWeek,
            Hour = a.ClassSession.StartTime.Hours,
            Status = a.Status
        }).ToListAsync();

        return records.GroupBy(r => new { r.DayOfWeek, r.Hour })
            .Select(g => new AttendanceHeatmapDto(
                g.Key.DayOfWeek.ToString(),
                g.Key.Hour,
                g.Count(x => x.Status == AttendanceStatus.Present) * 100.0 / Math.Max(g.Count(), 1),
                g.Count()))
            .OrderBy(h => h.DayOfWeek).ThenBy(h => h.Hour)
            .ToList();
    }

    public async Task<List<DropoutRiskDto>> GetDropoutRiskStudentsAsync(Guid? departmentId, int topN = 20)
    {
        var students = _userRepo.Query()
            .Where(u => u.Role == UserRole.Student && u.IsActive);

        if (departmentId.HasValue)
            students = students.Where(u => u.DepartmentId == departmentId.Value);

        var studentList = await students.Select(u => new { u.Id, u.FullName, DeptName = u.Department != null ? u.Department.Name : null }).ToListAsync();
        var results = new List<DropoutRiskDto>();

        foreach (var student in studentList)
        {
            var total = await _attendanceRepo.CountAsync(a => a.StudentId == student.Id);
            var present = await _attendanceRepo.CountAsync(a => a.StudentId == student.Id && a.Status == AttendanceStatus.Present);
            var late = await _attendanceRepo.CountAsync(a => a.StudentId == student.Id && a.Status == AttendanceStatus.Late);
            var percentage = total > 0 ? present * 100.0 / total : 100;

            var riskFactors = new List<string>();
            double riskScore = 0;

            if (percentage < 60) { riskScore += 40; riskFactors.Add("Very low attendance (<60%)"); }
            else if (percentage < 75) { riskScore += 25; riskFactors.Add("Below threshold attendance (<75%)"); }

            if (total > 0 && late * 100.0 / total > 30) { riskScore += 15; riskFactors.Add("High late percentage (>30%)"); }

            // Recent trend — check last 7 days
            var recentTotal = await _attendanceRepo.CountAsync(a => a.StudentId == student.Id && a.MarkedAt >= DateTime.UtcNow.AddDays(-7));
            var recentPresent = await _attendanceRepo.CountAsync(a => a.StudentId == student.Id && a.MarkedAt >= DateTime.UtcNow.AddDays(-7) && a.Status == AttendanceStatus.Present);
            var recentPercentage = recentTotal > 0 ? recentPresent * 100.0 / recentTotal : 100;

            if (recentPercentage < 50) { riskScore += 20; riskFactors.Add("Sharp recent decline in attendance"); }

            var riskLevel = riskScore switch
            {
                >= 60 => "Critical",
                >= 40 => "High",
                >= 20 => "Medium",
                _ => "Low"
            };

            if (riskScore >= 20)
            {
                results.Add(new DropoutRiskDto(
                    student.Id, student.FullName, student.DeptName,
                    Math.Round(percentage, 2), Math.Round(riskScore, 2), riskLevel, riskFactors));
            }
        }

        return results.OrderByDescending(r => r.RiskScore).Take(topN).ToList();
    }

    public async Task<List<FacultyStrictnessDto>> GetFacultyStrictnessAsync(Guid? departmentId)
    {
        var faculty = _userRepo.Query().Where(u => u.Role == UserRole.Faculty && u.IsActive);
        if (departmentId.HasValue) faculty = faculty.Where(u => u.DepartmentId == departmentId.Value);

        var facultyList = await faculty.Select(u => new { u.Id, u.FullName }).ToListAsync();
        var results = new List<FacultyStrictnessDto>();

        foreach (var f in facultyList)
        {
            var sessionIds = await _sessionRepo.Query()
                .Where(s => s.FacultyId == f.Id)
                .Select(s => s.Id).ToListAsync();

            if (!sessionIds.Any()) continue;

            var total = await _attendanceRepo.CountAsync(a => sessionIds.Contains(a.ClassSessionId));
            var present = await _attendanceRepo.CountAsync(a => sessionIds.Contains(a.ClassSessionId) && a.Status == AttendanceStatus.Present);
            var late = await _attendanceRepo.CountAsync(a => sessionIds.Contains(a.ClassSessionId) && a.Status == AttendanceStatus.Late);

            var avgAttendance = total > 0 ? present * 100.0 / total : 0;
            var latePercentage = total > 0 ? late * 100.0 / total : 0;

            var strictness = avgAttendance switch
            {
                < 60 => "Very Strict",
                < 75 => "Strict",
                < 85 => "Moderate",
                _ => "Lenient"
            };

            results.Add(new FacultyStrictnessDto(
                f.Id, f.FullName, Math.Round(avgAttendance, 2),
                sessionIds.Count, Math.Round(latePercentage, 2), strictness));
        }

        return results.OrderBy(r => r.AverageAttendance).ToList();
    }

    public async Task<List<CourseAnalyticsDto>> GetCourseAnalyticsAsync(Guid? departmentId)
    {
        var courses = _courseRepo.Query();
        if (departmentId.HasValue) courses = courses.Where(c => c.DepartmentId == departmentId.Value);

        var courseList = await courses.Select(c => new { c.Id, c.Name, c.Code }).ToListAsync();
        var results = new List<CourseAnalyticsDto>();

        foreach (var course in courseList)
        {
            var enrolled = await _enrollmentRepo.CountAsync(e => e.CourseId == course.Id && e.IsActive);
            var sessionIds = await _sessionRepo.Query().Where(s => s.CourseId == course.Id).Select(s => s.Id).ToListAsync();

            var total = await _attendanceRepo.CountAsync(a => sessionIds.Contains(a.ClassSessionId));
            var present = await _attendanceRepo.CountAsync(a => sessionIds.Contains(a.ClassSessionId) && a.Status == AttendanceStatus.Present);
            var percentage = total > 0 ? present * 100.0 / total : 0;

            // Count defaulters (below 75%)
            var defaulters = 0;
            var studentIds = await _enrollmentRepo.Query().Where(e => e.CourseId == course.Id && e.IsActive).Select(e => e.StudentId).ToListAsync();
            foreach (var sid in studentIds)
            {
                var stotal = await _attendanceRepo.CountAsync(a => a.StudentId == sid && sessionIds.Contains(a.ClassSessionId));
                var spresent = await _attendanceRepo.CountAsync(a => a.StudentId == sid && sessionIds.Contains(a.ClassSessionId) && a.Status == AttendanceStatus.Present);
                if (stotal > 0 && spresent * 100.0 / stotal < 75) defaulters++;
            }

            results.Add(new CourseAnalyticsDto(
                course.Id, course.Name, course.Code,
                Math.Round(percentage, 2), enrolled, defaulters, 0));
        }

        return results.OrderBy(r => r.AttendancePercentage).ToList();
    }

    public async Task<AdvancedDashboardDto> GetAdvancedDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;

        var activeSOS = await _sosRepo.CountAsync(s =>
            s.Status == SOSStatus.Active || s.Status == SOSStatus.Acknowledged || s.Status == SOSStatus.Responding);
        var pendingLeaves = await _leaveRepo.CountAsync(l => l.Status == LeaveStatus.Pending);
        var fraudToday = await _fraudRepo.CountAsync(f => f.CreatedAt >= today);
        var curfewToday = await _curfewRepo.CountAsync(c => c.CurfewTime.Date == today);

        var heatmap = await GetAttendanceHeatmapAsync(null, today.AddDays(-30), today);
        var riskStudents = await GetDropoutRiskStudentsAsync(null, 10);
        var facultyStrictness = await GetFacultyStrictnessAsync(null);

        return new AdvancedDashboardDto(
            activeSOS, pendingLeaves, fraudToday, curfewToday,
            heatmap, riskStudents, facultyStrictness);
    }
}
