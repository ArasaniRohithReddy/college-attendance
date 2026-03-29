import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  usersApi, departmentsApi, coursesApi, sessionsApi,
  attendanceApi, hostelApi, messApi, outingApi,
  notificationsApi, analyticsApi, qrApi,
  gamificationApi, leaveApi, emergencyApi, offlineSyncApi,
  fraudApi, adminConfigApi, bulkImportApi, advancedAnalyticsApi,
} from '../api/services';
import type {
  CreateUserRequest, UpdateUserRequest,
  CreateDepartmentRequest, UpdateDepartmentRequest,
  CreateCourseRequest, CreateClassSessionRequest,
  GenerateQRRequest, MarkAttendanceRequest, ManualAttendanceRequest,
  CreateHostelRequest, CreateHostelLogRequest,
  CreateMessLogRequest, CreateOutingRequestDto,
  CreateLeaveRequest, ApproveLeaveRequest, RejectLeaveRequest,
  CreateSOSRequest, ResolveSOSRequest,
  SyncBatchRequest, BindDeviceRequest, ResolveFraudRequest,
  CreateConfigRequest, UpdateConfigRequest,
} from '../types';

// ===== Users =====
export const useUsers = (page = 1, size = 20) =>
  useQuery({ queryKey: ['users', page, size], queryFn: () => usersApi.list(page, size) });

export const useUser = (id: string) =>
  useQuery({ queryKey: ['users', id], queryFn: () => usersApi.get(id), enabled: !!id });

export const useUsersByRole = (role: number) =>
  useQuery({ queryKey: ['users', 'role', role], queryFn: () => usersApi.byRole(role) });

export function useCreateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateUserRequest) => usersApi.create(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  });
}

export function useUpdateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, req }: { id: string; req: UpdateUserRequest }) => usersApi.update(id, req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  });
}

export function useDeleteUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => usersApi.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  });
}

// ===== Departments =====
export const useDepartments = () =>
  useQuery({ queryKey: ['departments'], queryFn: departmentsApi.list });

export const useDepartment = (id: string) =>
  useQuery({ queryKey: ['departments', id], queryFn: () => departmentsApi.get(id), enabled: !!id });

export function useCreateDepartment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateDepartmentRequest) => departmentsApi.create(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['departments'] }),
  });
}

export function useUpdateDepartment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, req }: { id: string; req: UpdateDepartmentRequest }) => departmentsApi.update(id, req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['departments'] }),
  });
}

// ===== Courses =====
export const useCourses = () =>
  useQuery({ queryKey: ['courses'], queryFn: coursesApi.list });

export const useCourse = (id: string) =>
  useQuery({ queryKey: ['courses', id], queryFn: () => coursesApi.get(id), enabled: !!id });

export const useCoursesByDepartment = (deptId: string) =>
  useQuery({ queryKey: ['courses', 'dept', deptId], queryFn: () => coursesApi.byDepartment(deptId), enabled: !!deptId });

export const useCoursesByFaculty = () =>
  useQuery({ queryKey: ['courses', 'faculty'], queryFn: () => coursesApi.byFaculty() });

export function useCreateCourse() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateCourseRequest) => coursesApi.create(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['courses'] }),
  });
}

// ===== Class Sessions =====
export const useSessions = (courseId?: string) =>
  useQuery({ queryKey: ['sessions', courseId], queryFn: () => sessionsApi.list(courseId) });

export const useSession = (id: string) =>
  useQuery({ queryKey: ['sessions', id], queryFn: () => sessionsApi.get(id), enabled: !!id });

export const useTodaySessions = () =>
  useQuery({ queryKey: ['sessions', 'today'], queryFn: sessionsApi.today });

export function useCreateSession() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateClassSessionRequest) => sessionsApi.create(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sessions'] }),
  });
}

export function useStartSession() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => sessionsApi.start(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sessions'] }),
  });
}

export function useEndSession() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => sessionsApi.end(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sessions'] }),
  });
}

// ===== QR =====
export function useGenerateQR() {
  return useMutation({ mutationFn: (req: GenerateQRRequest) => qrApi.generate(req) });
}

export const useActiveQR = (sessionId: string) =>
  useQuery({
    queryKey: ['qr', sessionId],
    queryFn: () => qrApi.active(sessionId),
    enabled: !!sessionId,
    refetchInterval: 5000,
  });

// ===== Attendance =====
export const useAttendanceBySession = (sessionId: string) =>
  useQuery({ queryKey: ['attendance', 'session', sessionId], queryFn: () => attendanceApi.bySession(sessionId), enabled: !!sessionId });

