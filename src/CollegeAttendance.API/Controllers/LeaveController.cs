using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeaveController(ILeaveService leaveService) => _leaveService = leaveService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<LeaveRequestDto>> CreateLeave([FromBody] CreateLeaveRequest request)
    {
        var leave = await _leaveService.CreateAsync(UserId, request);
        return CreatedAtAction(nameof(GetLeave), new { id = leave.Id }, leave);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LeaveRequestDto>> GetLeave(Guid id)
    {
        var leave = await _leaveService.GetByIdAsync(id);
        return leave is null ? NotFound() : Ok(leave);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<List<LeaveRequestDto>>> GetMyLeaves()
        => Ok(await _leaveService.GetStudentLeavesAsync(UserId));

    [HttpGet("pending")]
    [Authorize(Roles = "Faculty,Admin")]
    public async Task<ActionResult<List<LeaveRequestDto>>> GetPendingLeaves([FromQuery] Guid? courseId)
        => Ok(await _leaveService.GetPendingLeavesAsync(courseId));

    [HttpGet]
    [Authorize(Roles = "Faculty,Admin")]
    public async Task<ActionResult<PagedResult<LeaveRequestDto>>> GetAll(
        [FromQuery] LeaveStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => Ok(await _leaveService.GetAllLeavesAsync(status, page, pageSize));

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Faculty,Admin")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveLeaveRequest request)
    {
        await _leaveService.ApproveAsync(id, UserId, request.Remarks);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Faculty,Admin")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectLeaveRequest request)
    {
        await _leaveService.RejectAsync(id, UserId, request.Reason);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _leaveService.CancelAsync(id, UserId);
        return NoContent();
    }
}
