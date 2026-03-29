import { useState } from 'react';
import { useActiveEmergencies, useCreateSOS, useAcknowledgeSOS, useResolveSOS } from '../../hooks/useApi';
import { PageHeader, LoadingSpinner, StatCard } from '../../components/ui';
import { Siren, ShieldAlert, CheckCircle } from 'lucide-react';
import { SOSStatus, SOSPriority, type CreateSOSRequest } from '../../types';

const priorityColors: Record<SOSPriority, string> = {
  [SOSPriority.Low]: 'bg-blue-100 text-blue-700',
  [SOSPriority.Medium]: 'bg-yellow-100 text-yellow-700',
  [SOSPriority.High]: 'bg-orange-100 text-orange-700',
  [SOSPriority.Critical]: 'bg-red-100 text-red-700',
};

const statusLabels: Record<SOSStatus, string> = {
  [SOSStatus.Active]: 'Active',
  [SOSStatus.Acknowledged]: 'Acknowledged',
  [SOSStatus.Responding]: 'Responding',
  [SOSStatus.Resolved]: 'Resolved',
  [SOSStatus.FalseAlarm]: 'False Alarm',
};

export default function EmergencySOSPage() {
  const { data: activeList, isLoading } = useActiveEmergencies();
  const createMut = useCreateSOS();
  const ackMut = useAcknowledgeSOS();
  const resolveMut = useResolveSOS();
  const [showTrigger, setShowTrigger] = useState(false);
  const [resolveId, setResolveId] = useState<string | null>(null);
  const [resolveNotes, setResolveNotes] = useState('');

  const handleTrigger = async () => {
    const req: CreateSOSRequest = {
      latitude: 0,
      longitude: 0,
      message: 'Emergency SOS triggered',
      priority: SOSPriority.High,
    };
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        async pos => {
          req.latitude = pos.coords.latitude;
          req.longitude = pos.coords.longitude;
          await createMut.mutateAsync(req);
          setShowTrigger(false);
        },
        async () => {
          await createMut.mutateAsync(req);
          setShowTrigger(false);
        }
      );
    } else {
      await createMut.mutateAsync(req);
      setShowTrigger(false);
    }
  };

  const handleResolve = async () => {
    if (!resolveId) return;
    await resolveMut.mutateAsync({ id: resolveId, req: { resolutionNotes: resolveNotes } });
    setResolveId(null);
    setResolveNotes('');
  };

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="Emergency SOS">
        <button onClick={() => setShowTrigger(true)} className="inline-flex items-center gap-2 rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700">
          <Siren className="h-4 w-4" /> Trigger SOS
        </button>
      </PageHeader>

      {showTrigger && (
        <div className="mb-6 rounded-xl border-2 border-red-300 bg-red-50 p-6 text-center">
          <Siren className="mx-auto mb-3 h-12 w-12 text-red-600" />
          <h3 className="text-lg font-bold text-red-900">Are you sure?</h3>
          <p className="mb-4 text-sm text-red-700">This will immediately alert all wardens and security.</p>
          <div className="flex justify-center gap-3">
            <button onClick={handleTrigger} disabled={createMut.isPending} className="rounded-lg bg-red-600 px-6 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50">
              {createMut.isPending ? 'Sending...' : 'Confirm SOS'}
            </button>
            <button onClick={() => setShowTrigger(false)} className="rounded-lg border border-gray-300 px-6 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">Cancel</button>
          </div>
        </div>
      )}

      <div className="mb-6 grid gap-4 sm:grid-cols-3">
        <StatCard icon={<Siren className="h-6 w-6" />} title="Active Alerts" value={activeList?.filter(s => s.status === SOSStatus.Active).length ?? 0} color="red" />
        <StatCard icon={<ShieldAlert className="h-6 w-6" />} title="Responding" value={activeList?.filter(s => s.status === SOSStatus.Responding || s.status === SOSStatus.Acknowledged).length ?? 0} color="yellow" />
        <StatCard icon={<CheckCircle className="h-6 w-6" />} title="Total Active" value={activeList?.length ?? 0} color="blue" />
      </div>

      {/* Resolve dialog */}
      {resolveId && (
        <div className="mb-6 rounded-xl border border-gray-200 bg-white p-6">
          <h3 className="mb-2 text-lg font-semibold">Resolve Emergency</h3>
          <textarea value={resolveNotes} onChange={e => setResolveNotes(e.target.value)} placeholder="Resolution notes..." rows={3} className="mb-3 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" required />
          <div className="flex gap-3">
            <button onClick={handleResolve} disabled={resolveMut.isPending} className="rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50">Resolve</button>
            <button onClick={() => setResolveId(null)} className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50">Cancel</button>
          </div>
        </div>
      )}

      <div className="space-y-4">
        {activeList?.map(sos => (
          <div key={sos.id} className="rounded-xl border border-gray-200 bg-white p-5">
            <div className="flex items-start justify-between">
              <div>
                <div className="flex items-center gap-2">
                  <h4 className="font-semibold text-gray-900">{sos.studentName}</h4>
                  <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${priorityColors[sos.priority]}`}>{SOSPriority[sos.priority]}</span>
                  <span className="text-xs text-gray-400">{statusLabels[sos.status]}</span>
                </div>
                {sos.message && <p className="mt-1 text-sm text-gray-600">{sos.message}</p>}
                <p className="mt-1 text-xs text-gray-400">
                  Created {new Date(sos.createdAt).toLocaleString()}
                  {sos.latitude !== 0 && ` | Location: ${sos.latitude.toFixed(4)}, ${sos.longitude.toFixed(4)}`}
                </p>
              </div>
              <div className="flex gap-2">
                {sos.status === SOSStatus.Active && (
                  <button onClick={() => ackMut.mutate(sos.id)} className="rounded-lg bg-yellow-500 px-3 py-1.5 text-xs font-medium text-white hover:bg-yellow-600">Acknowledge</button>
                )}
                {(sos.status === SOSStatus.Active || sos.status === SOSStatus.Acknowledged || sos.status === SOSStatus.Responding) && (
                  <button onClick={() => setResolveId(sos.id)} className="rounded-lg bg-green-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-green-700">Resolve</button>
                )}
              </div>
            </div>
          </div>
        ))}
        {(!activeList || activeList.length === 0) && (
          <div className="rounded-xl border border-gray-200 bg-white p-12 text-center text-gray-400">
            No active emergencies
          </div>
        )}
      </div>
    </div>
  );
}
