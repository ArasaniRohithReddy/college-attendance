import { useState } from 'react';
import { useFraudLogs, useResolveFraud } from '../../hooks/useApi';
import { PageHeader, LoadingSpinner } from '../../components/ui';
import { ShieldAlert, CheckCircle, X, Eye } from 'lucide-react';
import { FraudSeverity, FraudType } from '../../types';

const severityColor: Record<FraudSeverity, string> = {
  [FraudSeverity.Low]: 'bg-blue-100 text-blue-700',
  [FraudSeverity.Medium]: 'bg-yellow-100 text-yellow-700',
  [FraudSeverity.High]: 'bg-orange-100 text-orange-700',
  [FraudSeverity.Critical]: 'bg-red-100 text-red-700',
};

const severityLabel: Record<FraudSeverity, string> = {
  [FraudSeverity.Low]: 'Low',
  [FraudSeverity.Medium]: 'Medium',
  [FraudSeverity.High]: 'High',
  [FraudSeverity.Critical]: 'Critical',
};

const fraudTypeLabel: Record<FraudType, string> = {
  [FraudType.MultipleDevices]: 'Multiple Devices',
  [FraudType.LocationSpoof]: 'Location Spoofing',
  [FraudType.RapidScanSpike]: 'Rapid Scan Spike',
  [FraudType.ProxyAttendance]: 'Proxy Attendance',
  [FraudType.DeviceMismatch]: 'Device Mismatch',
  [FraudType.TimeAnomaly]: 'Time Anomaly',
  [FraudType.Other]: 'Other',
};

export default function FraudLogsPage() {
  const [page, setPage] = useState(1);
  const [resolved, setResolved] = useState<boolean | undefined>(undefined);
  const { data, isLoading } = useFraudLogs(page, 15, resolved);
  const resolveMut = useResolveFraud();
  const [resolveId, setResolveId] = useState<string | null>(null);
  const [notes, setNotes] = useState('');

  const handleResolve = async () => {
    if (!resolveId) return;
    await resolveMut.mutateAsync({ id: resolveId, req: { resolutionNotes: notes } });
    setResolveId(null);
    setNotes('');
  };

  if (isLoading) return <LoadingSpinner />;

  const logs = data?.items ?? [];
  const totalPages = data ? Math.ceil(data.totalCount / data.pageSize) : 1;

  return (
    <div>
      <PageHeader title="Fraud Detection Logs">
        <div className="flex gap-2">
          <button onClick={() => setResolved(undefined)} className={`rounded-full px-3 py-1 text-sm ${resolved === undefined ? 'bg-indigo-600 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}>All</button>
          <button onClick={() => setResolved(false)} className={`rounded-full px-3 py-1 text-sm ${resolved === false ? 'bg-red-600 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}>Unresolved</button>
          <button onClick={() => setResolved(true)} className={`rounded-full px-3 py-1 text-sm ${resolved === true ? 'bg-green-600 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}>Resolved</button>
        </div>
      </PageHeader>

      {/* Resolve dialog */}
      {resolveId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
            <div className="mb-4 flex items-center justify-between">
              <h3 className="text-lg font-semibold text-gray-900">Resolve Fraud Alert</h3>
              <button onClick={() => setResolveId(null)} className="text-gray-400 hover:text-gray-600"><X className="h-5 w-5" /></button>
            </div>
            <textarea value={notes} onChange={e => setNotes(e.target.value)} rows={4} placeholder="Resolution notes..." className="mb-4 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" />
            <div className="flex justify-end gap-3">
              <button onClick={() => setResolveId(null)} className="rounded-lg px-4 py-2 text-sm text-gray-600 hover:bg-gray-100">Cancel</button>
              <button onClick={handleResolve} disabled={resolveMut.isPending} className="rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50">Resolve</button>
            </div>
          </div>
        </div>
      )}

      <div className="rounded-xl border border-gray-200 bg-white">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
              <tr>
                <th className="px-4 py-3">Student</th>
                <th className="px-4 py-3">Type</th>
                <th className="px-4 py-3">Severity</th>
                <th className="px-4 py-3">Description</th>
                <th className="px-4 py-3">Detected At</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {logs.map(log => (
                <tr key={log.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-900">{log.userName}</td>
                  <td className="px-4 py-3">
                    <span className="inline-flex items-center gap-1 text-gray-700">
                      <ShieldAlert className="h-3.5 w-3.5 text-gray-400" />{fraudTypeLabel[log.fraudType]}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${severityColor[log.severity]}`}>{severityLabel[log.severity]}</span>
                  </td>
                  <td className="max-w-xs truncate px-4 py-3 text-gray-500">{log.description}</td>
                  <td className="px-4 py-3 text-gray-500">{new Date(log.createdAt).toLocaleString()}</td>
                  <td className="px-4 py-3">
                    {log.isResolved
                      ? <span className="inline-flex items-center gap-1 text-green-600"><CheckCircle className="h-3.5 w-3.5" />Resolved</span>
                      : <span className="text-red-600 font-medium">Unresolved</span>}
                  </td>
                  <td className="px-4 py-3">
                    {!log.isResolved && (
                      <button onClick={() => setResolveId(log.id)} className="inline-flex items-center gap-1 rounded-lg bg-green-50 px-3 py-1.5 text-xs font-medium text-green-700 hover:bg-green-100">
                        <CheckCircle className="h-3.5 w-3.5" />Resolve
                      </button>
                    )}
                    {log.isResolved && log.resolvedByName && (
                      <span title={log.resolvedByName} className="inline-flex items-center gap-1 text-gray-400 cursor-help">
                        <Eye className="h-3.5 w-3.5" />View
                      </span>
                    )}
                  </td>
                </tr>
              ))}
              {logs.length === 0 && (
                <tr><td colSpan={7} className="px-4 py-8 text-center text-gray-400">No fraud logs found</td></tr>
              )}
            </tbody>
          </table>
        </div>
        {totalPages > 1 && (
          <div className="flex items-center justify-between border-t border-gray-200 px-4 py-3">
            <span className="text-sm text-gray-500">Page {page} of {totalPages} ({data?.totalCount} total)</span>
            <div className="flex gap-2">
              <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page <= 1} className="rounded-lg border border-gray-300 px-3 py-1.5 text-sm disabled:opacity-50">Previous</button>
              <button onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page >= totalPages} className="rounded-lg border border-gray-300 px-3 py-1.5 text-sm disabled:opacity-50">Next</button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
