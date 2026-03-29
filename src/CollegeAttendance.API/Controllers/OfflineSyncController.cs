using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OfflineSyncController : ControllerBase
{
    private readonly IOfflineSyncService _syncService;

    public OfflineSyncController(IOfflineSyncService syncService) => _syncService = syncService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("batch")]
    public async Task<ActionResult<SyncBatchResponse>> ProcessBatch([FromBody] SyncBatchRequest request)
        => Ok(await _syncService.ProcessBatchAsync(UserId, request));

    [HttpGet("pending")]
    public async Task<ActionResult<List<OfflineSyncLogDto>>> GetPending()
        => Ok(await _syncService.GetPendingLogsAsync(UserId));

    [HttpGet("last-sync")]
    public async Task<ActionResult<DateTime>> GetLastSync()
        => Ok(await _syncService.GetLastSyncTimestampAsync(UserId));
}
