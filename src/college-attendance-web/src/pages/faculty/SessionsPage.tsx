import { useState } from 'react';
import { useTodaySessions, useSessions, useCreateSession, useStartSession, useEndSession } from '../../hooks/useApi';
import { useCourses } from '../../hooks/useApi';
import { SessionStatus, type CreateClassSessionRequest } from '../../types';
import { PageHeader, LoadingSpinner, EmptyState, Badge } from '../../components/ui';
import { Plus, X, Play, Square, Calendar } from 'lucide-react';
import { format } from 'date-fns';

export default function SessionsPage() {
  const [showForm, setShowForm] = useState(false);
  const [tab, setTab] = useState<'today' | 'all'>('today');
  const { data: todaySessions, isLoading: l1 } = useTodaySessions();
  const { data: allSessions, isLoading: l2 } = useSessions();
  const startSession = useStartSession();
  const endSession = useEndSession();

  const sessions = tab === 'today' ? todaySessions : allSessions;
  const loading = tab === 'today' ? l1 : l2;

  const statusVariant = (s: SessionStatus) => {
    switch (s) {
      case SessionStatus.Active: return 'green';
      case SessionStatus.Scheduled: return 'blue';
      case SessionStatus.Completed: return 'gray';
      case SessionStatus.Cancelled: return 'red';
      default: return 'gray';
    }
  };

  if (loading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="Class Sessions">
        <button onClick={() => setShowForm(true)} className="inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700">
          <Plus className="h-4 w-4" /> New Session
        </button>
      </PageHeader>

      <div className="mb-6 flex gap-2">
        <button onClick={() => setTab('today')} className={`rounded-lg px-4 py-2 text-sm font-medium ${tab === 'today' ? 'bg-indigo-600 text-white' : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'}`}>
          Today
        </button>
        <button onClick={() => setTab('all')} className={`rounded-lg px-4 py-2 text-sm font-medium ${tab === 'all' ? 'bg-indigo-600 text-white' : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'}`}>
          All Sessions
        </button>
      </div>

      {showForm && <CreateSessionForm onClose={() => setShowForm(false)} />}

      {!sessions || sessions.length === 0 ? (
        <EmptyState message={tab === 'today' ? 'No sessions scheduled for today' : 'No sessions found'} />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {sessions.map(s => (
            <div key={s.id} className="rounded-xl border border-gray-200 bg-white p-5">
              <div className="mb-3 flex items-center justify-between">
                <h3 className="font-semibold text-gray-900">{s.courseName}</h3>
                <Badge label={SessionStatus[s.status]} variant={statusVariant(s.status) as 'green' | 'red' | 'blue' | 'gray' | 'indigo' | 'yellow'} />
              </div>
              <div className="space-y-1 text-sm text-gray-500">
                <div className="flex items-center gap-2"><Calendar className="h-3.5 w-3.5" /> {format(new Date(s.scheduledDate), 'MMM d, yyyy')}</div>
                <p>Time: {s.startTime} – {s.endTime}</p>
                <p>Room: {s.room}</p>
                <p>Present: {s.presentCount} / {s.totalStudents}</p>
              </div>
              <div className="mt-4 flex gap-2">
                {s.status === SessionStatus.Scheduled && (
                  <button onClick={() => startSession.mutate(s.id)}
                    disabled={startSession.isPending}
                    className="inline-flex items-center gap-1 rounded-lg bg-green-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-green-700 disabled:opacity-50">
                    <Play className="h-3.5 w-3.5" /> Start
                  </button>
                )}
                {s.status === SessionStatus.Active && (
                  <button onClick={() => endSession.mutate(s.id)}
                    disabled={endSession.isPending}
                    className="inline-flex items-center gap-1 rounded-lg bg-red-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-red-700 disabled:opacity-50">
                    <Square className="h-3.5 w-3.5" /> End
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function CreateSessionForm({ onClose }: { onClose: () => void }) {
  const { data: courses } = useCourses();
  const createSession = useCreateSession();
  const [form, setForm] = useState<CreateClassSessionRequest>({
    title: '', courseId: '', scheduledDate: format(new Date(), 'yyyy-MM-dd'), startTime: '09:00', endTime: '10:00', room: '', geofenceRadiusMeters: 100,
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await createSession.mutateAsync(form);
    onClose();
  };

  return (
    <div className="mb-6 rounded-xl border border-gray-200 bg-white p-6">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-900">New Session</h3>
        <button onClick={onClose} aria-label="Close"><X className="h-5 w-5 text-gray-400" /></button>
      </div>
      <form onSubmit={handleSubmit} className="grid gap-4 sm:grid-cols-2">
        <input type="text" placeholder="Session Title" required value={form.title}
          onChange={e => setForm((f: CreateClassSessionRequest) => ({ ...f, title: e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <select required value={form.courseId} onChange={e => setForm((f: CreateClassSessionRequest) => ({ ...f, courseId: e.target.value }))} aria-label="Course"
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500">
          <option value="">Select Course</option>
          {courses?.map(c => <option key={c.id} value={c.id}>{c.name} ({c.code})</option>)}
        </select>
        <input type="text" placeholder="Room (e.g. A-202)" value={form.room ?? ''}
          onChange={e => setForm((f: CreateClassSessionRequest) => ({ ...f, room: e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <input type="date" required value={form.scheduledDate} aria-label="Scheduled date"
          onChange={e => setForm((f: CreateClassSessionRequest) => ({ ...f, scheduledDate: e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <div className="flex gap-2">
          <input type="time" required value={form.startTime} aria-label="Start time" onChange={e => setForm((f: CreateClassSessionRequest) => ({ ...f, startTime: e.target.value }))}
            className="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
          <input type="time" required value={form.endTime} aria-label="End time" onChange={e => setForm((f: CreateClassSessionRequest) => ({ ...f, endTime: e.target.value }))}
            className="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        </div>
        <div className="sm:col-span-2 flex justify-end gap-2">
          <button type="button" onClick={onClose} className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">Cancel</button>
          <button type="submit" disabled={createSession.isPending}
            className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-50">
            {createSession.isPending ? 'Creating...' : 'Create'}
          </button>
        </div>
      </form>
    </div>
  );
}
