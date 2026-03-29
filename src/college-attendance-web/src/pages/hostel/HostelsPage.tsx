import { useHostels } from '../../hooks/useApi';
import { PageHeader, LoadingSpinner, EmptyState } from '../../components/ui';
import { Building } from 'lucide-react';

export default function HostelsPage() {
  const { data: hostels, isLoading } = useHostels();

  if (isLoading) return <LoadingSpinner />;
  if (!hostels || hostels.length === 0) return <><PageHeader title="Hostels" /><EmptyState message="No hostels found" /></>;

  return (
    <div>
      <PageHeader title="Hostels" />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {hostels.map(h => (
          <div key={h.id} className="rounded-xl border border-gray-200 bg-white p-5">
            <div className="mb-3 flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-indigo-100">
                <Building className="h-5 w-5 text-indigo-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900">{h.name}</h3>
                <p className="text-xs text-gray-500">Block {h.block}</p>
              </div>
            </div>
            <div className="space-y-1 text-sm text-gray-500">
              <p>Capacity: {h.capacity} rooms</p>
              <p>Warden: {h.wardenName ?? 'Not assigned'}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
