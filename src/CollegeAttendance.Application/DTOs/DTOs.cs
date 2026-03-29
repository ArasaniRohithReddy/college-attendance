using CollegeAttendance.Domain.Enums;

namespace CollegeAttendance.Application.DTOs;

// ===== Auth DTOs =====
public record GoogleLoginRequest(string IdToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record RefreshTokenRequest(string RefreshToken);

// ===== User DTOs =====
public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role,
    string? StudentId,
    string? EmployeeId,
    string? ProfileImageUrl,
    string? Phone,
    Guid? DepartmentId,
    string? DepartmentName,
    bool IsActive
);

public record CreateUserRequest(
    string Email,
    string FullName,
    UserRole Role,
    string? StudentId,
    string? EmployeeId,
    string? Phone,
    Guid? DepartmentId,
    Guid? HostelId
);

public record UpdateUserRequest(
    string? FullName,
    string? Phone,
    Guid? DepartmentId,
    Guid? HostelId,
    bool? IsActive
);

// ===== Department DTOs =====
public record DepartmentDto(Guid Id, string Name, string Code, string? Description, int UserCount, int CourseCount);
public record CreateDepartmentRequest(string Name, string Code, string? Description, Guid? HeadOfDepartmentId);
public record UpdateDepartmentRequest(string? Name, string? Code, string? Description, Guid? HeadOfDepartmentId);

// ===== Course DTOs =====
public record CourseDto(
    Guid Id, string Name, string Code, string? Description,
    int Credits, int Semester, int Year,
    Guid DepartmentId, string DepartmentName,
    Guid FacultyId, string FacultyName,
    int EnrolledStudents
);

public record CreateCourseRequest(
    string Name, string Code, string? Description,
    int Credits, int Semester, int Year,
    Guid DepartmentId, Guid FacultyId
);

public record EnrollStudentRequest(Guid StudentId);

// ===== Class Session DTOs =====
public record ClassSessionDto(
    Guid Id, string Title, DateTime ScheduledDate,
    TimeSpan StartTime, TimeSpan EndTime, string? Room,
    SessionStatus Status, Guid CourseId, string CourseName,
    Guid FacultyId, string FacultyName,
    double? Latitude, double? Longitude, double GeofenceRadiusMeters,
    int PresentCount, int TotalStudents
);

public record CreateClassSessionRequest(
    string Title, DateTime ScheduledDate,
    TimeSpan StartTime, TimeSpan EndTime, string? Room,
    Guid CourseId, double? Latitude, double? Longitude,
    double GeofenceRadiusMeters
);

// ===== QR Session DTOs =====
public record QRSessionDto(
    Guid Id, string QRToken, string QRImageBase64,
    DateTime GeneratedAt, DateTime ExpiresAt,
    bool IsActive, int ScanCount
);

public record GenerateQRRequest(Guid ClassSessionId, int ExpirationSeconds = 30);

// ===== Attendance DTOs =====
public record AttendanceDto(
    Guid Id, Guid StudentId, string StudentName, string? StudentId_Number,
    Guid ClassSessionId, string SessionTitle,
    AttendanceStatus Status, DateTime MarkedAt,
    bool IsManualEntry, bool IsGeofenceValid, bool IsFraudSuspected,
    string? Remarks
);

public record MarkAttendanceRequest(
    string QRToken,
    double Latitude,
    double Longitude,
    string? DeviceId
);

public record ManualAttendanceRequest(
    Guid ClassSessionId,
    List<ManualAttendanceEntry> Entries
);

public record ManualAttendanceEntry(Guid StudentId, AttendanceStatus Status, string? Remarks);

public record AttendanceReportDto(
    Guid StudentId, string StudentName, string? StudentIdNumber,
    string CourseName, string CourseCode,
    int TotalSessions, int PresentCount, int AbsentCount,
    int LateCount, int ExcusedCount,
    double AttendancePercentage, bool IsDefaulter
);

// ===== Hostel DTOs =====
public record HostelDto(Guid Id, string Name, string? Block, int Capacity, string? WardenName, int ResidentCount);
public record CreateHostelRequest(string Name, string? Block, int Capacity, Guid? WardenId);

public record HostelLogDto(
    Guid Id, Guid StudentId, string StudentName,
    HostelLogType LogType, DateTime Timestamp,
    string? VerificationMethod, string? VerifiedByName
);

