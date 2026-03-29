using CollegeAttendance.Domain.Common;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Domain.Entities;

public class ScheduledNotification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsSent { get; set; }
    public UserRole? TargetRole { get; set; }
    public Guid? TargetUserId { get; set; }
    public User? TargetUser { get; set; }
    public bool IsRecurring { get; set; }
    public string? CronExpression { get; set; }
    public bool IsActive { get; set; } = true;
}
