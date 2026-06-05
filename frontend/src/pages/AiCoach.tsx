import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react';
import { aiApi } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, ErrorAlert, Field, Input, PageHeader, Spinner } from '../components/ui';
import type { CoachResponse, MealPhotoResponse, MealPlanResponse } from '../types';

export default function AiCoach() {
  const [enabled, setEnabled] = useState<boolean | null>(null);

  useEffect(() => {
    aiApi.status().then((s) => setEnabled(s.enabled)).catch(() => setEnabled(false));
  }, []);

  if (enabled === null) return <Spinner />;

  return (
    <div>
      <PageHeader title="🤖 AI Koç" subtitle="Claude destekli öğün planı, kişisel koçluk ve fotoğraftan analiz" />

      {!enabled ? (
        <Card className="border-l-4 border-amber-400">
          <h3 className="mb-1 font-semibold text-slate-800">⚠️ AI özellikleri şu an kapalı</h3>
          <p className="text-sm text-slate-600">
            Bu özellikler Claude API gerektirir. Sunucuda <code className="rounded bg-slate-100 px-1">ANTHROPIC_API_KEY</code> ortam
            değişkeni (veya <code className="rounded bg-slate-100 px-1">appsettings.json → Anthropic:ApiKey</code>) tanımlandığında otomatik
            etkinleşir. Uygulamanın geri kalanı anahtar olmadan da tam çalışır.
          </p>
        </Card>
      ) : (
        <div className="grid gap-6 lg:grid-cols-2">
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
      <h3 className="mb-3 font-semibold text-slate-800">🍳 Akıllı Öğün Planı</h3>
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
          <p className="text-sm text-slate-600">{plan.summary}</p>
          <div className="text-xs font-medium text-slate-500">Toplam ~{plan.totalCalories} kcal · {plan.totalProtein} g protein</div>
          {plan.meals.map((meal, i) => (
            <div key={i} className="rounded-lg border border-slate-200 p-3">
              <div className="mb-1 flex justify-between text-sm font-semibold text-slate-700">
                <span>{meal.mealType}</span>
                <span>{meal.calories} kcal</span>
              </div>
              <ul className="space-y-0.5 text-sm text-slate-600">
                {meal.foods.map((f, j) => (
                  <li key={j}>• {f.name} — {f.amount} <span className="text-slate-400">({f.calories} kcal, {f.protein}g P)</span></li>
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
      <h3 className="mb-3 font-semibold text-slate-800">💬 Kişisel Koçluk</h3>
      <p className="mb-3 text-sm text-slate-500">Verilerinden çıkan analizleri sıcak, uygulanabilir bir koçluk mesajına dönüştürür.</p>
      <Button onClick={run} disabled={loading}>{loading ? 'Hazırlanıyor…' : 'Koçluk al'}</Button>
      <div className="mt-3"><ErrorAlert message={error} /></div>
      {coach && (
        <div className="mt-4 space-y-3">
          <p className="whitespace-pre-line text-sm leading-relaxed text-slate-700">{coach.message}</p>
          {coach.focusAreas.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {coach.focusAreas.map((f, i) => (
                <span key={i} className="rounded-full bg-brand-50 px-3 py-1 text-xs font-medium text-brand-700">{f}</span>
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
      <h3 className="mb-3 font-semibold text-slate-800">📷 Fotoğraftan Yemek Analizi</h3>
      <p className="mb-3 text-sm text-slate-500">Bir yemek fotoğrafı yükle; Claude görüntüyü analiz edip kalori/makro tahmini yapsın.</p>
      <input type="file" accept="image/*" onChange={onFile} className="block text-sm text-slate-600 file:mr-3 file:rounded-lg file:border-0 file:bg-brand-600 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-white hover:file:bg-brand-700" />
      <div className="mt-3"><ErrorAlert message={error} /></div>

      <div className="mt-4 grid gap-4 sm:grid-cols-2">
        {preview && <img src={preview} alt="Önizleme" className="max-h-56 rounded-lg border border-slate-200 object-cover" />}
        <div>
          {loading && <Spinner label="Fotoğraf analiz ediliyor…" />}
          {result && (
            <div className="space-y-2">
              <p className="text-sm text-slate-700">{result.description}</p>
              <div className="grid grid-cols-4 gap-2 text-center">
                <Macro label="kcal" value={result.estimatedCalories} />
                <Macro label="Protein" value={`${result.estimatedProtein}g`} />
                <Macro label="Karb" value={`${result.estimatedCarbs}g`} />
                <Macro label="Yağ" value={`${result.estimatedFat}g`} />
              </div>
              <ul className="space-y-0.5 text-sm text-slate-600">
                {result.foods.map((f, i) => (
                  <li key={i}>• {f.name} — {f.portion} <span className="text-slate-400">({f.calories} kcal)</span></li>
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
    <div className="rounded-lg bg-slate-50 py-2">
      <div className="text-sm font-bold text-slate-800">{value}</div>
      <div className="text-xs text-slate-400">{label}</div>
    </div>
  );
}