public record CreateHostelLogRequest(Guid StudentId, Guid HostelId, HostelLogType LogType, string? VerificationMethod);

// ===== Mess DTOs =====
public record MessLogDto(
    Guid Id, Guid StudentId, string StudentName,
    MealType MealType, DateTime Date, DateTime ScannedAt,
    string? VerificationMethod
);

public record CreateMessLogRequest(Guid StudentId, MealType MealType, string? VerificationMethod);

public record MessAnalyticsDto(DateTime Date, int BreakfastCount, int LunchCount, int SnacksCount, int DinnerCount);

// ===== Outing DTOs =====
public record OutingRequestDto(
    Guid Id, Guid StudentId, string StudentName,
    string Purpose, string Destination,
    DateTime RequestedOutTime, DateTime ExpectedReturnTime,
    DateTime? ActualOutTime, DateTime? ActualReturnTime,
    OutingStatus Status, string? WardenRemarks, string? SecurityRemarks,
    string? GatePassQRCode, DateTime? GatePassExpiresAt
);

public record CreateOutingRequest(
    string Purpose, string Destination,
    DateTime RequestedOutTime, DateTime ExpectedReturnTime,
    string? EmergencyContact
);

public record ApproveOutingRequest(string? Remarks);
public record RejectOutingRequest(string Remarks);

// ===== Notification DTOs =====
public record NotificationDto(
    Guid Id, string Title, string Message,
    NotificationType Type, bool IsRead,
    DateTime CreatedAt, string? ActionUrl
);

// ===== Analytics DTOs =====
public record DashboardAnalyticsDto(
    int TotalStudents, int TotalFaculty, int TotalCourses,
    int TotalSessionsToday, double OverallAttendancePercentage,
    int DefaulterCount, List<DepartmentAttendanceDto> DepartmentWise
);

public record DepartmentAttendanceDto(string DepartmentName, double AttendancePercentage, int StudentCount);

public record StudentAttendancePredictionDto(
    Guid StudentId, string StudentName,
    double CurrentPercentage, double PredictedEndPercentage,
    bool AtRisk, string RiskLevel
);

// ===== Gamification DTOs =====
public record BadgeDto(Guid Id, string Name, string Description, BadgeType Type, string? IconUrl, int SortOrder);
public record StudentBadgeDto(Guid Id, Guid StudentId, string StudentName, BadgeDto Badge, DateTime EarnedAt);
public record StreakDto(Guid StudentId, string StudentName, int CurrentStreak, int LongestStreak, DateTime? LastPresentDate, int TotalPresentDays);

public record LeaderboardEntryDto(
    int Rank, Guid StudentId, string StudentName, string? DepartmentName,
    double TotalScore, double AttendanceScore, double StreakScore,
    double ConsistencyScore, string Period
);

public record GamificationDashboardDto(
    StreakDto Streak,
    List<StudentBadgeDto> Badges,
    int CurrentRank,
    double TotalScore,
    int TotalBadges,
    LeaderboardEntryDto? LeaderboardPosition
);

// ===== Leave Management DTOs =====
public record LeaveRequestDto(
    Guid Id, Guid StudentId, string StudentName,
    LeaveType LeaveType, DateTime StartDate, DateTime EndDate,
    string Reason, LeaveStatus Status,
    string? ApprovedByName, DateTime? ApprovedAt,
    string? RejectionReason, string? DocumentUrl,
    Guid? CourseId, string? CourseName
);

public record CreateLeaveRequest(
    LeaveType LeaveType, DateTime StartDate, DateTime EndDate,
    string Reason, Guid? CourseId, string? DocumentUrl
);

public record ApproveLeaveRequest(string? Remarks);
public record RejectLeaveRequest(string Reason);

// ===== Emergency SOS DTOs =====
public record EmergencySOSDto(
    Guid Id, Guid StudentId, string StudentName,
    double Latitude, double Longitude, string? Message,
    SOSStatus Status, SOSPriority Priority,
    string? RespondedByName, DateTime? RespondedAt,
    DateTime? ResolvedAt, string? ResolutionNotes,
    DateTime CreatedAt
);

public record CreateSOSRequest(double Latitude, double Longitude, string? Message, SOSPriority Priority);
public record RespondSOSRequest(string? Notes);
public record ResolveSOSRequest(string ResolutionNotes);

