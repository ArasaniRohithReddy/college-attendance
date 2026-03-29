using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IRepository<User> userRepo, IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepo.Query()
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _userRepo.Query()
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        return user == null ? null : MapToDto(user);
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(string? role, string? search, int page, int pageSize)
    {
        var query = _userRepo.Query()
            .Include(u => u.Department)
            .Where(u => !u.IsDeleted);

        if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var parsedRole))
            query = query.Where(u => u.Role == parsedRole);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search) || (u.StudentId != null && u.StudentId.Contains(search)));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => MapToDto(u))
            .ToListAsync();

        return new PagedResult<UserDto>(items, total, page, pageSize);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            Role = request.Role,
            StudentId = request.StudentId,
            EmployeeId = request.EmployeeId,
            Phone = request.Phone,
            DepartmentId = request.DepartmentId,
            HostelId = request.HostelId
        };

        await _userRepo.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _userRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User {id} not found");

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.Phone != null) user.Phone = request.Phone;
        if (request.DepartmentId != null) user.DepartmentId = request.DepartmentId;
        if (request.HostelId != null) user.HostelId = request.HostelId;
        if (request.IsActive != null) user.IsActive = request.IsActive.Value;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User {id} not found");
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    private static UserDto MapToDto(User u) => new(
        u.Id, u.Email, u.FullName, u.Role,
        u.StudentId, u.EmployeeId, u.ProfileImageUrl, u.Phone,
        u.DepartmentId, u.Department?.Name, u.IsActive
    );
}
