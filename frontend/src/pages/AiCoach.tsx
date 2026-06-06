import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react';
import { aiApi } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, ErrorAlert, Field, Icon, Input, PageHeader, Spinner } from '../components/ui';
import type { CoachResponse, MealPhotoResponse, MealPlanResponse } from '../types';

export default function AiCoach() {
  const [enabled, setEnabled] = useState<boolean | null>(null);

  useEffect(() => {
    aiApi.status().then((s) => setEnabled(s.enabled)).catch(() => setEnabled(false));
  }, []);

  if (enabled === null) return <Spinner />;

  return (
    <div>
      <PageHeader title="AI Koç" subtitle="Yapay zeka destekli öğün planı, kişisel koçluk ve fotoğraftan analiz" />

      {!enabled ? (
        <Card className="border-l-4 border-l-amber-400">
          <h3 className="mb-1 flex items-center gap-2 font-semibold text-on-surface">
            <Icon name="warning" className="text-amber-400" /> AI özellikleri şu an kapalı
          </h3>
          <p className="text-body-sm text-on-surface-variant">
            Bu özellikler bir AI sağlayıcı gerektirir. Sunucuda Ollama çalışıyorsa veya{' '}
            <code className="rounded bg-white/10 px-1">Anthropic:ApiKey</code> tanımlıysa otomatik etkinleşir.
            Uygulamanın geri kalanı AI olmadan da tam çalışır.
          </p>
        </Card>
      ) : (
        <div className="grid gap-card-gap lg:grid-cols-2">
          <MealPlanCard />
          <CoachCard />
          <PhotoCard />
        </div>
      )}
    </div>
  );
}

