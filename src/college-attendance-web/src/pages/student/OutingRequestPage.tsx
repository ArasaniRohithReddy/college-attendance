import { useState } from 'react';
import { useMyOutings, useCreateOuting } from '../../hooks/useApi';
import { OutingStatus, type CreateOutingRequestDto } from '../../types';
import { PageHeader, LoadingSpinner, EmptyState, Badge } from '../../components/ui';
import { Plus, X, MapPin } from 'lucide-react';
import { format } from 'date-fns';

export default function OutingRequestPage() {
  const [showForm, setShowForm] = useState(false);
  const { data: outings, isLoading } = useMyOutings();

  if (isLoading) return <LoadingSpinner />;

  const statusVariant = (s: OutingStatus): 'green' | 'red' | 'yellow' | 'blue' | 'gray' => {
    switch (s) {
      case OutingStatus.ApprovedByWarden:
      case OutingStatus.ApprovedBySecurity: return 'green';
      case OutingStatus.Rejected: return 'red';
      case OutingStatus.Pending: return 'yellow';
      case OutingStatus.CheckedOut: return 'blue';
      default: return 'gray';
    }
  };

  return (
    <div>
      <PageHeader title="Outing Requests">
        <button onClick={() => setShowForm(true)} className="inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700">
          <Plus className="h-4 w-4" /> New Request
        </button>
      </PageHeader>

      {showForm && <CreateOutingForm onClose={() => setShowForm(false)} />}

      {!outings || outings.length === 0 ? (
        <EmptyState message="No outing requests found" />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {outings.map(o => (
            <div key={o.id} className="rounded-xl border border-gray-200 bg-white p-5">
              <div className="mb-3 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <MapPin className="h-4 w-4 text-indigo-500" />
                  <span className="font-semibold text-gray-900">{o.destination}</span>
                </div>
                <Badge label={OutingStatus[o.status]} variant={statusVariant(o.status)} />
              </div>
              <div className="space-y-1 text-sm text-gray-500">
                <p>Purpose: {o.purpose}</p>
                <p>Out: {format(new Date(o.requestedOutTime), 'MMM d, HH:mm')}</p>
                <p>Return: {format(new Date(o.expectedReturnTime), 'MMM d, HH:mm')}</p>
                {o.wardenRemarks && <p className="text-xs">Remarks: {o.wardenRemarks}</p>}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function CreateOutingForm({ onClose }: { onClose: () => void }) {
  const createOuting = useCreateOuting();
  const now = format(new Date(), "yyyy-MM-dd'T'HH:mm");
  const [form, setForm] = useState<CreateOutingRequestDto>({
    destination: '', purpose: '', requestedOutTime: now, expectedReturnTime: now,
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await createOuting.mutateAsync(form);
    onClose();
  };

  return (
    <div className="mb-6 rounded-xl border border-gray-200 bg-white p-6">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-900">New Outing Request</h3>
        <button onClick={onClose} aria-label="Close"><X className="h-5 w-5 text-gray-400" /></button>
      </div>
      <form onSubmit={handleSubmit} className="grid gap-4 sm:grid-cols-2">
        <input placeholder="Destination" required value={form.destination}
          onChange={e => setForm(f => ({ ...f, destination: e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <input placeholder="Purpose" required value={form.purpose}
          onChange={e => setForm(f => ({ ...f, purpose: e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <div>
          <label htmlFor="outing-out-time" className="mb-1 block text-xs text-gray-500">Out Time</label>
          <input id="outing-out-time" type="datetime-local" required value={form.requestedOutTime}
            onChange={e => setForm(f => ({ ...f, requestedOutTime: e.target.value }))}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        </div>
        <div>
          <label htmlFor="outing-return-time" className="mb-1 block text-xs text-gray-500">Expected Return</label>
          <input id="outing-return-time" type="datetime-local" required value={form.expectedReturnTime}
            onChange={e => setForm(f => ({ ...f, expectedReturnTime: e.target.value }))}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        </div>
        <div className="sm:col-span-2 flex justify-end gap-2">
          <button type="button" onClick={onClose} className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">Cancel</button>
          <button type="submit" disabled={createOuting.isPending}
            className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-50">
            {createOuting.isPending ? 'Submitting...' : 'Submit'}
          </button>
        </div>
      </form>
    </div>
  );
}
