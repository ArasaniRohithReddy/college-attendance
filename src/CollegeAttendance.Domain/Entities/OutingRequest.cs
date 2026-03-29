using CollegeAttendance.Domain.Common;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Domain.Entities;

public class OutingRequest : BaseEntity
{
    public string Purpose { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime RequestedOutTime { get; set; }
    public DateTime ExpectedReturnTime { get; set; }
    public DateTime? ActualOutTime { get; set; }
    public DateTime? ActualReturnTime { get; set; }
    public OutingStatus Status { get; set; } = OutingStatus.Pending;
    public string? WardenRemarks { get; set; }
    public string? SecurityRemarks { get; set; }
    public string? GatePassQRCode { get; set; }
    public DateTime? GatePassExpiresAt { get; set; }
    public string? EmergencyContact { get; set; }

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;
    public Guid? ApprovedByWardenId { get; set; }
    public User? ApprovedByWarden { get; set; }
    public Guid? ProcessedBySecurityId { get; set; }
    public User? ProcessedBySecurity { get; set; }
}
