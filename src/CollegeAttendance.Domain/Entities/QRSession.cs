using CollegeAttendance.Domain.Common;

namespace CollegeAttendance.Domain.Entities;

public class QRSession : BaseEntity
{
    public string EncryptedPayload { get; set; } = string.Empty;
    public string QRToken { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int ScanCount { get; set; } = 0;

    public Guid ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; } = null!;
    public Guid GeneratedById { get; set; }
    public User GeneratedBy { get; set; } = null!;
}
