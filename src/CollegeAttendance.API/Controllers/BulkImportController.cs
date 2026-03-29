using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/admin/import")]
[Authorize(Roles = "Admin")]
public class BulkImportController : ControllerBase
{
    private readonly IBulkImportService _importService;

    public BulkImportController(IBulkImportService importService) => _importService = importService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("students")]
    public async Task<ActionResult<BulkImportResultDto>> ImportStudents(IFormFile file)
    {
        if (file.Length == 0) return BadRequest("File is empty");
        using var stream = file.OpenReadStream();
        return Ok(await _importService.ImportStudentsAsync(stream, UserId));
    }

    [HttpPost("courses")]
    public async Task<ActionResult<BulkImportResultDto>> ImportCourses(IFormFile file)
    {
        if (file.Length == 0) return BadRequest("File is empty");
        using var stream = file.OpenReadStream();
        return Ok(await _importService.ImportCoursesAsync(stream, UserId));
    }

    [HttpGet("template/students")]
    public async Task<IActionResult> DownloadStudentTemplate()
    {
        var bytes = await _importService.GenerateStudentTemplateAsync();
        return File(bytes, "text/csv", "student_import_template.csv");
    }

    [HttpGet("template/courses")]
    public async Task<IActionResult> DownloadCourseTemplate()
    {
        var bytes = await _importService.GenerateCourseTemplateAsync();
        return File(bytes, "text/csv", "course_import_template.csv");
    }
}
