using System.Text.Json.Serialization;

namespace CollegeAttendance.Mobile.Models;

// ===== Auth =====
public class GoogleLoginRequest
{
    [JsonPropertyName("idToken")]
    public string IdToken { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    [JsonPropertyName("user")]
    public UserDto User { get; set; } = new();
}

// ===== User =====
public class UserDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public UserRole Role { get; set; }

    [JsonPropertyName("studentId")]
    public string? StudentId { get; set; }

    [JsonPropertyName("employeeId")]
    public string? EmployeeId { get; set; }

    [JsonPropertyName("profileImageUrl")]
    public string? ProfileImageUrl { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("departmentId")]
    public Guid? DepartmentId { get; set; }

    [JsonPropertyName("departmentName")]
    public string? DepartmentName { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

// ===== Class Session =====
public class ClassSessionDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("scheduledDate")]
    public DateTime ScheduledDate { get; set; }

    [JsonPropertyName("startTime")]
    public TimeSpan StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public TimeSpan EndTime { get; set; }

    [JsonPropertyName("room")]
    public string? Room { get; set; }

    [JsonPropertyName("status")]
    public SessionStatus Status { get; set; }

    [JsonPropertyName("courseId")]
    public Guid CourseId { get; set; }

    [JsonPropertyName("courseName")]
    public string CourseName { get; set; } = string.Empty;

    [JsonPropertyName("facultyId")]
    public Guid FacultyId { get; set; }

    [JsonPropertyName("facultyName")]
    public string FacultyName { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("geofenceRadiusMeters")]
    public double GeofenceRadiusMeters { get; set; }

    [JsonPropertyName("presentCount")]
    public int PresentCount { get; set; }

    [JsonPropertyName("totalStudents")]
    public int TotalStudents { get; set; }

    public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    public string StatusDisplay => Status.ToString();
}

// ===== QR Session =====
public class QRSessionDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("qrToken")]
    public string QRToken { get; set; } = string.Empty;

    [JsonPropertyName("qrImageBase64")]
    public string QRImageBase64 { get; set; } = string.Empty;

    [JsonPropertyName("generatedAt")]
    public DateTime GeneratedAt { get; set; }

    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("scanCount")]
    public int ScanCount { get; set; }
}

public class GenerateQRRequest
{
    [JsonPropertyName("classSessionId")]
    public Guid ClassSessionId { get; set; }

    [JsonPropertyName("expirationSeconds")]
    public int ExpirationSeconds { get; set; } = 30;
}

// ===== Attendance =====
public class AttendanceDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("studentId")]
    public Guid StudentId { get; set; }

    [JsonPropertyName("studentName")]
    public string StudentName { get; set; } = string.Empty;

    [JsonPropertyName("studentId_Number")]
    public string? StudentId_Number { get; set; }

    [JsonPropertyName("classSessionId")]
    public Guid ClassSessionId { get; set; }

    [JsonPropertyName("sessionTitle")]
    public string SessionTitle { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public AttendanceStatus Status { get; set; }

    [JsonPropertyName("markedAt")]
    public DateTime MarkedAt { get; set; }

    [JsonPropertyName("isGeofenceValid")]
    public bool IsGeofenceValid { get; set; }

    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }
}

public class MarkAttendanceRequest
{
    [JsonPropertyName("qrToken")]
    public string QRToken { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("deviceId")]
    public string? DeviceId { get; set; }
}

public class AttendanceReportDto
{
    [JsonPropertyName("studentId")]
    public Guid StudentId { get; set; }

    [JsonPropertyName("studentName")]
    public string StudentName { get; set; } = string.Empty;

    [JsonPropertyName("courseName")]
    public string CourseName { get; set; } = string.Empty;

    [JsonPropertyName("courseCode")]
    public string CourseCode { get; set; } = string.Empty;

    [JsonPropertyName("totalSessions")]
    public int TotalSessions { get; set; }

