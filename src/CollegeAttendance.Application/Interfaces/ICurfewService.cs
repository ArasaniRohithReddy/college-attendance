using CollegeAttendance.Application.DTOs;

namespace CollegeAttendance.Application.Interfaces;

public interface ICurfewService
{
    Task CheckCurfewViolationsAsync();
    Task<List<CurfewLogDto>> GetCurfewLogsAsync(Guid? hostelId, DateTime from, DateTime to);
    Task<List<CurfewLogDto>> GetStudentCurfewLogsAsync(Guid studentId);
    Task<CurfewConfigDto> GetCurfewConfigAsync();
    Task UpdateCurfewConfigAsync(Guid userId, CurfewConfigDto config);
}
