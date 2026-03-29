import { useAttendanceByStudent } from '../../hooks/useApi';
import { useAuth } from '../../contexts/AuthContext';
import { AttendanceStatus } from '../../types';
import { PageHeader, LoadingSpinner, EmptyState, Badge } from '../../components/ui';
import { format } from 'date-fns';

export default function MyAttendancePage() {
  const { user } = useAuth();
  const { data: records, isLoading } = useAttendanceByStudent(user?.id ?? '');

  if (isLoading) return <LoadingSpinner />;
  if (!records || records.length === 0) return <><PageHeader title="My Attendance" /><EmptyState message="No attendance records found" /></>;

  const grouped = records.reduce<Record<string, typeof records>>((acc, r) => {
    const key = r.sessionTitle ?? 'Unknown';
    (acc[key] ??= []).push(r);
    return acc;
  }, {});

  return (
    <div>
      <PageHeader title="My Attendance" />

      {Object.entries(grouped).map(([course, courseRecords]) => {
        const total = courseRecords.length;
        const present = courseRecords.filter(r => r.status === AttendanceStatus.Present || r.status === AttendanceStatus.Late).length;
        const pct = total > 0 ? ((present / total) * 100).toFixed(1) : '0';

        return (
          <div key={course} className="mb-6 rounded-xl border border-gray-200 bg-white">
            <div className="flex items-center justify-between border-b border-gray-200 px-6 py-4">
              <h3 className="font-semibold text-gray-900">{course}</h3>
              <div className="flex items-center gap-3 text-sm">
                <span className="text-gray-500">{present}/{total} classes</span>
                <span className={`font-bold ${+pct >= 75 ? 'text-green-600' : +pct >= 60 ? 'text-yellow-600' : 'text-red-600'}`}>{pct}%</span>
              </div>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead className="bg-gray-50 text-xs uppercase text-gray-500">
                  <tr>
                    <th className="px-4 py-3">Date</th>
                    <th className="px-4 py-3">Session</th>
                    <th className="px-4 py-3">Status</th>
                    <th className="px-4 py-3">Marked At</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {courseRecords.map(r => (
                    <tr key={r.id} className="hover:bg-gray-50">
                      <td className="px-4 py-3 text-gray-500">{r.markedAt ? format(new Date(r.markedAt), 'MMM d, yyyy') : '—'}</td>
                      <td className="px-4 py-3 text-gray-500">{r.sessionTitle ?? '—'}</td>
                      <td className="px-4 py-3">
                        <Badge
                          label={AttendanceStatus[r.status]}
                          variant={r.status === AttendanceStatus.Present ? 'green' : r.status === AttendanceStatus.Late ? 'yellow' : 'red'}
                        />
                      </td>
                      <td className="px-4 py-3 text-gray-500">{r.markedAt ? format(new Date(r.markedAt), 'HH:mm') : '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        );
      })}
    </div>
  );
}
