using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class GamificationService : IGamificationService
{
    private readonly IRepository<Badge> _badgeRepo;
    private readonly IRepository<StudentBadge> _studentBadgeRepo;
    private readonly IRepository<AttendanceStreak> _streakRepo;
    private readonly IRepository<LeaderboardEntry> _leaderboardRepo;
    private readonly IRepository<AttendanceRecord> _attendanceRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public GamificationService(
        IRepository<Badge> badgeRepo,
        IRepository<StudentBadge> studentBadgeRepo,
        IRepository<AttendanceStreak> streakRepo,
        IRepository<LeaderboardEntry> leaderboardRepo,
        IRepository<AttendanceRecord> attendanceRepo,
        IRepository<User> userRepo,
        IUnitOfWork unitOfWork)
    {
        _badgeRepo = badgeRepo;
        _studentBadgeRepo = studentBadgeRepo;
        _streakRepo = streakRepo;
        _leaderboardRepo = leaderboardRepo;
        _attendanceRepo = attendanceRepo;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<GamificationDashboardDto> GetStudentDashboardAsync(Guid studentId)
    {
        var streak = await GetStreakAsync(studentId);
        var badges = await GetStudentBadgesAsync(studentId);
        var leaderboard = await _leaderboardRepo.Query()
            .Where(l => l.StudentId == studentId)
            .OrderByDescending(l => l.CalculatedAt)
            .FirstOrDefaultAsync();

        return new GamificationDashboardDto(
            streak, badges,
            leaderboard?.Rank ?? 0,
            leaderboard?.TotalScore ?? 0,
            badges.Count,
            leaderboard == null ? null : new LeaderboardEntryDto(
                leaderboard.Rank, leaderboard.StudentId, streak.StudentName,
                null, leaderboard.TotalScore, leaderboard.AttendanceScore,
                leaderboard.StreakScore, leaderboard.ConsistencyScore, leaderboard.Period)
        );
    }

    public async Task<StreakDto> GetStreakAsync(Guid studentId)
    {
        var streak = await _streakRepo.FirstOrDefaultAsync(s => s.StudentId == studentId);
        var student = await _userRepo.GetByIdAsync(studentId);
        var name = student?.FullName ?? "Unknown";

        if (streak == null)
            return new StreakDto(studentId, name, 0, 0, null, 0);

        return new StreakDto(studentId, name, streak.CurrentStreak, streak.LongestStreak, streak.LastPresentDate, streak.TotalPresentDays);
    }

    public async Task<List<StudentBadgeDto>> GetStudentBadgesAsync(Guid studentId)
    {
        return await _studentBadgeRepo.Query()
            .Where(sb => sb.StudentId == studentId)
            .Include(sb => sb.Badge)
            .Include(sb => sb.Student)
            .OrderByDescending(sb => sb.EarnedAt)
            .Select(sb => new StudentBadgeDto(
                sb.Id, sb.StudentId, sb.Student.FullName,
                new BadgeDto(sb.Badge.Id, sb.Badge.Name, sb.Badge.Description, sb.Badge.Type, sb.Badge.IconUrl, sb.Badge.SortOrder),
                sb.EarnedAt))
            .ToListAsync();
    }

    public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(string period, Guid? departmentId, int top = 50)
    {
        var query = _leaderboardRepo.Query()
            .Where(l => l.Period == period);

        if (departmentId.HasValue)
            query = query.Where(l => l.DepartmentId == departmentId);

        return await query
            .OrderBy(l => l.Rank)
            .Take(top)
            .Include(l => l.Student)
            .Include(l => l.Department)
            .Select(l => new LeaderboardEntryDto(
                l.Rank, l.StudentId, l.Student.FullName, l.Department != null ? l.Department.Name : null,
                l.TotalScore, l.AttendanceScore, l.StreakScore, l.ConsistencyScore, l.Period))
            .ToListAsync();
    }

    public async Task EvaluateBadgesForStudentAsync(Guid studentId)
    {
        var existingBadgeIds = await _studentBadgeRepo.Query()
            .Where(sb => sb.StudentId == studentId)
            .Select(sb => sb.BadgeId)
            .ToListAsync();

        var badges = await _badgeRepo.Query().Where(b => b.IsActive).ToListAsync();
        var streak = await _streakRepo.FirstOrDefaultAsync(s => s.StudentId == studentId);
        var totalRecords = await _attendanceRepo.CountAsync(a => a.StudentId == studentId);
        var presentCount = await _attendanceRepo.CountAsync(a => a.StudentId == studentId && a.Status == AttendanceStatus.Present);
        var lateCount = await _attendanceRepo.CountAsync(a => a.StudentId == studentId && a.Status == AttendanceStatus.Late);
        var attendancePct = totalRecords > 0 ? (double)presentCount / totalRecords * 100 : 0;

        foreach (var badge in badges.Where(b => !existingBadgeIds.Contains(b.Id)))
        {
            bool earned = badge.Type switch
            {
                BadgeType.FirstScan => totalRecords >= 1,
                BadgeType.WeekWarrior => (streak?.CurrentStreak ?? 0) >= 5,
                BadgeType.MonthlyChampion => (streak?.CurrentStreak ?? 0) >= 22,
                BadgeType.StreakMaster => (streak?.CurrentStreak ?? 0) >= 30,
                BadgeType.OnTimeHero => totalRecords >= 10 && lateCount == 0,
                BadgeType.HundredPercent => attendancePct >= 99.9 && totalRecords >= 20,
                BadgeType.EarlyBird => (streak?.CurrentStreak ?? 0) >= 3,
                _ => false
            };

            if (earned)
            {
                await _studentBadgeRepo.AddAsync(new StudentBadge
                {
                    StudentId = studentId,
                    BadgeId = badge.Id,
                    EarnedAt = DateTime.UtcNow
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RecalculateStreaksAsync()
    {
        var students = await _userRepo.Query()
            .Where(u => u.Role == UserRole.Student && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync();

        foreach (var studentId in students)
        {
            var records = await _attendanceRepo.Query()
                .Where(a => a.StudentId == studentId && (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late))
                .OrderByDescending(a => a.MarkedAt)
                .Select(a => a.MarkedAt.Date)
                .Distinct()
                .ToListAsync();

            int currentStreak = 0;
            var today = DateTime.UtcNow.Date;
            foreach (var date in records)
            {
                if (date == today.AddDays(-currentStreak) || date == today.AddDays(-currentStreak - 1))
                    currentStreak++;
                else
                    break;
            }

            var streak = await _streakRepo.FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (streak == null)
            {
                streak = new AttendanceStreak { StudentId = studentId };
                await _streakRepo.AddAsync(streak);
            }

            streak.CurrentStreak = currentStreak;
            streak.LongestStreak = Math.Max(streak.LongestStreak, currentStreak);
            streak.LastPresentDate = records.FirstOrDefault();
            streak.TotalPresentDays = records.Count;
            streak.StreakStartDate = currentStreak > 0 ? today.AddDays(-(currentStreak - 1)) : null;
            streak.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RecalculateLeaderboardAsync(string period)
    {
        var students = await _userRepo.Query()
            .Where(u => u.Role == UserRole.Student && u.IsActive)
            .Include(u => u.Department)
            .ToListAsync();

        var entries = new List<LeaderboardEntry>();

        foreach (var student in students)
        {
            var totalSessions = await _attendanceRepo.CountAsync(a => a.StudentId == student.Id);
            var presentCount = await _attendanceRepo.CountAsync(a => a.StudentId == student.Id && (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late));
            var streak = await _streakRepo.FirstOrDefaultAsync(s => s.StudentId == student.Id);

            double attendanceScore = totalSessions > 0 ? ((double)presentCount / totalSessions) * 50 : 0;
            double streakScore = (streak?.CurrentStreak ?? 0) * 1.0;
            double consistencyScore = totalSessions > 0 ? ((double)presentCount / totalSessions) * 30 : 0;
            double totalScore = attendanceScore + streakScore + consistencyScore;

            entries.Add(new LeaderboardEntry
            {
                StudentId = student.Id,
                DepartmentId = student.DepartmentId,
                TotalScore = Math.Round(totalScore, 2),
                AttendanceScore = Math.Round(attendanceScore, 2),
                StreakScore = Math.Round(streakScore, 2),
                ConsistencyScore = Math.Round(consistencyScore, 2),
                Period = period,
                CalculatedAt = DateTime.UtcNow
            });
        }

        // Rank them
        var ranked = entries.OrderByDescending(e => e.TotalScore).ToList();
        for (int i = 0; i < ranked.Count; i++)
            ranked[i].Rank = i + 1;

        // Remove old entries for this period
        var old = await _leaderboardRepo.Query().Where(l => l.Period == period).ToListAsync();
        foreach (var o in old) _leaderboardRepo.Remove(o);

        await _leaderboardRepo.AddRangeAsync(ranked);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<BadgeDto>> GetAllBadgesAsync()
    {
        return await _badgeRepo.Query()
            .Where(b => b.IsActive)
            .OrderBy(b => b.SortOrder)
            .Select(b => new BadgeDto(b.Id, b.Name, b.Description, b.Type, b.IconUrl, b.SortOrder))
            .ToListAsync();
    }
}
