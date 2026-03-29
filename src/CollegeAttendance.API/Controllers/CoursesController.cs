using System.Security.Claims;
using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollegeAttendance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService) => _courseService = courseService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<CourseDto>>> GetAll()
    {
        return Ok(await _courseService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseDto>> GetById(Guid id)
    {
        var course = await _courseService.GetByIdAsync(id);
        return course == null ? NotFound() : Ok(course);
    }

    [HttpGet("department/{departmentId:guid}")]
    public async Task<ActionResult<List<CourseDto>>> GetByDepartment(Guid departmentId)
    {
        return Ok(await _courseService.GetByDepartmentAsync(departmentId));
    }

    [HttpGet("my-courses")]
    public async Task<ActionResult<List<CourseDto>>> GetMyCourses()
    {
        return Ok(await _courseService.GetByStudentAsync(UserId));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Faculty")]
    public async Task<ActionResult<CourseDto>> Create([FromBody] CreateCourseRequest request)
    {
        var course = await _courseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
    }

    [HttpPost("{courseId:guid}/enroll")]
    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> EnrollStudent(Guid courseId, [FromBody] EnrollStudentRequest request)
    {
        await _courseService.EnrollStudentAsync(courseId, request.StudentId);
        return Ok(new { message = "Student enrolled successfully" });
    }

    [HttpDelete("{courseId:guid}/unenroll/{studentId:guid}")]
    [Authorize(Roles = "Admin,Faculty")]
    public async Task<IActionResult> UnenrollStudent(Guid courseId, Guid studentId)
    {
        await _courseService.UnenrollStudentAsync(courseId, studentId);
        return Ok(new { message = "Student unenrolled" });
    }
}
