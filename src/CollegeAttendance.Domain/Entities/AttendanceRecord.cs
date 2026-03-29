using CollegeAttendance.Domain.Common;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Domain.Entities;

public class AttendanceRecord : BaseEntity
{
    public AttendanceStatus Status { get; set; }
    public DateTime MarkedAt { get; set; } = DateTime.UtcNow;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsGeofenceValid { get; set; }
    public bool IsManualEntry { get; set; }
    public string? DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public string? Remarks { get; set; }
    public bool IsFraudSuspected { get; set; }

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;
    public Guid ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; } = null!;
    public Guid? QRSessionId { get; set; }
    public QRSession? QRSession { get; set; }
    public Guid? MarkedById { get; set; }
    public User? MarkedBy { get; set; }
}
