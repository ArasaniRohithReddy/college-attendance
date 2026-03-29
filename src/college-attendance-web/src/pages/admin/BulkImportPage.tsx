import { useState, useRef } from 'react';
import { useImportStudents, useImportCourses } from '../../hooks/useApi';
import { bulkImportApi } from '../../api/services';
import { PageHeader, LoadingSpinner } from '../../components/ui';
import { Upload, Download, FileSpreadsheet, Users, BookOpen, CheckCircle, XCircle, AlertTriangle } from 'lucide-react';
import type { BulkImportResultDto } from '../../types';

export default function BulkImportPage() {
  const studentFileRef = useRef<HTMLInputElement>(null);
  const courseFileRef = useRef<HTMLInputElement>(null);
  const importStudents = useImportStudents();
  const importCourses = useImportCourses();
  const [result, setResult] = useState<BulkImportResultDto | null>(null);
  const [importing, setImporting] = useState(false);

  const handleImport = async (type: 'students' | 'courses', file: File) => {
    setImporting(true);
    try {
      const res = type === 'students'
        ? await importStudents.mutateAsync(file)
        : await importCourses.mutateAsync(file);
      setResult(res);
    } finally {
      setImporting(false);
    }
  };

  const downloadTemplate = async (type: 'students' | 'courses') => {
    const blob = type === 'students'
      ? await bulkImportApi.templateStudents()
      : await bulkImportApi.templateCourses();
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${type}-template.csv`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div>
      <PageHeader title="Bulk Import" />

      <div className="grid gap-6 md:grid-cols-2">
        {/* Students Import */}
        <div className="rounded-xl border border-gray-200 bg-white p-6">
          <div className="mb-4 flex items-center gap-3">
            <div className="rounded-lg bg-blue-100 p-2"><Users className="h-6 w-6 text-blue-600" /></div>
            <div>
              <h3 className="text-lg font-semibold text-gray-900">Import Students</h3>
              <p className="text-sm text-gray-500">Upload a CSV file with student records</p>
            </div>
          </div>
          <div className="flex gap-3">
            <input ref={studentFileRef} type="file" accept=".csv" className="hidden" onChange={e => {
              const f = e.target.files?.[0];
              if (f) handleImport('students', f);
              e.target.value = '';
            }} />
            <button onClick={() => studentFileRef.current?.click()} disabled={importing} className="flex-1 inline-flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50">
              <Upload className="h-4 w-4" />Upload CSV
            </button>
            <button onClick={() => downloadTemplate('students')} className="inline-flex items-center gap-2 rounded-lg border border-gray-300 px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50">
              <Download className="h-4 w-4" />Template
            </button>
          </div>
        </div>

        {/* Courses Import */}
        <div className="rounded-xl border border-gray-200 bg-white p-6">
          <div className="mb-4 flex items-center gap-3">
            <div className="rounded-lg bg-purple-100 p-2"><BookOpen className="h-6 w-6 text-purple-600" /></div>
            <div>
              <h3 className="text-lg font-semibold text-gray-900">Import Courses</h3>
              <p className="text-sm text-gray-500">Upload a CSV file with course records</p>
            </div>
          </div>
          <div className="flex gap-3">
            <input ref={courseFileRef} type="file" accept=".csv" className="hidden" onChange={e => {
              const f = e.target.files?.[0];
              if (f) handleImport('courses', f);
              e.target.value = '';
            }} />
            <button onClick={() => courseFileRef.current?.click()} disabled={importing} className="flex-1 inline-flex items-center justify-center gap-2 rounded-lg bg-purple-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-purple-700 disabled:opacity-50">
              <Upload className="h-4 w-4" />Upload CSV
            </button>
            <button onClick={() => downloadTemplate('courses')} className="inline-flex items-center gap-2 rounded-lg border border-gray-300 px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50">
              <Download className="h-4 w-4" />Template
            </button>
          </div>
        </div>
      </div>

      {importing && (
        <div className="mt-6 flex items-center justify-center gap-3 rounded-xl border border-gray-200 bg-white p-8">
          <LoadingSpinner />
          <span className="text-gray-500">Importing records...</span>
        </div>
      )}

      {result && !importing && (
        <div className="mt-6 rounded-xl border border-gray-200 bg-white p-6">
          <h3 className="mb-4 flex items-center gap-2 text-lg font-semibold text-gray-900">
            <FileSpreadsheet className="h-5 w-5 text-gray-500" />Import Results
          </h3>
          <div className="grid gap-4 sm:grid-cols-3">
            <div className="rounded-lg border border-gray-100 bg-gray-50 p-4 text-center">
              <p className="text-2xl font-bold text-gray-900">{result.totalRows}</p>
              <p className="text-sm text-gray-500">Total Records</p>
            </div>
            <div className="rounded-lg border border-green-100 bg-green-50 p-4 text-center">
              <p className="text-2xl font-bold text-green-600">{result.succeeded}</p>
              <p className="text-sm text-green-700">Succeeded</p>
            </div>
            <div className="rounded-lg border border-red-100 bg-red-50 p-4 text-center">
              <p className="text-2xl font-bold text-red-600">{result.failed}</p>
              <p className="text-sm text-red-700">Failed</p>
            </div>
          </div>

          {result.errors && result.errors.length > 0 && (
            <div className="mt-4">
              <h4 className="mb-2 flex items-center gap-1 text-sm font-medium text-red-700">
                <AlertTriangle className="h-4 w-4" />Errors ({result.errors.length})
              </h4>
              <div className="max-h-48 overflow-y-auto rounded-lg border border-red-200 bg-red-50 p-3">
                {result.errors.map((err, idx) => (
                  <div key={idx} className="flex items-start gap-2 py-1 text-sm text-red-700">
                    <XCircle className="mt-0.5 h-3.5 w-3.5 flex-shrink-0" />
                    <span>{err}</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {result.failed === 0 && (
            <div className="mt-4 flex items-center gap-2 rounded-lg bg-green-50 p-3 text-sm text-green-700">
              <CheckCircle className="h-5 w-5" />All records imported successfully!
            </div>
          )}
        </div>
      )}
    </div>
  );
}
