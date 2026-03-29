import { NavLink } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { UserRole } from '../types';
import {
  LayoutDashboard, Users, BookOpen, Building2, CalendarDays,
  QrCode, ClipboardList, Hotel, UtensilsCrossed, DoorOpen,
  Bell, BarChart3, X, Trophy, FileText, Siren, LineChart,
  Settings, Upload, ShieldAlert, Moon,
} from 'lucide-react';

const link = 'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors';
const active = 'bg-indigo-50 text-indigo-700';
const inactive = 'text-gray-600 hover:bg-gray-50 hover:text-gray-900';

interface Props { open: boolean; onClose: () => void }

export default function Sidebar({ open, onClose }: Props) {
  const { user, hasRole } = useAuth();

  const nav = (to: string, label: string, Icon: React.ElementType, roles?: UserRole[]) => {
    if (roles && !hasRole(...roles)) return null;
    return (
      <NavLink to={to} onClick={onClose} className={({ isActive }) => `${link} ${isActive ? active : inactive}`}>
        <Icon className="h-5 w-5" />{label}
      </NavLink>
    );
  };

  return (
    <>
      {open && <div className="fixed inset-0 z-40 bg-black/20 lg:hidden" onClick={onClose} />}
      <aside className={`fixed inset-y-0 left-0 z-50 flex w-64 flex-col border-r border-gray-200 bg-white transition-transform lg:static lg:translate-x-0 ${open ? 'translate-x-0' : '-translate-x-full'}`}>
        <div className="flex h-16 items-center justify-between border-b border-gray-200 px-4">
          <span className="text-lg font-bold text-indigo-700">AttendEase</span>
          <button onClick={onClose} className="lg:hidden" aria-label="Close menu"><X className="h-5 w-5" /></button>
        </div>
        <nav className="flex-1 space-y-1 overflow-y-auto px-3 py-4">
          {nav('/dashboard', 'Dashboard', LayoutDashboard)}

          {hasRole(UserRole.Admin) && (
            <div className="pt-4">
              <p className="px-3 text-xs font-semibold uppercase tracking-wider text-gray-400">Admin</p>
              {nav('/users', 'Users', Users, [UserRole.Admin])}
              {nav('/departments', 'Departments', Building2, [UserRole.Admin])}
              {nav('/courses', 'Courses', BookOpen, [UserRole.Admin])}
              {nav('/analytics', 'Analytics', BarChart3, [UserRole.Admin])}
              {nav('/advanced-analytics', 'Advanced Analytics', LineChart, [UserRole.Admin])}
              {nav('/fraud', 'Fraud Logs', ShieldAlert, [UserRole.Admin])}
              {nav('/admin/config', 'Configuration', Settings, [UserRole.Admin])}
              {nav('/admin/import', 'Bulk Import', Upload, [UserRole.Admin])}
            </div>
          )}

          {hasRole(UserRole.Faculty) && (
            <div className="pt-4">
              <p className="px-3 text-xs font-semibold uppercase tracking-wider text-gray-400">Faculty</p>
              {nav('/my-courses', 'My Courses', BookOpen, [UserRole.Faculty])}
              {nav('/sessions', 'Sessions', CalendarDays, [UserRole.Faculty])}
              {nav('/qr-generate', 'Generate QR', QrCode, [UserRole.Faculty])}
              {nav('/attendance', 'Attendance', ClipboardList, [UserRole.Faculty])}
            </div>
          )}

          {hasRole(UserRole.Student) && (
            <div className="pt-4">
              <p className="px-3 text-xs font-semibold uppercase tracking-wider text-gray-400">Student</p>
              {nav('/my-attendance', 'My Attendance', ClipboardList, [UserRole.Student])}
              {nav('/my-courses', 'My Courses', BookOpen, [UserRole.Student])}
              {nav('/outing/new', 'Request Outing', DoorOpen, [UserRole.Student])}
              {nav('/gamification', 'Gamification', Trophy, [UserRole.Student])}
              {nav('/leave', 'Leave Requests', FileText, [UserRole.Student])}
            </div>
          )}

          {hasRole(UserRole.Warden, UserRole.Security) && (
            <div className="pt-4">
              <p className="px-3 text-xs font-semibold uppercase tracking-wider text-gray-400">Hostel</p>
              {nav('/hostels', 'Hostels', Hotel, [UserRole.Warden, UserRole.Admin])}
              {nav('/mess', 'Mess', UtensilsCrossed, [UserRole.Warden, UserRole.Admin])}
              {nav('/outings', 'Outing Requests', DoorOpen, [UserRole.Warden, UserRole.Security])}
              {nav('/curfew', 'Curfew', Moon, [UserRole.Warden, UserRole.Admin])}
            </div>
          )}

          <div className="pt-4">
            <p className="px-3 text-xs font-semibold uppercase tracking-wider text-gray-400">Safety</p>
            {nav('/emergency', 'Emergency SOS', Siren, [UserRole.Student, UserRole.Warden, UserRole.Security, UserRole.Admin])}
          </div>

          <div className="pt-4">
            <p className="px-3 text-xs font-semibold uppercase tracking-wider text-gray-400">General</p>
            {nav('/notifications', 'Notifications', Bell)}
          </div>
        </nav>
        <div className="border-t border-gray-200 p-4">
          <div className="flex items-center gap-3">
            <div className="flex h-9 w-9 items-center justify-center rounded-full bg-indigo-100 text-sm font-semibold text-indigo-700">
              {user?.fullName?.charAt(0) ?? '?'}
            </div>
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-medium text-gray-900">{user?.fullName}</p>
              <p className="truncate text-xs text-gray-500">{UserRole[user?.role ?? 0]}</p>
            </div>
          </div>
        </div>
      </aside>
    </>
  );
}
