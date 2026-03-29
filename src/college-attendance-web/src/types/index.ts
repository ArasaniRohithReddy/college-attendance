// ===== Enums =====
export enum UserRole {
  Admin = 0,
  Faculty = 1,
  Student = 2,
  Warden = 3,
  Security = 4,
  LabAssistant = 5,
  Parent = 6,
}

export enum AttendanceStatus {
  Present = 0,
  Absent = 1,
  Late = 2,
  Excused = 3,
}

export enum OutingStatus {
  Pending = 0,
  ApprovedByWarden = 1,
  ApprovedBySecurity = 2,
  Rejected = 3,
  CheckedOut = 4,
  CheckedIn = 5,
  Expired = 6,
}

export enum MealType {
  Breakfast = 0,
  Lunch = 1,
  Snacks = 2,
  Dinner = 3,
}

export enum NotificationType {
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
  SystemAlert = 13,
}

export enum HostelLogType {
  Entry = 0,
  Exit = 1,
}

export enum SessionStatus {
  Scheduled = 0,
  Active = 1,
  Completed = 2,
  Cancelled = 3,
}

// ===== Auth =====
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserDto;
}

export interface GoogleLoginRequest {
  idToken: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

// ===== User =====
export interface UserDto {
  id: string;
  email: string;
  fullName: string;
  role: UserRole;
  studentId?: string;
  employeeId?: string;
  profileImageUrl?: string;
  phone?: string;
  departmentId?: string;
  departmentName?: string;
  isActive: boolean;
}

export interface CreateUserRequest {
  email: string;
  fullName: string;
  role: UserRole;
  studentId?: string;
  employeeId?: string;
  phone?: string;
  departmentId?: string;
  hostelId?: string;
}

export interface UpdateUserRequest {
  fullName?: string;
  phone?: string;
  departmentId?: string;
  hostelId?: string;
  isActive?: boolean;
}

// ===== Department =====
export interface DepartmentDto {
  id: string;
  name: string;
  code: string;
  description?: string;
  userCount: number;
  courseCount: number;
}

export interface CreateDepartmentRequest {
  name: string;
  code: string;
  description?: string;
  headOfDepartmentId?: string;
}

export interface UpdateDepartmentRequest {
  name?: string;
  code?: string;
  description?: string;
  headOfDepartmentId?: string;
}

// ===== Course =====
export interface CourseDto {
  id: string;
  name: string;
  code: string;
  description?: string;
  credits: number;
  semester: number;
  year: number;
  departmentId: string;
  departmentName: string;
  facultyId: string;
  facultyName: string;
  enrolledStudents: number;
}

export interface CreateCourseRequest {
  name: string;
  code: string;
  description?: string;
  credits: number;
  semester: number;
  year: number;
  departmentId: string;
  facultyId: string;
}

// ===== Class Session =====
export interface ClassSessionDto {
  id: string;
  title: string;
  scheduledDate: string;
  startTime: string;
  endTime: string;
  room?: string;
  status: SessionStatus;
  courseId: string;
  courseName: string;
  facultyId: string;
  facultyName: string;
  latitude?: number;
  longitude?: number;
  geofenceRadiusMeters: number;
  presentCount: number;
  totalStudents: number;
}

export interface CreateClassSessionRequest {
  title: string;
  scheduledDate: string;
  startTime: string;
  endTime: string;
  room?: string;
  courseId: string;
  latitude?: number;
  longitude?: number;
  geofenceRadiusMeters: number;
}

// ===== QR Session =====
export interface QRSessionDto {
  id: string;
  qrToken: string;
  qrImageBase64: string;
  generatedAt: string;
  expiresAt: string;
  isActive: boolean;
  scanCount: number;
}

export interface GenerateQRRequest {
  classSessionId: string;
  expirationSeconds?: number;
}

// ===== Attendance =====
export interface AttendanceDto {
  id: string;
  studentId: string;
  studentName: string;
  studentId_Number?: string;
  classSessionId: string;
  sessionTitle: string;
  status: AttendanceStatus;
  markedAt: string;
  isManualEntry: boolean;
  isGeofenceValid: boolean;
  isFraudSuspected: boolean;
  remarks?: string;
}

export interface MarkAttendanceRequest {
  qrToken: string;
  latitude: number;
  longitude: number;
  deviceId?: string;
}

export interface ManualAttendanceRequest {
  classSessionId: string;
  entries: ManualAttendanceEntry[];
}

export interface ManualAttendanceEntry {
  studentId: string;
  status: AttendanceStatus;
  remarks?: string;
}

export interface AttendanceReportDto {
  studentId: string;
  studentName: string;
  studentIdNumber?: string;
  courseName: string;
  courseCode: string;
  totalSessions: number;
  presentCount: number;
  absentCount: number;
  lateCount: number;
  excusedCount: number;
  attendancePercentage: number;
  isDefaulter: boolean;
}

// ===== Hostel =====
export interface HostelDto {
  id: string;
  name: string;
  block?: string;
  capacity: number;
  wardenName?: string;
  residentCount: number;
}

export interface CreateHostelRequest {
  name: string;
  block?: string;
  capacity: number;
  wardenId?: string;
}

export interface HostelLogDto {
  id: string;
  studentId: string;
  studentName: string;
  logType: HostelLogType;
  timestamp: string;
  verificationMethod?: string;
  verifiedByName?: string;
}

export interface CreateHostelLogRequest {
  studentId: string;
  hostelId: string;
  logType: HostelLogType;
  verificationMethod?: string;
}

// ===== Mess =====
export interface MessLogDto {
  id: string;
  studentId: string;
  studentName: string;
  mealType: MealType;
  date: string;
  scannedAt: string;
  verificationMethod?: string;
}

export interface CreateMessLogRequest {
  studentId: string;
  mealType: MealType;
  verificationMethod?: string;
}

export interface MessAnalyticsDto {
  date: string;
  breakfastCount: number;
  lunchCount: number;
  snacksCount: number;
  dinnerCount: number;
}

// ===== Outing =====
export interface OutingRequestDto {
  id: string;
  studentId: string;
  studentName: string;
  purpose: string;
  destination: string;
  requestedOutTime: string;
  expectedReturnTime: string;
  actualOutTime?: string;
  actualReturnTime?: string;
  status: OutingStatus;
  wardenRemarks?: string;
  securityRemarks?: string;
  gatePassQRCode?: string;
  gatePassExpiresAt?: string;
}

export interface CreateOutingRequestDto {
  purpose: string;
  destination: string;
  requestedOutTime: string;
  expectedReturnTime: string;
  emergencyContact?: string;
}

// ===== Notification =====
export interface NotificationDto {
  id: string;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: string;
  actionUrl?: string;
}

// ===== Analytics =====
export interface DashboardAnalyticsDto {
  totalStudents: number;
  totalFaculty: number;
  totalCourses: number;
  totalSessionsToday: number;
  overallAttendancePercentage: number;
  defaulterCount: number;
  departmentWise: DepartmentAttendanceDto[];
}

export interface DepartmentAttendanceDto {
  departmentName: string;
  attendancePercentage: number;
  studentCount: number;
}

export interface StudentAttendancePredictionDto {
  studentId: string;
  studentName: string;
  currentPercentage: number;
  predictedEndPercentage: number;
  atRisk: boolean;
  riskLevel: string;
}

// ===== Gamification Enums =====
export enum BadgeType {
  FirstScan = 0,
  WeekWarrior = 1,
  MonthlyChampion = 2,
  StreakMaster = 3,
  OnTimeHero = 4,
  HundredPercent = 5,
  EarlyBird = 6,
  Consistent = 7,
  Improver = 8,
  Custom = 9,
}

export enum LeaveStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Cancelled = 3,
  Expired = 4,
}

