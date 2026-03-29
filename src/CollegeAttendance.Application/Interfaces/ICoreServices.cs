using CollegeAttendance.Application.DTOs;

namespace CollegeAttendance.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<PagedResult<UserDto>> GetUsersAsync(string? role, string? search, int page, int pageSize);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task DeleteUserAsync(Guid id);
}

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto?> GetByIdAsync(Guid id);
    Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request);
    Task<DepartmentDto> UpdateAsync(Guid id, UpdateDepartmentRequest request);
    Task DeleteAsync(Guid id);
}

public interface ICourseService
{
    Task<List<CourseDto>> GetAllAsync();
    Task<List<CourseDto>> GetByFacultyAsync(Guid facultyId);
    Task<List<CourseDto>> GetByDepartmentAsync(Guid departmentId);
    Task<List<CourseDto>> GetByStudentAsync(Guid studentId);
    Task<CourseDto?> GetByIdAsync(Guid id);
    Task<CourseDto> CreateAsync(CreateCourseRequest request);
    Task EnrollStudentAsync(Guid courseId, Guid studentId);
    Task UnenrollStudentAsync(Guid courseId, Guid studentId);
}

public interface IClassSessionService
{
    Task<List<ClassSessionDto>> GetByCourseAsync(Guid courseId);
    Task<ClassSessionDto?> GetByIdAsync(Guid id);
    Task<List<ClassSessionDto>> GetTodaySessionsAsync(Guid userId);
    Task<ClassSessionDto> CreateAsync(Guid facultyId, CreateClassSessionRequest request);
    Task StartSessionAsync(Guid sessionId, Guid facultyId);
    Task EndSessionAsync(Guid sessionId, Guid facultyId);
}
