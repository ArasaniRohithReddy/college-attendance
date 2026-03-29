using CollegeAttendance.Domain.Common;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Domain.Entities;

public class EmergencySOS : BaseEntity
{
    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Message { get; set; }
    public SOSStatus Status { get; set; } = SOSStatus.Active;
    public SOSPriority Priority { get; set; } = SOSPriority.High;
    public Guid? RespondedById { get; set; }
    public User? RespondedBy { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
}
