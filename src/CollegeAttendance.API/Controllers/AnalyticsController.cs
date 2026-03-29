using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService) => _analyticsService = analyticsService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin,Faculty")]
    public async Task<ActionResult<DashboardAnalyticsDto>> GetDashboard()
    {
        return Ok(await _analyticsService.GetDashboardAsync());
    }

    [HttpGet("course/{courseId:guid}")]
    [Authorize(Roles = "Admin,Faculty")]
    public async Task<ActionResult<AttendanceReportDto>> GetCourseReport(Guid courseId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(await _analyticsService.GetCourseReportAsync(courseId, from ?? DateTime.UtcNow.AddDays(-30), to ?? DateTime.UtcNow));
    }

    [HttpGet("department/{departmentId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> GetDepartmentReport(Guid departmentId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(await _analyticsService.GetDepartmentReportAsync(departmentId, from ?? DateTime.UtcNow.AddDays(-30), to ?? DateTime.UtcNow));
    }

    [HttpGet("predictions")]
    [Authorize(Roles = "Admin,Faculty")]
    public async Task<ActionResult<List<StudentAttendancePredictionDto>>> GetPredictions([FromQuery] Guid? courseId)
    {
        return Ok(await _analyticsService.GetAttendancePredictionsAsync(courseId));
    }

    [HttpGet("my-stats")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<object>> GetMyStats()
    {
        return Ok(await _analyticsService.GetStudentStatsAsync(UserId));
    }
}
