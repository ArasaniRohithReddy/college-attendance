import { useAuth } from '../contexts/AuthContext';
import { useUnreadCount } from '../hooks/useApi';
import { Bell, Menu, LogOut } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

interface Props { onMenuClick: () => void }

export default function Header({ onMenuClick }: Props) {
  const { user, logout } = useAuth();
  const { data: unread } = useUnreadCount();
  const navigate = useNavigate();

  return (
    <header className="flex h-16 items-center justify-between border-b border-gray-200 bg-white px-4 lg:px-6">
      <button onClick={onMenuClick} className="lg:hidden" aria-label="Open menu">
        <Menu className="h-6 w-6 text-gray-600" />
      </button>
      <h1 className="hidden text-lg font-semibold text-gray-900 lg:block">
        College Attendance System
      </h1>
      <div className="flex items-center gap-4">
        <button onClick={() => navigate('/notifications')} className="relative rounded-lg p-2 text-gray-500 hover:bg-gray-100" aria-label="Notifications">
          <Bell className="h-5 w-5" />
          {!!unread && unread > 0 && (
            <span className="absolute -right-0.5 -top-0.5 flex h-5 w-5 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white">
              {unread > 99 ? '99+' : unread}
            </span>
          )}
        </button>
        <div className="hidden items-center gap-2 sm:flex">
          {user?.profileImageUrl ? (
            <img src={user.profileImageUrl} alt="" className="h-8 w-8 rounded-full" />
          ) : (
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-indigo-100 text-sm font-semibold text-indigo-700">
              {user?.fullName?.charAt(0)}
            </div>
          )}
          <span className="text-sm font-medium text-gray-700">{user?.fullName}</span>
        </div>
        <button onClick={logout} className="rounded-lg p-2 text-gray-500 hover:bg-gray-100" title="Logout">
          <LogOut className="h-5 w-5" />
        </button>
      </div>
    </header>
  );
}
