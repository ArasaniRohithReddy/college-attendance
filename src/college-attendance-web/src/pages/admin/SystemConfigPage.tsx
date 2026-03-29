import { useState } from 'react';
import { useSystemConfigs, useCreateConfig, useUpdateConfig, useDeleteConfig } from '../../hooks/useApi';
import { PageHeader, LoadingSpinner } from '../../components/ui';
import { Settings, Plus, Pencil, Trash2, X, Save } from 'lucide-react';
import { ConfigCategory, type CreateConfigRequest, type UpdateConfigRequest } from '../../types';

const categoryLabels: Record<ConfigCategory, string> = {
  [ConfigCategory.General]: 'General',
  [ConfigCategory.Attendance]: 'Attendance',
  [ConfigCategory.Hostel]: 'Hostel',
  [ConfigCategory.Curfew]: 'Curfew',
  [ConfigCategory.Gamification]: 'Gamification',
  [ConfigCategory.Notification]: 'Notification',
  [ConfigCategory.Security]: 'Security',
  [ConfigCategory.Integration]: 'Integration',
};

export default function SystemConfigPage() {
  const [category, setCategory] = useState<number | undefined>(undefined);
  const { data: configs, isLoading } = useSystemConfigs(category);
  const createMut = useCreateConfig();
  const updateMut = useUpdateConfig();
  const deleteMut = useDeleteConfig();
  const [showAdd, setShowAdd] = useState(false);
  const [editKey, setEditKey] = useState<string | null>(null);
  const [editVal, setEditVal] = useState('');
  const [form, setForm] = useState<CreateConfigRequest>({
    key: '', value: '', description: '', dataType: 'string', category: ConfigCategory.General,
  });

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    await createMut.mutateAsync(form);
    setShowAdd(false);
    setForm({ key: '', value: '', description: '', dataType: 'string', category: ConfigCategory.General });
  };

  const handleUpdate = async (key: string) => {
    const req: UpdateConfigRequest = { value: editVal };
    await updateMut.mutateAsync({ key, req });
    setEditKey(null);
  };

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="System Configuration">
        <button onClick={() => setShowAdd(!showAdd)} className="inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700">
          {showAdd ? <X className="h-4 w-4" /> : <Plus className="h-4 w-4" />}
          {showAdd ? 'Cancel' : 'Add Config'}
        </button>
      </PageHeader>

      {/* Category filter */}
      <div className="mb-4 flex gap-2 flex-wrap">
        <button onClick={() => setCategory(undefined)} className={`rounded-full px-3 py-1 text-sm ${category === undefined ? 'bg-indigo-600 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}>All</button>
        {Object.entries(categoryLabels).map(([k, v]) => (
          <button key={k} onClick={() => setCategory(Number(k))} className={`rounded-full px-3 py-1 text-sm ${category === Number(k) ? 'bg-indigo-600 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}>{v}</button>
        ))}
      </div>

      {showAdd && (
        <form onSubmit={handleCreate} className="mb-6 rounded-xl border border-gray-200 bg-white p-6">
          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Key</label>
              <input type="text" value={form.key} onChange={e => setForm({ ...form, key: e.target.value })} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" required />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Value</label>
              <input type="text" value={form.value} onChange={e => setForm({ ...form, value: e.target.value })} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" required />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Category</label>
              <select value={form.category} onChange={e => setForm({ ...form, category: Number(e.target.value) })} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm">
                {Object.entries(categoryLabels).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
              </select>
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-gray-700">Data Type</label>
              <select value={form.dataType} onChange={e => setForm({ ...form, dataType: e.target.value })} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm">
                <option value="string">String</option>
                <option value="int">Integer</option>
                <option value="bool">Boolean</option>
                <option value="double">Double</option>
                <option value="json">JSON</option>
              </select>
            </div>
            <div className="sm:col-span-2">
              <label className="mb-1 block text-sm font-medium text-gray-700">Description</label>
              <input type="text" value={form.description ?? ''} onChange={e => setForm({ ...form, description: e.target.value })} className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" />
            </div>
          </div>
          <div className="mt-4 flex justify-end">
            <button type="submit" disabled={createMut.isPending} className="rounded-lg bg-indigo-600 px-6 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-50">Create</button>
          </div>
        </form>
      )}

      <div className="rounded-xl border border-gray-200 bg-white">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
              <tr>
                <th className="px-4 py-3">Key</th>
                <th className="px-4 py-3">Value</th>
                <th className="px-4 py-3">Category</th>
                <th className="px-4 py-3">Type</th>
                <th className="px-4 py-3">Description</th>
                <th className="px-4 py-3">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {configs?.map(cfg => (
                <tr key={cfg.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-900">
                    <Settings className="mr-1 inline h-4 w-4 text-gray-400" />{cfg.key}
                  </td>
                  <td className="px-4 py-3">
                    {editKey === cfg.key ? (
                      <div className="flex gap-2">
                        <input type="text" value={editVal} onChange={e => setEditVal(e.target.value)} className="rounded border border-gray-300 px-2 py-1 text-sm" />
                        <button onClick={() => handleUpdate(cfg.key)} className="text-green-600 hover:text-green-800"><Save className="h-4 w-4" /></button>
                        <button onClick={() => setEditKey(null)} className="text-gray-400 hover:text-gray-600"><X className="h-4 w-4" /></button>
                      </div>
                    ) : (
                      <span className="text-gray-700">{cfg.value}</span>
                    )}
                  </td>
                  <td className="px-4 py-3"><span className="rounded-full bg-gray-100 px-2 py-0.5 text-xs text-gray-600">{categoryLabels[cfg.category]}</span></td>
                  <td className="px-4 py-3 text-gray-500">{cfg.dataType}</td>
                  <td className="max-w-xs truncate px-4 py-3 text-gray-400">{cfg.description || '-'}</td>
                  <td className="px-4 py-3">
                    <div className="flex gap-2">
                      <button onClick={() => { setEditKey(cfg.key); setEditVal(cfg.value); }} className="text-indigo-600 hover:text-indigo-800"><Pencil className="h-4 w-4" /></button>
                      <button onClick={() => { if (confirm(`Delete config "${cfg.key}"?`)) deleteMut.mutate(cfg.key); }} className="text-red-600 hover:text-red-800"><Trash2 className="h-4 w-4" /></button>
                    </div>
                  </td>
                </tr>
              ))}
              {(!configs || configs.length === 0) && (
                <tr><td colSpan={6} className="px-4 py-8 text-center text-gray-400">No configurations found</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
