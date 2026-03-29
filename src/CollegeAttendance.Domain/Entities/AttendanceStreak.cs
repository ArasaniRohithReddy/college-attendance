using CollegeAttendance.Domain.Common;

namespace CollegeAttendance.Domain.Entities;

public class AttendanceStreak : BaseEntity
{
    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastPresentDate { get; set; }
    public DateTime? StreakStartDate { get; set; }
    public int TotalPresentDays { get; set; }
}
