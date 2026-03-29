import api from './client';
import type {
  AuthResponse, GoogleLoginRequest, RefreshTokenRequest,
  UserDto, CreateUserRequest, UpdateUserRequest,
  DepartmentDto, CreateDepartmentRequest, UpdateDepartmentRequest,
  CourseDto, CreateCourseRequest,
  ClassSessionDto, CreateClassSessionRequest,
  QRSessionDto, GenerateQRRequest,
  AttendanceDto, MarkAttendanceRequest, ManualAttendanceRequest, AttendanceReportDto,
  HostelDto, CreateHostelRequest, HostelLogDto, CreateHostelLogRequest,
  MessLogDto, CreateMessLogRequest, MessAnalyticsDto,
  OutingRequestDto, CreateOutingRequestDto,
  NotificationDto,
  DashboardAnalyticsDto, StudentAttendancePredictionDto,
  PagedResult,
  GamificationDashboardDto, StudentBadgeDto, StreakDto, LeaderboardEntryDto,
  LeaveRequestDto, CreateLeaveRequest, ApproveLeaveRequest, RejectLeaveRequest,
  EmergencySOSDto, CreateSOSRequest, ResolveSOSRequest,
  SyncBatchRequest, SyncBatchResponse, OfflineSyncLogDto,
  DeviceBindingDto, BindDeviceRequest, FraudLogDto, ResolveFraudRequest,
  SystemConfigDto, CreateConfigRequest, UpdateConfigRequest, BulkImportResultDto,
  AttendanceHeatmapDto, DropoutRiskDto, FacultyStrictnessDto, CourseAnalyticsDto,
  AdvancedDashboardDto, CurfewLogDto, CurfewConfigDto,
} from '../types';

// ===== Auth =====
export const authApi = {
  googleLogin: (req: GoogleLoginRequest) =>
    api.post<AuthResponse>('/auth/google', req).then(r => r.data),
  refresh: (req: RefreshTokenRequest) =>
    api.post<AuthResponse>('/auth/refresh', req).then(r => r.data),
  me: () => api.get<UserDto>('/auth/me').then(r => r.data),
  logout: () => api.post('/auth/logout'),
};

// ===== Users =====
export const usersApi = {
  list: (page = 1, size = 20) =>
    api.get<PagedResult<UserDto>>('/users', { params: { page, pageSize: size } }).then(r => r.data),
  get: (id: string) => api.get<UserDto>(`/users/${id}`).then(r => r.data),
  create: (req: CreateUserRequest) => api.post<UserDto>('/users', req).then(r => r.data),
  update: (id: string, req: UpdateUserRequest) => api.put(`/users/${id}`, req),
  delete: (id: string) => api.delete(`/users/${id}`),
  byRole: (role: number) => api.get<UserDto[]>(`/users/role/${role}`).then(r => r.data),
};

// ===== Departments =====
export const departmentsApi = {
  list: () => api.get<DepartmentDto[]>('/departments').then(r => r.data),
  get: (id: string) => api.get<DepartmentDto>(`/departments/${id}`).then(r => r.data),
  create: (req: CreateDepartmentRequest) => api.post<DepartmentDto>('/departments', req).then(r => r.data),
  update: (id: string, req: UpdateDepartmentRequest) => api.put(`/departments/${id}`, req),
  delete: (id: string) => api.delete(`/departments/${id}`),
};

// ===== Courses =====
export const coursesApi = {
  list: () => api.get<CourseDto[]>('/courses').then(r => r.data),
  get: (id: string) => api.get<CourseDto>(`/courses/${id}`).then(r => r.data),
  create: (req: CreateCourseRequest) => api.post<CourseDto>('/courses', req).then(r => r.data),
  byDepartment: (deptId: string) => api.get<CourseDto[]>(`/courses/department/${deptId}`).then(r => r.data),
  byFaculty: (facId: string) => api.get<CourseDto[]>(`/courses/faculty/${facId}`).then(r => r.data),
  enroll: (courseId: string, studentId: string) =>
    api.post(`/courses/${courseId}/enroll`, { studentId }),
  unenroll: (courseId: string, studentId: string) =>
    api.delete(`/courses/${courseId}/enroll/${studentId}`),
};

// ===== Class Sessions =====
export const sessionsApi = {
  list: (courseId?: string) =>
    api.get<ClassSessionDto[]>('/classsessions', { params: { courseId } }).then(r => r.data),
  get: (id: string) => api.get<ClassSessionDto>(`/classsessions/${id}`).then(r => r.data),
  create: (req: CreateClassSessionRequest) =>
    api.post<ClassSessionDto>('/classsessions', req).then(r => r.data),
  start: (id: string) => api.post(`/classsessions/${id}/start`),
  end: (id: string) => api.post(`/classsessions/${id}/end`),
  today: () => api.get<ClassSessionDto[]>('/classsessions/today').then(r => r.data),
};

