import { useState } from 'react';
import { useTodaySessions, useActiveQR, useGenerateQR } from '../../hooks/useApi';
import { SessionStatus } from '../../types';
import { PageHeader, LoadingSpinner, EmptyState } from '../../components/ui';
import { QrCode, RefreshCw } from 'lucide-react';

export default function QRGeneratePage() {
  const { data: sessions, isLoading } = useTodaySessions();
  const [selectedSessionId, setSelectedSessionId] = useState<string>('');
  const activeSessions = sessions?.filter(s => s.status === SessionStatus.Active) ?? [];

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="QR Code Generator" />

      {activeSessions.length === 0 ? (
        <EmptyState message="No active sessions. Start a session first to generate QR codes." />
      ) : (
        <div className="space-y-6">
          <div className="rounded-xl border border-gray-200 bg-white p-6">
            <label htmlFor="qr-session-select" className="mb-2 block text-sm font-medium text-gray-700">Select Active Session</label>
            <select id="qr-session-select" value={selectedSessionId} onChange={e => setSelectedSessionId(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 sm:w-auto sm:min-w-[300px]">
              <option value="">Choose a session</option>
              {activeSessions.map(s => (
                <option key={s.id} value={s.id}>{s.courseName} — {s.room} ({s.startTime}–{s.endTime})</option>
              ))}
            </select>
          </div>

          {selectedSessionId && <QRDisplay sessionId={selectedSessionId} />}
        </div>
      )}
    </div>
  );
}

function QRDisplay({ sessionId }: { sessionId: string }) {
  const { data: qr, isLoading, refetch } = useActiveQR(sessionId);
  const generateQR = useGenerateQR();

  const handleGenerate = async () => {
    await generateQR.mutateAsync({
      classSessionId: sessionId,
      expirationSeconds: 30,
    });
    refetch();
  };

  return (
    <div className="rounded-xl border border-gray-200 bg-white p-6">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-900">QR Code</h3>
        <button onClick={handleGenerate} disabled={generateQR.isPending}
          className="inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-50">
          <RefreshCw className={`h-4 w-4 ${generateQR.isPending ? 'animate-spin' : ''}`} />
          {generateQR.isPending ? 'Generating...' : 'Generate New QR'}
        </button>
      </div>

      {isLoading ? (
        <LoadingSpinner />
      ) : qr ? (
        <div className="flex flex-col items-center gap-4">
          <div className="rounded-xl border-2 border-indigo-100 bg-white p-4">
            {qr.qrImageBase64 ? (
              <img src={`data:image/png;base64,${qr.qrImageBase64}`} alt="QR Code" className="h-64 w-64" />
            ) : (
              <div className="flex h-64 w-64 items-center justify-center bg-gray-100 rounded-lg">
                <QrCode className="h-24 w-24 text-gray-300" />
              </div>
            )}
          </div>
          <div className="text-center text-sm text-gray-500">
            <p>Token: <span className="font-mono font-medium text-gray-900">{qr.qrToken.slice(0, 16)}...</span></p>
            <p>Expires: {new Date(qr.expiresAt).toLocaleTimeString()}</p>
            <p className="mt-1 text-xs text-indigo-600">Auto-refreshes every 5 seconds</p>
          </div>
        </div>
      ) : (
        <div className="flex flex-col items-center gap-4 py-12">
          <QrCode className="h-16 w-16 text-gray-300" />
          <p className="text-sm text-gray-500">Click "Generate New QR" to create a code for this session</p>
        </div>
      )}
    </div>
  );
}