// ===== Offline Sync DTOs =====
public record OfflineSyncLogDto(
    Guid Id, SyncEntityType EntityType, string Action,
    string Payload, SyncStatus SyncStatus,
    int AttemptCount, DateTime? SyncedAt
);

public record SyncBatchRequest(List<SyncItemRequest> Items);

public record SyncItemRequest(
    SyncEntityType EntityType,
    string Action,
    string Payload,
    string? DeviceId,
    DateTime ClientTimestamp
);

public record SyncBatchResponse(
    int TotalItems, int Succeeded, int Failed, int Conflicts,
    List<SyncItemResult> Results
);

public record SyncItemResult(int Index, bool Success, string? Error, string? ConflictDetails, Guid? EntityId);

// ===== Device Binding & Fraud DTOs =====
public record DeviceBindingDto(Guid Id, string DeviceFingerprint, string? DeviceName, string? Platform, DateTime BoundAt, bool IsActive, DateTime? LastUsedAt);
public record BindDeviceRequest(string DeviceFingerprint, string? DeviceName, string? Platform);

public record FraudLogDto(
    Guid Id, Guid UserId, string UserName,
    FraudType FraudType, FraudSeverity Severity,
    string Description, string? Evidence,
    Guid? ClassSessionId, string? SessionTitle,
    bool IsResolved, string? ResolvedByName, DateTime? ResolvedAt,
    DateTime CreatedAt
);

public record ResolveFraudRequest(string ResolutionNotes);

// ===== Admin & System Config DTOs =====
public record SystemConfigDto(Guid Id, string Key, string Value, string? Description, string DataType, ConfigCategory Category);
public record UpdateConfigRequest(string Value);
public record CreateConfigRequest(string Key, string Value, string? Description, string DataType, ConfigCategory Category);

public record BulkImportResultDto(int TotalRows, int Succeeded, int Failed, List<string> Errors);
public record BulkImportStudentRow(string Email, string FullName, string? StudentId, string? Phone, string? DepartmentCode, string? HostelName);
public record BulkImportCourseRow(string Name, string Code, int Credits, int Semester, int Year, string DepartmentCode, string FacultyEmail);

// ===== Advanced Analytics DTOs =====
public record AttendanceHeatmapDto(string DayOfWeek, int Hour, double AttendancePercentage, int SessionCount);
public record DropoutRiskDto(Guid StudentId, string StudentName, string? DepartmentName, double AttendancePercentage, double RiskScore, string RiskLevel, List<string> RiskFactors);
public record FacultyStrictnessDto(Guid FacultyId, string FacultyName, double AverageAttendance, int TotalSessions, double LateMarkPercentage, string StrictnessLevel);
public record CourseAnalyticsDto(Guid CourseId, string CourseName, string CourseCode, double AttendancePercentage, int EnrolledCount, int DefaulterCount, double TrendPercentage);

public record AdvancedDashboardDto(
    int ActiveSOSCount,
    int PendingLeaves,
    int FraudAlertsToday,
    int CurfewViolationsToday,
    List<AttendanceHeatmapDto> Heatmap,
    List<DropoutRiskDto> TopRiskStudents,
    List<FacultyStrictnessDto> FacultyStrictness
);

// ===== Curfew DTOs =====
public record CurfewLogDto(
    Guid Id, Guid StudentId, string StudentName,
    string HostelName, DateTime CurfewTime, DateTime? ReturnTime,
    int MinutesLate, bool ParentNotified, DateTime? ParentNotifiedAt
);

public record CurfewConfigDto(TimeSpan CurfewTime, int GracePeriodMinutes, bool AutoNotifyParent);

// ===== Scheduled Notification DTOs =====
public record ScheduledNotificationDto(
    Guid Id, string Title, string Message, NotificationType Type,
    DateTime ScheduledAt, DateTime? SentAt, bool IsSent,
    UserRole? TargetRole, Guid? TargetUserId, bool IsRecurring,
    string? CronExpression, bool IsActive
);

public record CreateScheduledNotificationRequest(
    string Title, string Message, NotificationType Type,
    DateTime ScheduledAt, UserRole? TargetRole, Guid? TargetUserId,
    bool IsRecurring, string? CronExpression
);

// ===== Common =====
public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
