using CollegeAttendance.Domain.Common;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Domain.Entities;

public class SystemConfig : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DataType { get; set; } = "string"; // string, int, bool, json
    public ConfigCategory Category { get; set; }
    public Guid? ModifiedById { get; set; }
    public User? ModifiedBy { get; set; }
}
