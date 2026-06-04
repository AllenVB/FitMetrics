import { useCallback, useEffect, useState } from 'react';
import { insightsApi } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, EmptyState, ErrorAlert, PageHeader, Spinner } from '../components/ui';
import { formatDateTime } from '../lib/labels';
import type { Insight, InsightSeverity, InsightsResponse } from '../types';

const severityStyle: Record<InsightSeverity, { wrap: string; badge: string; icon: string; label: string }> = {
  Positive: { wrap: 'border-l-4 border-emerald-400', badge: 'bg-emerald-100 text-emerald-700', icon: '✅', label: 'Olumlu' },
  Info: { wrap: 'border-l-4 border-blue-400', badge: 'bg-blue-100 text-blue-700', icon: 'ℹ️', label: 'Bilgi' },
  Warning: { wrap: 'border-l-4 border-amber-400', badge: 'bg-amber-100 text-amber-700', icon: '⚠️', label: 'Dikkat' },
};

function InsightCard({ insight }: { insight: Insight }) {
  const style = severityStyle[insight.severity];
  return (
    <Card className={`${style.wrap}`}>
      <div className="mb-2 flex items-center justify-between gap-2">
        <h3 className="flex items-center gap-2 font-semibold text-slate-800">
          <span>{style.icon}</span> {insight.title}
        </h3>
        <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${style.badge}`}>{style.label}</span>
      </div>
      <p className="text-sm leading-relaxed text-slate-600">{insight.message}</p>
      {insight.metric && (
        <div className="mt-3 inline-block rounded-md bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-500">
          {insight.metric}
        </div>
      )}
    </Card>
  );
}

export default function Insights() {
  const [data, setData] = useState<InsightsResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = useCallback(() => {
    setLoading(true);
    setError('');
    insightsApi.get()
      .then(setData)
      .catch((e) => setError(getErrorMessage(e)))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => { load(); }, [load]);

  return (
    <div>
      <PageHeader
        title="🧠 AI Insights"
        subtitle={data ? `Son ${data.daysAnalyzed} günün analizi • ${formatDateTime(data.generatedAt)}` : 'Kişiselleştirilmiş analiz'}
        action={<Button variant="secondary" onClick={load} disabled={loading}>Yenile</Button>}
      />

      <div className="mb-4"><ErrorAlert message={error} /></div>

      {loading ? (
        <Spinner label="Veriler analiz ediliyor…" />
      ) : data && data.insights.length > 0 ? (
        <div className="grid gap-4 md:grid-cols-2">
          {data.insights.map((insight, i) => <InsightCard key={i} insight={insight} />)}
        </div>
      ) : (
        <EmptyState message="Analiz için yeterli veri yok. Beslenme ve antrenman kaydı girdikçe öneriler burada görünecek." />
      )}
    </div>
  );
}
