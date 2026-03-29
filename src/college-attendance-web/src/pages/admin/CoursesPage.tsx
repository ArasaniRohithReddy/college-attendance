import { useState } from 'react';
import { useCourses, useCreateCourse, useDepartments, useUsersByRole } from '../../hooks/useApi';
import { UserRole, type CreateCourseRequest } from '../../types';
import { PageHeader, LoadingSpinner, EmptyState, Badge } from '../../components/ui';
import { Plus, X, BookOpen } from 'lucide-react';

export default function CoursesPage() {
  const [showForm, setShowForm] = useState(false);
  const { data, isLoading } = useCourses();

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="Courses">
        <button onClick={() => setShowForm(true)} className="inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700">
          <Plus className="h-4 w-4" /> Add Course
        </button>
      </PageHeader>

      {showForm && <CreateCourseForm onClose={() => setShowForm(false)} />}

      {!data || data.length === 0 ? (
        <EmptyState message="No courses found" />
      ) : (
        <div className="overflow-x-auto rounded-xl border border-gray-200 bg-white">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
              <tr>
                <th className="px-4 py-3">Course</th>
                <th className="px-4 py-3">Code</th>
                <th className="px-4 py-3">Department</th>
                <th className="px-4 py-3">Faculty</th>
                <th className="px-4 py-3">Semester</th>
                <th className="px-4 py-3">Credits</th>
                <th className="px-4 py-3">Enrolled</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {data.map(c => (
                <tr key={c.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <BookOpen className="h-4 w-4 text-indigo-500" />
                      <span className="font-medium text-gray-900">{c.name}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3"><Badge label={c.code} variant="indigo" /></td>
                  <td className="px-4 py-3 text-gray-500">{c.departmentName}</td>
                  <td className="px-4 py-3 text-gray-500">{c.facultyName}</td>
                  <td className="px-4 py-3 text-gray-500">Sem {c.semester}, Year {c.year}</td>
                  <td className="px-4 py-3 text-gray-500">{c.credits}</td>
                  <td className="px-4 py-3 text-gray-500">{c.enrolledStudents}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function CreateCourseForm({ onClose }: { onClose: () => void }) {
  const createCourse = useCreateCourse();
  const { data: depts } = useDepartments();
  const { data: faculty } = useUsersByRole(UserRole.Faculty);
  const [form, setForm] = useState<CreateCourseRequest>({
    name: '', code: '', credits: 3, semester: 1, year: 1, departmentId: '', facultyId: '',
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await createCourse.mutateAsync(form);
    onClose();
  };

  return (
    <div className="mb-6 rounded-xl border border-gray-200 bg-white p-6">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-lg font-semibold text-gray-900">New Course</h3>
        <button onClick={onClose} aria-label="Close"><X className="h-5 w-5 text-gray-400" /></button>
      </div>
      <form onSubmit={handleSubmit} className="grid gap-4 sm:grid-cols-2">
        <input placeholder="Course Name" required value={form.name}
          onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <input placeholder="Code (e.g. CS201)" required value={form.code}
          onChange={e => setForm(f => ({ ...f, code: e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <select required value={form.departmentId} onChange={e => setForm(f => ({ ...f, departmentId: e.target.value }))} aria-label="Department"
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500">
          <option value="">Select Department</option>
          {depts?.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
        </select>
        <select required value={form.facultyId} onChange={e => setForm(f => ({ ...f, facultyId: e.target.value }))} aria-label="Faculty"
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500">
          <option value="">Select Faculty</option>
          {faculty?.map(f => <option key={f.id} value={f.id}>{f.fullName}</option>)}
        </select>
        <input type="number" placeholder="Credits" required min={1} max={10} value={form.credits}
          onChange={e => setForm(f => ({ ...f, credits: +e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <input type="number" placeholder="Semester" required min={1} max={8} value={form.semester}
          onChange={e => setForm(f => ({ ...f, semester: +e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <input type="number" placeholder="Year" required min={1} max={5} value={form.year}
          onChange={e => setForm(f => ({ ...f, year: +e.target.value }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <textarea placeholder="Description (optional)" rows={2} value={form.description ?? ''}
          onChange={e => setForm(f => ({ ...f, description: e.target.value || undefined }))}
          className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500" />
        <div className="sm:col-span-2 flex justify-end gap-2">
          <button type="button" onClick={onClose} className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">Cancel</button>
          <button type="submit" disabled={createCourse.isPending}
            className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-50">
            {createCourse.isPending ? 'Creating...' : 'Create'}
          </button>
        </div>
      </form>
    </div>
  );
}