// ===== QR =====
export const qrApi = {
  generate: (req: GenerateQRRequest) =>
    api.post<QRSessionDto>('/qr/generate', req).then(r => r.data),
  active: (sessionId: string) =>
    api.get<QRSessionDto>(`/qr/active/${sessionId}`).then(r => r.data),
  deactivate: (id: string) => api.post(`/qr/${id}/deactivate`),
};

// ===== Attendance =====
export const attendanceApi = {
  mark: (req: MarkAttendanceRequest) =>
    api.post<AttendanceDto>('/attendance/mark', req).then(r => r.data),
  markManual: (req: ManualAttendanceRequest) =>
    api.post('/attendance/manual', req),
  bySession: (sessionId: string) =>
    api.get<AttendanceDto[]>(`/attendance/session/${sessionId}`).then(r => r.data),
  byStudent: (studentId: string) =>
    api.get<AttendanceDto[]>(`/attendance/student/${studentId}`).then(r => r.data),
  report: (courseId: string) =>
    api.get<AttendanceReportDto[]>(`/attendance/report/${courseId}`).then(r => r.data),
  defaulters: (threshold?: number) =>
    api.get<AttendanceReportDto[]>('/attendance/defaulters', { params: { threshold } }).then(r => r.data),
};

// ===== Hostel =====
export const hostelApi = {
  list: () => api.get<HostelDto[]>('/hostel').then(r => r.data),
  create: (req: CreateHostelRequest) => api.post<HostelDto>('/hostel', req).then(r => r.data),
  logs: (hostelId: string, date?: string) =>
    api.get<HostelLogDto[]>(`/hostel/${hostelId}/logs`, { params: { date } }).then(r => r.data),
  logEntry: (req: CreateHostelLogRequest) => api.post('/hostel/log', req),
};

// ===== Mess =====
export const messApi = {
  logs: (date?: string) =>
    api.get<MessLogDto[]>('/mess/logs', { params: { date } }).then(r => r.data),
  log: (req: CreateMessLogRequest) => api.post('/mess/log', req),
  analytics: (from?: string, to?: string) =>
    api.get<MessAnalyticsDto[]>('/mess/analytics', { params: { from, to } }).then(r => r.data),
};

// ===== Outing =====
export const outingApi = {
  list: (status?: number) =>
    api.get<OutingRequestDto[]>('/outing', { params: { status } }).then(r => r.data),
  get: (id: string) => api.get<OutingRequestDto>(`/outing/${id}`).then(r => r.data),
  create: (req: CreateOutingRequestDto) =>
    api.post<OutingRequestDto>('/outing', req).then(r => r.data),
  approve: (id: string, remarks?: string) =>
    api.post(`/outing/${id}/approve`, { remarks }),
  reject: (id: string, remarks: string) =>
    api.post(`/outing/${id}/reject`, { remarks }),
  checkout: (id: string, remarks?: string) =>
    api.post(`/outing/${id}/checkout`, { remarks }),
  checkin: (id: string, remarks?: string) =>
    api.post(`/outing/${id}/checkin`, { remarks }),
  myRequests: () => api.get<OutingRequestDto[]>('/outing/my').then(r => r.data),
};

// ===== Notifications =====
export const notificationsApi = {
  my: () => api.get<NotificationDto[]>('/notifications').then(r => r.data),
  markRead: (id: string) => api.put(`/notifications/${id}/read`),
  markAllRead: () => api.put('/notifications/read-all'),
  unreadCount: () => api.get<number>('/notifications/unread-count').then(r => r.data),
};

// ===== Analytics =====
export const analyticsApi = {
  dashboard: () => api.get<DashboardAnalyticsDto>('/analytics/dashboard').then(r => r.data),
  predictions: () =>
    api.get<StudentAttendancePredictionDto[]>('/analytics/predictions').then(r => r.data),
  courseAttendanceTrend: (courseId: string, days = 30) =>
    api.get(`/analytics/course/${courseId}/trend`, { params: { days } }).then(r => r.data),
};

// ===== Gamification =====
export const gamificationApi = {
  dashboard: () => api.get<GamificationDashboardDto>('/gamification/dashboard').then(r => r.data),
  streak: () => api.get<StreakDto>('/gamification/streak').then(r => r.data),
  badges: () => api.get<StudentBadgeDto[]>('/gamification/badges').then(r => r.data),
  leaderboard: (period = 'monthly', dept?: string) =>
    api.get<LeaderboardEntryDto[]>('/gamification/leaderboard', { params: { period, departmentId: dept } }).then(r => r.data),
  recalculate: (studentId: string) => api.post(`/gamification/recalculate/${studentId}`),
};

// ===== Leave Management =====
export const leaveApi = {
  list: (page = 1, size = 20, status?: number) =>
    api.get<PagedResult<LeaveRequestDto>>('/leave', { params: { page, pageSize: size, status } }).then(r => r.data),
  get: (id: string) => api.get<LeaveRequestDto>(`/leave/${id}`).then(r => r.data),
  create: (req: CreateLeaveRequest) => api.post<LeaveRequestDto>('/leave', req).then(r => r.data),
  approve: (id: string, req?: ApproveLeaveRequest) => api.post(`/leave/${id}/approve`, req),
  reject: (id: string, req: RejectLeaveRequest) => api.post(`/leave/${id}/reject`, req),
  cancel: (id: string) => api.post(`/leave/${id}/cancel`),
  my: (page = 1, size = 20) =>
    api.get<PagedResult<LeaveRequestDto>>('/leave/my', { params: { page, pageSize: size } }).then(r => r.data),
};

