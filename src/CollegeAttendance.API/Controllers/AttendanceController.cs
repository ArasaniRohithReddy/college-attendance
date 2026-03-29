using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
        => _attendanceService = attendanceService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("mark")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<AttendanceDto>> MarkAttendance([FromBody] MarkAttendanceRequest request)
    {
        var result = await _attendanceService.MarkAttendanceByQRAsync(UserId, request);
        return Ok(result);
    }

    [HttpPost("manual")]
    [Authorize(Roles = "Faculty,Admin")]
    public async Task<ActionResult<List<AttendanceDto>>> MarkManualAttendance([FromBody] ManualAttendanceRequest request)
    {
        return Ok(await _attendanceService.MarkManualAttendanceAsync(UserId, request));
    }

    [HttpGet("session/{sessionId:guid}")]
    [Authorize(Roles = "Faculty,Admin")]
    public async Task<ActionResult<List<AttendanceDto>>> GetSessionAttendance(Guid sessionId)
    {
        return Ok(await _attendanceService.GetSessionAttendanceAsync(sessionId));
    }

    [HttpGet("student/{studentId:guid}/report")]
    public async Task<ActionResult<List<AttendanceReportDto>>> GetStudentReport(Guid studentId)
    {
        return Ok(await _attendanceService.GetStudentAttendanceReportAsync(studentId));
    }

    [HttpGet("my-report")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<List<AttendanceReportDto>>> GetMyReport()
    {
        return Ok(await _attendanceService.GetStudentAttendanceReportAsync(UserId));
    }

    [HttpGet("course/{courseId:guid}/report")]
    [Authorize(Roles = "Faculty,Admin")]
    public async Task<ActionResult<List<AttendanceReportDto>>> GetCourseReport(Guid courseId)
    {
        return Ok(await _attendanceService.GetCourseAttendanceReportAsync(courseId));
    }

    [HttpGet("history")]
    public async Task<ActionResult<PagedResult<AttendanceDto>>> GetHistory(
        [FromQuery] Guid? studentId, [FromQuery] Guid? courseId,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await _attendanceService.GetAttendanceHistoryAsync(studentId, courseId, from, to, page, pageSize));
    }

    [HttpGet("defaulters")]
    [Authorize(Roles = "Faculty,Admin")]
    public async Task<ActionResult<List<AttendanceReportDto>>> GetDefaulters(
        [FromQuery] Guid? courseId, [FromQuery] double threshold = 75.0)
    {
        return Ok(await _attendanceService.GetDefaultersAsync(courseId, threshold));
    }

    [HttpPut("{recordId:guid}")]
    [Authorize(Roles = "Faculty,Admin")]
    public async Task<ActionResult<AttendanceDto>> UpdateAttendance(Guid recordId, [FromBody] AttendanceDto update)
    {
        return Ok(await _attendanceService.UpdateAttendanceAsync(recordId, UserId, update));
    }
}
