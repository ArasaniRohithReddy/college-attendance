import { useOutingRequests, useApproveOuting, useRejectOuting } from '../../hooks/useApi';
import { OutingStatus } from '../../types';
import { PageHeader, LoadingSpinner, EmptyState, Badge } from '../../components/ui';
import { Check, X as XIcon, MapPin } from 'lucide-react';
import { format } from 'date-fns';

export default function OutingsPage() {
  const { data: outings, isLoading } = useOutingRequests(0);
  const approveOuting = useApproveOuting();
  const rejectOuting = useRejectOuting();

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="Outing Approvals" />

      {!outings || outings.length === 0 ? (
        <EmptyState message="No pending outing requests" />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {outings.map(o => (
            <div key={o.id} className="rounded-xl border border-gray-200 bg-white p-5">
              <div className="mb-3 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <MapPin className="h-4 w-4 text-indigo-500" />
                  <span className="font-semibold text-gray-900">{o.studentName}</span>
                </div>
                <Badge label={OutingStatus[o.status]} variant="yellow" />
              </div>
              <div className="space-y-1 text-sm text-gray-500">
                <p>Destination: {o.destination}</p>
                <p>Purpose: {o.purpose}</p>
                <p>Out: {format(new Date(o.requestedOutTime), 'MMM d, HH:mm')}</p>
                <p>Return: {format(new Date(o.expectedReturnTime), 'MMM d, HH:mm')}</p>
              </div>
              {o.status === OutingStatus.Pending && (
                <div className="mt-4 flex gap-2">
                  <button onClick={() => approveOuting.mutate({ id: o.id })} disabled={approveOuting.isPending}
                    className="inline-flex items-center gap-1 rounded-lg bg-green-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-green-700 disabled:opacity-50">
                    <Check className="h-3.5 w-3.5" /> Approve
                  </button>
                  <button onClick={() => rejectOuting.mutate({ id: o.id, remarks: 'Rejected' })} disabled={rejectOuting.isPending}
                    className="inline-flex items-center gap-1 rounded-lg bg-red-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-red-700 disabled:opacity-50">
                    <XIcon className="h-3.5 w-3.5" /> Reject
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
