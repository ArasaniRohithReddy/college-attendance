using CollegeAttendance.Application.DTOs;

namespace CollegeAttendance.Application.Interfaces;

public interface IOfflineSyncService
{
    Task<SyncBatchResponse> ProcessBatchAsync(Guid userId, SyncBatchRequest request);
    Task<List<OfflineSyncLogDto>> GetPendingLogsAsync(Guid userId);
    Task<DateTime> GetLastSyncTimestampAsync(Guid userId);
}
