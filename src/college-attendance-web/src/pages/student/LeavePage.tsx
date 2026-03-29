import { useState } from 'react';
import { useMyLeaves, useCreateLeave, useCancelLeave } from '../../hooks/useApi';
import { PageHeader, LoadingSpinner } from '../../components/ui';
import { CalendarDays, Plus, X } from 'lucide-react';
import { LeaveType, LeaveStatus, type CreateLeaveRequest } from '../../types';

const leaveTypeLabels: Record<LeaveType, string> = {
  [LeaveType.Sick]: 'Sick',
  [LeaveType.Personal]: 'Personal',
  [LeaveType.Academic]: 'Academic',
  [LeaveType.Emergency]: 'Emergency',
  [LeaveType.Other]: 'Other',
};

const statusColors: Record<LeaveStatus, string> = {
  [LeaveStatus.Pending]: 'bg-yellow-100 text-yellow-700',
  [LeaveStatus.Approved]: 'bg-green-100 text-green-700',
  [LeaveStatus.Rejected]: 'bg-red-100 text-red-700',
  [LeaveStatus.Cancelled]: 'bg-gray-100 text-gray-600',
  [LeaveStatus.Expired]: 'bg-gray-100 text-gray-400',
};

export default function LeavePage() {
  const [page, setPage] = useState(1);
  const { data, isLoading } = useMyLeaves(page);
  const createMut = useCreateLeave();
  const cancelMut = useCancelLeave();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState<CreateLeaveRequest>({
    leaveType: LeaveType.Sick,
    startDate: '',
    endDate: '',
    reason: '',
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await createMut.mutateAsync(form);
    setShowForm(false);
    setForm({ leaveType: LeaveType.Sick, startDate: '', endDate: '', reason: '' });
  };

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="Leave Requests">
        <button onClick={() => setShowForm(!showForm)} className="inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700">
          {showForm ? <X className="h-4 w-4" /> : <Plus className="h-4 w-4" />}
          {showForm ? 'Cancel' : 'New Request'}
        </button>
      </PageHeader>

      {showForm && (
        <form onSubmit={handleSubmit} className="mb-6 rounded-xl border border-gray-200 bg-white p-6">
          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Leave Type</label>
              <select value={form.leaveType} onChange={e => setForm({ ...form, leaveType: Number(e.target.value) })} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm">
                {Object.entries(leaveTypeLabels).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
              </select>
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Start Date</label>
              <input type="date" value={form.startDate} onChange={e => setForm({ ...form, startDate: e.target.value })} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" required />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">End Date</label>
              <input type="date" value={form.endDate} onChange={e => setForm({ ...form, endDate: e.target.value })} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" required />
            </div>
            <div className="sm:col-span-2">
              <label className="mb-1 block text-sm font-medium text-gray-700">Reason</label>
              <textarea value={form.reason} onChange={e => setForm({ ...form, reason: e.target.value })} rows={3} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" required />
            </div>
          </div>
          <div className="mt-4 flex justify-end">
            <button type="submit" disabled={createMut.isPending} className="rounded-lg bg-indigo-600 px-6 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-50">
              {createMut.isPending ? 'Submitting...' : 'Submit Request'}
            </button>
          </div>
        </form>
      )}

      <div className="rounded-xl border border-gray-200 bg-white">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
              <tr>
                <th className="px-4 py-3">Type</th>
                <th className="px-4 py-3">Start</th>
                <th className="px-4 py-3">End</th>
                <th className="px-4 py-3">Reason</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {(data?.items ?? []).map(lr => (
                <tr key={lr.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-900">
                    <CalendarDays className="mr-1 inline h-4 w-4 text-gray-400" />
                    {leaveTypeLabels[lr.leaveType]}
                  </td>
                  <td className="px-4 py-3 text-gray-500">{new Date(lr.startDate).toLocaleDateString()}</td>
                  <td className="px-4 py-3 text-gray-500">{new Date(lr.endDate).toLocaleDateString()}</td>
                  <td className="max-w-xs truncate px-4 py-3 text-gray-500">{lr.reason}</td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${statusColors[lr.status]}`}>
                      {LeaveStatus[lr.status]}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    {lr.status === LeaveStatus.Pending && (
                      <button onClick={() => cancelMut.mutate(lr.id)} className="text-sm text-red-600 hover:text-red-800">Cancel</button>
                    )}
                  </td>
                </tr>
              ))}
              {(!data || data.items.length === 0) && (
                <tr><td colSpan={6} className="px-4 py-8 text-center text-gray-400">No leave requests found</td></tr>
              )}
            </tbody>
          </table>
        </div>
        {data && data.totalPages > 1 && (
          <div className="flex items-center justify-between border-t border-gray-200 px-4 py-3">
            <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={!data.hasPrevious} className="rounded px-3 py-1 text-sm text-gray-600 hover:bg-gray-100 disabled:opacity-50">Previous</button>
            <span className="text-sm text-gray-500">Page {data.page} of {data.totalPages}</span>
            <button onClick={() => setPage(p => p + 1)} disabled={!data.hasNext} className="rounded px-3 py-1 text-sm text-gray-600 hover:bg-gray-100 disabled:opacity-50">Next</button>
          </div>
        )}
      </div>
    </div>
  );
}