export enum LeaveType {
  Sick = 0,
  Personal = 1,
  Academic = 2,
  Emergency = 3,
  Other = 4,
}

export enum SOSStatus {
  Active = 0,
  Acknowledged = 1,
  Responding = 2,
  Resolved = 3,
  FalseAlarm = 4,
}

export enum SOSPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3,
}

export enum SyncStatus {
  Pending = 0,
  Synced = 1,
  Failed = 2,
  Conflict = 3,
}

export enum SyncEntityType {
  Attendance = 0,
  HostelLog = 1,
  MessLog = 2,
  OutingRequest = 3,
}

export enum FraudType {
  MultipleDevices = 0,
  LocationSpoof = 1,
  RapidScanSpike = 2,
  ProxyAttendance = 3,
  DeviceMismatch = 4,
  TimeAnomaly = 5,
  Other = 6,
}

export enum FraudSeverity {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3,
}

export enum ConfigCategory {
  General = 0,
  Attendance = 1,
  Hostel = 2,
  Curfew = 3,
  Gamification = 4,
  Notification = 5,
  Security = 6,
  Integration = 7,
}

export enum AttendanceSessionType {
  Regular = 0,
  Lab = 1,
  Tutorial = 2,
  Seminar = 3,
  Exam = 4,
}

// ===== Gamification =====
export interface BadgeDto {
  id: string;
  name: string;
  description: string;
  type: BadgeType;
  iconUrl?: string;
  sortOrder: number;
}

