using CollegeAttendance.Domain.Common;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Domain.Entities;

public class HostelLog : BaseEntity
{
    public HostelLogType LogType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? VerificationMethod { get; set; } // QR, ID Card, Manual

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;
    public Guid HostelId { get; set; }
    public Hostel Hostel { get; set; } = null!;
    public Guid? VerifiedById { get; set; }
    public User? VerifiedBy { get; set; }
}
