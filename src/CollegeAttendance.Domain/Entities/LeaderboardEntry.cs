using CollegeAttendance.Domain.Common;

namespace CollegeAttendance.Domain.Entities;

public class LeaderboardEntry : BaseEntity
{
    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public double TotalScore { get; set; }
    public double AttendanceScore { get; set; }
    public double StreakScore { get; set; }
    public double ConsistencyScore { get; set; }
    public int Rank { get; set; }
    public string Period { get; set; } = string.Empty; // e.g. "2025-W03", "2025-01", "2025-S1"
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
