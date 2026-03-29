using CollegeAttendance.Domain.Common;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Domain.Entities;

public class Badge : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public BadgeType Type { get; set; }
    public string? IconUrl { get; set; }
    public string? RuleExpression { get; set; } // e.g. "streak >= 30" or "attendance >= 95"
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<StudentBadge> StudentBadges { get; set; } = new List<StudentBadge>();
}
