using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OutingController : ControllerBase
{
    private readonly IOutingService _outingService;

    public OutingController(IOutingService outingService) => _outingService = outingService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("request")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<OutingRequestDto>> CreateRequest([FromBody] CreateOutingRequest request)
    {
        return Ok(await _outingService.CreateRequestAsync(UserId, request));
    }

    [HttpGet("my-requests")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<List<OutingRequestDto>>> GetMyRequests()
    {
        return Ok(await _outingService.GetStudentRequestsAsync(UserId));
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Warden,Security")]
    public async Task<ActionResult<List<OutingRequestDto>>> GetPending()
    {
        var role = User.FindFirstValue(ClaimTypes.Role)!;
        return Ok(await _outingService.GetPendingRequestsAsync(role));
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Warden")]
    public async Task<IActionResult> WardenApprove(Guid id)
    {
        await _outingService.WardenApproveAsync(id, UserId);
        return Ok(new { message = "Approved by warden" });
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Warden")]
    public async Task<IActionResult> WardenReject(Guid id, [FromBody] RejectOutingRequest request)
    {
        await _outingService.WardenRejectAsync(id, UserId, request.Remarks);
        return Ok(new { message = "Rejected" });
    }

    [HttpPut("{id:guid}/checkout")]
    [Authorize(Roles = "Security")]
    public async Task<IActionResult> SecurityCheckout(Guid id)
    {
        await _outingService.SecurityCheckoutAsync(id, UserId);
        return Ok(new { message = "Checked out" });
    }

    [HttpPut("{id:guid}/checkin")]
    [Authorize(Roles = "Security")]
    public async Task<IActionResult> SecurityCheckin(Guid id)
    {
        await _outingService.SecurityCheckinAsync(id, UserId);
        return Ok(new { message = "Checked in" });
    }

    [HttpGet("{id:guid}/gate-pass")]
    public async Task<ActionResult<byte[]>> GetGatePass(Guid id)
    {
        var qrBytes = await _outingService.GenerateGatePassQRAsync(id);
        return File(qrBytes, "image/png");
    }
}
