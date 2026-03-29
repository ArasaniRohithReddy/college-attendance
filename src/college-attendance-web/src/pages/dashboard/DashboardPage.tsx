import { useAuth } from '../../contexts/AuthContext';
import { UserRole } from '../../types';
import { useDashboardAnalytics, useTodaySessions, useDefaulters } from '../../hooks/useApi';
import { StatCard, PageHeader, LoadingSpinner } from '../../components/ui';
import { Users, BookOpen, CalendarDays, AlertTriangle, BarChart3, TrendingUp } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

export default function DashboardPage() {
  const { hasRole } = useAuth();

  if (hasRole(UserRole.Admin)) return <AdminDashboard />;
  if (hasRole(UserRole.Faculty)) return <FacultyDashboard />;
  if (hasRole(UserRole.Student)) return <StudentDashboard />;
  return <StaffDashboard />;
}

function AdminDashboard() {
  const { data, isLoading } = useDashboardAnalytics();
  const { data: defaulters } = useDefaulters(75);

  if (isLoading || !data) return <LoadingSpinner />;

  const deptData = (data.departmentWise ?? []).map(d => ({
    name: d.departmentName,
    attendance: Math.round(d.attendancePercentage),
    students: d.studentCount,
  }));

  return (
    <div>
      <PageHeader title="Admin Dashboard" />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard title="Total Students" value={data.totalStudents} icon={<Users className="h-6 w-6" />} color="indigo" />
        <StatCard title="Total Faculty" value={data.totalFaculty} icon={<Users className="h-6 w-6" />} color="blue" />
        <StatCard title="Courses" value={data.totalCourses} icon={<BookOpen className="h-6 w-6" />} color="green" />
        <StatCard title="Sessions Today" value={data.totalSessionsToday} icon={<CalendarDays className="h-6 w-6" />} color="yellow" />
      </div>

      <div className="mt-6 grid gap-6 lg:grid-cols-2">
        <div className="rounded-xl border border-gray-200 bg-white p-6">
          <h3 className="mb-4 flex items-center gap-2 text-lg font-semibold text-gray-900">
            <BarChart3 className="h-5 w-5 text-indigo-600" /> Overall Attendance
          </h3>
          <div className="flex items-center gap-4">
            <div className="relative h-32 w-32">
              <svg viewBox="0 0 36 36" className="h-full w-full -rotate-90">
                <circle cx="18" cy="18" r="15.5" fill="none" stroke="#e5e7eb" strokeWidth="3" />
                <circle cx="18" cy="18" r="15.5" fill="none" stroke="#4f46e5" strokeWidth="3"
                  strokeDasharray={`${data.overallAttendancePercentage} ${100 - data.overallAttendancePercentage}`} strokeLinecap="round" />
              </svg>
              <div className="absolute inset-0 flex items-center justify-center">
                <span className="text-2xl font-bold text-gray-900">{Math.round(data.overallAttendancePercentage)}%</span>
              </div>
            </div>
            <div>
              <p className="text-sm text-gray-500">Defaulters ({`<75%`})</p>
              <p className="text-3xl font-bold text-red-600">{data.defaulterCount}</p>
            </div>
          </div>
        </div>

        <div className="rounded-xl border border-gray-200 bg-white p-6">
          <h3 className="mb-4 flex items-center gap-2 text-lg font-semibold text-gray-900">
            <TrendingUp className="h-5 w-5 text-indigo-600" /> Department-wise Attendance
          </h3>
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={deptData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="name" tick={{ fontSize: 12 }} />
              <YAxis domain={[0, 100]} tick={{ fontSize: 12 }} />
              <Tooltip />
              <Bar dataKey="attendance" fill="#4f46e5" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      {defaulters && defaulters.length > 0 && (
        <div className="mt-6 rounded-xl border border-gray-200 bg-white p-6">
          <h3 className="mb-4 flex items-center gap-2 text-lg font-semibold text-gray-900">
            <AlertTriangle className="h-5 w-5 text-red-500" /> Defaulters List
          </h3>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="border-b border-gray-200 text-xs uppercase text-gray-500">
                <tr>
                  <th className="px-4 py-3">Student</th>
                  <th className="px-4 py-3">ID</th>
                  <th className="px-4 py-3">Course</th>
                  <th className="px-4 py-3">Attendance</th>
                  <th className="px-4 py-3">Present</th>
                  <th className="px-4 py-3">Absent</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {defaulters.slice(0, 10).map((d) => (
                  <tr key={`${d.studentId}-${d.courseCode}`} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium text-gray-900">{d.studentName}</td>
                    <td className="px-4 py-3 text-gray-500">{d.studentIdNumber}</td>
                    <td className="px-4 py-3 text-gray-500">{d.courseCode}</td>
                    <td className="px-4 py-3">
                      <span className={`font-semibold ${d.attendancePercentage < 75 ? 'text-red-600' : 'text-green-600'}`}>
                        {d.attendancePercentage.toFixed(1)}%
                      </span>
                    </td>
                    <td className="px-4 py-3 text-gray-500">{d.presentCount}/{d.totalSessions}</td>
                    <td className="px-4 py-3 text-gray-500">{d.absentCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}

function FacultyDashboard() {
  const { data: sessions, isLoading } = useTodaySessions();

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="Faculty Dashboard" />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <StatCard title="Today's Sessions" value={sessions?.length ?? 0} icon={<CalendarDays className="h-6 w-6" />} color="indigo" />
        <StatCard title="Active" value={sessions?.filter(s => s.status === 1).length ?? 0} icon={<BarChart3 className="h-6 w-6" />} color="green" />
        <StatCard title="Completed" value={sessions?.filter(s => s.status === 2).length ?? 0} icon={<BookOpen className="h-6 w-6" />} color="blue" />
      </div>
      {sessions && sessions.length > 0 && (
        <div className="mt-6 rounded-xl border border-gray-200 bg-white p-6">
          <h3 className="mb-4 text-lg font-semibold text-gray-900">Today's Sessions</h3>
          <div className="space-y-3">
            {sessions.map(s => (
              <div key={s.id} className="flex items-center justify-between rounded-lg border border-gray-100 p-4 hover:bg-gray-50">
                <div>
                  <p className="font-medium text-gray-900">{s.title}</p>
                  <p className="text-sm text-gray-500">{s.courseName} &middot; {s.room ?? 'No room'}</p>
                </div>
                <div className="text-right">
                  <p className="text-sm font-medium text-gray-900">{s.presentCount}/{s.totalStudents}</p>
                  <p className="text-xs text-gray-500">
                    {['Scheduled', 'Active', 'Completed', 'Cancelled'][s.status]}
                  </p>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

function StudentDashboard() {
  const { user } = useAuth();

  return (
    <div>
      <PageHeader title={`Welcome, ${user?.fullName?.split(' ')[0]}`} />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <StatCard title="My Courses" value="—" icon={<BookOpen className="h-6 w-6" />} color="indigo" />
        <StatCard title="Attendance" value="—" icon={<BarChart3 className="h-6 w-6" />} color="green" />
        <StatCard title="Pending Outings" value="—" icon={<CalendarDays className="h-6 w-6" />} color="yellow" />
      </div>
      <div className="mt-6 rounded-xl border border-dashed border-gray-300 p-12 text-center text-gray-500">
        Scan QR codes from the mobile app to mark attendance
      </div>
    </div>
  );
}

function StaffDashboard() {
  return (
    <div>
      <PageHeader title="Dashboard" />
      <div className="grid gap-4 sm:grid-cols-2">
        <StatCard title="Pending Outings" value="—" icon={<AlertTriangle className="h-6 w-6" />} color="yellow" />
        <StatCard title="Today's Logs" value="—" icon={<CalendarDays className="h-6 w-6" />} color="indigo" />
      </div>
    </div>
  );
}
