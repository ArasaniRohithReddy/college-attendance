using CollegeAttendance.Domain.Common;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Domain.Entities;

public class OfflineSyncLog : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public SyncEntityType EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string Action { get; set; } = string.Empty; // Create, Update, Delete
    public string Payload { get; set; } = string.Empty; // JSON serialized data
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? SyncedAt { get; set; }
    public string? ConflictDetails { get; set; }
    public string? DeviceId { get; set; }
}
