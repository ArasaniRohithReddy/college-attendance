using CollegeAttendance.Domain.Common;

namespace CollegeAttendance.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? HeadOfDepartmentId { get; set; }
    public User? HeadOfDepartment { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
