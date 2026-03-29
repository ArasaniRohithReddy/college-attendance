using CollegeAttendance.Domain.Common;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Domain.Entities;

public class LeaveRequest : BaseEntity
{
    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public Guid? ApprovedById { get; set; }
    public User? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? DocumentUrl { get; set; }
    public Guid? CourseId { get; set; }
    public Course? Course { get; set; }
}
