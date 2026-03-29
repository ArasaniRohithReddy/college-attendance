using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Course> _courseRepo;
    private readonly IRepository<ClassSession> _sessionRepo;
    private readonly IRepository<AttendanceRecord> _attendanceRepo;
    private readonly IRepository<CourseEnrollment> _enrollmentRepo;
    private readonly IRepository<Department> _deptRepo;

    public AnalyticsService(
        IRepository<User> userRepo, IRepository<Course> courseRepo,
        IRepository<ClassSession> sessionRepo, IRepository<AttendanceRecord> attendanceRepo,
        IRepository<CourseEnrollment> enrollmentRepo, IRepository<Department> deptRepo)
    {
        _userRepo = userRepo;
        _courseRepo = courseRepo;
        _sessionRepo = sessionRepo;
        _attendanceRepo = attendanceRepo;
        _enrollmentRepo = enrollmentRepo;
        _deptRepo = deptRepo;
    }

    public async Task<DashboardAnalyticsDto> GetDashboardAsync()
    {
        var totalStudents = await _userRepo.CountAsync(u => u.Role == UserRole.Student && !u.IsDeleted);
        var totalFaculty = await _userRepo.CountAsync(u => u.Role == UserRole.Faculty && !u.IsDeleted);
        var totalCourses = await _courseRepo.CountAsync(c => !c.IsDeleted);
        var today = DateTime.UtcNow.Date;
        var totalSessionsToday = await _sessionRepo.CountAsync(s => s.ScheduledDate.Date == today && !s.IsDeleted);

        var totalRecords = await _attendanceRepo.CountAsync(a => !a.IsDeleted);
        var presentRecords = await _attendanceRepo.CountAsync(a =>
            !a.IsDeleted && (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late));
        var overallPercentage = totalRecords > 0 ? presentRecords * 100.0 / totalRecords : 0;

        var defaulterCount = 0;
        var students = await _userRepo.FindAsync(u => u.Role == UserRole.Student && !u.IsDeleted);
        foreach (var student in students)
        {
            var enrollments = await _enrollmentRepo.FindAsync(e => e.StudentId == student.Id && e.IsActive);
            foreach (var enrollment in enrollments)
            {
                var sessions = await _sessionRepo.CountAsync(s => s.CourseId == enrollment.CourseId && s.Status == SessionStatus.Completed);
                if (sessions == 0) continue;
                var attended = await _attendanceRepo.CountAsync(a =>
                    a.StudentId == student.Id && a.ClassSession.CourseId == enrollment.CourseId &&
                    (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late) && !a.IsDeleted);
                if (attended * 100.0 / sessions < 75)
                {
                    defaulterCount++;
                    break;
                }
            }
        }

        var depts = await _deptRepo.Query().Where(d => !d.IsDeleted).ToListAsync();
        var deptWise = new List<DepartmentAttendanceDto>();
        foreach (var dept in depts)
        {
            var deptStudents = await _userRepo.CountAsync(u => u.DepartmentId == dept.Id && u.Role == UserRole.Student && !u.IsDeleted);
            var deptTotal = await _attendanceRepo.Query()
                .Where(a => a.Student.DepartmentId == dept.Id && !a.IsDeleted).CountAsync();
            var deptPresent = await _attendanceRepo.Query()
                .Where(a => a.Student.DepartmentId == dept.Id && !a.IsDeleted &&
                    (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late)).CountAsync();
            var deptPct = deptTotal > 0 ? deptPresent * 100.0 / deptTotal : 0;
            deptWise.Add(new DepartmentAttendanceDto(dept.Name, Math.Round(deptPct, 2), deptStudents));
        }

        return new DashboardAnalyticsDto(totalStudents, totalFaculty, totalCourses,
            totalSessionsToday, Math.Round(overallPercentage, 2), defaulterCount, deptWise);
    }

    public async Task<AttendanceReportDto> GetCourseReportAsync(Guid courseId, DateTime from, DateTime to)
    {
        var course = await _courseRepo.GetByIdAsync(courseId)
            ?? throw new KeyNotFoundException("Course not found");

        var sessions = await _sessionRepo.CountAsync(s =>
            s.CourseId == courseId && s.Status == SessionStatus.Completed &&
            s.ScheduledDate >= from && s.ScheduledDate <= to);

        var records = await _attendanceRepo.Query()
            .Where(a => a.ClassSession.CourseId == courseId && !a.IsDeleted &&
                        a.ClassSession.ScheduledDate >= from && a.ClassSession.ScheduledDate <= to)
            .ToListAsync();

        var present = records.Count(r => r.Status == AttendanceStatus.Present);
        var late = records.Count(r => r.Status == AttendanceStatus.Late);
        var excused = records.Count(r => r.Status == AttendanceStatus.Excused);
        var absent = records.Count(r => r.Status == AttendanceStatus.Absent);
        var total = present + late + excused + absent;
        var pct = total > 0 ? (present + late + excused) * 100.0 / total : 0;

        return new AttendanceReportDto(
            Guid.Empty, "All Students", null,
            course.Name, course.Code, sessions, present, absent, late, excused,
            Math.Round(pct, 2), pct < 75);
    }

    public async Task<object> GetDepartmentReportAsync(Guid departmentId, DateTime from, DateTime to)
    {
        var dept = await _deptRepo.GetByIdAsync(departmentId)
            ?? throw new KeyNotFoundException("Department not found");

        var courses = await _courseRepo.FindAsync(c => c.DepartmentId == departmentId && !c.IsDeleted);
        var reports = new List<AttendanceReportDto>();

        foreach (var course in courses)
        {
            var enrollments = await _enrollmentRepo.Query()
                .Include(e => e.Student)
                .Where(e => e.CourseId == course.Id && e.IsActive).ToListAsync();

            var totalSessions = await _sessionRepo.CountAsync(s =>
                s.CourseId == course.Id && s.Status == SessionStatus.Completed &&
                s.ScheduledDate >= from && s.ScheduledDate <= to);

            foreach (var e in enrollments)
            {
                var records = await _attendanceRepo.Query()
                    .Where(a => a.StudentId == e.StudentId && a.ClassSession.CourseId == course.Id && !a.IsDeleted &&
                                a.ClassSession.ScheduledDate >= from && a.ClassSession.ScheduledDate <= to)
                    .ToListAsync();

                var present = records.Count(r => r.Status == AttendanceStatus.Present);
                var late = records.Count(r => r.Status == AttendanceStatus.Late);
                var excused = records.Count(r => r.Status == AttendanceStatus.Excused);
                var absent = totalSessions - (present + late + excused);
                var pct = totalSessions > 0 ? (present + late + excused) * 100.0 / totalSessions : 0;

                reports.Add(new AttendanceReportDto(
                    e.StudentId, e.Student.FullName, e.Student.StudentId,
                    course.Name, course.Code, totalSessions, present, absent, late, excused,
                    Math.Round(pct, 2), pct < 75));
            }
        }

        return new { Department = dept.Name, Reports = reports };
    }

    public async Task<List<StudentAttendancePredictionDto>> GetAttendancePredictionsAsync(Guid? courseId)
    {
        var predictions = new List<StudentAttendancePredictionDto>();

        IEnumerable<CourseEnrollment> enrollments;
        if (courseId.HasValue)
            enrollments = await _enrollmentRepo.Query()
                .Include(e => e.Student)
                .Where(e => e.CourseId == courseId.Value && e.IsActive).ToListAsync();
        else
            enrollments = await _enrollmentRepo.Query()
                .Include(e => e.Student)
                .Where(e => e.IsActive).ToListAsync();

        var grouped = enrollments.GroupBy(e => e.StudentId);

        foreach (var group in grouped)
        {
            var student = group.First().Student;
            var totalSessions = 0;
            var totalAttended = 0;

            foreach (var enrollment in group)
            {
                var sessions = await _sessionRepo.CountAsync(s =>
                    s.CourseId == enrollment.CourseId && s.Status == SessionStatus.Completed);
                var attended = await _attendanceRepo.CountAsync(a =>
                    a.StudentId == student.Id && a.ClassSession.CourseId == enrollment.CourseId &&
                    (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late) && !a.IsDeleted);
                totalSessions += sessions;
                totalAttended += attended;
            }

            var current = totalSessions > 0 ? totalAttended * 100.0 / totalSessions : 100;
            var predicted = current;
            var riskLevel = current < 60 ? "Critical" : current < 75 ? "High" : current < 85 ? "Medium" : "Low";

            predictions.Add(new StudentAttendancePredictionDto(
                student.Id, student.FullName,
                Math.Round(current, 2), Math.Round(predicted, 2),
                current < 80, riskLevel));
        }

        return predictions.OrderBy(p => p.CurrentPercentage).ToList();
    }

    public async Task<object> GetStudentStatsAsync(Guid studentId)
    {
        var student = await _userRepo.GetByIdAsync(studentId)
            ?? throw new KeyNotFoundException("Student not found");

        var enrollments = await _enrollmentRepo.Query()
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId && e.IsActive)
            .ToListAsync();

        var courseStats = new List<object>();
        var totalSessions = 0;
        var totalAttended = 0;

        foreach (var enrollment in enrollments)
        {
            var sessions = await _sessionRepo.CountAsync(s =>
                s.CourseId == enrollment.CourseId && s.Status == SessionStatus.Completed);
            var attended = await _attendanceRepo.CountAsync(a =>
                a.StudentId == studentId && a.ClassSession.CourseId == enrollment.CourseId &&
                (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late) && !a.IsDeleted);

            totalSessions += sessions;
            totalAttended += attended;
            var pct = sessions > 0 ? attended * 100.0 / sessions : 100;

            courseStats.Add(new
            {
                CourseId = enrollment.CourseId,
                CourseName = enrollment.Course.Name,
                CourseCode = enrollment.Course.Code,
                TotalSessions = sessions,
                Attended = attended,
                Percentage = Math.Round(pct, 2),
                IsDefaulter = pct < 75
            });
        }

        var overallPct = totalSessions > 0 ? totalAttended * 100.0 / totalSessions : 100;

        return new
        {
            StudentId = student.Id,
            StudentName = student.FullName,
            OverallPercentage = Math.Round(overallPct, 2),
            TotalCourses = enrollments.Count,
            TotalSessions = totalSessions,
            TotalAttended = totalAttended,
            IsDefaulter = overallPct < 75,
            Courses = courseStats
        };
    }
}
