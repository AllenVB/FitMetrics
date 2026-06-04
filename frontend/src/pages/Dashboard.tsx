import { useEffect, useState } from 'react';
import { Line, Doughnut } from 'react-chartjs-2';
import { dashboardApi } from '../api';
import { getErrorMessage } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { Card, ErrorAlert, PageHeader, Spinner, StatCard } from '../components/ui';
import { formatDate } from '../lib/labels';
import type { Dashboard as DashboardData } from '../types';

function greeting() {
  const h = new Date().getHours();
  if (h < 12) return 'Günaydın';
  if (h < 18) return 'İyi günler';
  return 'İyi akşamlar';
}

export default function Dashboard() {
  const { user } = useAuth();
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    dashboardApi.get()
      .then(setData)
      .catch((e) => setError(getErrorMessage(e)))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <Spinner />;
  if (error) return <ErrorAlert message={error} />;
  if (!data) return null;

  const { today, todayMacros } = data;
  const caloriePct = today.calorieGoal > 0 ? Math.min(100, Math.round((today.totalCalories / today.calorieGoal) * 100)) : 0;
  const proteinPct = today.proteinGoal > 0 ? Math.min(100, Math.round((today.totalProtein / today.proteinGoal) * 100)) : 0;

  const calorieTrendData = {
    labels: data.calorieTrend.map((p) => formatDate(p.date)),
    datasets: [
      {
        label: 'Alınan',
        data: data.calorieTrend.map((p) => p.calories),
        borderColor: '#059669',
        backgroundColor: 'rgba(16, 185, 129, 0.12)',
        fill: true,
        tension: 0.3,
      },
      {
        label: 'Hedef',
        data: data.calorieTrend.map((p) => p.goal),
        borderColor: '#94a3b8',
        borderDash: [5, 5],
        pointRadius: 0,
        fill: false,
      },
      {
        label: 'Yakılan',
        data: data.calorieTrend.map((p) => p.burnedCalories),
        borderColor: '#f43f5e',
        backgroundColor: 'rgba(244, 63, 94, 0.1)',
        tension: 0.3,
        fill: false,
      },
    ],
  };

  const macroData = {
    labels: ['Protein', 'Karbonhidrat', 'Yağ'],
    datasets: [{
      data: [todayMacros.protein, todayMacros.carbs, todayMacros.fat],
      backgroundColor: ['#10b981', '#3b82f6', '#f59e0b'],
      borderWidth: 0,
    }],
  };

  const weightTrendData = {
    labels: data.weightTrend.map((p) => formatDate(p.date)),
    datasets: [{
      label: 'Kilo (kg)',
      data: data.weightTrend.map((p) => p.weightKg),
      borderColor: '#7c3aed',
      backgroundColor: 'rgba(124, 58, 237, 0.12)',
      fill: true,
      tension: 0.3,
    }],
  };

  const noAxesLegendBottom = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom' as const, labels: { boxWidth: 12, font: { size: 11 } } } },
  };

  return (
    <div>
      <PageHeader
        title={`${greeting()}, ${user?.fullName?.split(' ')[0]} 👋`}
        subtitle="Bugünün özeti ve son 14 günlük gidişatın"
      />

      <div className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard icon="🔥" label="Kalori" value={`${Math.round(today.totalCalories)}`} sub={`/ ${today.calorieGoal} kcal • %${caloriePct}`} accent="amber" />
        <StatCard icon="🥩" label="Protein" value={`${Math.round(today.totalProtein)} g`} sub={`/ ${today.proteinGoal} g • %${proteinPct}`} accent="brand" />
        <StatCard icon="💪" label="Bu Hafta Antrenman" value={data.workoutsThisWeek} sub={`${Math.round(data.caloriesBurnedThisWeek)} kcal yakıldı`} accent="rose" />
        <StatCard icon="⚖️" label="Kilo / BMI" value={`${data.currentWeightKg ?? '-'} kg`} sub={`BMI ${data.bmi}${data.targetWeightKg ? ` • hedef ${data.targetWeightKg} kg` : ''}`} accent="violet" />
      </div>

      <div className="mb-6 grid gap-4 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <h2 className="mb-3 font-semibold text-slate-800">Kalori Trendi (14 gün)</h2>
          <div className="h-64">
            <Line data={calorieTrendData} options={noAxesLegendBottom} />
          </div>
        </Card>
        <Card>
          <h2 className="mb-3 font-semibold text-slate-800">Bugünkü Makrolar</h2>
          {todayMacros.protein + todayMacros.carbs + todayMacros.fat > 0 ? (
            <div className="h-64">
              <Doughnut data={macroData} options={noAxesLegendBottom} />
            </div>
          ) : (
            <div className="flex h-64 items-center justify-center text-sm text-slate-400">
              Bugün için besin kaydı yok
            </div>
          )}
        </Card>
      </div>

      <Card>
        <h2 className="mb-3 font-semibold text-slate-800">Kilo Değişimi</h2>
        {data.weightTrend.length > 1 ? (
          <div className="h-64">
            <Line data={weightTrendData} options={noAxesLegendBottom} />
          </div>
        ) : (
          <div className="flex h-32 items-center justify-center text-sm text-slate-400">
            Kilo grafiği için en az iki kayıt gerekli. İlerleme sayfasından kilo ekleyebilirsin.
          </div>
        )}
      </Card>
    </div>
  );
}
