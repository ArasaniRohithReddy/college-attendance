using CollegeAttendance.Domain.Common;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Domain.Entities;

public class ClassSession : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Room { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Scheduled;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double GeofenceRadiusMeters { get; set; } = 200;

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public Guid FacultyId { get; set; }
    public User Faculty { get; set; } = null!;

    public QRSession? QRSession { get; set; }
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
