using CollegeAttendance.Application.DTOs;

namespace CollegeAttendance.Application.Interfaces;

public interface IFraudDetectionService
{
    Task<FraudLogDto?> AnalyzeAttendanceScanAsync(Guid studentId, Guid sessionId, double latitude, double longitude, string? deviceId);
    Task<List<FraudLogDto>> GetFraudLogsAsync(Guid? userId, bool unresolvedOnly, int page, int pageSize);
    Task ResolveAsync(Guid fraudLogId, Guid resolvedById, string resolutionNotes);
    Task<List<DeviceBindingDto>> GetUserDevicesAsync(Guid userId);
    Task<DeviceBindingDto> BindDeviceAsync(Guid userId, BindDeviceRequest request);
    Task DeactivateDeviceAsync(Guid bindingId);
    Task<bool> ValidateDeviceAsync(Guid userId, string deviceFingerprint);
}
