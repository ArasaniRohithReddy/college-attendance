using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CollegeAttendance.Application.Services;

public class OfflineSyncService : IOfflineSyncService
{
    private readonly IRepository<OfflineSyncLog> _syncRepo;
    private readonly IAttendanceService _attendanceService;
    private readonly IUnitOfWork _unitOfWork;

    public OfflineSyncService(
        IRepository<OfflineSyncLog> syncRepo,
        IAttendanceService attendanceService,
        IUnitOfWork unitOfWork)
    {
        _syncRepo = syncRepo;
        _attendanceService = attendanceService;
        _unitOfWork = unitOfWork;
    }

    public async Task<SyncBatchResponse> ProcessBatchAsync(Guid userId, SyncBatchRequest request)
    {
        int succeeded = 0, failed = 0, conflicts = 0;
        var results = new List<SyncItemResult>();

        for (int i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];
            var log = new OfflineSyncLog
            {
                UserId = userId,
                EntityType = item.EntityType,
                Action = item.Action,
                Payload = item.Payload,
                DeviceId = item.DeviceId,
                SyncStatus = SyncStatus.Pending,
                AttemptCount = 1,
                LastAttemptAt = DateTime.UtcNow
            };

            try
            {
                Guid? entityId = await ProcessSyncItemAsync(userId, item);
                log.SyncStatus = SyncStatus.Synced;
                log.SyncedAt = DateTime.UtcNow;
                log.EntityId = entityId;
                succeeded++;
                results.Add(new SyncItemResult(i, true, null, null, entityId));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("conflict", StringComparison.OrdinalIgnoreCase))
            {
                log.SyncStatus = SyncStatus.Conflict;
                log.ConflictDetails = ex.Message;
                conflicts++;
                results.Add(new SyncItemResult(i, false, ex.Message, ex.Message, null));
            }
            catch (Exception ex)
            {
                log.SyncStatus = SyncStatus.Failed;
                failed++;
                results.Add(new SyncItemResult(i, false, ex.Message, null, null));
            }

            await _syncRepo.AddAsync(log);
        }

        await _unitOfWork.SaveChangesAsync();

        return new SyncBatchResponse(request.Items.Count, succeeded, failed, conflicts, results);
    }

    private async Task<Guid?> ProcessSyncItemAsync(Guid userId, SyncItemRequest item)
    {
        switch (item.EntityType)
        {
            case SyncEntityType.Attendance:
                var attendanceReq = JsonSerializer.Deserialize<MarkAttendanceRequest>(item.Payload);
                if (attendanceReq != null)
                {
                    var result = await _attendanceService.MarkAttendanceByQRAsync(userId, attendanceReq);
                    return result.Id;
                }
                break;
        }
        return null;
    }

    public async Task<List<OfflineSyncLogDto>> GetPendingLogsAsync(Guid userId)
    {
        return await _syncRepo.Query()
            .Where(s => s.UserId == userId && (s.SyncStatus == SyncStatus.Pending || s.SyncStatus == SyncStatus.Failed))
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new OfflineSyncLogDto(s.Id, s.EntityType, s.Action, s.Payload, s.SyncStatus, s.AttemptCount, s.SyncedAt))
            .ToListAsync();
    }

    public async Task<DateTime> GetLastSyncTimestampAsync(Guid userId)
    {
        var lastSync = await _syncRepo.Query()
            .Where(s => s.UserId == userId && s.SyncStatus == SyncStatus.Synced)
            .OrderByDescending(s => s.SyncedAt)
            .FirstOrDefaultAsync();

        return lastSync?.SyncedAt ?? DateTime.MinValue;
    }
}
