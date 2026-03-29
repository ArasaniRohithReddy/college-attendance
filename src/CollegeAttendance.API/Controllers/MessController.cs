using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessController : ControllerBase
{
    private readonly IMessService _messService;

    public MessController(IMessService messService) => _messService = messService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("log")]
    [Authorize(Roles = "Admin,Warden,Security")]
    public async Task<ActionResult<MessLogDto>> LogMeal([FromBody] CreateMessLogRequest request)
    {
        return Ok(await _messService.LogMealAsync(request));
    }

    [HttpGet("logs/student/{studentId:guid}")]
    [Authorize(Roles = "Admin,Warden")]
    public async Task<ActionResult<List<MessLogDto>>> GetStudentLogs(Guid studentId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(await _messService.GetStudentLogsAsync(studentId, from ?? DateTime.UtcNow.AddDays(-30), to ?? DateTime.UtcNow));
    }

    [HttpGet("logs/my")]
    public async Task<ActionResult<List<MessLogDto>>> GetMyLogs([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(await _messService.GetStudentLogsAsync(UserId, from ?? DateTime.UtcNow.AddDays(-30), to ?? DateTime.UtcNow));
    }

    [HttpGet("analytics")]
    [Authorize(Roles = "Admin,Warden")]
    public async Task<ActionResult<MessAnalyticsDto>> GetAnalytics([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(await _messService.GetAnalyticsAsync(from ?? DateTime.UtcNow.AddDays(-30), to ?? DateTime.UtcNow));
    }
}
