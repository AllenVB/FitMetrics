import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { Line } from 'react-chartjs-2';
import { measurementsApi, reportsApi, weightApi, type CreateBodyMeasurementRequest } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, EmptyState, ErrorAlert, Field, Icon, Input, PageHeader, Spinner, StatCard } from '../components/ui';
import { chartColors } from '../lib/charts';
import { formatDate } from '../lib/labels';
import type { BodyMeasurement, WeightEntry } from '../types';

type Tab = 'weight' | 'measurements';

const MEASURE_FIELDS: { key: keyof CreateBodyMeasurementRequest; label: string; unit: string }[] = [
  { key: 'waistCm', label: 'Bel', unit: 'cm' },
  { key: 'hipCm', label: 'Kalça', unit: 'cm' },
  { key: 'chestCm', label: 'Göğüs', unit: 'cm' },
  { key: 'armCm', label: 'Kol (bicep)', unit: 'cm' },
  { key: 'neckCm', label: 'Boyun', unit: 'cm' },
];

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { position: 'bottom' as const, labels: { boxWidth: 12, font: { size: 11 } } } },
  scales: {
    x: { grid: { color: 'rgba(255,255,255,0.05)' } },
    y: { grid: { color: 'rgba(255,255,255,0.05)' } },
  },
};

