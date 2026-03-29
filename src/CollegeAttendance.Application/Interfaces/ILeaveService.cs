using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Application.Interfaces;

public interface ILeaveService
{
    Task<LeaveRequestDto> CreateAsync(Guid studentId, CreateLeaveRequest request);
    Task<LeaveRequestDto?> GetByIdAsync(Guid leaveId);
    Task<List<LeaveRequestDto>> GetStudentLeavesAsync(Guid studentId);
    Task<List<LeaveRequestDto>> GetPendingLeavesAsync(Guid? courseId);
    Task ApproveAsync(Guid leaveId, Guid approverId, string? remarks);
    Task RejectAsync(Guid leaveId, Guid approverId, string reason);
    Task CancelAsync(Guid leaveId, Guid studentId);
    Task<PagedResult<LeaveRequestDto>> GetAllLeavesAsync(LeaveStatus? status, int page, int pageSize);
}
