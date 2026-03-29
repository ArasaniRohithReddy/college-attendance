using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class BulkImportService : IBulkImportService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Department> _deptRepo;
    private readonly IRepository<Course> _courseRepo;
    private readonly IRepository<Hostel> _hostelRepo;
    private readonly IUnitOfWork _unitOfWork;

    public BulkImportService(
        IRepository<User> userRepo,
        IRepository<Department> deptRepo,
        IRepository<Course> courseRepo,
        IRepository<Hostel> hostelRepo,
        IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _deptRepo = deptRepo;
        _courseRepo = courseRepo;
        _hostelRepo = hostelRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<BulkImportResultDto> ImportStudentsAsync(Stream fileStream, Guid importedById)
    {
        var errors = new List<string>();
        int succeeded = 0;
        int totalRows = 0;

        using var reader = new StreamReader(fileStream);
        var header = await reader.ReadLineAsync(); // skip header

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            totalRows++;

            try
            {
                var cols = line.Split(',');
                if (cols.Length < 4)
                {
                    errors.Add($"Row {totalRows}: Insufficient columns");
                    continue;
                }

                var email = cols[0].Trim();
                var fullName = cols[1].Trim();
                var studentId = cols[2].Trim();
                var phone = cols.Length > 3 ? cols[3].Trim() : null;
                var deptCode = cols.Length > 4 ? cols[4].Trim() : null;
                var hostelName = cols.Length > 5 ? cols[5].Trim() : null;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName))
                {
                    errors.Add($"Row {totalRows}: Email and FullName are required");
                    continue;
                }

                var existing = await _userRepo.AnyAsync(u => u.Email == email);
                if (existing)
                {
                    errors.Add($"Row {totalRows}: User with email {email} already exists");
                    continue;
                }

                var user = new User
                {
                    Email = email,
                    FullName = fullName,
                    StudentId = studentId,
                    Phone = phone,
                    Role = UserRole.Student,
                    IsActive = true,
                    CreatedBy = importedById.ToString()
                };

                if (!string.IsNullOrEmpty(deptCode))
                {
                    var dept = await _deptRepo.FirstOrDefaultAsync(d => d.Code == deptCode);
                    if (dept != null) user.DepartmentId = dept.Id;
                    else errors.Add($"Row {totalRows}: Department '{deptCode}' not found, student created without department");
                }

                if (!string.IsNullOrEmpty(hostelName))
                {
                    var hostel = await _hostelRepo.FirstOrDefaultAsync(h => h.Name == hostelName);
                    if (hostel != null) user.HostelId = hostel.Id;
                }

                await _userRepo.AddAsync(user);
                succeeded++;
            }
            catch (Exception ex)
            {
                errors.Add($"Row {totalRows}: {ex.Message}");
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return new BulkImportResultDto(totalRows, succeeded, totalRows - succeeded, errors);
    }

    public async Task<BulkImportResultDto> ImportCoursesAsync(Stream fileStream, Guid importedById)
    {
        var errors = new List<string>();
        int succeeded = 0;
        int totalRows = 0;

        using var reader = new StreamReader(fileStream);
        var header = await reader.ReadLineAsync(); // skip header

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            totalRows++;

            try
            {
                var cols = line.Split(',');
                if (cols.Length < 7)
                {
                    errors.Add($"Row {totalRows}: Insufficient columns");
                    continue;
                }

                var name = cols[0].Trim();
                var code = cols[1].Trim();
                if (!int.TryParse(cols[2].Trim(), out var credits)) { errors.Add($"Row {totalRows}: Invalid credits"); continue; }
                if (!int.TryParse(cols[3].Trim(), out var semester)) { errors.Add($"Row {totalRows}: Invalid semester"); continue; }
                if (!int.TryParse(cols[4].Trim(), out var year)) { errors.Add($"Row {totalRows}: Invalid year"); continue; }
                var deptCode = cols[5].Trim();
                var facultyEmail = cols[6].Trim();

                var existing = await _courseRepo.AnyAsync(c => c.Code == code);
                if (existing)
                {
                    errors.Add($"Row {totalRows}: Course with code {code} already exists");
                    continue;
                }

                var dept = await _deptRepo.FirstOrDefaultAsync(d => d.Code == deptCode);
                if (dept == null) { errors.Add($"Row {totalRows}: Department '{deptCode}' not found"); continue; }

                var faculty = await _userRepo.FirstOrDefaultAsync(u => u.Email == facultyEmail && u.Role == UserRole.Faculty);
                if (faculty == null) { errors.Add($"Row {totalRows}: Faculty '{facultyEmail}' not found"); continue; }

                await _courseRepo.AddAsync(new Course
                {
                    Name = name,
                    Code = code,
                    Credits = credits,
                    Semester = semester,
                    Year = year,
                    DepartmentId = dept.Id,
                    FacultyId = faculty.Id,
                    CreatedBy = importedById.ToString()
                });
                succeeded++;
            }
            catch (Exception ex)
            {
                errors.Add($"Row {totalRows}: {ex.Message}");
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return new BulkImportResultDto(totalRows, succeeded, totalRows - succeeded, errors);
    }

    public Task<byte[]> GenerateStudentTemplateAsync()
    {
        var csv = "Email,FullName,StudentId,Phone,DepartmentCode,HostelName\n";
        csv += "student@college.edu,John Doe,STU2025001,9876543210,CSE,Boys Hostel A\n";
        return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(csv));
    }

    public Task<byte[]> GenerateCourseTemplateAsync()
    {
        var csv = "Name,Code,Credits,Semester,Year,DepartmentCode,FacultyEmail\n";
        csv += "Data Structures,CS201,4,3,2026,CSE,faculty@college.edu\n";
        return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(csv));
    }
}
