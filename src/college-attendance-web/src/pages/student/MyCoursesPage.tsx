import { useCourses } from '../../hooks/useApi';
import { PageHeader, LoadingSpinner, EmptyState } from '../../components/ui';
import { BookOpen } from 'lucide-react';

export default function MyCoursesPage() {
  const { data: courses, isLoading } = useCourses();

  if (isLoading) return <LoadingSpinner />;
  if (!courses || courses.length === 0) return <><PageHeader title="My Courses" /><EmptyState message="You are not enrolled in any courses" /></>;

  return (
    <div>
      <PageHeader title="My Courses" />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {courses.map(c => (
          <div key={c.id} className="rounded-xl border border-gray-200 bg-white p-5">
            <div className="mb-3 flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-indigo-100">
                <BookOpen className="h-5 w-5 text-indigo-600" />
              </div>
              <div>
                <h3 className="font-semibold text-gray-900">{c.name}</h3>
                <p className="text-xs text-gray-500">{c.code}</p>
              </div>
            </div>
            <div className="space-y-1 text-sm text-gray-500">
              <p>Faculty: {c.facultyName}</p>
              <p>Department: {c.departmentName}</p>
              <p>Semester {c.semester}, Year {c.year}</p>
              <p>Credits: {c.credits}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