export default function Progress() {
  const [tab, setTab] = useState<Tab>('weight');

  // ── Kilo ──────────────────────────────────────────────────────────────────
  const [entries, setEntries] = useState<WeightEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [weight, setWeight] = useState('');
  const [bodyFat, setBodyFat] = useState('');
  const [wDate, setWDate] = useState('');
  const [adding, setAdding] = useState(false);
  const [downloading, setDownloading] = useState(false);

  const loadWeight = useCallback(() => {
    setLoading(true);
    weightApi.history()
      .then(setEntries)
      .catch((e) => setError(getErrorMessage(e)))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => { loadWeight(); }, [loadWeight]);

  const onAddWeight = async (e: FormEvent) => {
    e.preventDefault();
    if (!weight) return;
    setAdding(true); setError('');
    try {
      await weightApi.add({ weightKg: Number(weight), bodyFatPercentage: bodyFat ? Number(bodyFat) : null, recordedAt: wDate ? `${wDate}T12:00:00` : null });
      setWeight(''); setBodyFat(''); setWDate('');
      loadWeight();
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setAdding(false); }
  };

  const onDeleteWeight = async (id: number) => {
    try { await weightApi.remove(id); loadWeight(); }
    catch (err) { setError(getErrorMessage(err)); }
  };

  const downloadReport = async () => {
    setDownloading(true); setError('');
    try {
      const blob = await reportsApi.downloadMonthly();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url; a.download = 'fitmetrics-rapor.pdf';
      document.body.appendChild(a); a.click(); a.remove();
      URL.revokeObjectURL(url);
    } catch (err) { setError(getErrorMessage(err)); }
    finally { setDownloading(false); }
  };

  // ── Vücut Ölçümleri ───────────────────────────────────────────────────────
  const [measurements, setMeasurements] = useState<BodyMeasurement[]>([]);
  const [mLoading, setMLoading] = useState(false);
  const [mError, setMError] = useState('');
  const [mAdding, setMAdding] = useState(false);
  const [mForm, setMForm] = useState<Record<string, string>>({});
  const [mDate, setMDate] = useState('');

  const loadMeasurements = useCallback(() => {
    setMLoading(true);
    measurementsApi.getHistory()
      .then(setMeasurements)
      .catch((e) => setMError(getErrorMessage(e)))
      .finally(() => setMLoading(false));
  }, []);

  useEffect(() => {
    if (tab === 'measurements') loadMeasurements();
  }, [tab, loadMeasurements]);

  const onAddMeasurement = async (e: FormEvent) => {
    e.preventDefault();
    setMAdding(true); setMError('');
    try {
      const payload: CreateBodyMeasurementRequest = {
        recordedAt: mDate ? `${mDate}T12:00:00` : null,
        waistCm: mForm.waistCm ? Number(mForm.waistCm) : null,
        hipCm: mForm.hipCm ? Number(mForm.hipCm) : null,
        chestCm: mForm.chestCm ? Number(mForm.chestCm) : null,
        armCm: mForm.armCm ? Number(mForm.armCm) : null,
        neckCm: mForm.neckCm ? Number(mForm.neckCm) : null,
      };
      await measurementsApi.add(payload);
      setMForm({}); setMDate('');
      loadMeasurements();
    } catch (err) { setMError(getErrorMessage(err)); }
    finally { setMAdding(false); }
  };

  const onDeleteMeasurement = async (id: number) => {
    try { await measurementsApi.remove(id); loadMeasurements(); }
    catch (err) { setMError(getErrorMessage(err)); }
  };

  // ── Grafik verileri ──────────────────────────────────────────────────────
  const first = entries[0];
  const last = entries[entries.length - 1];
  const change = first && last ? +(last.weightKg - first.weightKg).toFixed(1) : 0;

  const mLabels = measurements.map((m) => formatDate(m.recordedAt));
  const measureCharts = MEASURE_FIELDS.map((f) => ({
    field: f,
    data: {
      labels: mLabels,
      datasets: [{
        label: `${f.label} (${f.unit})`,
        data: measurements.map((m) => (m as Record<string, unknown>)[f.key] as number | null ?? null),
        borderColor: chartColors.primary,
        backgroundColor: chartColors.primaryFill,
        fill: true, tension: 0.3, spanGaps: true,
      }],
    },
    hasData: measurements.some((m) => (m as Record<string, unknown>)[f.key] != null),
  }));

  return (
    <div>
      <PageHeader
        title="İlerleme"
        subtitle="Kilo, yağ oranı ve vücut ölçümleri"
        action={
          tab === 'weight' ? (
            <Button variant="secondary" onClick={downloadReport} disabled={downloading} className="flex items-center gap-1.5">
              <Icon name="picture_as_pdf" className="text-base" /> {downloading ? 'Hazırlanıyor…' : 'PDF Rapor'}
            </Button>
          ) : undefined
        }
      />

      {/* Sekme */}
      <div className="mb-6 flex gap-2">
        {([['weight', 'monitor_weight', 'Kilo & Yağ'], ['measurements', 'straighten', 'Vücut Ölçüleri']] as const).map(([t, icon, label]) => (
          <button
            key={t}
            onClick={() => setTab(t)}
            className={`flex items-center gap-2 rounded-xl px-4 py-2 text-body-sm font-semibold transition-all ${
              tab === t ? 'bg-primary text-on-primary' : 'bg-surface-container text-on-surface-variant hover:text-on-surface'
            }`}
          >
            <Icon name={icon} className="text-base" />
            {label}
          </button>
        ))}
      </div>

      {/* ── Kilo Sekmesi ─────────────────────────────────────────────── */}
      {tab === 'weight' && (
        <>
          <div className="mb-4"><ErrorAlert message={error} /></div>
          <Card className="mb-6">
            <form onSubmit={onAddWeight} className="grid grid-cols-2 gap-4 lg:grid-cols-4 lg:items-end">
              <Field label="Kilo (kg)"><Input type="number" step="0.1" value={weight} onChange={(e) => setWeight(e.target.value)} min={20} max={400} placeholder="80" required /></Field>
              <Field label="Yağ Oranı (%)" hint="İsteğe bağlı"><Input type="number" step="0.1" value={bodyFat} onChange={(e) => setBodyFat(e.target.value)} min={2} max={70} placeholder="18" /></Field>
              <Field label="Tarih" hint="Boş = bugün"><Input type="date" value={wDate} onChange={(e) => setWDate(e.target.value)} /></Field>
              <Button type="submit" disabled={adding}>{adding ? 'Ekleniyor…' : '+ Kayıt Ekle'}</Button>
            </form>
          </Card>

          {loading ? <Spinner /> : entries.length === 0 ? (
            <EmptyState message="Henüz kilo kaydın yok. İlk kaydını ekleyerek başla!" />
          ) : (
            <>
              <div className="mb-6 grid grid-cols-3 gap-4">
                <StatCard icon={<Icon name="flag" />} label="Başlangıç" value={`${first.weightKg} kg`} sub={formatDate(first.recordedAt)} accent="blue" />
                <StatCard icon={<Icon name="monitor_weight" />} label="Güncel" value={`${last.weightKg} kg`} sub={formatDate(last.recordedAt)} accent="violet" />
                <StatCard icon={<Icon name={change <= 0 ? 'trending_down' : 'trending_up'} />} label="Değişim" value={`${change > 0 ? '+' : ''}${change} kg`} accent={change <= 0 ? 'brand' : 'rose'} />
              </div>
              <div className="mb-6 grid gap-4 lg:grid-cols-2">
                <Card>
                  <h2 className="mb-3 text-title-md font-bold text-on-surface">Kilo Değişimi</h2>
                  <div className="h-64"><Line data={{ labels: entries.map((e) => formatDate(e.recordedAt)), datasets: [{ label: 'Kilo (kg)', data: entries.map((e) => e.weightKg), borderColor: chartColors.primary, backgroundColor: chartColors.primaryFill, fill: true, tension: 0.3 }] }} options={chartOptions} /></div>
                </Card>
                <Card>
                  <h2 className="mb-3 text-title-md font-bold text-on-surface">Vücut Yağı</h2>
                  {entries.some((e) => e.bodyFatPercentage != null) ? (
                    <div className="h-64"><Line data={{ labels: entries.map((e) => formatDate(e.recordedAt)), datasets: [{ label: 'Yağ Oranı (%)', data: entries.map((e) => e.bodyFatPercentage ?? null), borderColor: '#fbbf24', backgroundColor: 'rgba(251,191,36,0.12)', fill: true, tension: 0.3, spanGaps: true }] }} options={chartOptions} /></div>
                  ) : (
                    <div className="flex h-64 items-center justify-center text-body-sm text-on-surface-variant">Yağ oranı kaydı eklenince grafik burada görünecek.</div>
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
                      <button onClick={() => onDeleteWeight(e.id)} className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg text-on-surface-variant transition-colors hover:bg-error/10 hover:text-error">
                        <Icon name="delete" className="text-lg" />
                      </button>
                    </li>
                  ))}
                </ul>
              </Card>
            </>
          )}
        </>
      )}

      {/* ── Vücut Ölçüleri Sekmesi ────────────────────────────────────── */}
      {tab === 'measurements' && (
        <>
          <div className="mb-4"><ErrorAlert message={mError} /></div>
          <Card className="mb-6">
            <form onSubmit={onAddMeasurement}>
              <div className="mb-4 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
                {MEASURE_FIELDS.map((f) => (
                  <Field key={f.key as string} label={`${f.label} (${f.unit})`}>
                    <Input type="number" step="0.1" min={10} max={300} placeholder="—"
                      value={mForm[f.key as string] ?? ''}
                      onChange={(e) => setMForm((prev) => ({ ...prev, [f.key as string]: e.target.value }))}
                    />
                  </Field>
                ))}
              </div>
              <div className="flex items-end gap-4">
                <div className="flex-1 max-w-[200px]">
                  <Field label="Tarih" hint="Boş = bugün"><Input type="date" value={mDate} onChange={(e) => setMDate(e.target.value)} /></Field>
                </div>
                <Button type="submit" disabled={mAdding}>{mAdding ? 'Ekleniyor…' : '+ Ölçüm Ekle'}</Button>
              </div>
            </form>
          </Card>

          {mLoading ? <Spinner /> : measurements.length === 0 ? (
            <EmptyState message="Henüz ölçüm kaydın yok. Bel, kalça, kol ölçülerini girerek vücut değişimini takip et!" />
          ) : (
            <>
              {/* Grafikler — sadece verisi olan ölçümler */}
              <div className="mb-6 grid gap-4 md:grid-cols-2 xl:grid-cols-3">
                {measureCharts.filter((c) => c.hasData).map(({ field, data }) => (
                  <Card key={field.key as string}>
                    <h2 className="mb-3 text-sm font-bold text-on-surface">{field.label} Değişimi</h2>
                    <div className="h-48"><Line data={data} options={chartOptions} /></div>
                  </Card>
                ))}
              </div>

              <Card>
                <h2 className="mb-3 text-title-md font-bold text-on-surface">Tüm Ölçüm Kayıtları</h2>
                <div className="overflow-x-auto">
                  <table className="w-full text-body-sm">
                    <thead>
                      <tr className="border-b border-white/5 text-left text-xs text-on-surface-variant">
                        <th className="pb-2 pr-4">Tarih</th>
                        {MEASURE_FIELDS.map((f) => <th key={f.key as string} className="pb-2 pr-4">{f.label}</th>)}
                        <th />
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-white/5">
                      {[...measurements].reverse().map((m) => (
                        <tr key={m.id}>
                          <td className="py-2.5 pr-4 text-xs text-on-surface-variant">{formatDate(m.recordedAt)}</td>
                          {MEASURE_FIELDS.map((f) => (
                            <td key={f.key as string} className="py-2.5 pr-4 font-medium text-on-surface">
                              {(m as Record<string, unknown>)[f.key as string] != null ? `${(m as Record<string, unknown>)[f.key as string]} cm` : <span className="text-on-surface-variant">—</span>}
                            </td>
                          ))}
                          <td className="py-2.5">
                            <button onClick={() => onDeleteMeasurement(m.id)} className="flex h-7 w-7 items-center justify-center rounded-lg text-on-surface-variant transition-colors hover:bg-error/10 hover:text-error">
                              <Icon name="delete" className="text-base" />
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </Card>
            </>
          )}
        </>
      )}
    </div>
  );
}
