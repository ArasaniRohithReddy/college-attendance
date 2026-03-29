using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HostelController : ControllerBase
{
    private readonly IHostelService _hostelService;

    public HostelController(IHostelService hostelService) => _hostelService = hostelService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Authorize(Roles = "Admin,Warden")]
    public async Task<ActionResult<List<HostelDto>>> GetAll()
    {
        return Ok(await _hostelService.GetAllHostelsAsync());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<HostelDto>> Create([FromBody] CreateHostelRequest request)
    {
        return Ok(await _hostelService.CreateHostelAsync(request));
    }

    [HttpPost("log")]
    [Authorize(Roles = "Warden,Security")]
    public async Task<ActionResult<HostelLogDto>> LogEntry([FromBody] CreateHostelLogRequest request)
    {
        return Ok(await _hostelService.LogEntryAsync(request));
    }

    [HttpGet("logs/student/{studentId:guid}")]
    [Authorize(Roles = "Admin,Warden")]
    public async Task<ActionResult<List<HostelLogDto>>> GetStudentLogs(Guid studentId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(await _hostelService.GetStudentLogsAsync(studentId, from ?? DateTime.UtcNow.AddDays(-30), to ?? DateTime.UtcNow));
    }

    [HttpGet("logs/my")]
    public async Task<ActionResult<List<HostelLogDto>>> GetMyLogs([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(await _hostelService.GetStudentLogsAsync(UserId, from ?? DateTime.UtcNow.AddDays(-30), to ?? DateTime.UtcNow));
    }
}
