using CollegeAttendance.Application.DTOs;

namespace CollegeAttendance.Application.Interfaces;

public interface IEmergencyService
{
    Task<EmergencySOSDto> CreateSOSAsync(Guid studentId, CreateSOSRequest request);
    Task<EmergencySOSDto?> GetByIdAsync(Guid sosId);
    Task<List<EmergencySOSDto>> GetActiveSOSAsync();
    Task AcknowledgeAsync(Guid sosId, Guid responderId);
    Task ResolveAsync(Guid sosId, Guid responderId, string resolutionNotes);
    Task<List<EmergencySOSDto>> GetHistoryAsync(Guid? studentId, int page, int pageSize);
}