export const useAttendanceByStudent = (studentId: string) =>
  useQuery({ queryKey: ['attendance', 'student', studentId], queryFn: () => attendanceApi.byStudent(studentId), enabled: !!studentId });

export const useAttendanceReport = (courseId: string) =>
  useQuery({ queryKey: ['attendance', 'report', courseId], queryFn: () => attendanceApi.report(courseId), enabled: !!courseId });

export const useDefaulters = (threshold?: number) =>
  useQuery({ queryKey: ['attendance', 'defaulters', threshold], queryFn: () => attendanceApi.defaulters(threshold) });

export function useMarkAttendance() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: MarkAttendanceRequest) => attendanceApi.mark(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['attendance'] }),
  });
}

export function useManualAttendance() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: ManualAttendanceRequest) => attendanceApi.markManual(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['attendance'] }),
  });
}

// ===== Hostel =====
export const useHostels = () =>
  useQuery({ queryKey: ['hostels'], queryFn: hostelApi.list });

export function useCreateHostel() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateHostelRequest) => hostelApi.create(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['hostels'] }),
  });
}

export const useHostelLogs = (hostelId: string, date?: string) =>
  useQuery({ queryKey: ['hostel-logs', hostelId, date], queryFn: () => hostelApi.logs(hostelId, date), enabled: !!hostelId });

export function useLogHostelEntry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateHostelLogRequest) => hostelApi.logEntry(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['hostel-logs'] }),
  });
}

// ===== Mess =====
export const useMessLogs = (date?: string) =>
  useQuery({ queryKey: ['mess-logs', date], queryFn: () => messApi.logs(date) });

export const useMessAnalytics = (from?: string, to?: string) =>
  useQuery({ queryKey: ['mess-analytics', from, to], queryFn: () => messApi.analytics(from, to) });

export function useLogMess() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateMessLogRequest) => messApi.log(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['mess-logs'] }),
  });
}

// ===== Outing =====
export const useOutingRequests = (status?: number) =>
  useQuery({ queryKey: ['outings', status], queryFn: () => outingApi.list(status) });

export const useMyOutings = () =>
  useQuery({ queryKey: ['outings', 'my'], queryFn: outingApi.myRequests });

export function useCreateOuting() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateOutingRequestDto) => outingApi.create(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['outings'] }),
  });
}

export function useApproveOuting() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, remarks }: { id: string; remarks?: string }) => outingApi.approve(id, remarks),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['outings'] }),
  });
}

export function useRejectOuting() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, remarks }: { id: string; remarks: string }) => outingApi.reject(id, remarks),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['outings'] }),
  });
}

// ===== Notifications =====
export const useNotifications = () =>
  useQuery({ queryKey: ['notifications'], queryFn: notificationsApi.my });

export const useUnreadCount = () =>
  useQuery({ queryKey: ['notifications', 'unread'], queryFn: notificationsApi.unreadCount, refetchInterval: 30000 });

export function useMarkNotificationRead() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => notificationsApi.markRead(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['notifications'] }),
  });
}

export function useMarkAllRead() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: () => notificationsApi.markAllRead(),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['notifications'] }),
  });
}

// ===== Analytics =====
export const useDashboardAnalytics = () =>
  useQuery({ queryKey: ['analytics', 'dashboard'], queryFn: analyticsApi.dashboard });

export const usePredictions = () =>
  useQuery({ queryKey: ['analytics', 'predictions'], queryFn: analyticsApi.predictions });

// ===== Gamification =====
export const useGamificationDashboard = () =>
  useQuery({ queryKey: ['gamification', 'dashboard'], queryFn: gamificationApi.dashboard });

export const useStreak = () =>
  useQuery({ queryKey: ['gamification', 'streak'], queryFn: gamificationApi.streak });

export const useBadges = () =>
  useQuery({ queryKey: ['gamification', 'badges'], queryFn: gamificationApi.badges });

export const useLeaderboard = (period = 'monthly', dept?: string) =>
  useQuery({ queryKey: ['gamification', 'leaderboard', period, dept], queryFn: () => gamificationApi.leaderboard(period, dept) });

// ===== Leave Management =====
export const useLeaves = (page = 1, size = 20, status?: number) =>
  useQuery({ queryKey: ['leaves', page, size, status], queryFn: () => leaveApi.list(page, size, status) });

export const useMyLeaves = (page = 1, size = 20) =>
  useQuery({ queryKey: ['leaves', 'my', page, size], queryFn: () => leaveApi.my(page, size) });

