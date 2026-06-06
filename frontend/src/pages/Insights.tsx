import { useCallback, useEffect, useState } from 'react';
import { insightsApi } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, EmptyState, ErrorAlert, Icon, PageHeader, Spinner } from '../components/ui';
import { formatDateTime } from '../lib/labels';
import type { Insight, InsightSeverity, InsightsResponse } from '../types';

const severityStyle: Record<InsightSeverity, { wrap: string; badge: string; icon: string; iconColor: string; label: string }> = {
  Positive: { wrap: 'border-l-4 border-l-tertiary', badge: 'bg-tertiary/10 text-tertiary', icon: 'check_circle', iconColor: 'text-tertiary', label: 'Olumlu' },
  Info: { wrap: 'border-l-4 border-l-blue-400', badge: 'bg-blue-500/10 text-blue-400', icon: 'info', iconColor: 'text-blue-400', label: 'Bilgi' },
  Warning: { wrap: 'border-l-4 border-l-amber-400', badge: 'bg-amber-500/10 text-amber-400', icon: 'warning', iconColor: 'text-amber-400', label: 'Dikkat' },
};

function InsightCard({ insight }: { insight: Insight }) {
  const style = severityStyle[insight.severity];
  return (
    <Card className={`${style.wrap}`}>
      <div className="mb-2 flex items-center justify-between gap-2">
        <h3 className="flex items-center gap-2 font-semibold text-on-surface">
          <Icon name={style.icon} className={style.iconColor} /> {insight.title}
        </h3>
        <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${style.badge}`}>{style.label}</span>
      </div>
      <p className="text-body-sm leading-relaxed text-on-surface-variant">{insight.message}</p>
      {insight.metric && (
        <div className="mt-3 inline-block rounded-md bg-white/5 px-2.5 py-1 text-xs font-medium text-on-surface-variant">
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
        title="AI Insights"
        subtitle={data ? `Son ${data.daysAnalyzed} günün analizi • ${formatDateTime(data.generatedAt)}` : 'Kişiselleştirilmiş analiz'}
        action={
          <Button variant="secondary" onClick={load} disabled={loading} className="flex items-center gap-1.5">
            <Icon name="refresh" className="text-base" /> Yenile
          </Button>
        }
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