export interface StudentBadgeDto {
  id: string;
  studentId: string;
  studentName: string;
  badge: BadgeDto;
  earnedAt: string;
}

export interface StreakDto {
  studentId: string;
  studentName: string;
  currentStreak: number;
  longestStreak: number;
  lastPresentDate?: string;
  totalPresentDays: number;
}

export interface LeaderboardEntryDto {
  rank: number;
  studentId: string;
  studentName: string;
  departmentName?: string;
  totalScore: number;
  attendanceScore: number;
  streakScore: number;
  consistencyScore: number;
  period: string;
}

export interface GamificationDashboardDto {
  streak: StreakDto;
  badges: StudentBadgeDto[];
  currentRank: number;
  totalScore: number;
  totalBadges: number;
  leaderboardPosition?: LeaderboardEntryDto;
}

// ===== Leave Management =====
export interface LeaveRequestDto {
  id: string;
  studentId: string;
  studentName: string;
  leaveType: LeaveType;
  startDate: string;
  endDate: string;
  reason: string;
  status: LeaveStatus;
  approvedByName?: string;
  approvedAt?: string;
  rejectionReason?: string;
  documentUrl?: string;
  courseId?: string;
  courseName?: string;
}

export interface CreateLeaveRequest {
  leaveType: LeaveType;
  startDate: string;
  endDate: string;
  reason: string;
  courseId?: string;
  documentUrl?: string;
}

export interface ApproveLeaveRequest {
  remarks?: string;
}

export interface RejectLeaveRequest {
  reason: string;
}

// ===== Emergency SOS =====
export interface EmergencySOSDto {
  id: string;
  studentId: string;
  studentName: string;
  latitude: number;
  longitude: number;
  message?: string;
  status: SOSStatus;
  priority: SOSPriority;
  respondedByName?: string;
  respondedAt?: string;
  resolvedAt?: string;
  resolutionNotes?: string;
  createdAt: string;
}

export interface CreateSOSRequest {
  latitude: number;
  longitude: number;
  message?: string;
  priority: SOSPriority;
}

export interface RespondSOSRequest {
  notes?: string;
}

export interface ResolveSOSRequest {
  resolutionNotes: string;
}

// ===== Offline Sync =====
export interface OfflineSyncLogDto {
  id: string;
  entityType: SyncEntityType;
  action: string;
  payload: string;
  syncStatus: SyncStatus;
  attemptCount: number;
  syncedAt?: string;
}

export interface SyncBatchRequest {
  items: SyncItemRequest[];
}

