using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/analytics/advanced")]
[Authorize(Roles = "Admin,Faculty")]
public class AdvancedAnalyticsController : ControllerBase
{
    private readonly IAdvancedAnalyticsService _analyticsService;
    private readonly ICurfewService _curfewService;

    public AdvancedAnalyticsController(IAdvancedAnalyticsService analyticsService, ICurfewService curfewService)
    {
        _analyticsService = analyticsService;
        _curfewService = curfewService;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdvancedDashboardDto>> GetDashboard()
        => Ok(await _analyticsService.GetAdvancedDashboardAsync());

    [HttpGet("heatmap")]
    public async Task<ActionResult<List<AttendanceHeatmapDto>>> GetHeatmap(
        [FromQuery] Guid? departmentId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;
        return Ok(await _analyticsService.GetAttendanceHeatmapAsync(departmentId, fromDate, toDate));
    }

    [HttpGet("dropout-risk")]
    public async Task<ActionResult<List<DropoutRiskDto>>> GetDropoutRisk(
        [FromQuery] Guid? departmentId, [FromQuery] int topN = 20)
        => Ok(await _analyticsService.GetDropoutRiskStudentsAsync(departmentId, topN));

    [HttpGet("faculty-strictness")]
    public async Task<ActionResult<List<FacultyStrictnessDto>>> GetFacultyStrictness([FromQuery] Guid? departmentId)
        => Ok(await _analyticsService.GetFacultyStrictnessAsync(departmentId));

    [HttpGet("courses")]
    public async Task<ActionResult<List<CourseAnalyticsDto>>> GetCourseAnalytics([FromQuery] Guid? departmentId)
        => Ok(await _analyticsService.GetCourseAnalyticsAsync(departmentId));

    // ===== Curfew Endpoints =====
    [HttpGet("curfew/logs")]
    [Authorize(Roles = "Admin,Warden")]
    public async Task<ActionResult<List<CurfewLogDto>>> GetCurfewLogs(
        [FromQuery] Guid? hostelId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
        var toDate = to ?? DateTime.UtcNow;
        return Ok(await _curfewService.GetCurfewLogsAsync(hostelId, fromDate, toDate));
    }

    [HttpGet("curfew/student/{studentId:guid}")]
    [Authorize(Roles = "Admin,Warden")]
    public async Task<ActionResult<List<CurfewLogDto>>> GetStudentCurfewLogs(Guid studentId)
        => Ok(await _curfewService.GetStudentCurfewLogsAsync(studentId));

    [HttpGet("curfew/config")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CurfewConfigDto>> GetCurfewConfig()
        => Ok(await _curfewService.GetCurfewConfigAsync());

    [HttpPut("curfew/config")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCurfewConfig([FromBody] CurfewConfigDto config)
    {
        await _curfewService.UpdateCurfewConfigAsync(UserId, config);
        return NoContent();
    }

    [HttpPost("curfew/check")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RunCurfewCheck()
    {
        await _curfewService.CheckCurfewViolationsAsync();
        return NoContent();
    }
}
