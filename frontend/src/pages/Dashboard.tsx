import { useEffect, useState, type ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { Line, Doughnut } from 'react-chartjs-2';
import { dashboardApi, waterApi } from '../api';
import { getErrorMessage } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { Card, ErrorAlert, Icon, PageHeader, Spinner, StatCard } from '../components/ui';
import { formatDate } from '../lib/labels';
import { chartColors } from '../lib/charts';
import type { Dashboard as DashboardData, WaterToday } from '../types';

function greeting() {
  const h = new Date().getHours();
  if (h < 12) return 'Günaydın';
  if (h < 18) return 'İyi günler';
  return 'İyi akşamlar';
}

function MiniProgress({ value, goal, unit, accent }: { value: number; goal: number; unit: string; accent: string }) {
  const pct = goal > 0 ? Math.min(100, Math.round((value / goal) * 100)) : 0;
  return (
    <div className="mt-1">
      <div className="mb-1.5 flex items-center justify-between text-[11px] text-on-surface-variant">
        <span>/ {goal} {unit}</span>
        <span className="font-bold text-on-surface">%{pct}</span>
      </div>
      <div className="h-1.5 w-full overflow-hidden rounded-full bg-white/5">
        <div className={`h-full rounded-full ${accent}`} style={{ width: `${pct}%` }} />
      </div>
    </div>
  );
}

function Ring({ value, goal, color, children }: { value: number; goal: number; color: string; children: ReactNode }) {
  const pct = goal > 0 ? Math.min(100, (value / goal) * 100) : 0;
  const r = 34;
  const circ = 2 * Math.PI * r;
  const offset = circ - (pct / 100) * circ;
  return (
    <div className="relative h-24 w-24 shrink-0">
      <svg viewBox="0 0 80 80" className="h-24 w-24 -rotate-90">
        <circle cx="40" cy="40" r={r} fill="none" stroke="rgba(255,255,255,0.08)" strokeWidth="8" />
        <circle
          cx="40" cy="40" r={r} fill="none" stroke={color} strokeWidth="8" strokeLinecap="round"
          strokeDasharray={circ} strokeDashoffset={offset} className="transition-all duration-700"
        />
      </svg>
      <div className="absolute inset-0 flex flex-col items-center justify-center">{children}</div>
    </div>
  );
}

export default function Dashboard() {
  const { user } = useAuth();
  const [data, setData] = useState<DashboardData | null>(null);
  const [water, setWater] = useState<WaterToday | null>(null);
  const [addingWater, setAddingWater] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    dashboardApi.get()
      .then((d) => {
        setData(d);
        setWater({ intakeMl: d.waterIntakeMl, goalMl: d.waterGoalMl });
      })
      .catch((e) => setError(getErrorMessage(e)))
      .finally(() => setLoading(false));
  }, []);

  const addWater = async (ml: number) => {
    setAddingWater(true);
    try {
      setWater(await waterApi.add(ml));
    } catch (e) {
      setError(getErrorMessage(e));
    } finally {
      setAddingWater(false);
    }
  };

  if (loading) return <Spinner />;
  if (error && !data) return <ErrorAlert message={error} />;
  if (!data) return null;

  const { today, todayMacros } = data;

  // Seri: bugünden geriye, kalori girilen ardışık gün sayısı
  let streak = 0;
  for (let i = data.calorieTrend.length - 1; i >= 0; i--) {
    if (data.calorieTrend[i].calories > 0) streak++;
    else break;
  }

  const badges = [
    { label: 'Kalori hedefi', icon: 'local_fire_department',
      met: today.calorieGoal > 0 && today.totalCalories >= today.calorieGoal * 0.9 && today.totalCalories <= today.calorieGoal * 1.1 },
    { label: 'Protein hedefi', icon: 'egg_alt', met: today.proteinGoal > 0 && today.totalProtein >= today.proteinGoal },
    { label: 'Su hedefi', icon: 'water_drop', met: !!water && water.goalMl > 0 && water.intakeMl >= water.goalMl },
    { label: '3+ antrenman', icon: 'fitness_center', met: data.workoutsThisWeek >= 3 },
  ];

  const calorieTrendData = {
    labels: data.calorieTrend.map((p) => formatDate(p.date)),
    datasets: [
      {
        label: 'Alınan', data: data.calorieTrend.map((p) => p.calories),
        borderColor: chartColors.primary, backgroundColor: chartColors.primaryFill,
        fill: true, tension: 0.4, pointRadius: 0, pointHoverRadius: 5, borderWidth: 3,
      },
      {
        label: 'Hedef', data: data.calorieTrend.map((p) => p.goal),
        borderColor: chartColors.outline, borderDash: [5, 5], pointRadius: 0, fill: false, borderWidth: 1.5,
      },
      {
        label: 'Yakılan', data: data.calorieTrend.map((p) => p.burnedCalories),
        borderColor: chartColors.error, backgroundColor: chartColors.errorFill,
        tension: 0.4, fill: false, pointRadius: 0, pointHoverRadius: 5, borderWidth: 2,
      },
    ],
  };

  const macroData = {
    labels: ['Protein', 'Karbonhidrat', 'Yağ'],
    datasets: [{
      data: [todayMacros.protein, todayMacros.carbs, todayMacros.fat],
      backgroundColor: [chartColors.tertiary, chartColors.primary, chartColors.secondary],
      borderWidth: 0,
    }],
  };

  const weightTrendData = {
    labels: data.weightTrend.map((p) => formatDate(p.date)),
    datasets: [{
      label: 'Kilo (kg)', data: data.weightTrend.map((p) => p.weightKg),
      borderColor: chartColors.secondary, backgroundColor: chartColors.secondaryFill,
      fill: true, tension: 0.4, pointRadius: 0, pointHoverRadius: 5, borderWidth: 3,
    }],
  };

  const lineOpts = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom' as const, labels: { boxWidth: 12, usePointStyle: true, font: { size: 11 } } } },
    scales: {
      x: { grid: { display: false } },
      y: { grid: { color: 'rgba(255,255,255,0.05)' }, border: { display: false } },
    },
  };

  const doughnutOpts = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '68%',
    plugins: { legend: { position: 'bottom' as const, labels: { boxWidth: 12, usePointStyle: true, font: { size: 11 } } } },
  };

  return (
    <div className="space-y-6">
      <PageHeader
        title={`${greeting()}, ${user?.fullName?.split(' ')[0]} 👋`}
        subtitle="Bugünün özeti ve son 14 günlük gidişatın"
        action={
          <div className="flex gap-3">
            <span className="flex items-center gap-2 rounded-xl border border-white/5 bg-surface-container px-4 py-2.5 text-body-sm font-medium text-on-surface-variant">
              <Icon name="calendar_today" className="text-base" /> Son 14 Gün
            </span>
            <Link
              to="/nutrition"
              className="flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-body-sm font-bold text-on-primary shadow-lg shadow-primary/10 transition-all hover:brightness-110 active:scale-95"
            >
              <Icon name="add" className="text-base" /> Yeni Kayıt
            </Link>
          </div>
        }
      />

      <div className="grid grid-cols-2 gap-card-gap lg:grid-cols-4">
        <StatCard
          icon={<Icon name="local_fire_department" />} label="Kalori" accent="amber"
          value={`${Math.round(today.totalCalories)}`}
          sub={<MiniProgress value={today.totalCalories} goal={today.calorieGoal} unit="kcal" accent="bg-amber-400" />}
        />
        <StatCard
          icon={<Icon name="egg_alt" />} label="Protein" accent="primary"
          value={`${Math.round(today.totalProtein)} g`}
          sub={<MiniProgress value={today.totalProtein} goal={today.proteinGoal} unit="g" accent="bg-primary" />}
        />
        <StatCard
          icon={<Icon name="fitness_center" />} label="Bu Hafta Antrenman" accent="rose"
          value={data.workoutsThisWeek}
          sub={`${Math.round(data.caloriesBurnedThisWeek)} kcal yakıldı`}
        />
        <StatCard
          icon={<Icon name="monitor_weight" />} label="Kilo / BMI" accent="secondary"
          value={`${data.currentWeightKg ?? '-'} kg`}
          sub={`BMI ${data.bmi}${data.targetWeightKg ? ` • hedef ${data.targetWeightKg} kg` : ''}`}
        />
      </div>

      {/* Su takibi · Hızlı işlemler · Seri & rozetler */}
      <div className="grid gap-card-gap lg:grid-cols-3">
        <Card>
          <h2 className="mb-4 flex items-center gap-2 text-title-md font-bold text-on-surface">
            <Icon name="water_drop" className="text-blue-400" /> Su Takibi
          </h2>
          <div className="flex items-center gap-5">
            <Ring value={water?.intakeMl ?? 0} goal={water?.goalMl ?? 1} color={chartColors.blue}>
              <span className="text-lg font-bold text-on-surface">{((water?.intakeMl ?? 0) / 1000).toFixed(1)}</span>
              <span className="text-[10px] text-on-surface-variant">/ {((water?.goalMl ?? 0) / 1000).toFixed(1)} L</span>
            </Ring>
            <div className="flex flex-1 flex-col gap-2">
              <button
                onClick={() => addWater(250)} disabled={addingWater}
                className="flex items-center justify-center gap-1.5 rounded-xl border border-blue-400/30 bg-blue-500/10 py-2 text-body-sm font-semibold text-blue-400 transition-colors hover:bg-blue-500/20 active:scale-95 disabled:opacity-50"
              >
                <Icon name="add" className="text-base" /> 250 ml
              </button>
              <button
                onClick={() => addWater(500)} disabled={addingWater}
                className="flex items-center justify-center gap-1.5 rounded-xl border border-blue-400/30 bg-blue-500/10 py-2 text-body-sm font-semibold text-blue-400 transition-colors hover:bg-blue-500/20 active:scale-95 disabled:opacity-50"
              >
                <Icon name="add" className="text-base" /> 500 ml
              </button>
            </div>
          </div>
        </Card>

        <Card>
          <h2 className="mb-4 flex items-center gap-2 text-title-md font-bold text-on-surface">
            <Icon name="bolt" className="text-amber-400" /> Hızlı İşlemler
          </h2>
          <div className="grid grid-cols-2 gap-2.5">
            <Link to="/nutrition" className="flex flex-col items-center gap-1.5 rounded-xl bg-surface-container-high py-4 text-on-surface transition-colors hover:bg-white/10 active:scale-95">
              <Icon name="restaurant" className="text-2xl text-tertiary" />
              <span className="text-xs font-medium">Öğün ekle</span>
            </Link>
            <Link to="/workouts" className="flex flex-col items-center gap-1.5 rounded-xl bg-surface-container-high py-4 text-on-surface transition-colors hover:bg-white/10 active:scale-95">
              <Icon name="fitness_center" className="text-2xl text-rose-400" />
              <span className="text-xs font-medium">Antrenman</span>
            </Link>
            <Link to="/progress" className="flex flex-col items-center gap-1.5 rounded-xl bg-surface-container-high py-4 text-on-surface transition-colors hover:bg-white/10 active:scale-95">
              <Icon name="monitor_weight" className="text-2xl text-secondary" />
              <span className="text-xs font-medium">Kilo ekle</span>
            </Link>
            <button onClick={() => addWater(250)} disabled={addingWater} className="flex flex-col items-center gap-1.5 rounded-xl bg-surface-container-high py-4 text-on-surface transition-colors hover:bg-white/10 active:scale-95 disabled:opacity-50">
              <Icon name="water_drop" className="text-2xl text-blue-400" />
              <span className="text-xs font-medium">+250 ml su</span>
            </button>
          </div>
        </Card>

        <Card>
          <h2 className="mb-4 flex items-center gap-2 text-title-md font-bold text-on-surface">
            <Icon name="emoji_events" className="text-amber-400" /> Seri & Rozetler
          </h2>
          <div className="mb-4 flex items-center gap-3 rounded-xl bg-surface-container-high p-3">
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-amber-500/10 text-amber-400">
              <Icon name="local_fire_department" className="text-2xl" filled />
            </div>
            <div>
              <div className="text-2xl font-bold text-on-surface">{streak} <span className="text-base font-normal text-on-surface-variant">gün</span></div>
              <div className="text-xs text-on-surface-variant">kayıt serisi</div>
            </div>
          </div>
          <div className="flex flex-wrap gap-2">
            {badges.map((b) => (
              <span
                key={b.label}
                className={`flex items-center gap-1.5 rounded-full px-3 py-1.5 text-xs font-semibold transition-colors ${
                  b.met ? 'bg-tertiary/15 text-tertiary' : 'bg-white/5 text-on-surface-variant/60'
                }`}
                title={b.met ? 'Tamamlandı' : 'Henüz tamamlanmadı'}
              >
                <Icon name={b.met ? 'check_circle' : b.icon} className="text-sm" filled={b.met} /> {b.label}
              </span>
            ))}
          </div>
        </Card>
      </div>

      <div className="grid gap-card-gap lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <h2 className="mb-1 text-title-md font-bold text-on-surface">Kalori Trendi</h2>
          <p className="mb-6 text-body-sm text-on-surface-variant">Son 14 günlük alınan ve yakılan kalori</p>
          <div className="h-64">
            <Line data={calorieTrendData} options={lineOpts} />
          </div>
        </Card>
        <Card>
          <h2 className="mb-1 text-title-md font-bold text-on-surface">Bugünkü Makrolar</h2>
          <p className="mb-6 text-body-sm text-on-surface-variant">Protein • Karbonhidrat • Yağ</p>
          {todayMacros.protein + todayMacros.carbs + todayMacros.fat > 0 ? (
            <div className="h-64">
              <Doughnut data={macroData} options={doughnutOpts} />
            </div>
          ) : (
            <div className="flex h-64 items-center justify-center text-body-sm text-on-surface-variant">
              Bugün için besin kaydı yok
            </div>
          )}
        </Card>
      </div>

      <Card>
        <h2 className="mb-1 text-title-md font-bold text-on-surface">Kilo Değişimi</h2>
        <p className="mb-6 text-body-sm text-on-surface-variant">Zaman içindeki kilo gidişatın</p>
        {data.weightTrend.length > 1 ? (
          <div className="h-64">
            <Line data={weightTrendData} options={lineOpts} />
          </div>
        ) : (
          <div className="flex h-32 items-center justify-center text-center text-body-sm text-on-surface-variant">
            Kilo grafiği için en az iki kayıt gerekli. İlerleme sayfasından kilo ekleyebilirsin.
          </div>
        )}
      </Card>
    </div>
  );
}