    [JsonPropertyName("presentCount")]
    public int PresentCount { get; set; }

    [JsonPropertyName("absentCount")]
    public int AbsentCount { get; set; }

    [JsonPropertyName("attendancePercentage")]
    public double AttendancePercentage { get; set; }

    [JsonPropertyName("isDefaulter")]
    public bool IsDefaulter { get; set; }
}

// ===== Outing =====
public class OutingRequestDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("studentId")]
    public Guid StudentId { get; set; }

    [JsonPropertyName("studentName")]
    public string StudentName { get; set; } = string.Empty;

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;

    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;

    [JsonPropertyName("requestedOutTime")]
    public DateTime RequestedOutTime { get; set; }

    [JsonPropertyName("expectedReturnTime")]
    public DateTime ExpectedReturnTime { get; set; }

    [JsonPropertyName("actualOutTime")]
    public DateTime? ActualOutTime { get; set; }

    [JsonPropertyName("actualReturnTime")]
    public DateTime? ActualReturnTime { get; set; }

    [JsonPropertyName("status")]
    public OutingStatus Status { get; set; }

    [JsonPropertyName("wardenRemarks")]
    public string? WardenRemarks { get; set; }

    [JsonPropertyName("securityRemarks")]
    public string? SecurityRemarks { get; set; }

    [JsonPropertyName("gatePassQRCode")]
    public string? GatePassQRCode { get; set; }

    [JsonPropertyName("gatePassExpiresAt")]
    public DateTime? GatePassExpiresAt { get; set; }

    public string StatusDisplay => Status.ToString();
}

public class CreateOutingRequest
{
    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;

    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;

    [JsonPropertyName("requestedOutTime")]
    public DateTime RequestedOutTime { get; set; }

    [JsonPropertyName("expectedReturnTime")]
    public DateTime ExpectedReturnTime { get; set; }

    [JsonPropertyName("emergencyContact")]
    public string? EmergencyContact { get; set; }
}

// ===== Notification =====
public class NotificationDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public NotificationType Type { get; set; }

    [JsonPropertyName("isRead")]
    public bool IsRead { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("actionUrl")]
    public string? ActionUrl { get; set; }

    public string TimeAgo
    {
        get
        {
            var diff = DateTime.UtcNow - CreatedAt;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            return $"{(int)diff.TotalDays}d ago";
        }
    }
}

// ===== Analytics =====
public class DashboardAnalyticsDto
{
    [JsonPropertyName("totalStudents")]
    public int TotalStudents { get; set; }

    [JsonPropertyName("totalFaculty")]
    public int TotalFaculty { get; set; }

    [JsonPropertyName("totalCourses")]
    public int TotalCourses { get; set; }

    [JsonPropertyName("totalSessionsToday")]
    public int TotalSessionsToday { get; set; }

    [JsonPropertyName("overallAttendancePercentage")]
    public double OverallAttendancePercentage { get; set; }

    [JsonPropertyName("defaulterCount")]
    public int DefaulterCount { get; set; }
}

// ===== Common =====
public class PagedResult<T>
{
    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = [];

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
}

// ===== Gamification =====
public class BadgeDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public BadgeType Type { get; set; }

    [JsonPropertyName("iconUrl")]
    public string? IconUrl { get; set; }

    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }
}

public class StudentBadgeDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("studentId")]
    public Guid StudentId { get; set; }

    [JsonPropertyName("studentName")]
    public string StudentName { get; set; } = string.Empty;

    [JsonPropertyName("badge")]
    public BadgeDto Badge { get; set; } = new();

    [JsonPropertyName("earnedAt")]
    public DateTime EarnedAt { get; set; }
}

public class StreakDto
{
    [JsonPropertyName("studentId")]
    public Guid StudentId { get; set; }

    [JsonPropertyName("studentName")]
    public string StudentName { get; set; } = string.Empty;

    [JsonPropertyName("currentStreak")]
    public int CurrentStreak { get; set; }

