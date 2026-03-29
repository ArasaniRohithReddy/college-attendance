import { useState } from 'react';
import { useSessions, useAttendanceBySession, useManualAttendance } from '../../hooks/useApi';
import { AttendanceStatus, SessionStatus } from '../../types';
import { PageHeader, LoadingSpinner, EmptyState, Badge } from '../../components/ui';
import { CheckCircle, XCircle, Clock } from 'lucide-react';
import { format } from 'date-fns';

export default function AttendancePage() {
  const { data: sessions, isLoading } = useSessions();
  const [selectedSessionId, setSelectedSessionId] = useState<string>('');

  if (isLoading) return <LoadingSpinner />;

  const recentSessions = sessions?.filter(s => s.status === SessionStatus.Active || s.status === SessionStatus.Completed).slice(0, 50) ?? [];

  return (
    <div>
      <PageHeader title="Attendance Management" />

      <div className="mb-6 rounded-xl border border-gray-200 bg-white p-6">
        <label htmlFor="attendance-session-select" className="mb-2 block text-sm font-medium text-gray-700">Select Session</label>
        <select id="attendance-session-select" value={selectedSessionId} onChange={e => setSelectedSessionId(e.target.value)}
          className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 sm:w-auto sm:min-w-[400px]">
          <option value="">Choose a session</option>
          {recentSessions.map(s => (
            <option key={s.id} value={s.id}>
              {s.courseName} — {format(new Date(s.scheduledDate), 'MMM d')} ({s.startTime}–{s.endTime}) [{SessionStatus[s.status]}]
            </option>
          ))}
        </select>
      </div>

      {selectedSessionId ? (
        <AttendanceList sessionId={selectedSessionId} />
      ) : (
        <EmptyState message="Select a session to view and manage attendance" />
      )}
    </div>
  );
}

function AttendanceList({ sessionId }: { sessionId: string }) {
  const { data: records, isLoading } = useAttendanceBySession(sessionId);
  const manualAttendance = useManualAttendance();

  const handleMark = (studentId: string, status: AttendanceStatus) => {
    manualAttendance.mutate({
      classSessionId: sessionId,
      entries: [{ studentId, status }],
    });
  };

  if (isLoading) return <LoadingSpinner />;
  if (!records || records.length === 0) return <EmptyState message="No attendance records for this session" />;

  const statusIcon = (s: AttendanceStatus) => {
    switch (s) {
      case AttendanceStatus.Present: return <CheckCircle className="h-4 w-4 text-green-500" />;
      case AttendanceStatus.Absent: return <XCircle className="h-4 w-4 text-red-500" />;
      case AttendanceStatus.Late: return <Clock className="h-4 w-4 text-yellow-500" />;
      default: return null;
    }
  };

  const statusVariant = (s: AttendanceStatus): 'green' | 'red' | 'yellow' | 'gray' => {
    switch (s) {
      case AttendanceStatus.Present: return 'green';
      case AttendanceStatus.Absent: return 'red';
      case AttendanceStatus.Late: return 'yellow';
      default: return 'gray';
    }
  };

  const present = records.filter(r => r.status === AttendanceStatus.Present).length;
  const absent = records.filter(r => r.status === AttendanceStatus.Absent).length;
  const late = records.filter(r => r.status === AttendanceStatus.Late).length;

  return (
    <div>
      <div className="mb-4 flex gap-4 text-sm">
        <span className="text-green-600 font-medium">Present: {present}</span>
        <span className="text-red-600 font-medium">Absent: {absent}</span>
        <span className="text-yellow-600 font-medium">Late: {late}</span>
        <span className="text-gray-500">Total: {records.length}</span>
      </div>

      <div className="overflow-x-auto rounded-xl border border-gray-200 bg-white">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
            <tr>
              <th className="px-4 py-3">Student</th>
              <th className="px-4 py-3">Status</th>
              <th className="px-4 py-3">Marked At</th>
              <th className="px-4 py-3">Entry Type</th>
              <th className="px-4 py-3">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {records.map(r => (
              <tr key={r.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <div className="flex items-center gap-2">
                    {statusIcon(r.status)}
                    <span className="font-medium text-gray-900">{r.studentName}</span>
                  </div>
                </td>
                <td className="px-4 py-3"><Badge label={AttendanceStatus[r.status]} variant={statusVariant(r.status)} /></td>
                <td className="px-4 py-3 text-gray-500">{r.markedAt ? format(new Date(r.markedAt), 'HH:mm:ss') : '—'}</td>
                <td className="px-4 py-3 text-gray-500">{r.isManualEntry ? 'Manual' : 'QR Scan'}</td>
                <td className="px-4 py-3">
                  <div className="flex gap-1">
                    {r.status !== AttendanceStatus.Present && (
                      <button onClick={() => handleMark(r.studentId, AttendanceStatus.Present)}
                        className="rounded bg-green-100 px-2 py-1 text-xs font-medium text-green-700 hover:bg-green-200">P</button>
                    )}
                    {r.status !== AttendanceStatus.Absent && (
                      <button onClick={() => handleMark(r.studentId, AttendanceStatus.Absent)}
                        className="rounded bg-red-100 px-2 py-1 text-xs font-medium text-red-700 hover:bg-red-200">A</button>
                    )}
                    {r.status !== AttendanceStatus.Late && (
                      <button onClick={() => handleMark(r.studentId, AttendanceStatus.Late)}
                        className="rounded bg-yellow-100 px-2 py-1 text-xs font-medium text-yellow-700 hover:bg-yellow-200">L</button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
