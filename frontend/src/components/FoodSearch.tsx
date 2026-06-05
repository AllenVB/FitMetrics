import { useState, type FormEvent } from 'react';
import { nutritionApi } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, ErrorAlert, Input } from './ui';
import type { FatSecretFoodResult, Food } from '../types';

export default function FoodSearch({ onFoodImported }: { onFoodImported: (food: Food) => void }) {
  const [q, setQ] = useState('');
  const [results, setResults] = useState<FatSecretFoodResult[]>([]);
  const [searching, setSearching] = useState(false);
  const [importingId, setImportingId] = useState<string | null>(null);
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

  const pick = async (r: FatSecretFoodResult) => {
    setImportingId(r.id);
    setError('');
    try {
      const food = await nutritionApi.importFood(r.id);
      onFoodImported(food);
      setResults([]);
      setQ('');
      setSearched(false);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setImportingId(null);
    }
  };

  return (
    <Card className="mb-6">
      <h3 className="mb-3 font-semibold text-slate-800">🔎 Besin Ara (FatSecret)</h3>
      <form onSubmit={search} className="flex flex-wrap items-end gap-3">
        <div className="min-w-[200px] flex-1">
          <Input value={q} onChange={(e) => setQ(e.target.value)} placeholder="örn. tavuk göğsü, elma, yulaf…" />
        </div>
        <Button type="submit" variant="secondary" disabled={searching}>{searching ? 'Aranıyor…' : 'Ara'}</Button>
      </form>

      <div className="mt-3"><ErrorAlert message={error} /></div>

      {searched && !searching && !error && results.length === 0 && (
        <p className="mt-3 text-sm text-slate-400">Sonuç bulunamadı.</p>
      )}

      {results.length > 0 && (
        <ul className="mt-3 max-h-72 divide-y divide-slate-100 overflow-y-auto">
          {results.map((r) => (
            <li key={r.id} className="flex items-center justify-between gap-3 py-2">
              <div className="min-w-0">
                <div className="truncate font-medium text-slate-700">
                  {r.name}{r.brand ? <span className="text-slate-400"> · {r.brand}</span> : null}
                </div>
                <div className="truncate text-xs text-slate-400">{r.description}</div>
              </div>
              <Button onClick={() => pick(r)} disabled={importingId === r.id} variant="secondary">
                {importingId === r.id ? 'Ekleniyor…' : 'Seç'}
              </Button>
            </li>
          ))}
        </ul>
      )}
    </Card>
  );
}
