using CollegeAttendance.Domain.Common;

namespace CollegeAttendance.Domain.Entities;

public class CurfewLog : BaseEntity
{
    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;
    public Guid HostelId { get; set; }
    public Hostel Hostel { get; set; } = null!;
    public DateTime CurfewTime { get; set; }
    public DateTime? ReturnTime { get; set; }
    public int MinutesLate { get; set; }
    public bool ParentNotified { get; set; }
    public DateTime? ParentNotifiedAt { get; set; }
    public string? Notes { get; set; }
}
