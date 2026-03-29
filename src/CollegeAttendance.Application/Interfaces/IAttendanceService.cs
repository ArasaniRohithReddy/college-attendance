using CollegeAttendance.Application.DTOs;

namespace CollegeAttendance.Application.Interfaces;

public interface IAttendanceService
{
    Task<AttendanceDto> MarkAttendanceByQRAsync(Guid studentId, MarkAttendanceRequest request);
    Task<List<AttendanceDto>> MarkManualAttendanceAsync(Guid facultyId, ManualAttendanceRequest request);
    Task<List<AttendanceDto>> GetSessionAttendanceAsync(Guid sessionId);
    Task<List<AttendanceReportDto>> GetStudentAttendanceReportAsync(Guid studentId);
    Task<List<AttendanceReportDto>> GetCourseAttendanceReportAsync(Guid courseId);
    Task<PagedResult<AttendanceDto>> GetAttendanceHistoryAsync(Guid? studentId, Guid? courseId, DateTime? from, DateTime? to, int page, int pageSize);
    Task<AttendanceDto> UpdateAttendanceAsync(Guid recordId, Guid facultyId, AttendanceDto update);
    Task<List<AttendanceReportDto>> GetDefaultersAsync(Guid? courseId, double threshold = 75.0);
    Task<byte[]> ExportAttendanceReportAsync(Guid courseId, string format);
}
