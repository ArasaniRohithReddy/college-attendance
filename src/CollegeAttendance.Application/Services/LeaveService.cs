using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Application.Interfaces;
using CollegeAttendance.Domain.Entities;
using CollegeAttendance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CollegeAttendance.Application.Services;

public class LeaveService : ILeaveService
{
    private readonly IRepository<LeaveRequest> _leaveRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Course> _courseRepo;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public LeaveService(
        IRepository<LeaveRequest> leaveRepo,
        IRepository<User> userRepo,
        IRepository<Course> courseRepo,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _leaveRepo = leaveRepo;
        _userRepo = userRepo;
        _courseRepo = courseRepo;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeaveRequestDto> CreateAsync(Guid studentId, CreateLeaveRequest request)
    {
        var student = await _userRepo.GetByIdAsync(studentId)
            ?? throw new KeyNotFoundException("Student not found");

        var leave = new LeaveRequest
        {
            StudentId = studentId,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            CourseId = request.CourseId,
            DocumentUrl = request.DocumentUrl,
            Status = LeaveStatus.Pending
        };

        await _leaveRepo.AddAsync(leave);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(leave, student.FullName, null, null);
    }

    public async Task<LeaveRequestDto?> GetByIdAsync(Guid leaveId)
    {
        var leave = await _leaveRepo.Query()
            .Include(l => l.Student)
            .Include(l => l.ApprovedBy)
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == leaveId);

        return leave == null ? null : MapToDto(leave, leave.Student.FullName, leave.ApprovedBy?.FullName, leave.Course?.Name);
    }

    public async Task<List<LeaveRequestDto>> GetStudentLeavesAsync(Guid studentId)
    {
        return await _leaveRepo.Query()
            .Where(l => l.StudentId == studentId)
            .Include(l => l.Student)
            .Include(l => l.ApprovedBy)
            .Include(l => l.Course)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LeaveRequestDto(
                l.Id, l.StudentId, l.Student.FullName, l.LeaveType,
                l.StartDate, l.EndDate, l.Reason, l.Status,
                l.ApprovedBy != null ? l.ApprovedBy.FullName : null, l.ApprovedAt,
                l.RejectionReason, l.DocumentUrl,
                l.CourseId, l.Course != null ? l.Course.Name : null))
            .ToListAsync();
    }

    public async Task<List<LeaveRequestDto>> GetPendingLeavesAsync(Guid? courseId)
    {
        var query = _leaveRepo.Query()
            .Where(l => l.Status == LeaveStatus.Pending);

        if (courseId.HasValue)
            query = query.Where(l => l.CourseId == courseId);

        return await query
            .Include(l => l.Student)
            .Include(l => l.Course)
            .OrderBy(l => l.StartDate)
            .Select(l => new LeaveRequestDto(
                l.Id, l.StudentId, l.Student.FullName, l.LeaveType,
                l.StartDate, l.EndDate, l.Reason, l.Status,
                null, null, null, l.DocumentUrl,
                l.CourseId, l.Course != null ? l.Course.Name : null))
            .ToListAsync();
    }

    public async Task ApproveAsync(Guid leaveId, Guid approverId, string? remarks)
    {
        var leave = await _leaveRepo.GetByIdAsync(leaveId)
            ?? throw new KeyNotFoundException("Leave request not found");

        leave.Status = LeaveStatus.ApprovedByFaculty;
        leave.ApprovedById = approverId;
        leave.ApprovedAt = DateTime.UtcNow;
        leave.UpdatedAt = DateTime.UtcNow;
        _leaveRepo.Update(leave);
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(leave.StudentId,
            "Leave Approved", "Your leave request has been approved.", NotificationType.LeaveUpdate);
    }

    public async Task RejectAsync(Guid leaveId, Guid approverId, string reason)
    {
        var leave = await _leaveRepo.GetByIdAsync(leaveId)
            ?? throw new KeyNotFoundException("Leave request not found");

        leave.Status = LeaveStatus.Rejected;
        leave.ApprovedById = approverId;
        leave.RejectionReason = reason;
        leave.UpdatedAt = DateTime.UtcNow;
        _leaveRepo.Update(leave);
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(leave.StudentId,
            "Leave Rejected", $"Your leave request was rejected: {reason}", NotificationType.LeaveUpdate);
    }

    public async Task CancelAsync(Guid leaveId, Guid studentId)
    {
        var leave = await _leaveRepo.GetByIdAsync(leaveId)
            ?? throw new KeyNotFoundException("Leave request not found");

        if (leave.StudentId != studentId)
            throw new UnauthorizedAccessException("Not authorized to cancel this leave");

        leave.Status = LeaveStatus.Cancelled;
        leave.UpdatedAt = DateTime.UtcNow;
        _leaveRepo.Update(leave);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedResult<LeaveRequestDto>> GetAllLeavesAsync(LeaveStatus? status, int page, int pageSize)
    {
        var query = _leaveRepo.Query().AsQueryable();
        if (status.HasValue) query = query.Where(l => l.Status == status);

        var total = await query.CountAsync();
        var items = await query
            .Include(l => l.Student)
            .Include(l => l.ApprovedBy)
            .Include(l => l.Course)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => new LeaveRequestDto(
                l.Id, l.StudentId, l.Student.FullName, l.LeaveType,
                l.StartDate, l.EndDate, l.Reason, l.Status,
                l.ApprovedBy != null ? l.ApprovedBy.FullName : null, l.ApprovedAt,
                l.RejectionReason, l.DocumentUrl,
                l.CourseId, l.Course != null ? l.Course.Name : null))
            .ToListAsync();

        return new PagedResult<LeaveRequestDto>(items, total, page, pageSize);
    }

    private static LeaveRequestDto MapToDto(LeaveRequest l, string studentName, string? approvedByName, string? courseName) =>
        new(l.Id, l.StudentId, studentName, l.LeaveType, l.StartDate, l.EndDate,
            l.Reason, l.Status, approvedByName, l.ApprovedAt, l.RejectionReason,
            l.DocumentUrl, l.CourseId, courseName);
}
