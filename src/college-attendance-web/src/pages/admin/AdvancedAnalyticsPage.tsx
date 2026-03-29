import { useAdvancedDashboard, useHeatmap, useDropoutRisk, useFacultyStrictness, useCourseAnalytics } from '../../hooks/useApi';
import { PageHeader, LoadingSpinner, StatCard } from '../../components/ui';
import { AlertTriangle, UserCheck, BookOpen, Siren, FileWarning } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

export default function AdvancedAnalyticsPage() {
  const { data: dashboard, isLoading: l1 } = useAdvancedDashboard();
  const { data: heatmap, isLoading: l2 } = useHeatmap();
  const { data: riskStudents, isLoading: l3 } = useDropoutRisk();
  const { data: faculty, isLoading: l4 } = useFacultyStrictness();
  const { data: courses, isLoading: l5 } = useCourseAnalytics();

  if (l1 || l2 || l3 || l4 || l5) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="Advanced Analytics" />

      {dashboard && (
        <div className="mb-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <StatCard icon={<Siren className="h-6 w-6" />} title="Active SOS" value={dashboard.activeSOSCount} color="red" />
          <StatCard icon={<FileWarning className="h-6 w-6" />} title="Pending Leaves" value={dashboard.pendingLeaves} color="yellow" />
          <StatCard icon={<AlertTriangle className="h-6 w-6" />} title="Fraud Alerts Today" value={dashboard.fraudAlertsToday} color="orange" />
          <StatCard icon={<UserCheck className="h-6 w-6" />} title="Curfew Violations" value={dashboard.curfewViolationsToday} color="purple" />
        </div>
      )}

      {/* Attendance Heatmap */}
      {heatmap && heatmap.length > 0 && (
        <div className="mb-8 rounded-xl border border-gray-200 bg-white p-6">
          <h3 className="mb-4 text-lg font-semibold text-gray-900">Attendance Heatmap</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={heatmap}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="dayOfWeek" />
              <YAxis domain={[0, 100]} />
              <Tooltip formatter={(v) => `${Number(v).toFixed(1)}%`} />
              <Bar dataKey="attendancePercentage" name="Attendance %" fill="#6366f1" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* Dropout Risk */}
      {riskStudents && riskStudents.length > 0 && (
        <div className="mb-8 rounded-xl border border-gray-200 bg-white p-6">
          <h3 className="mb-4 text-lg font-semibold text-gray-900">
            <AlertTriangle className="mr-2 inline h-5 w-5 text-red-500" />Dropout Risk Students
          </h3>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
                <tr>
                  <th className="px-4 py-3">Student</th>
                  <th className="px-4 py-3">Department</th>
                  <th className="px-4 py-3">Attendance</th>
                  <th className="px-4 py-3">Risk Score</th>
                  <th className="px-4 py-3">Risk Level</th>
                  <th className="px-4 py-3">Factors</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {riskStudents.map(s => (
                  <tr key={s.studentId} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium text-gray-900">{s.studentName}</td>
                    <td className="px-4 py-3 text-gray-500">{s.departmentName || '-'}</td>
                    <td className="px-4 py-3 text-gray-500">{s.attendancePercentage.toFixed(1)}%</td>
                    <td className="px-4 py-3 font-semibold text-gray-900">{s.riskScore.toFixed(2)}</td>
                    <td className="px-4 py-3">
                      <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                        s.riskLevel === 'High' ? 'bg-red-100 text-red-700' :
                        s.riskLevel === 'Medium' ? 'bg-yellow-100 text-yellow-700' : 'bg-green-100 text-green-700'
                      }`}>{s.riskLevel}</span>
                    </td>
                    <td className="max-w-xs px-4 py-3 text-xs text-gray-400">{s.riskFactors.join(', ')}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Faculty Strictness */}
      {faculty && faculty.length > 0 && (
        <div className="mb-8 rounded-xl border border-gray-200 bg-white p-6">
          <h3 className="mb-4 text-lg font-semibold text-gray-900">
            <UserCheck className="mr-2 inline h-5 w-5 text-indigo-500" />Faculty Strictness
          </h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={faculty}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="facultyName" />
              <YAxis domain={[0, 100]} />
              <Tooltip />
              <Bar dataKey="averageAttendance" name="Avg Attendance %" fill="#10b981" radius={[4, 4, 0, 0]} />
              <Bar dataKey="lateMarkPercentage" name="Late Mark %" fill="#f59e0b" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* Course Analytics */}
      {courses && courses.length > 0 && (
        <div className="rounded-xl border border-gray-200 bg-white p-6">
          <h3 className="mb-4 text-lg font-semibold text-gray-900">
            <BookOpen className="mr-2 inline h-5 w-5 text-blue-500" />Course Analytics
          </h3>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
                <tr>
                  <th className="px-4 py-3">Course</th>
                  <th className="px-4 py-3">Code</th>
                  <th className="px-4 py-3">Attendance</th>
                  <th className="px-4 py-3">Enrolled</th>
                  <th className="px-4 py-3">Defaulters</th>
                  <th className="px-4 py-3">Trend</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {courses.map(c => (
                  <tr key={c.courseId} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium text-gray-900">{c.courseName}</td>
                    <td className="px-4 py-3 text-gray-500">{c.courseCode}</td>
                    <td className="px-4 py-3 text-gray-500">{c.attendancePercentage.toFixed(1)}%</td>
                    <td className="px-4 py-3 text-gray-500">{c.enrolledCount}</td>
                    <td className="px-4 py-3 text-gray-500">{c.defaulterCount}</td>
                    <td className="px-4 py-3">
                      <span className={c.trendPercentage >= 0 ? 'text-green-600' : 'text-red-600'}>
                        {c.trendPercentage >= 0 ? '+' : ''}{c.trendPercentage.toFixed(1)}%
                      </span>
                    </td>
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
