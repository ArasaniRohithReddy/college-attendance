import { useState } from 'react';
import { useDepartments, useCreateDepartment } from '../../hooks/useApi';
import type { CreateDepartmentRequest } from '../../types';
import { PageHeader, LoadingSpinner, EmptyState } from '../../components/ui';
import { Plus, X, Building2 } from 'lucide-react';

export default function DepartmentsPage() {
  const [showForm, setShowForm] = useState(false);
  const { data, isLoading } = useDepartments();

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="Departments">
        <button onClick={() => setShowForm(true)} className="inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700">
          <Plus className="h-4 w-4" /> Add Department
        </button>
      </PageHeader>

      {showForm && <CreateDeptForm onClose={() => setShowForm(false)} />}

      {!data || data.length === 0 ? (
        <EmptyState message="No departments found" />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {data.map(d => (
            <div key={d.id} className="rounded-xl border border-gray-200 bg-white p-6 hover:shadow-md transition-shadow">
              <div className="mb-3 flex items-center gap-3">
                <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-indigo-50">
                  <Building2 className="h-5 w-5 text-indigo-600" />
                </div>
                <div>
                  <h3 className="font-semibold text-gray-900">{d.name}</h3>
                  <p className="text-sm text-gray-500">{d.code}</p>
                </div>
              </div>
              {d.description && <p className="mb-3 text-sm text-gray-500">{d.description}</p>}
              <div className="flex gap-4 text-sm text-gray-500">
                <span>{d.userCount} users</span>
                <span>{d.courseCount} courses</span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function CreateDeptForm({ onClose }: { onClose: () => void }) {
  const createDept = useCreateDepartment();
  const [form, setForm] = useState<CreateDepartmentRequest>({ name: '', code: '' });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await createDept.mutateAsync(form);
    onClose();
  };

  return (
    <div className="mb-6 rounded-xl border border-gray-200 bg-white p-6">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-900">New Department</h3>
        <button onClick={onClose} aria-label="Close"><X className="h-5 w-5 text-gray-400" /></button>
      </div>
      <form onSubmit={handleSubmit} className="grid gap-4 sm:grid-cols-2">
        <input placeholder="Name" required value={form.name}
          onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <input placeholder="Code (e.g. CSE)" required value={form.code}
          onChange={e => setForm(f => ({ ...f, code: e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <textarea placeholder="Description (optional)" rows={2} value={form.description ?? ''}
          onChange={e => setForm(f => ({ ...f, description: e.target.value || undefined }))}
          className="sm:col-span-2 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <div className="sm:col-span-2 flex justify-end gap-2">
          <button type="button" onClick={onClose} className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">Cancel</button>
          <button type="submit" disabled={createDept.isPending}
            className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-50">
            {createDept.isPending ? 'Creating...' : 'Create'}
          </button>
        </div>
      </form>
    </div>
  );
}