// ===== Emergency SOS =====
export const emergencyApi = {
  create: (req: CreateSOSRequest) => api.post<EmergencySOSDto>('/emergency/sos', req).then(r => r.data),
  active: () => api.get<EmergencySOSDto[]>('/emergency/active').then(r => r.data),
  acknowledge: (id: string) => api.post(`/emergency/${id}/acknowledge`),
  resolve: (id: string, req: ResolveSOSRequest) => api.post(`/emergency/${id}/resolve`, req),
  history: (page = 1, size = 20) =>
    api.get<PagedResult<EmergencySOSDto>>('/emergency/history', { params: { page, pageSize: size } }).then(r => r.data),
};

// ===== Offline Sync =====
export const offlineSyncApi = {
  sync: (req: SyncBatchRequest) => api.post<SyncBatchResponse>('/offlinesync/sync', req).then(r => r.data),
  pending: () => api.get<OfflineSyncLogDto[]>('/offlinesync/pending').then(r => r.data),
  lastSync: () => api.get<string>('/offlinesync/last-sync').then(r => r.data),
};

// ===== Fraud Detection =====
export const fraudApi = {
  logs: (page = 1, size = 20, resolved?: boolean) =>
    api.get<PagedResult<FraudLogDto>>('/fraud/logs', { params: { page, pageSize: size, resolved } }).then(r => r.data),
  resolve: (id: string, req: ResolveFraudRequest) => api.post(`/fraud/${id}/resolve`, req),
  devices: (userId: string) => api.get<DeviceBindingDto[]>(`/fraud/devices/${userId}`).then(r => r.data),
  bindDevice: (userId: string, req: BindDeviceRequest) => api.post<DeviceBindingDto>(`/fraud/devices/${userId}/bind`, req).then(r => r.data),
  validateDevice: (userId: string, fingerprint: string) =>
    api.get<boolean>(`/fraud/devices/${userId}/validate`, { params: { fingerprint } }).then(r => r.data),
};

// ===== Admin Config =====
export const adminConfigApi = {
  list: (category?: number) =>
    api.get<SystemConfigDto[]>('/admin/config', { params: { category } }).then(r => r.data),
  get: (key: string) => api.get<SystemConfigDto>(`/admin/config/${encodeURIComponent(key)}`).then(r => r.data),
  create: (req: CreateConfigRequest) => api.post<SystemConfigDto>('/admin/config', req).then(r => r.data),
  update: (key: string, req: UpdateConfigRequest) => api.put(`/admin/config/${encodeURIComponent(key)}`, req),
  delete: (key: string) => api.delete(`/admin/config/${encodeURIComponent(key)}`),
};

// ===== Bulk Import =====
export const bulkImportApi = {
  importStudents: (file: File) => {
    const form = new FormData();
    form.append('file', file);
    return api.post<BulkImportResultDto>('/admin/import/students', form, { headers: { 'Content-Type': 'multipart/form-data' } }).then(r => r.data);
  },
  importCourses: (file: File) => {
    const form = new FormData();
    form.append('file', file);
    return api.post<BulkImportResultDto>('/admin/import/courses', form, { headers: { 'Content-Type': 'multipart/form-data' } }).then(r => r.data);
  },
  templateStudents: () => api.get('/admin/import/template/students', { responseType: 'blob' }).then(r => r.data),
  templateCourses: () => api.get('/admin/import/template/courses', { responseType: 'blob' }).then(r => r.data),
};

// ===== Advanced Analytics =====
export const advancedAnalyticsApi = {
  dashboard: () => api.get<AdvancedDashboardDto>('/analytics/advanced/dashboard').then(r => r.data),
  heatmap: (courseId?: string) =>
    api.get<AttendanceHeatmapDto[]>('/analytics/advanced/heatmap', { params: { courseId } }).then(r => r.data),
  dropoutRisk: (threshold = 0.6) =>
    api.get<DropoutRiskDto[]>('/analytics/advanced/dropout-risk', { params: { threshold } }).then(r => r.data),
  facultyStrictness: () =>
    api.get<FacultyStrictnessDto[]>('/analytics/advanced/faculty-strictness').then(r => r.data),
  courseAnalytics: () =>
    api.get<CourseAnalyticsDto[]>('/analytics/advanced/course-analytics').then(r => r.data),
  curfewLogs: (page = 1, size = 20) =>
    api.get<PagedResult<CurfewLogDto>>('/analytics/advanced/curfew/logs', { params: { page, pageSize: size } }).then(r => r.data),
  curfewConfig: () => api.get<CurfewConfigDto>('/analytics/advanced/curfew/config').then(r => r.data),
  checkCurfew: () => api.post('/analytics/advanced/curfew/check'),
};
