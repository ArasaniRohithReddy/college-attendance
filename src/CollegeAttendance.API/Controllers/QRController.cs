using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QRController : ControllerBase
{
    private readonly IQRService _qrService;

    public QRController(IQRService qrService) => _qrService = qrService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("generate")]
    [Authorize(Roles = "Faculty")]
    public async Task<ActionResult<QRSessionDto>> GenerateQR([FromBody] GenerateQRRequest request)
    {
        return Ok(await _qrService.GenerateQRAsync(UserId, request));
    }

    [HttpGet("validate/{token}")]
    public async Task<ActionResult<QRSessionDto>> ValidateQR(string token)
    {
        var result = await _qrService.ValidateQRTokenAsync(token);
        return result == null ? BadRequest(new { message = "Invalid or expired QR code" }) : Ok(result);
    }

    [HttpPost("deactivate/{qrSessionId:guid}")]
    [Authorize(Roles = "Faculty")]
    public async Task<IActionResult> DeactivateQR(Guid qrSessionId)
    {
        await _qrService.DeactivateQRAsync(qrSessionId);
        return Ok(new { message = "QR deactivated" });
    }
}
