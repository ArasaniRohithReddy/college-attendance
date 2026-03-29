using CollegeAttendance.Domain.Common;

namespace CollegeAttendance.Domain.Entities;

public class Course : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public int Semester { get; set; }
    public int Year { get; set; }

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public Guid FacultyId { get; set; }
    public User Faculty { get; set; } = null!;

    public ICollection<ClassSession> Sessions { get; set; } = new List<ClassSession>();
    public ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
}
