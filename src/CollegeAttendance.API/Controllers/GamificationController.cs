using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GamificationController : ControllerBase
{
    private readonly IGamificationService _gamificationService;

    public GamificationController(IGamificationService gamificationService)
        => _gamificationService = gamificationService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("dashboard")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<GamificationDashboardDto>> GetDashboard()
        => Ok(await _gamificationService.GetStudentDashboardAsync(UserId));

    [HttpGet("streak")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<StreakDto>> GetStreak()
        => Ok(await _gamificationService.GetStreakAsync(UserId));

    [HttpGet("badges")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<List<StudentBadgeDto>>> GetMyBadges()
        => Ok(await _gamificationService.GetStudentBadgesAsync(UserId));

    [HttpGet("badges/{studentId:guid}")]
    [Authorize(Roles = "Faculty,Admin")]
    public async Task<ActionResult<List<StudentBadgeDto>>> GetStudentBadges(Guid studentId)
        => Ok(await _gamificationService.GetStudentBadgesAsync(studentId));

    [HttpGet("leaderboard")]
    public async Task<ActionResult<List<LeaderboardEntryDto>>> GetLeaderboard(
        [FromQuery] string period = "weekly",
        [FromQuery] Guid? departmentId = null,
        [FromQuery] int top = 50)
        => Ok(await _gamificationService.GetLeaderboardAsync(period, departmentId, top));

    [HttpGet("badges/all")]
    public async Task<ActionResult<List<BadgeDto>>> GetAllBadges()
        => Ok(await _gamificationService.GetAllBadgesAsync());

    [HttpPost("recalculate/streaks")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RecalculateStreaks()
    {
        await _gamificationService.RecalculateStreaksAsync();
        return NoContent();
    }

    [HttpPost("recalculate/leaderboard")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RecalculateLeaderboard([FromQuery] string period = "weekly")
    {
        await _gamificationService.RecalculateLeaderboardAsync(period);
        return NoContent();
    }
}
