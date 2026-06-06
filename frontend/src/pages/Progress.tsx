import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { Line } from 'react-chartjs-2';
import { reportsApi, weightApi } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, EmptyState, ErrorAlert, Field, Icon, Input, PageHeader, Spinner, StatCard } from '../components/ui';
import { chartColors } from '../lib/charts';
import { formatDate } from '../lib/labels';
import type { WeightEntry } from '../types';

export default function Progress() {
  const [entries, setEntries] = useState<WeightEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [weight, setWeight] = useState('');
  const [bodyFat, setBodyFat] = useState('');
  const [date, setDate] = useState('');
  const [adding, setAdding] = useState(false);
  const [downloading, setDownloading] = useState(false);

  const load = useCallback(() => {
    setLoading(true);
    weightApi.history()
      .then(setEntries)
      .catch((e) => setError(getErrorMessage(e)))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => { load(); }, [load]);

  const onAdd = async (e: FormEvent) => {
    e.preventDefault();
    if (weight === '') return;
    setAdding(true);
    setError('');
    try {
      await weightApi.add({
        weightKg: Number(weight),
        bodyFatPercentage: bodyFat === '' ? null : Number(bodyFat),
        recordedAt: date === '' ? null : `${date}T12:00:00`,
      });
      setWeight(''); setBodyFat(''); setDate('');
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setAdding(false);
    }
  };

  const onDelete = async (id: number) => {
    try {
      await weightApi.remove(id);
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };

  const downloadReport = async () => {
    setDownloading(true);
    setError('');
    try {
      const blob = await reportsApi.downloadMonthly();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'fitmetrics-rapor.pdf';
      document.body.appendChild(a);
      a.click();
      a.remove();
      URL.revokeObjectURL(url);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setDownloading(false);
    }
  };

  const first = entries[0];
  const last = entries[entries.length - 1];
  const change = first && last ? +(last.weightKg - first.weightKg).toFixed(1) : 0;

  const labels = entries.map((e) => formatDate(e.recordedAt));
  const weightChart = {
    labels,
    datasets: [{
      label: 'Kilo (kg)',
      data: entries.map((e) => e.weightKg),
      borderColor: chartColors.primary,
      backgroundColor: chartColors.primaryFill,
      pointBackgroundColor: chartColors.primary,
      fill: true,
      tension: 0.3,
    }],
  };
  const hasBodyFat = entries.some((e) => e.bodyFatPercentage != null);
  const bodyFatChart = {
    labels,
    datasets: [{
      label: 'Yağ Oranı (%)',
      data: entries.map((e) => e.bodyFatPercentage ?? null),
      borderColor: '#fbbf24',
      backgroundColor: 'rgba(251, 191, 36, 0.12)',
      pointBackgroundColor: '#fbbf24',
      fill: true,
      tension: 0.3,
      spanGaps: true,
    }],
  };
  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom' as const, labels: { boxWidth: 12, font: { size: 11 } } } },
    scales: {
      x: { grid: { color: 'rgba(255,255,255,0.05)' } },
      y: { grid: { color: 'rgba(255,255,255,0.05)' } },
    },
  };

  return (
    <div>
      <PageHeader
        title="İlerleme"
        subtitle="Kilo ve vücut yağı değişimini takip et"
        action={
          <Button variant="secondary" onClick={downloadReport} disabled={downloading} className="flex items-center gap-1.5">
            <Icon name="picture_as_pdf" className="text-base" /> {downloading ? 'Hazırlanıyor…' : 'PDF Rapor'}
          </Button>
        }
      />

      <div className="mb-4"><ErrorAlert message={error} /></div>

      <Card className="mb-6">
        <form onSubmit={onAdd} className="grid grid-cols-2 gap-4 lg:grid-cols-4 lg:items-end">
          <Field label="Kilo (kg)"><Input type="number" step="0.1" value={weight} onChange={(e) => setWeight(e.target.value)} min={20} max={400} placeholder="80" required /></Field>
          <Field label="Yağ Oranı (%)" hint="İsteğe bağlı"><Input type="number" step="0.1" value={bodyFat} onChange={(e) => setBodyFat(e.target.value)} min={2} max={70} placeholder="18" /></Field>
          <Field label="Tarih" hint="Boş = bugün"><Input type="date" value={date} onChange={(e) => setDate(e.target.value)} /></Field>
          <Button type="submit" disabled={adding}>{adding ? 'Ekleniyor…' : '+ Kayıt Ekle'}</Button>
        </form>
      </Card>

      {loading ? (
        <Spinner />
      ) : entries.length === 0 ? (
        <EmptyState message="Henüz kilo kaydın yok. İlk kaydını ekleyerek başla!" />
      ) : (
        <>
          <div className="mb-6 grid grid-cols-3 gap-4">
            <StatCard icon={<Icon name="flag" />} label="Başlangıç" value={`${first.weightKg} kg`} sub={formatDate(first.recordedAt)} accent="blue" />
            <StatCard icon={<Icon name="monitor_weight" />} label="Güncel" value={`${last.weightKg} kg`} sub={formatDate(last.recordedAt)} accent="violet" />
            <StatCard
              icon={<Icon name={change <= 0 ? 'trending_down' : 'trending_up'} />}
              label="Değişim"
              value={`${change > 0 ? '+' : ''}${change} kg`}
              accent={change <= 0 ? 'brand' : 'rose'}
            />
          </div>

          <div className="mb-6 grid gap-4 lg:grid-cols-2">
            <Card>
              <h2 className="mb-3 text-title-md font-bold text-on-surface">Kilo Değişimi</h2>
              <div className="h-64"><Line data={weightChart} options={chartOptions} /></div>
            </Card>
            <Card>
              <h2 className="mb-3 text-title-md font-bold text-on-surface">Vücut Yağı</h2>
              {hasBodyFat ? (
                <div className="h-64"><Line data={bodyFatChart} options={chartOptions} /></div>
              ) : (
                <div className="flex h-64 items-center justify-center text-body-sm text-on-surface-variant">
                  Yağ oranı kaydı eklenince grafik burada görünecek.
                </div>
              )}
            </Card>
          </div>

          <Card>
            <h2 className="mb-3 text-title-md font-bold text-on-surface">Tüm Kayıtlar</h2>
            <ul className="divide-y divide-white/5">
              {[...entries].reverse().map((e) => (
                <li key={e.id} className="flex items-center justify-between py-2.5">
                  <div>
                    <span className="font-medium text-on-surface">{e.weightKg} kg</span>
                    {e.bodyFatPercentage != null && <span className="ml-2 text-body-sm text-amber-400">%{e.bodyFatPercentage} yağ</span>}
                    <span className="ml-2 text-xs text-on-surface-variant">{formatDate(e.recordedAt)}</span>
                  </div>
                  <button onClick={() => onDelete(e.id)} className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg text-on-surface-variant transition-colors hover:bg-error/10 hover:text-error" title="Sil">
                    <Icon name="delete" className="text-lg" />
                  </button>
                </li>
              ))}
            </ul>
          </Card>
        </>
      )}
    </div>
  );
}
