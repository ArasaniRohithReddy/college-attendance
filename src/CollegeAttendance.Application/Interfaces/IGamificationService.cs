using CollegeAttendance.Application.DTOs;

namespace CollegeAttendance.Application.Interfaces;

public interface IGamificationService
{
    Task<GamificationDashboardDto> GetStudentDashboardAsync(Guid studentId);
    Task<StreakDto> GetStreakAsync(Guid studentId);
    Task<List<StudentBadgeDto>> GetStudentBadgesAsync(Guid studentId);
    Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(string period, Guid? departmentId, int top = 50);
    Task EvaluateBadgesForStudentAsync(Guid studentId);
    Task RecalculateStreaksAsync();
    Task RecalculateLeaderboardAsync(string period);
    Task<List<BadgeDto>> GetAllBadgesAsync();
}
