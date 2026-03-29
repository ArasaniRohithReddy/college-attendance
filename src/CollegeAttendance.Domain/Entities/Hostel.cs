using CollegeAttendance.Domain.Common;

namespace CollegeAttendance.Domain.Entities;

public class Hostel : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Block { get; set; }
    public int Capacity { get; set; }
    public Guid? WardenId { get; set; }
    public User? Warden { get; set; }

    public ICollection<User> Residents { get; set; } = new List<User>();
    public ICollection<HostelLog> Logs { get; set; } = new List<HostelLog>();
}
