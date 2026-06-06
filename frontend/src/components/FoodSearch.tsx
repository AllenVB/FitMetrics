import { useState, type FormEvent } from 'react';
import { nutritionApi } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, ErrorAlert, Icon, Input } from './ui';
import type { AiFoodSuggestion, Food } from '../types';

export default function FoodSearch({ onFoodImported }: { onFoodImported: (food: Food) => void }) {
  const [q, setQ] = useState('');
  const [results, setResults] = useState<AiFoodSuggestion[]>([]);
  const [searching, setSearching] = useState(false);
  const [importingIdx, setImportingIdx] = useState<number | null>(null);
  const [error, setError] = useState('');
  const [searched, setSearched] = useState(false);

  const search = async (e: FormEvent) => {
    e.preventDefault();
    if (!q.trim()) return;
    setSearching(true);
    setError('');
    setSearched(true);
    try {
      setResults(await nutritionApi.search(q.trim()));
    } catch (err) {
      setError(getErrorMessage(err));
      setResults([]);
    } finally {
      setSearching(false);
    }
  };

  const pick = async (r: AiFoodSuggestion, idx: number) => {
    setImportingIdx(idx);
    setError('');
    try {
      const food = await nutritionApi.createFood({
        name: r.name,
        brand: r.brand ?? null,
        category: 'AI',
        caloriesPer100g: r.caloriesPer100g,
        proteinPer100g: r.proteinPer100g,
        carbsPer100g: r.carbsPer100g,
        fatPer100g: r.fatPer100g,
      });
      onFoodImported(food);
      setResults([]);
      setQ('');
      setSearched(false);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setImportingIdx(null);
    }
  };

  return (
    <Card className="mb-6">
      <h3 className="mb-1 flex items-center gap-2 font-semibold text-on-surface">
        <Icon name="smart_toy" className="text-primary" /> Besin Ara <span className="ai-gradient-text font-bold">(AI)</span>
      </h3>
      <p className="mb-3 text-xs text-on-surface-variant">
        Yapay zeka 100 g başına tahmini kalori ve makroları üretir. Değerler tahminîdir; seçtiğinde kataloğa eklenir.
      </p>
      <form onSubmit={search} className="flex flex-wrap items-end gap-3">
        <div className="min-w-[200px] flex-1">
          <Input value={q} onChange={(e) => setQ(e.target.value)} placeholder="örn. tavuk göğsü, elma, yulaf, mercimek çorbası…" />
        </div>
        <Button type="submit" variant="secondary" disabled={searching}>{searching ? 'Analiz ediliyor…' : 'Ara'}</Button>
      </form>

      <div className="mt-3"><ErrorAlert message={error} /></div>

      {searched && !searching && !error && results.length === 0 && (
        <p className="mt-3 text-body-sm text-on-surface-variant">Sonuç bulunamadı.</p>
      )}

      {results.length > 0 && (
        <ul className="custom-scrollbar mt-3 max-h-80 divide-y divide-white/5 overflow-y-auto">
          {results.map((r, idx) => (
            <li key={idx} className="flex items-center justify-between gap-3 py-2.5">
              <div className="min-w-0">
                <div className="truncate font-medium text-on-surface">
                  {r.name}{r.brand ? <span className="text-on-surface-variant"> · {r.brand}</span> : null}
                </div>
                <div className="truncate text-xs text-on-surface-variant">{r.description}</div>
                <div className="mt-1 flex flex-wrap gap-1.5 text-[11px] font-semibold">
                  <span className="rounded bg-amber-500/10 px-1.5 py-0.5 text-amber-400">{Math.round(r.caloriesPer100g)} kcal</span>
                  <span className="rounded bg-tertiary/10 px-1.5 py-0.5 text-tertiary">P {Math.round(r.proteinPer100g)}</span>
                  <span className="rounded bg-primary/10 px-1.5 py-0.5 text-primary">K {Math.round(r.carbsPer100g)}</span>
                  <span className="rounded bg-secondary/10 px-1.5 py-0.5 text-secondary">Y {Math.round(r.fatPer100g)}</span>
                  <span className="text-on-surface-variant/60">/100g</span>
                </div>
              </div>
              <Button onClick={() => pick(r, idx)} disabled={importingIdx === idx} variant="secondary">
                {importingIdx === idx ? 'Ekleniyor…' : 'Seç'}
              </Button>
            </li>
          ))}
        </ul>
      )}
    </Card>
  );
}