export interface SyncItemRequest {
  entityType: SyncEntityType;
  action: string;
  payload: string;
  deviceId?: string;
  clientTimestamp: string;
}

export interface SyncBatchResponse {
  totalItems: number;
  succeeded: number;
  failed: number;
  conflicts: number;
  results: SyncItemResult[];
}

export interface SyncItemResult {
  index: number;
  success: boolean;
  error?: string;
  conflictDetails?: string;
  entityId?: string;
}

// ===== Device Binding & Fraud =====
export interface DeviceBindingDto {
  id: string;
  deviceFingerprint: string;
  deviceName?: string;
  platform?: string;
  boundAt: string;
  isActive: boolean;
  lastUsedAt?: string;
}

export interface BindDeviceRequest {
  deviceFingerprint: string;
  deviceName?: string;
  platform?: string;
}

export interface FraudLogDto {
  id: string;
  userId: string;
  userName: string;
  fraudType: FraudType;
  severity: FraudSeverity;
  description: string;
  evidence?: string;
  classSessionId?: string;
  sessionTitle?: string;
  isResolved: boolean;
  resolvedByName?: string;
  resolvedAt?: string;
  createdAt: string;
}

export interface ResolveFraudRequest {
  resolutionNotes: string;
}

// ===== Admin & System Config =====
export interface SystemConfigDto {
  id: string;
  key: string;
  value: string;
  description?: string;
  dataType: string;
  category: ConfigCategory;
}

export interface CreateConfigRequest {
  key: string;
  value: string;
  description?: string;
  dataType: string;
  category: ConfigCategory;
}

export interface UpdateConfigRequest {
  value: string;
}

export interface BulkImportResultDto {
  totalRows: number;
  succeeded: number;
  failed: number;
  errors: string[];
}

// ===== Advanced Analytics =====
export interface AttendanceHeatmapDto {
  dayOfWeek: string;
  hour: number;
  attendancePercentage: number;
  sessionCount: number;
}

export interface DropoutRiskDto {
  studentId: string;
  studentName: string;
  departmentName?: string;
  attendancePercentage: number;
  riskScore: number;
  riskLevel: string;
  riskFactors: string[];
}

export interface FacultyStrictnessDto {
  facultyId: string;
  facultyName: string;
  averageAttendance: number;
  totalSessions: number;
  lateMarkPercentage: number;
  strictnessLevel: string;
}

export interface CourseAnalyticsDto {
  courseId: string;
  courseName: string;
  courseCode: string;
  attendancePercentage: number;
  enrolledCount: number;
  defaulterCount: number;
  trendPercentage: number;
}

export interface AdvancedDashboardDto {
  activeSOSCount: number;
  pendingLeaves: number;
  fraudAlertsToday: number;
  curfewViolationsToday: number;
  heatmap: AttendanceHeatmapDto[];
  topRiskStudents: DropoutRiskDto[];
  facultyStrictness: FacultyStrictnessDto[];
}

// ===== Curfew =====
export interface CurfewLogDto {
  id: string;
  studentId: string;
  studentName: string;
  hostelName: string;
  curfewTime: string;
  returnTime?: string;
  minutesLate: number;
  parentNotified: boolean;
  parentNotifiedAt?: string;
}

export interface CurfewConfigDto {
  curfewTime: string;
  gracePeriodMinutes: number;
  autoNotifyParent: boolean;
}

// ===== Scheduled Notification =====
export interface ScheduledNotificationDto {
  id: string;
  title: string;
  message: string;
  type: NotificationType;
  scheduledAt: string;
  sentAt?: string;
  isSent: boolean;
  targetRole?: UserRole;
  targetUserId?: string;
  isRecurring: boolean;
  cronExpression?: string;
  isActive: boolean;
}

export interface CreateScheduledNotificationRequest {
  title: string;
  message: string;
  type: NotificationType;
  scheduledAt: string;
  targetRole?: UserRole;
  targetUserId?: string;
  isRecurring: boolean;
  cronExpression?: string;
}

// ===== Common =====
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}
