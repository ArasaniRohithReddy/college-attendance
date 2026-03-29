import { useState } from 'react';
import { useUsers, useCreateUser, useDeleteUser } from '../../hooks/useApi';
import { useDepartments } from '../../hooks/useApi';
import { UserRole, type CreateUserRequest } from '../../types';
import { PageHeader, LoadingSpinner, Badge, EmptyState } from '../../components/ui';
import { Plus, Trash2, X } from 'lucide-react';

const roleName = (r: UserRole) => UserRole[r];
const roleColor = (r: UserRole) => ['indigo', 'blue', 'green', 'yellow', 'gray'][r] ?? 'gray';

export default function UsersPage() {
  const [page, setPage] = useState(1);
  const [showForm, setShowForm] = useState(false);
  const { data, isLoading } = useUsers(page, 20);
  const deleteUser = useDeleteUser();

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="Users">
        <button onClick={() => setShowForm(true)} className="inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700">
          <Plus className="h-4 w-4" /> Add User
        </button>
      </PageHeader>

      {showForm && <CreateUserForm onClose={() => setShowForm(false)} />}

      {!data?.items?.length ? (
        <EmptyState message="No users found" />
      ) : (
        <>
          <div className="overflow-x-auto rounded-xl border border-gray-200 bg-white">
            <table className="w-full text-left text-sm">
              <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
                <tr>
                  <th className="px-4 py-3">Name</th>
                  <th className="px-4 py-3">Email</th>
                  <th className="px-4 py-3">Role</th>
                  <th className="px-4 py-3">Department</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {data.items.map((u) => (
                  <tr key={u.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium text-gray-900">{u.fullName}</td>
                    <td className="px-4 py-3 text-gray-500">{u.email}</td>
                    <td className="px-4 py-3"><Badge label={roleName(u.role)} variant={roleColor(u.role)} /></td>
                    <td className="px-4 py-3 text-gray-500">{u.departmentName ?? '—'}</td>
                    <td className="px-4 py-3">
                      <Badge label={u.isActive ? 'Active' : 'Inactive'} variant={u.isActive ? 'green' : 'red'} />
                    </td>
                    <td className="px-4 py-3 text-right">
                      <button onClick={() => { if (confirm('Delete this user?')) deleteUser.mutate(u.id); }}
                        className="rounded p-1 text-red-500 hover:bg-red-50" aria-label="Delete user">
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="mt-4 flex items-center justify-between">
            <p className="text-sm text-gray-500">Page {data.page} of {data.totalPages} ({data.totalCount} total)</p>
            <div className="flex gap-2">
              <button disabled={!data.hasPrevious} onClick={() => setPage(p => p - 1)}
                className="rounded-lg border border-gray-300 px-3 py-1.5 text-sm disabled:opacity-50">Previous</button>
              <button disabled={!data.hasNext} onClick={() => setPage(p => p + 1)}
                className="rounded-lg border border-gray-300 px-3 py-1.5 text-sm disabled:opacity-50">Next</button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

function CreateUserForm({ onClose }: { onClose: () => void }) {
  const createUser = useCreateUser();
  const { data: depts } = useDepartments();
  const [form, setForm] = useState<CreateUserRequest>({
    email: '', fullName: '', role: UserRole.Student,
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await createUser.mutateAsync(form);
    onClose();
  };

  return (
    <div className="mb-6 rounded-xl border border-gray-200 bg-white p-6">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-900">Create User</h3>
        <button onClick={onClose} aria-label="Close"><X className="h-5 w-5 text-gray-400" /></button>
      </div>
      <form onSubmit={handleSubmit} className="grid gap-4 sm:grid-cols-2">
        <input placeholder="Full Name" required value={form.fullName}
          onChange={e => setForm(f => ({ ...f, fullName: e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <input placeholder="Email" type="email" required value={form.email}
          onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <select value={form.role} onChange={e => setForm(f => ({ ...f, role: +e.target.value }))} aria-label="Role"
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500">
          {[0, 1, 2, 3, 4].map(r => <option key={r} value={r}>{UserRole[r]}</option>)}
        </select>
        <select value={form.departmentId ?? ''} onChange={e => setForm(f => ({ ...f, departmentId: e.target.value || undefined }))} aria-label="Department"
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500">
          <option value="">No Department</option>
          {depts?.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
        </select>
        <input placeholder="Student ID (optional)" value={form.studentId ?? ''}
          onChange={e => setForm(f => ({ ...f, studentId: e.target.value || undefined }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <input placeholder="Phone (optional)" value={form.phone ?? ''}
          onChange={e => setForm(f => ({ ...f, phone: e.target.value || undefined }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <div className="sm:col-span-2 flex justify-end gap-2">
          <button type="button" onClick={onClose} className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">Cancel</button>
          <button type="submit" disabled={createUser.isPending}
            className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-50">
            {createUser.isPending ? 'Creating...' : 'Create'}
          </button>
        </div>
      </form>
    </div>
  );
}
