using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IRepository<Department> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IRepository<Department> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        return await _repo.Query()
            .Where(d => !d.IsDeleted)
            .Select(d => new DepartmentDto(
                d.Id, d.Name, d.Code, d.Description,
                d.Users.Count(u => !u.IsDeleted),
                d.Courses.Count(c => !c.IsDeleted)))
            .ToListAsync();
    }

    public async Task<DepartmentDto?> GetByIdAsync(Guid id)
    {
        return await _repo.Query()
            .Where(d => d.Id == id && !d.IsDeleted)
            .Select(d => new DepartmentDto(
                d.Id, d.Name, d.Code, d.Description,
                d.Users.Count(u => !u.IsDeleted),
                d.Courses.Count(c => !c.IsDeleted)))
            .FirstOrDefaultAsync();
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request)
    {
        var dept = new Department
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            HeadOfDepartmentId = request.HeadOfDepartmentId
        };
        await _repo.AddAsync(dept);
        await _unitOfWork.SaveChangesAsync();
        return new DepartmentDto(dept.Id, dept.Name, dept.Code, dept.Description, 0, 0);
    }

    public async Task<DepartmentDto> UpdateAsync(Guid id, UpdateDepartmentRequest request)
    {
        var dept = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Department not found");
        if (request.Name != null) dept.Name = request.Name;
        if (request.Code != null) dept.Code = request.Code;
        if (request.Description != null) dept.Description = request.Description;
        if (request.HeadOfDepartmentId != null) dept.HeadOfDepartmentId = request.HeadOfDepartmentId;
        dept.UpdatedAt = DateTime.UtcNow;
        _repo.Update(dept);
        await _unitOfWork.SaveChangesAsync();
        return new DepartmentDto(dept.Id, dept.Name, dept.Code, dept.Description, 0, 0);
    }

    public async Task DeleteAsync(Guid id)
    {
        var dept = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Department not found");
        dept.IsDeleted = true;
        _repo.Update(dept);
        await _unitOfWork.SaveChangesAsync();
    }
}
