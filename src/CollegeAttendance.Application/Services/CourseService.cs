using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class CourseService : ICourseService
{
    private readonly IRepository<Course> _courseRepo;
    private readonly IRepository<CourseEnrollment> _enrollmentRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(IRepository<Course> courseRepo, IRepository<CourseEnrollment> enrollmentRepo, IUnitOfWork unitOfWork)
    {
        _courseRepo = courseRepo;
        _enrollmentRepo = enrollmentRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CourseDto>> GetAllAsync() =>
        await MapCoursesQuery(_courseRepo.Query().Where(c => !c.IsDeleted));

    public async Task<List<CourseDto>> GetByFacultyAsync(Guid facultyId) =>
        await MapCoursesQuery(_courseRepo.Query().Where(c => c.FacultyId == facultyId && !c.IsDeleted));

    public async Task<List<CourseDto>> GetByDepartmentAsync(Guid departmentId) =>
        await MapCoursesQuery(_courseRepo.Query().Where(c => c.DepartmentId == departmentId && !c.IsDeleted));

    public async Task<List<CourseDto>> GetByStudentAsync(Guid studentId)
    {
        var courseIds = _enrollmentRepo.Query()
            .Where(e => e.StudentId == studentId && e.IsActive)
            .Select(e => e.CourseId);
        return await MapCoursesQuery(_courseRepo.Query().Where(c => courseIds.Contains(c.Id) && !c.IsDeleted));
    }

    public async Task<CourseDto?> GetByIdAsync(Guid id)
    {
        var courses = await MapCoursesQuery(_courseRepo.Query().Where(c => c.Id == id && !c.IsDeleted));
        return courses.FirstOrDefault();
    }

    public async Task<CourseDto> CreateAsync(CreateCourseRequest request)
    {
        var course = new Course
        {
            Name = request.Name, Code = request.Code, Description = request.Description,
            Credits = request.Credits, Semester = request.Semester, Year = request.Year,
            DepartmentId = request.DepartmentId, FacultyId = request.FacultyId
        };
        await _courseRepo.AddAsync(course);
        await _unitOfWork.SaveChangesAsync();
        var result = await GetByIdAsync(course.Id);
        return result!;
    }

    public async Task EnrollStudentAsync(Guid courseId, Guid studentId)
    {
        var exists = await _enrollmentRepo.AnyAsync(e =>
            e.CourseId == courseId && e.StudentId == studentId && e.IsActive);
        if (exists) throw new InvalidOperationException("Student already enrolled");

        await _enrollmentRepo.AddAsync(new CourseEnrollment
        {
            CourseId = courseId, StudentId = studentId
        });
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnenrollStudentAsync(Guid courseId, Guid studentId)
    {
        var enrollment = await _enrollmentRepo.FirstOrDefaultAsync(e =>
            e.CourseId == courseId && e.StudentId == studentId && e.IsActive)
            ?? throw new KeyNotFoundException("Enrollment not found");
        enrollment.IsActive = false;
        _enrollmentRepo.Update(enrollment);
        await _unitOfWork.SaveChangesAsync();
    }

    private static async Task<List<CourseDto>> MapCoursesQuery(IQueryable<Course> query)
    {
        return await query
            .Include(c => c.Department)
            .Include(c => c.Faculty)
            .Include(c => c.Enrollments)
            .Select(c => new CourseDto(
                c.Id, c.Name, c.Code, c.Description,
                c.Credits, c.Semester, c.Year,
                c.DepartmentId, c.Department.Name,
                c.FacultyId, c.Faculty.FullName,
                c.Enrollments.Count(e => e.IsActive)))
            .ToListAsync();
    }
}
