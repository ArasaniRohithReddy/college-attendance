namespace CollegeAttendance.Domain.Enums;

public enum UserRole
{
    Admin = 0,
    Faculty = 1,
    Student = 2,
    Warden = 3,
    Security = 4,
    LabAssistant = 5,
    Parent = 6
}

public enum AttendanceStatus
{
    Present = 0,
    Absent = 1,
    Late = 2,
    Excused = 3
}

public enum OutingStatus
{
    Pending = 0,
    ApprovedByWarden = 1,
    ApprovedBySecurity = 2,
    Rejected = 3,
    CheckedOut = 4,
    CheckedIn = 5,
    Expired = 6
}

public enum MealType
{
    Breakfast = 0,
    Lunch = 1,
    Snacks = 2,
    Dinner = 3
}

public enum NotificationType
{
    LowAttendance = 0,
    OutingApproval = 1,
    OutingRejection = 2,
    ClassReminder = 3,
    General = 4,
    FraudAlert = 5,
    BadgeEarned = 6,
    StreakMilestone = 7,
    LeaveUpdate = 8,
    EmergencySOS = 9,
    CurfewViolation = 10,
    ParentAlert = 11,
    AttendanceRisk = 12,
    SystemAlert = 13
}

public enum HostelLogType
{
    Entry = 0,
    Exit = 1
}

public enum SessionStatus
{
    Scheduled = 0,
    Active = 1,
    Completed = 2,
    Cancelled = 3
}

// ===== New Enums for Campus Management Platform =====

public enum BadgeType
{
    PerfectAttendance = 0,
    OnTimeHero = 1,
    ConsistencyKing = 2,
    StreakMaster = 3,
    EarlyBird = 4,
    WeekWarrior = 5,
    MonthlyChampion = 6,
    SemesterStar = 7,
    FirstScan = 8,
    HundredPercent = 9
}

public enum LeaveStatus
{
    Pending = 0,
    ApprovedByFaculty = 1,
    ApprovedByHOD = 2,
    Rejected = 3,
    Cancelled = 4
}

public enum LeaveType
{
    Medical = 0,
    Personal = 1,
    Family = 2,
    Academic = 3,
    Other = 4
}

public enum SOSStatus
{
    Active = 0,
    Acknowledged = 1,
    Responding = 2,
    Resolved = 3,
    FalseAlarm = 4
}

public enum SOSPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum SyncStatus
{
    Pending = 0,
    Synced = 1,
    Failed = 2,
    Conflict = 3
}

public enum SyncEntityType
{
    Attendance = 0,
    HostelLog = 1,
    MessLog = 2,
    OutingRequest = 3
}

public enum FraudType
{
    MultiLocationScan = 0,
    RapidScanSpike = 1,
    DeviceMismatch = 2,
    LocationSpoofing = 3,
    ScreenshotQR = 4,
    ProxyAttendance = 5,
    TimeAnomaly = 6
}

public enum FraudSeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum ConfigCategory
{
    Attendance = 0,
    QR = 1,
    Geofencing = 2,
    Security = 3,
    Notification = 4,
    Hostel = 5,
    Gamification = 6,
    System = 7
}

public enum AttendanceSessionType
{
    Regular = 0,
    Lab = 1,
    Exam = 2,
    Transport = 3,
    Extra = 4
}
