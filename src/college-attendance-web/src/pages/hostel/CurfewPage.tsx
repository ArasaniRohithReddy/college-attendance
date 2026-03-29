import { useState } from 'react';
import { useCurfewLogs, useCurfewConfig } from '../../hooks/useApi';
import { advancedAnalyticsApi } from '../../api/services';
import { PageHeader, LoadingSpinner, StatCard } from '../../components/ui';
import { Moon, Clock, AlertTriangle, CheckCircle, RefreshCw } from 'lucide-react';

export default function CurfewPage() {
  const [page, setPage] = useState(1);
  const { data: logs, isLoading: logsLoading } = useCurfewLogs(page, 15);
  const { data: config, isLoading: configLoading } = useCurfewConfig();
  const [checking, setChecking] = useState(false);
  const [checkResult, setCheckResult] = useState<string | null>(null);

  const handleManualCheck = async () => {
    setChecking(true);
    try {
      await advancedAnalyticsApi.checkCurfew();
      setCheckResult('Curfew check completed successfully');
      setTimeout(() => setCheckResult(null), 5000);
    } catch {
      setCheckResult('Failed to run curfew check');
    } finally {
      setChecking(false);
    }
  };

  if (logsLoading || configLoading) return <LoadingSpinner />;

  const curfewLogs = logs?.items ?? [];
  const totalPages = logs ? Math.ceil(logs.totalCount / logs.pageSize) : 1;
  const lateCount = curfewLogs.filter(l => l.minutesLate > 0).length;

  return (
    <div>
      <PageHeader title="Curfew Management">
        <button onClick={handleManualCheck} disabled={checking} className="inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-50">
          <RefreshCw className={`h-4 w-4 ${checking ? 'animate-spin' : ''}`} />Run Curfew Check
        </button>
      </PageHeader>

      {checkResult && (
        <div className="mb-4 rounded-lg bg-green-50 px-4 py-3 text-sm text-green-700 flex items-center gap-2">
          <CheckCircle className="h-4 w-4" />{checkResult}
        </div>
      )}

      {/* Config summary */}
      {config && (
        <div className="mb-6 grid gap-4 sm:grid-cols-3">
          <StatCard title="Curfew Time" value={config.curfewTime} icon={<Moon className="h-5 w-5 text-indigo-600" />} />
          <StatCard title="Grace Period" value={`${config.gracePeriodMinutes} min`} icon={<Clock className="h-5 w-5 text-yellow-600" />} />
          <StatCard title="Late Returns on Page" value={lateCount.toString()} icon={<AlertTriangle className="h-5 w-5 text-red-600" />} />
        </div>
      )}

      {/* Curfew logs table */}
      <div className="rounded-xl border border-gray-200 bg-white">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
              <tr>
                <th className="px-4 py-3">Student</th>
                <th className="px-4 py-3">Hostel</th>
                <th className="px-4 py-3">Curfew Time</th>
                <th className="px-4 py-3">Return Time</th>
                <th className="px-4 py-3">Minutes Late</th>
                <th className="px-4 py-3">Parent Notified</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {curfewLogs.map(log => (
                <tr key={log.id} className={`hover:bg-gray-50 ${log.minutesLate > 0 ? 'bg-red-50/40' : ''}`}>
                  <td className="px-4 py-3 font-medium text-gray-900">{log.studentName}</td>
                  <td className="px-4 py-3 text-gray-600">{log.hostelName || '-'}</td>
                  <td className="px-4 py-3 text-gray-600">{log.curfewTime}</td>
                  <td className="px-4 py-3 text-gray-600">{log.returnTime ? new Date(log.returnTime).toLocaleString() : '-'}</td>
                  <td className="px-4 py-3">
                    {log.minutesLate > 0
                      ? <span className="inline-flex items-center gap-1 rounded-full bg-red-100 px-2 py-0.5 text-xs font-medium text-red-700"><AlertTriangle className="h-3 w-3" />{log.minutesLate} min</span>
                      : <span className="inline-flex items-center gap-1 rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-700"><CheckCircle className="h-3 w-3" />On time</span>}
                  </td>
                  <td className="px-4 py-3 text-gray-500">{log.parentNotified ? (log.parentNotifiedAt ? new Date(log.parentNotifiedAt).toLocaleString() : 'Yes') : 'No'}</td>
                </tr>
              ))}
              {curfewLogs.length === 0 && (
                <tr><td colSpan={6} className="px-4 py-8 text-center text-gray-400">No curfew logs found</td></tr>
              )}
            </tbody>
          </table>
        </div>
        {totalPages > 1 && (
          <div className="flex items-center justify-between border-t border-gray-200 px-4 py-3">
            <span className="text-sm text-gray-500">Page {page} of {totalPages} ({logs?.totalCount} total)</span>
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
