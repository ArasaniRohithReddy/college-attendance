using CollegeAttendance.Domain.Common;

namespace CollegeAttendance.Domain.Entities;

public class StudentBadge : BaseEntity
{
    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;
    public Guid BadgeId { get; set; }
    public Badge Badge { get; set; } = null!;
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}
