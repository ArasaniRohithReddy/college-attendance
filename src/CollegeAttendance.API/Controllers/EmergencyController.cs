using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmergencyController : ControllerBase
{
    private readonly IEmergencyService _emergencyService;

    public EmergencyController(IEmergencyService emergencyService) => _emergencyService = emergencyService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("sos")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<EmergencySOSDto>> CreateSOS([FromBody] CreateSOSRequest request)
    {
        var sos = await _emergencyService.CreateSOSAsync(UserId, request);
        return CreatedAtAction(nameof(GetSOS), new { id = sos.Id }, sos);
    }

    [HttpGet("sos/{id:guid}")]
    public async Task<ActionResult<EmergencySOSDto>> GetSOS(Guid id)
    {
        var sos = await _emergencyService.GetByIdAsync(id);
        return sos is null ? NotFound() : Ok(sos);
    }

    [HttpGet("sos/active")]
    [Authorize(Roles = "Admin,Warden,Security")]
    public async Task<ActionResult<List<EmergencySOSDto>>> GetActiveSOS()
        => Ok(await _emergencyService.GetActiveSOSAsync());

    [HttpPost("sos/{id:guid}/acknowledge")]
    [Authorize(Roles = "Admin,Warden,Security")]
    public async Task<IActionResult> Acknowledge(Guid id)
    {
        await _emergencyService.AcknowledgeAsync(id, UserId);
        return NoContent();
    }

    [HttpPost("sos/{id:guid}/resolve")]
    [Authorize(Roles = "Admin,Warden,Security")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveSOSRequest request)
    {
        await _emergencyService.ResolveAsync(id, UserId, request.ResolutionNotes);
        return NoContent();
    }

    [HttpGet("sos/history")]
    [Authorize(Roles = "Admin,Warden")]
    public async Task<ActionResult<List<EmergencySOSDto>>> GetHistory(
        [FromQuery] Guid? studentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _emergencyService.GetHistoryAsync(studentId, page, pageSize));
}
