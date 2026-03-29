using CollegeAttendance.Domain.Common;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Domain.Entities;

public class MessLog : BaseEntity
{
    public MealType MealType { get; set; }
    public DateTime Date { get; set; }
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
    public string? VerificationMethod { get; set; }

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;
}
