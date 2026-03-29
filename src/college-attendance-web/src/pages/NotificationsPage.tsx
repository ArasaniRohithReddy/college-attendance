import { useNotifications, useMarkNotificationRead } from '../hooks/useApi';
import { NotificationType, type NotificationDto } from '../types';
import { PageHeader, LoadingSpinner, EmptyState } from '../components/ui';
import { AlertTriangle, Info, CheckCircle, Clock } from 'lucide-react';
import { format } from 'date-fns';

export default function NotificationsPage() {
  const { data: notifications, isLoading } = useNotifications();
  const markRead = useMarkNotificationRead();

  if (isLoading) return <LoadingSpinner />;
  if (!notifications || notifications.length === 0) return <><PageHeader title="Notifications" /><EmptyState message="No notifications" /></>;

  const icon = (t: NotificationType) => {
    switch (t) {
      case NotificationType.LowAttendance:
      case NotificationType.FraudAlert: return <AlertTriangle className="h-5 w-5 text-red-500" />;
      case NotificationType.OutingApproval: return <CheckCircle className="h-5 w-5 text-green-500" />;
      case NotificationType.ClassReminder: return <Clock className="h-5 w-5 text-blue-500" />;
      default: return <Info className="h-5 w-5 text-gray-400" />;
    }
  };

  return (
    <div>
      <PageHeader title="Notifications" />
      <div className="space-y-2">
        {notifications.map((n: NotificationDto) => (
          <div
            key={n.id}
            onClick={() => !n.isRead && markRead.mutate(n.id)}
            className={`flex cursor-pointer items-start gap-4 rounded-xl border p-4 transition ${
              n.isRead ? 'border-gray-200 bg-white' : 'border-indigo-200 bg-indigo-50'
            } hover:shadow-sm`}
          >
            <div className="mt-0.5">{icon(n.type)}</div>
            <div className="flex-1 min-w-0">
              <div className="flex items-center justify-between">
                <h4 className={`text-sm font-medium ${n.isRead ? 'text-gray-700' : 'text-gray-900'}`}>{n.title}</h4>
                <span className="text-xs text-gray-400 whitespace-nowrap ml-2">{format(new Date(n.createdAt), 'MMM d, HH:mm')}</span>
              </div>
              <p className="mt-0.5 text-sm text-gray-500">{n.message}</p>
            </div>
            {!n.isRead && (
              <div className="mt-1.5 flex-shrink-0">
                <div className="h-2.5 w-2.5 rounded-full bg-indigo-500" />
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
