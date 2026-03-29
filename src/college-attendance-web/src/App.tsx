import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { UserRole } from './types';
import ProtectedRoute from './components/ProtectedRoute';
import AppLayout from './components/AppLayout';
import LoginPage from './pages/auth/LoginPage';
import UnauthorizedPage from './pages/auth/UnauthorizedPage';
import DashboardPage from './pages/dashboard/DashboardPage';
import UsersPage from './pages/admin/UsersPage';
import DepartmentsPage from './pages/admin/DepartmentsPage';
import CoursesPage from './pages/admin/CoursesPage';
import AnalyticsPage from './pages/admin/AnalyticsPage';
import SessionsPage from './pages/faculty/SessionsPage';
import QRGeneratePage from './pages/faculty/QRGeneratePage';
import AttendancePage from './pages/faculty/AttendancePage';
import MyAttendancePage from './pages/student/MyAttendancePage';
import MyCoursesPage from './pages/student/MyCoursesPage';
import OutingRequestPage from './pages/student/OutingRequestPage';
import HostelsPage from './pages/hostel/HostelsPage';
import MessPage from './pages/hostel/MessPage';
import OutingsPage from './pages/hostel/OutingsPage';
import NotificationsPage from './pages/NotificationsPage';
import GamificationPage from './pages/student/GamificationPage';
import LeavePage from './pages/student/LeavePage';
import EmergencySOSPage from './pages/student/EmergencySOSPage';
import AdvancedAnalyticsPage from './pages/admin/AdvancedAnalyticsPage';
import SystemConfigPage from './pages/admin/SystemConfigPage';
import BulkImportPage from './pages/admin/BulkImportPage';
import FraudLogsPage from './pages/admin/FraudLogsPage';
import CurfewPage from './pages/hostel/CurfewPage';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/unauthorized" element={<UnauthorizedPage />} />

        {/* Authenticated routes */}
        <Route element={<ProtectedRoute><AppLayout /></ProtectedRoute>}>
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/notifications" element={<NotificationsPage />} />

          {/* Admin */}
          <Route path="/users" element={<ProtectedRoute roles={[UserRole.Admin]}><UsersPage /></ProtectedRoute>} />
          <Route path="/departments" element={<ProtectedRoute roles={[UserRole.Admin]}><DepartmentsPage /></ProtectedRoute>} />
          <Route path="/courses" element={<ProtectedRoute roles={[UserRole.Admin]}><CoursesPage /></ProtectedRoute>} />
          <Route path="/analytics" element={<ProtectedRoute roles={[UserRole.Admin]}><AnalyticsPage /></ProtectedRoute>} />

          {/* Faculty */}
          <Route path="/sessions" element={<ProtectedRoute roles={[UserRole.Faculty, UserRole.Admin]}><SessionsPage /></ProtectedRoute>} />
          <Route path="/qr-generate" element={<ProtectedRoute roles={[UserRole.Faculty, UserRole.Admin]}><QRGeneratePage /></ProtectedRoute>} />
          <Route path="/attendance" element={<ProtectedRoute roles={[UserRole.Faculty, UserRole.Admin]}><AttendancePage /></ProtectedRoute>} />

          {/* Student */}
          <Route path="/my-attendance" element={<ProtectedRoute roles={[UserRole.Student]}><MyAttendancePage /></ProtectedRoute>} />
          <Route path="/my-courses" element={<ProtectedRoute roles={[UserRole.Faculty, UserRole.Student]}><MyCoursesPage /></ProtectedRoute>} />
          <Route path="/outing/new" element={<ProtectedRoute roles={[UserRole.Student]}><OutingRequestPage /></ProtectedRoute>} />

          {/* Hostel */}
          <Route path="/hostels" element={<ProtectedRoute roles={[UserRole.Warden, UserRole.Admin]}><HostelsPage /></ProtectedRoute>} />
          <Route path="/mess" element={<ProtectedRoute roles={[UserRole.Warden, UserRole.Admin]}><MessPage /></ProtectedRoute>} />
          <Route path="/outings" element={<ProtectedRoute roles={[UserRole.Warden, UserRole.Security]}><OutingsPage /></ProtectedRoute>} />
          <Route path="/curfew" element={<ProtectedRoute roles={[UserRole.Warden, UserRole.Admin]}><CurfewPage /></ProtectedRoute>} />

          {/* Campus Platform - Student */}
          <Route path="/gamification" element={<ProtectedRoute roles={[UserRole.Student]}><GamificationPage /></ProtectedRoute>} />
          <Route path="/leave" element={<ProtectedRoute roles={[UserRole.Student]}><LeavePage /></ProtectedRoute>} />
          <Route path="/emergency" element={<ProtectedRoute roles={[UserRole.Student, UserRole.Warden, UserRole.Security, UserRole.Admin]}><EmergencySOSPage /></ProtectedRoute>} />

          {/* Campus Platform - Admin */}
          <Route path="/advanced-analytics" element={<ProtectedRoute roles={[UserRole.Admin]}><AdvancedAnalyticsPage /></ProtectedRoute>} />
          <Route path="/admin/config" element={<ProtectedRoute roles={[UserRole.Admin]}><SystemConfigPage /></ProtectedRoute>} />
          <Route path="/admin/import" element={<ProtectedRoute roles={[UserRole.Admin]}><BulkImportPage /></ProtectedRoute>} />
          <Route path="/fraud" element={<ProtectedRoute roles={[UserRole.Admin]}><FraudLogsPage /></ProtectedRoute>} />
        </Route>

        {/* Default redirect */}
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