    [JsonPropertyName("longestStreak")]
    public int LongestStreak { get; set; }

    [JsonPropertyName("lastPresentDate")]
    public DateTime? LastPresentDate { get; set; }

    [JsonPropertyName("totalPresentDays")]
    public int TotalPresentDays { get; set; }
}

public class LeaderboardEntryDto
{
    [JsonPropertyName("rank")]
    public int Rank { get; set; }

    [JsonPropertyName("studentId")]
    public Guid StudentId { get; set; }

    [JsonPropertyName("studentName")]
    public string StudentName { get; set; } = string.Empty;

    [JsonPropertyName("departmentName")]
    public string? DepartmentName { get; set; }

    [JsonPropertyName("totalScore")]
    public double TotalScore { get; set; }

    [JsonPropertyName("attendanceScore")]
    public double AttendanceScore { get; set; }

    [JsonPropertyName("streakScore")]
    public double StreakScore { get; set; }

    [JsonPropertyName("consistencyScore")]
    public double ConsistencyScore { get; set; }

    [JsonPropertyName("period")]
    public string Period { get; set; } = string.Empty;
}

public class GamificationDashboardDto
{
    [JsonPropertyName("streak")]
    public StreakDto Streak { get; set; } = new();

    [JsonPropertyName("badges")]
    public List<StudentBadgeDto> Badges { get; set; } = [];

    [JsonPropertyName("currentRank")]
    public int CurrentRank { get; set; }

    [JsonPropertyName("totalScore")]
    public double TotalScore { get; set; }

    [JsonPropertyName("totalBadges")]
    public int TotalBadges { get; set; }

    [JsonPropertyName("leaderboardPosition")]
    public LeaderboardEntryDto? LeaderboardPosition { get; set; }
}

// ===== Leave Management =====
public class LeaveRequestDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("studentId")]
    public Guid StudentId { get; set; }

    [JsonPropertyName("studentName")]
    public string StudentName { get; set; } = string.Empty;

    [JsonPropertyName("leaveType")]
    public LeaveType LeaveType { get; set; }

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public LeaveStatus Status { get; set; }

    [JsonPropertyName("approvedByName")]
    public string? ApprovedByName { get; set; }

    [JsonPropertyName("approvedAt")]
    public DateTime? ApprovedAt { get; set; }

    [JsonPropertyName("rejectionReason")]
    public string? RejectionReason { get; set; }

    [JsonPropertyName("documentUrl")]
    public string? DocumentUrl { get; set; }

    [JsonPropertyName("courseId")]
    public Guid? CourseId { get; set; }

    [JsonPropertyName("courseName")]
    public string? CourseName { get; set; }

    public string StatusDisplay => Status.ToString();
    public string DateRange => $"{StartDate:MMM dd} - {EndDate:MMM dd, yyyy}";
}

public class CreateLeaveRequest
{
    [JsonPropertyName("leaveType")]
    public LeaveType LeaveType { get; set; }

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("courseId")]
    public Guid? CourseId { get; set; }

    [JsonPropertyName("documentUrl")]
    public string? DocumentUrl { get; set; }
}

// ===== Emergency SOS =====
public class EmergencySOSDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("studentId")]
    public Guid StudentId { get; set; }

    [JsonPropertyName("studentName")]
    public string StudentName { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("status")]
    public SOSStatus Status { get; set; }

    [JsonPropertyName("priority")]
    public SOSPriority Priority { get; set; }

    [JsonPropertyName("respondedByName")]
    public string? RespondedByName { get; set; }

    [JsonPropertyName("respondedAt")]
    public DateTime? RespondedAt { get; set; }

    [JsonPropertyName("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }

    [JsonPropertyName("resolutionNotes")]
    public string? ResolutionNotes { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    public string StatusDisplay => Status.ToString();
    public string PriorityDisplay => Priority.ToString();
}

public class CreateSOSRequest
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("priority")]
    public SOSPriority Priority { get; set; }
}
