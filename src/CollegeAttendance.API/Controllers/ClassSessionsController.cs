using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClassSessionsController : ControllerBase
{
    private readonly IClassSessionService _sessionService;

    public ClassSessionsController(IClassSessionService sessionService) => _sessionService = sessionService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("course/{courseId:guid}")]
    public async Task<ActionResult<List<ClassSessionDto>>> GetByCourse(Guid courseId)
    {
        return Ok(await _sessionService.GetByCourseAsync(courseId));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClassSessionDto>> GetById(Guid id)
    {
        var session = await _sessionService.GetByIdAsync(id);
        return session == null ? NotFound() : Ok(session);
    }

    [HttpGet("today")]
    public async Task<ActionResult<List<ClassSessionDto>>> GetToday()
    {
        return Ok(await _sessionService.GetTodaySessionsAsync(UserId));
    }

    [HttpPost]
    [Authorize(Roles = "Faculty,Admin")]
    public async Task<ActionResult<ClassSessionDto>> Create([FromBody] CreateClassSessionRequest request)
    {
        var session = await _sessionService.CreateAsync(UserId, request);
        return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
    }

    [HttpPut("{id:guid}/start")]
    [Authorize(Roles = "Faculty")]
    public async Task<IActionResult> StartSession(Guid id)
    {
        await _sessionService.StartSessionAsync(id, UserId);
        return Ok(new { message = "Session started" });
    }

    [HttpPut("{id:guid}/end")]
    [Authorize(Roles = "Faculty")]
    public async Task<IActionResult> EndSession(Guid id)
    {
        await _sessionService.EndSessionAsync(id, UserId);
        return Ok(new { message = "Session ended" });
    }
}
