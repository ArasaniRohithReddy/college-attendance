using CollegeAttendance.Domain.Common;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Domain.Entities;

public class FraudLog : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public FraudType FraudType { get; set; }
    public FraudSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Evidence { get; set; } // JSON serialized evidence
    public Guid? ClassSessionId { get; set; }
    public ClassSession? ClassSession { get; set; }
    public bool IsResolved { get; set; }
    public Guid? ResolvedById { get; set; }
    public User? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
}
