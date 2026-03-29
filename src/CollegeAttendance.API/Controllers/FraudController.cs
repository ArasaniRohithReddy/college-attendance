using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FraudController : ControllerBase
{
    private readonly IFraudDetectionService _fraudService;

    public FraudController(IFraudDetectionService fraudService) => _fraudService = fraudService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("logs")]
    [Authorize(Roles = "Admin,Faculty")]
    public async Task<ActionResult<List<FraudLogDto>>> GetFraudLogs(
        [FromQuery] Guid? userId,
        [FromQuery] bool unresolvedOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => Ok(await _fraudService.GetFraudLogsAsync(userId, unresolvedOnly, page, pageSize));

    [HttpPost("logs/{id:guid}/resolve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveFraudRequest request)
    {
        await _fraudService.ResolveAsync(id, UserId, request.ResolutionNotes);
        return NoContent();
    }

    [HttpGet("devices/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<DeviceBindingDto>>> GetDevices(Guid userId)
        => Ok(await _fraudService.GetUserDevicesAsync(userId));

    [HttpPost("devices/bind")]
    public async Task<ActionResult<DeviceBindingDto>> BindDevice([FromBody] BindDeviceRequest request)
        => Ok(await _fraudService.BindDeviceAsync(UserId, request));

    [HttpPost("devices/{bindingId:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateDevice(Guid bindingId)
    {
        await _fraudService.DeactivateDeviceAsync(bindingId);
        return NoContent();
    }

    [HttpGet("devices/validate")]
    public async Task<ActionResult<bool>> ValidateDevice([FromQuery] string fingerprint)
        => Ok(await _fraudService.ValidateDeviceAsync(UserId, fingerprint));
}
