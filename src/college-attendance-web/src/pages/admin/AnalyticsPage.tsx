import { useDashboardAnalytics, usePredictions } from '../../hooks/useApi';
import { PageHeader, LoadingSpinner, StatCard } from '../../components/ui';
import { BarChart3, TrendingUp, Users, AlertTriangle } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

export default function AnalyticsPage() {
  const { data: overview, isLoading: l1 } = useDashboardAnalytics();
  const { data: predictions, isLoading: l2 } = usePredictions();

  if (l1 || l2) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="Analytics & Predictions" />

      {overview && (
        <div className="mb-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <StatCard icon={<Users className="h-6 w-6" />} title="Total Students" value={overview.totalStudents} color="indigo" />
          <StatCard icon={<BarChart3 className="h-6 w-6" />} title="Avg Attendance" value={`${overview.overallAttendancePercentage.toFixed(1)}%`} color="green" />
          <StatCard icon={<AlertTriangle className="h-6 w-6" />} title="Defaulters" value={overview.defaulterCount} color="red" />
          <StatCard icon={<TrendingUp className="h-6 w-6" />} title="Total Sessions" value={overview.totalSessionsToday} color="blue" />
        </div>
      )}

      {predictions && predictions.length > 0 && (
        <div className="rounded-xl border border-gray-200 bg-white p-6">
          <h3 className="mb-4 text-lg font-semibold text-gray-900">Predicted At-Risk Students</h3>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
                <tr>
                  <th className="px-4 py-3">Student</th>
                  <th className="px-4 py-3">Current %</th>
                  <th className="px-4 py-3">Predicted %</th>
                  <th className="px-4 py-3">Risk Level</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {predictions.map((p, i) => (
                  <tr key={i} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium text-gray-900">{p.studentName}</td>
                    <td className="px-4 py-3 text-gray-500">{p.currentPercentage.toFixed(1)}%</td>
                    <td className="px-4 py-3 text-gray-500">{p.predictedEndPercentage.toFixed(1)}%</td>
                    <td className="px-4 py-3">
                      <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                        p.riskLevel === 'High' ? 'bg-red-100 text-red-700' :
                        p.riskLevel === 'Medium' ? 'bg-yellow-100 text-yellow-700' : 'bg-green-100 text-green-700'
                      }`}>{p.riskLevel}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {overview && (
        <div className="mt-8 rounded-xl border border-gray-200 bg-white p-6">
          <h3 className="mb-4 text-lg font-semibold text-gray-900">Attendance by Department</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={overview.departmentWise}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="departmentName" />
              <YAxis domain={[0, 100]} />
              <Tooltip />
              <Bar dataKey="attendancePercentage" name="Attendance %" fill="#6366f1" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}
    </div>
  );
}
