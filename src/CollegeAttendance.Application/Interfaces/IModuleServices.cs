using CollegeAttendance.Application.DTOs;
using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Application.Interfaces;

public interface IHostelService
{
    Task<List<HostelDto>> GetAllHostelsAsync();
    Task<HostelDto> CreateHostelAsync(CreateHostelRequest request);
    Task<HostelLogDto> LogEntryAsync(CreateHostelLogRequest request);
    Task<List<HostelLogDto>> GetStudentLogsAsync(Guid studentId, DateTime from, DateTime to);
}

public interface IMessService
{
    Task<MessLogDto> LogMealAsync(CreateMessLogRequest request);
    Task<List<MessLogDto>> GetStudentLogsAsync(Guid studentId, DateTime from, DateTime to);
    Task<MessAnalyticsDto> GetAnalyticsAsync(DateTime from, DateTime to);
}

public interface IOutingService
{
    Task<OutingRequestDto> CreateRequestAsync(Guid studentId, CreateOutingRequest request);
    Task<List<OutingRequestDto>> GetStudentRequestsAsync(Guid studentId);
    Task<List<OutingRequestDto>> GetPendingRequestsAsync(string role);
    Task WardenApproveAsync(Guid requestId, Guid wardenId);
    Task WardenRejectAsync(Guid requestId, Guid wardenId, string reason);
    Task SecurityCheckoutAsync(Guid requestId, Guid securityId);
    Task SecurityCheckinAsync(Guid requestId, Guid securityId);
    Task<byte[]> GenerateGatePassQRAsync(Guid requestId);
}

public interface INotificationService
{
    Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
    Task SendNotificationAsync(Guid userId, string title, string message, NotificationType type, string? actionUrl = null);
    Task SendBulkNotificationAsync(List<Guid> userIds, string title, string message, NotificationType type);
    Task<int> GetUnreadCountAsync(Guid userId);
}

public interface IAnalyticsService
{
    Task<DashboardAnalyticsDto> GetDashboardAsync();
    Task<AttendanceReportDto> GetCourseReportAsync(Guid courseId, DateTime from, DateTime to);
    Task<object> GetDepartmentReportAsync(Guid departmentId, DateTime from, DateTime to);
    Task<List<StudentAttendancePredictionDto>> GetAttendancePredictionsAsync(Guid? courseId);
    Task<object> GetStudentStatsAsync(Guid studentId);
}

public interface IGeofenceService
{
    bool IsWithinGeofence(double userLat, double userLng, double centerLat, double centerLng, double radiusMeters);
}
