using CollegeAttendance.Domain.Common;

namespace CollegeAttendance.Domain.Entities;

public class DeviceBinding : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string DeviceFingerprint { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? Platform { get; set; } // Android, iOS, Web
    public DateTime BoundAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime? LastUsedAt { get; set; }
    public string? IpAddress { get; set; }
}
