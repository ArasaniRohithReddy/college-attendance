import { useGamificationDashboard, useLeaderboard } from '../../hooks/useApi';
import { PageHeader, LoadingSpinner, StatCard } from '../../components/ui';
import { Trophy, Flame, Medal, Star } from 'lucide-react';
import { useState } from 'react';

export default function GamificationPage() {
  const { data: dashboard, isLoading } = useGamificationDashboard();
  const [period, setPeriod] = useState('monthly');
  const { data: leaderboard } = useLeaderboard(period);

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <PageHeader title="Gamification & Rewards" />

      {dashboard && (
        <>
          <div className="mb-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <StatCard icon={<Flame className="h-6 w-6" />} title="Current Streak" value={`${dashboard.streak.currentStreak} days`} color="orange" />
            <StatCard icon={<Trophy className="h-6 w-6" />} title="Rank" value={`#${dashboard.currentRank}`} color="yellow" />
            <StatCard icon={<Star className="h-6 w-6" />} title="Total Score" value={dashboard.totalScore.toFixed(0)} color="indigo" />
            <StatCard icon={<Medal className="h-6 w-6" />} title="Badges Earned" value={dashboard.totalBadges} color="green" />
          </div>

          {/* Badges */}
          <div className="mb-8 rounded-xl border border-gray-200 bg-white p-6">
            <h3 className="mb-4 text-lg font-semibold text-gray-900">Your Badges</h3>
            {!dashboard.badges?.length ? (
              <p className="text-gray-500">No badges earned yet. Keep attending classes!</p>
            ) : (
              <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {dashboard.badges.map(sb => (
                  <div key={sb.id} className="flex items-center gap-3 rounded-lg border border-gray-100 bg-gray-50 p-4">
                    <div className="flex h-12 w-12 items-center justify-center rounded-full bg-yellow-100 text-2xl">🏅</div>
                    <div>
                      <p className="font-medium text-gray-900">{sb.badge.name}</p>
                      <p className="text-sm text-gray-500">{sb.badge.description}</p>
                      <p className="text-xs text-gray-400">Earned {new Date(sb.earnedAt).toLocaleDateString()}</p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Streak Details */}
          <div className="mb-8 rounded-xl border border-gray-200 bg-white p-6">
            <h3 className="mb-4 text-lg font-semibold text-gray-900">Streak Details</h3>
            <div className="grid gap-4 sm:grid-cols-3">
              <div className="text-center">
                <p className="text-3xl font-bold text-orange-600">{dashboard.streak.currentStreak}</p>
                <p className="text-sm text-gray-500">Current Streak</p>
              </div>
              <div className="text-center">
                <p className="text-3xl font-bold text-indigo-600">{dashboard.streak.longestStreak}</p>
                <p className="text-sm text-gray-500">Longest Streak</p>
              </div>
              <div className="text-center">
                <p className="text-3xl font-bold text-green-600">{dashboard.streak.totalPresentDays}</p>
                <p className="text-sm text-gray-500">Total Present Days</p>
              </div>
            </div>
          </div>
        </>
      )}

      {/* Leaderboard */}
      <div className="rounded-xl border border-gray-200 bg-white p-6">
        <div className="mb-4 flex items-center justify-between">
          <h3 className="text-lg font-semibold text-gray-900">Leaderboard</h3>
          <select
            value={period}
            onChange={e => setPeriod(e.target.value)}
            className="rounded-lg border border-gray-300 px-3 py-1.5 text-sm"
          >
            <option value="weekly">Weekly</option>
            <option value="monthly">Monthly</option>
            <option value="semester">Semester</option>
          </select>
        </div>
        {leaderboard && leaderboard.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="border-b border-gray-200 bg-gray-50 text-xs uppercase text-gray-500">
                <tr>
                  <th className="px-4 py-3">Rank</th>
                  <th className="px-4 py-3">Student</th>
                  <th className="px-4 py-3">Department</th>
                  <th className="px-4 py-3">Score</th>
                  <th className="px-4 py-3">Streak</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {leaderboard.map(e => (
                  <tr key={e.studentId} className="hover:bg-gray-50">
                    <td className="px-4 py-3">
                      <span className={`inline-flex h-7 w-7 items-center justify-center rounded-full text-xs font-bold ${
                        e.rank <= 3 ? 'bg-yellow-100 text-yellow-700' : 'bg-gray-100 text-gray-600'}`}>
                        {e.rank}
                      </span>
                    </td>
                    <td className="px-4 py-3 font-medium text-gray-900">{e.studentName}</td>
                    <td className="px-4 py-3 text-gray-500">{e.departmentName || '-'}</td>
                    <td className="px-4 py-3 font-semibold text-indigo-600">{e.totalScore.toFixed(1)}</td>
                    <td className="px-4 py-3 text-gray-500">{e.streakScore.toFixed(0)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="text-gray-500">No leaderboard data available.</p>
        )}
      </div>
    </div>
  );
}
