using CollegeAttendance.Domain.Common;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? GoogleId { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public string? StudentId { get; set; }
    public string? EmployeeId { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public bool IsActive { get; set; } = true;
    public string? DeviceId { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public Guid? HostelId { get; set; }
    public Hostel? Hostel { get; set; }
    public ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<OutingRequest> OutingRequests { get; set; } = new List<OutingRequest>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