function MealPlanCard() {
  const [prompt, setPrompt] = useState('');
  const [target, setTarget] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [plan, setPlan] = useState<MealPlanResponse | null>(null);

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    setPlan(null);
    try {
      setPlan(await aiApi.mealPlan({ prompt, targetCalories: target === '' ? null : Number(target) }));
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card>
      <h3 className="mb-4 flex items-center gap-2 text-title-md font-bold text-on-surface">
        <Icon name="skillet" className="text-primary" /> Akıllı Öğün Planı
      </h3>
      <form onSubmit={submit} className="space-y-3">
        <Field label="Ne istiyorsun?">
          <Input value={prompt} onChange={(e) => setPrompt(e.target.value)} placeholder="örn. 2500 kalorilik kas kazanma programı" required />
        </Field>
        <Field label="Hedef kalori (opsiyonel)">
          <Input type="number" value={target} onChange={(e) => setTarget(e.target.value)} placeholder="örn. 2500" />
        </Field>
        <Button type="submit" disabled={loading}>{loading ? 'Oluşturuluyor…' : 'Plan oluştur'}</Button>
      </form>
      <div className="mt-3"><ErrorAlert message={error} /></div>
      {plan && (
        <div className="mt-4 space-y-3">
          <p className="text-body-sm text-on-surface-variant">{plan.summary}</p>
          <div className="text-xs font-medium text-primary">Toplam ~{plan.totalCalories} kcal · {plan.totalProtein} g protein</div>
          {plan.meals.map((meal, i) => (
            <div key={i} className="rounded-xl border border-white/10 bg-surface-container-high p-3">
              <div className="mb-1 flex justify-between text-body-sm font-semibold text-on-surface">
                <span>{meal.mealType}</span>
                <span>{meal.calories} kcal</span>
              </div>
              <ul className="space-y-0.5 text-body-sm text-on-surface-variant">
                {meal.foods.map((f, j) => (
                  <li key={j}>• {f.name} — {f.amount} <span className="text-on-surface-variant/60">({f.calories} kcal, {f.protein}g P)</span></li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
}

function CoachCard() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [coach, setCoach] = useState<CoachResponse | null>(null);

  const run = async () => {
    setError('');
    setLoading(true);
    try {
      setCoach(await aiApi.coach());
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card>
      <h3 className="mb-3 flex items-center gap-2 text-title-md font-bold text-on-surface">
        <Icon name="psychology" className="text-tertiary" /> Kişisel Koçluk
      </h3>
      <p className="mb-4 text-body-sm text-on-surface-variant">Verilerinden çıkan analizleri sıcak, uygulanabilir bir koçluk mesajına dönüştürür.</p>
      <Button onClick={run} disabled={loading}>{loading ? 'Hazırlanıyor…' : 'Koçluk al'}</Button>
      <div className="mt-3"><ErrorAlert message={error} /></div>
      {coach && (
        <div className="mt-4 space-y-3">
          <p className="whitespace-pre-line text-body-sm leading-relaxed text-on-surface">{coach.message}</p>
          {coach.focusAreas.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {coach.focusAreas.map((f, i) => (
                <span key={i} className="rounded-full bg-primary/10 px-3 py-1 text-xs font-medium text-primary">{f}</span>
              ))}
            </div>
          )}
        </div>
      )}
    </Card>
  );
}

function PhotoCard() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [preview, setPreview] = useState<string | null>(null);
  const [result, setResult] = useState<MealPhotoResponse | null>(null);

  const onFile = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setError('');
    setResult(null);
    const reader = new FileReader();
    reader.onload = async () => {
      const dataUrl = reader.result as string;
      setPreview(dataUrl);
      const base64 = dataUrl.split(',')[1];
      setLoading(true);
      try {
        setResult(await aiApi.analyzeMealPhoto(base64, file.type));
      } catch (err) {
        setError(getErrorMessage(err));
      } finally {
        setLoading(false);
      }
    };
    reader.readAsDataURL(file);
  };

  return (
    <Card className="lg:col-span-2">
      <h3 className="mb-3 flex items-center gap-2 text-title-md font-bold text-on-surface">
        <Icon name="photo_camera" className="text-secondary" /> Fotoğraftan Yemek Analizi
      </h3>
      <p className="mb-4 text-body-sm text-on-surface-variant">Bir yemek fotoğrafı yükle; AI görüntüyü analiz edip kalori/makro tahmini yapsın.</p>
      <input
        type="file"
        accept="image/*"
        onChange={onFile}
        className="block text-body-sm text-on-surface-variant file:mr-3 file:rounded-lg file:border-0 file:bg-primary file:px-4 file:py-2 file:text-sm file:font-semibold file:text-on-primary hover:file:brightness-110"
      />
      <div className="mt-3"><ErrorAlert message={error} /></div>

      <div className="mt-4 grid gap-4 sm:grid-cols-2">
        {preview && <img src={preview} alt="Önizleme" className="max-h-56 rounded-xl border border-white/10 object-cover" />}
        <div>
          {loading && <Spinner label="Fotoğraf analiz ediliyor…" />}
          {result && (
            <div className="space-y-3">
              <p className="text-body-sm text-on-surface">{result.description}</p>
              <div className="grid grid-cols-4 gap-2 text-center">
                <Macro label="kcal" value={result.estimatedCalories} />
                <Macro label="Protein" value={`${result.estimatedProtein}g`} />
                <Macro label="Karb" value={`${result.estimatedCarbs}g`} />
                <Macro label="Yağ" value={`${result.estimatedFat}g`} />
              </div>
              <ul className="space-y-0.5 text-body-sm text-on-surface-variant">
                {result.foods.map((f, i) => (
                  <li key={i}>• {f.name} — {f.portion} <span className="text-on-surface-variant/60">({f.calories} kcal)</span></li>
                ))}
              </ul>
            </div>
          )}
        </div>
      </div>
    </Card>
  );
}

function Macro({ label, value }: { label: string; value: string | number }) {
  return (
    <div className="rounded-xl bg-surface-container-high py-2.5">
      <div className="text-body-sm font-bold text-on-surface">{value}</div>
      <div className="text-xs text-on-surface-variant">{label}</div>
    </div>
  );
}
