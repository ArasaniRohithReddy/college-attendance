using CollegeAttendance.Application.DTOs;

namespace CollegeAttendance.Application.Interfaces;

public interface IAdvancedAnalyticsService
{
    Task<List<AttendanceHeatmapDto>> GetAttendanceHeatmapAsync(Guid? departmentId, DateTime from, DateTime to);
    Task<List<DropoutRiskDto>> GetDropoutRiskStudentsAsync(Guid? departmentId, int topN = 20);
    Task<List<FacultyStrictnessDto>> GetFacultyStrictnessAsync(Guid? departmentId);
    Task<List<CourseAnalyticsDto>> GetCourseAnalyticsAsync(Guid? departmentId);
    Task<AdvancedDashboardDto> GetAdvancedDashboardAsync();
}