export function useCreateLeave() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateLeaveRequest) => leaveApi.create(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['leaves'] }),
  });
}

export function useApproveLeave() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, req }: { id: string; req?: ApproveLeaveRequest }) => leaveApi.approve(id, req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['leaves'] }),
  });
}

export function useRejectLeave() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, req }: { id: string; req: RejectLeaveRequest }) => leaveApi.reject(id, req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['leaves'] }),
  });
}

export function useCancelLeave() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => leaveApi.cancel(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['leaves'] }),
  });
}

// ===== Emergency SOS =====
export const useActiveEmergencies = (enabled = true) =>
  useQuery({ queryKey: ['emergency', 'active'], queryFn: emergencyApi.active, refetchInterval: 10000, enabled });

export const useEmergencyHistory = (page = 1, size = 20) =>
  useQuery({ queryKey: ['emergency', 'history', page, size], queryFn: () => emergencyApi.history(page, size) });

export function useCreateSOS() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateSOSRequest) => emergencyApi.create(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['emergency'] }),
  });
}

export function useAcknowledgeSOS() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => emergencyApi.acknowledge(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['emergency'] }),
  });
}

export function useResolveSOS() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, req }: { id: string; req: ResolveSOSRequest }) => emergencyApi.resolve(id, req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['emergency'] }),
  });
}

// ===== Offline Sync =====
export function useSyncBatch() {
  return useMutation({ mutationFn: (req: SyncBatchRequest) => offlineSyncApi.sync(req) });
}

export const usePendingSync = () =>
  useQuery({ queryKey: ['sync', 'pending'], queryFn: offlineSyncApi.pending });

// ===== Fraud Detection =====
export const useFraudLogs = (page = 1, size = 20, resolved?: boolean) =>
  useQuery({ queryKey: ['fraud', 'logs', page, size, resolved], queryFn: () => fraudApi.logs(page, size, resolved) });

export const useDeviceBindings = (userId: string) =>
  useQuery({ queryKey: ['fraud', 'devices', userId], queryFn: () => fraudApi.devices(userId), enabled: !!userId });

export function useResolveFraud() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, req }: { id: string; req: ResolveFraudRequest }) => fraudApi.resolve(id, req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['fraud'] }),
  });
}

export function useBindDevice() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, req }: { userId: string; req: BindDeviceRequest }) => fraudApi.bindDevice(userId, req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['fraud', 'devices'] }),
  });
}

// ===== Admin Config =====
export const useSystemConfigs = (category?: number) =>
  useQuery({ queryKey: ['configs', category], queryFn: () => adminConfigApi.list(category) });

export function useCreateConfig() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateConfigRequest) => adminConfigApi.create(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['configs'] }),
  });
}

export function useUpdateConfig() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ key, req }: { key: string; req: UpdateConfigRequest }) => adminConfigApi.update(key, req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['configs'] }),
  });
}

export function useDeleteConfig() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (key: string) => adminConfigApi.delete(key),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['configs'] }),
  });
}

// ===== Bulk Import =====
export function useImportStudents() {
  return useMutation({ mutationFn: (file: File) => bulkImportApi.importStudents(file) });
}

export function useImportCourses() {
  return useMutation({ mutationFn: (file: File) => bulkImportApi.importCourses(file) });
}

// ===== Advanced Analytics =====
export const useAdvancedDashboard = () =>
  useQuery({ queryKey: ['analytics', 'advanced', 'dashboard'], queryFn: advancedAnalyticsApi.dashboard });

export const useHeatmap = (courseId?: string) =>
  useQuery({ queryKey: ['analytics', 'heatmap', courseId], queryFn: () => advancedAnalyticsApi.heatmap(courseId) });

export const useDropoutRisk = (threshold = 0.6) =>
  useQuery({ queryKey: ['analytics', 'dropout', threshold], queryFn: () => advancedAnalyticsApi.dropoutRisk(threshold) });

export const useFacultyStrictness = () =>
  useQuery({ queryKey: ['analytics', 'strictness'], queryFn: advancedAnalyticsApi.facultyStrictness });

export const useCourseAnalytics = () =>
  useQuery({ queryKey: ['analytics', 'courses'], queryFn: advancedAnalyticsApi.courseAnalytics });

export const useCurfewLogs = (page = 1, size = 20) =>
  useQuery({ queryKey: ['curfew', 'logs', page, size], queryFn: () => advancedAnalyticsApi.curfewLogs(page, size) });

export const useCurfewConfig = () =>
  useQuery({ queryKey: ['curfew', 'config'], queryFn: advancedAnalyticsApi.curfewConfig });
