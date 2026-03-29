import { useState } from 'react';
import { useMessLogs, useLogMess } from '../../hooks/useApi';
import { MealType, type CreateMessLogRequest } from '../../types';
import { PageHeader, LoadingSpinner, EmptyState, Badge } from '../../components/ui';
import { UtensilsCrossed, Plus, X } from 'lucide-react';
import { format } from 'date-fns';

export default function MessPage() {
  const [showForm, setShowForm] = useState(false);
  const { data: records, isLoading } = useMessLogs();

  if (isLoading) return <LoadingSpinner />;

  const mealColor = (m: MealType): 'blue' | 'yellow' | 'green' | 'indigo' => {
    switch (m) {
      case MealType.Breakfast: return 'blue';
      case MealType.Lunch: return 'yellow';
      case MealType.Dinner: return 'green';
      default: return 'indigo';
    }
  };

  return (
    <div>
      <PageHeader title="Mess Management">
        <button onClick={() => setShowForm(true)} className="inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700">
          <Plus className="h-4 w-4" /> Record Meal
        </button>
      </PageHeader>

      {showForm && <RecordMealForm onClose={() => setShowForm(false)} />}

      {!records || records.length === 0 ? (
        <EmptyState message="No mess records found" />
      ) : (
        <div className="overflow-x-auto rounded-xl border border-gray-200 bg-white">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
              <tr>
                <th className="px-4 py-3">Student</th>
                <th className="px-4 py-3">Meal</th>
                <th className="px-4 py-3">Date</th>
                <th className="px-4 py-3">Time</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {records.map(r => (
                <tr key={r.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <UtensilsCrossed className="h-4 w-4 text-indigo-500" />
                      <span className="font-medium text-gray-900">{r.studentName}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3"><Badge label={MealType[r.mealType]} variant={mealColor(r.mealType)} /></td>
                  <td className="px-4 py-3 text-gray-500">{format(new Date(r.date), 'MMM d, yyyy')}</td>
                  <td className="px-4 py-3 text-gray-500">{r.scannedAt ? format(new Date(r.scannedAt), 'HH:mm') : '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function RecordMealForm({ onClose }: { onClose: () => void }) {
  const recordMeal = useLogMess();
  const [form, setForm] = useState<CreateMessLogRequest>({
    studentId: '', mealType: MealType.Lunch,
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await recordMeal.mutateAsync(form);
    onClose();
  };

  return (
    <div className="mb-6 rounded-xl border border-gray-200 bg-white p-6">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-900">Record Meal</h3>
        <button onClick={onClose} aria-label="Close"><X className="h-5 w-5 text-gray-400" /></button>
      </div>
      <form onSubmit={handleSubmit} className="grid gap-4 sm:grid-cols-2">
        <input placeholder="Student ID" required value={form.studentId}
          onChange={e => setForm(f => ({ ...f, studentId: e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <select required value={form.mealType} onChange={e => setForm(f => ({ ...f, mealType: +e.target.value as MealType }))} aria-label="Meal type"
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500">
          <option value={MealType.Breakfast}>Breakfast</option>
          <option value={MealType.Lunch}>Lunch</option>
          <option value={MealType.Dinner}>Dinner</option>
          <option value={MealType.Snacks}>Snacks</option>
        </select>
        <input placeholder="Verification Method (optional)" value={form.verificationMethod ?? ''}
          onChange={e => setForm(f => ({ ...f, verificationMethod: e.target.value || undefined }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <div className="flex justify-end gap-2 sm:col-span-2">
          <button type="button" onClick={onClose} className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">Cancel</button>
          <button type="submit" disabled={recordMeal.isPending}
            className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-50">
            {recordMeal.isPending ? 'Recording...' : 'Record'}
          </button>
        </div>
      </form>
    </div>
  );
}
