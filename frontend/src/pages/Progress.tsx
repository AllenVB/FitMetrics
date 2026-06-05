import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { Line } from 'react-chartjs-2';
import { reportsApi, weightApi } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, EmptyState, ErrorAlert, Field, Input, PageHeader, Spinner, StatCard } from '../components/ui';
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
      borderColor: '#7c3aed',
      backgroundColor: 'rgba(124, 58, 237, 0.12)',
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
      borderColor: '#f59e0b',
      backgroundColor: 'rgba(245, 158, 11, 0.12)',
      fill: true,
      tension: 0.3,
      spanGaps: true,
    }],
  };
  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom' as const, labels: { boxWidth: 12, font: { size: 11 } } } },
  };

  return (
    <div>
      <PageHeader
        title="📈 İlerleme"
        subtitle="Kilo ve vücut yağı değişimini takip et"
        action={
          <Button variant="secondary" onClick={downloadReport} disabled={downloading}>
            {downloading ? 'Hazırlanıyor…' : '📄 PDF Rapor'}
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
            <StatCard icon="🏁" label="Başlangıç" value={`${first.weightKg} kg`} sub={formatDate(first.recordedAt)} accent="blue" />
            <StatCard icon="⚖️" label="Güncel" value={`${last.weightKg} kg`} sub={formatDate(last.recordedAt)} accent="violet" />
            <StatCard
              icon={change <= 0 ? '📉' : '📈'}
              label="Değişim"
              value={`${change > 0 ? '+' : ''}${change} kg`}
              accent={change <= 0 ? 'brand' : 'rose'}
            />
          </div>

          <div className="mb-6 grid gap-4 lg:grid-cols-2">
            <Card>
              <h2 className="mb-3 font-semibold text-slate-800">Kilo Değişimi</h2>
              <div className="h-64"><Line data={weightChart} options={chartOptions} /></div>
            </Card>
            <Card>
              <h2 className="mb-3 font-semibold text-slate-800">Vücut Yağı</h2>
              {hasBodyFat ? (
                <div className="h-64"><Line data={bodyFatChart} options={chartOptions} /></div>
              ) : (
                <div className="flex h-64 items-center justify-center text-sm text-slate-400">
                  Yağ oranı kaydı eklenince grafik burada görünecek.
                </div>
              )}
            </Card>
          </div>

          <Card>
            <h2 className="mb-3 font-semibold text-slate-800">Tüm Kayıtlar</h2>
            <ul className="divide-y divide-slate-100">
              {[...entries].reverse().map((e) => (
                <li key={e.id} className="flex items-center justify-between py-2.5">
                  <div>
                    <span className="font-medium text-slate-700">{e.weightKg} kg</span>
                    {e.bodyFatPercentage != null && <span className="ml-2 text-sm text-amber-600">%{e.bodyFatPercentage} yağ</span>}
                    <span className="ml-2 text-xs text-slate-400">{formatDate(e.recordedAt)}</span>
                  </div>
                  <button onClick={() => onDelete(e.id)} className="rounded-md px-2 py-1 text-sm text-rose-500 hover:bg-rose-50">Sil</button>
                </li>
              ))}
            </ul>
          </Card>
        </>
      )}
    </div>
  );
}
