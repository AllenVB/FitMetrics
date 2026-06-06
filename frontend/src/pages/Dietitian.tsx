import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { dietitianApi } from '../api';
import { getErrorMessage } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { Button, Card, EmptyState, ErrorAlert, Field, Icon, Input, PageHeader, Spinner, StatCard } from '../components/ui';
import { formatDate, goalLabels } from '../lib/labels';
import type { ClientSummary, Dashboard, InsightsResponse, InsightSeverity } from '../types';

export default function Dietitian() {
  const { user } = useAuth();
  return user?.role === 'Dietitian' ? <Panel /> : <EnrollView />;
}

function EnrollView() {
  const { updateUser } = useAuth();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const enroll = async () => {
    setLoading(true);
    setError('');
    try {
      updateUser(await dietitianApi.enroll());
    } catch (e) {
      setError(getErrorMessage(e));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <PageHeader title="Diyetisyen Paneli" subtitle="Danışanlarını yönet ve ilerlemelerini takip et" />
      <Card>
        <h3 className="mb-2 text-title-md font-bold text-on-surface">Diyetisyen moduna geç</h3>
        <p className="mb-4 text-body-sm text-on-surface-variant">
          Bu modda danışanlarını e-posta ile ekleyip onların panel verilerini ve AI analizlerini (yalnızca okuma)
          görüntüleyebilirsin. İstediğin zaman danışan bağını kaldırabilirsin.
        </p>
        <Button onClick={enroll} disabled={loading}>{loading ? 'Geçiliyor…' : 'Diyetisyen Moduna Geç'}</Button>
        <div className="mt-3"><ErrorAlert message={error} /></div>
      </Card>
    </div>
  );
}

function Panel() {
  const [clients, setClients] = useState<ClientSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [email, setEmail] = useState('');
  const [adding, setAdding] = useState(false);
  const [selected, setSelected] = useState<ClientSummary | null>(null);

  const load = useCallback(() => {
    setLoading(true);
    dietitianApi.clients()
      .then(setClients)
      .catch((e) => setError(getErrorMessage(e)))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => { load(); }, [load]);

  const addClient = async (e: FormEvent) => {
    e.preventDefault();
    setAdding(true);
    setError('');
    try {
      await dietitianApi.addClient(email.trim());
      setEmail('');
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setAdding(false);
    }
  };

  const remove = async (id: number) => {
    try {
      await dietitianApi.removeClient(id);
      if (selected?.id === id) setSelected(null);
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };

  return (
    <div>
      <PageHeader title="Diyetisyen Paneli" subtitle="Danışanlarını yönet ve ilerlemelerini takip et" />

      <Card className="mb-6">
        <form onSubmit={addClient} className="flex flex-wrap items-end gap-3">
          <div className="min-w-[220px] flex-1">
            <Field label="Danışan e-postası">
              <Input type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="danisan@mail.com" required />
            </Field>
          </div>
          <Button type="submit" disabled={adding}>{adding ? 'Ekleniyor…' : '+ Danışan Ekle'}</Button>
        </form>
      </Card>

      <div className="mb-4"><ErrorAlert message={error} /></div>

      {loading ? (
        <Spinner />
      ) : clients.length === 0 ? (
        <EmptyState message="Henüz danışanın yok. Kayıtlı bir kullanıcının e-postasıyla ekleyerek başla." />
      ) : (
        <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-3">
          {clients.map((c) => (
            <Card key={c.id}>
              <div className="font-semibold text-on-surface">{c.fullName}</div>
              <div className="text-xs text-on-surface-variant">{c.email}</div>
              <div className="mt-2 text-body-sm text-on-surface-variant">{goalLabels[c.goalType]} · {c.currentWeightKg} kg · BMI {c.bmi}</div>
              <div className="mt-3 flex gap-2">
                <Button variant="secondary" onClick={() => setSelected(c)}>Görüntüle</Button>
                <Button variant="ghost" onClick={() => remove(c.id)}>Kaldır</Button>
              </div>
            </Card>
          ))}
        </div>
      )}

      {selected && <ClientDetail key={selected.id} client={selected} onClose={() => setSelected(null)} />}
    </div>
  );
}

const severityColor: Record<InsightSeverity, string> = {
  Positive: 'border-l-tertiary',
  Info: 'border-l-blue-400',
  Warning: 'border-l-amber-400',
};

function ClientDetail({ client, onClose }: { client: ClientSummary; onClose: () => void }) {
  const [dash, setDash] = useState<Dashboard | null>(null);
  const [insights, setInsights] = useState<InsightsResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    setLoading(true);
    setError('');
    Promise.all([dietitianApi.clientDashboard(client.id), dietitianApi.clientInsights(client.id)])
      .then(([d, i]) => { setDash(d); setInsights(i); })
      .catch((e) => setError(getErrorMessage(e)))
      .finally(() => setLoading(false));
  }, [client.id]);

  return (
    <Card className="mt-6">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-title-md font-bold text-on-surface">{client.fullName} — detay</h3>
        <Button variant="ghost" onClick={onClose}>Kapat</Button>
      </div>

      <div className="mb-4"><ErrorAlert message={error} /></div>

      {loading ? (
        <Spinner />
      ) : dash ? (
        <>
          <div className="mb-5 grid grid-cols-2 gap-3 lg:grid-cols-4">
            <StatCard icon={<Icon name="local_fire_department" />} label="Bugün kalori" value={Math.round(dash.today.totalCalories)} sub={`/ ${dash.today.calorieGoal} kcal`} accent="amber" />
            <StatCard icon={<Icon name="restaurant" />} label="Bugün protein" value={`${Math.round(dash.today.totalProtein)} g`} sub={`/ ${dash.today.proteinGoal} g`} accent="brand" />
            <StatCard icon={<Icon name="fitness_center" />} label="Bu hafta antrenman" value={dash.workoutsThisWeek} accent="rose" />
            <StatCard icon={<Icon name="monitor_weight" />} label="Kilo / BMI" value={`${dash.currentWeightKg ?? '-'} kg`} sub={`BMI ${dash.bmi}`} accent="violet" />
          </div>

          {insights && insights.insights.length > 0 && (
            <div className="space-y-2">
              <h4 className="text-body-sm font-semibold text-on-surface-variant">AI Analizleri ({formatDate(insights.generatedAt)})</h4>
              {insights.insights.map((ins, i) => (
                <div key={i} className={`rounded-lg border-l-4 ${severityColor[ins.severity]} bg-surface-container-high p-3`}>
                  <div className="text-body-sm font-semibold text-on-surface">{ins.title}</div>
                  <div className="text-body-sm text-on-surface-variant">{ins.message}</div>
                </div>
              ))}
            </div>
          )}
        </>
      ) : null}
    </Card>
  );
}
