import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { nutritionApi } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, EmptyState, ErrorAlert, Field, Icon, Input, PageHeader, Select, Spinner } from '../components/ui';
import BarcodeAdd from '../components/BarcodeAdd';
import FoodSearch from '../components/FoodSearch';
import { mealLabels, mealOrder } from '../lib/labels';
import type { DailyNutritionSummary, Food, MealType } from '../types';

function todayStr() {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

const mealIcons: Record<MealType, { icon: string; chip: string }> = {
  Breakfast: { icon: 'sunny', chip: 'bg-amber-500/10 text-amber-400' },
  Lunch: { icon: 'restaurant', chip: 'bg-tertiary/10 text-tertiary' },
  Dinner: { icon: 'dinner_dining', chip: 'bg-secondary/10 text-secondary' },
  Snack: { icon: 'cookie', chip: 'bg-primary/10 text-primary' },
};

export default function Nutrition() {
  const [date, setDate] = useState(todayStr());
  const [summary, setSummary] = useState<DailyNutritionSummary | null>(null);
  const [foods, setFoods] = useState<Food[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [foodId, setFoodId] = useState<number | ''>('');
  const [amount, setAmount] = useState(100);
  const [mealType, setMealType] = useState<MealType>('Breakfast');
  const [adding, setAdding] = useState(false);

  const loadSummary = useCallback(() => {
    setLoading(true);
    nutritionApi.getSummary(date)
      .then(setSummary)
      .catch((e) => setError(getErrorMessage(e)))
      .finally(() => setLoading(false));
  }, [date]);

  useEffect(() => { loadSummary(); }, [loadSummary]);

  useEffect(() => {
    nutritionApi.getFoods()
      .then((list) => {
        setFoods(list);
        if (list.length > 0) setFoodId(list[0].id);
      })
      .catch((e) => setError(getErrorMessage(e)));
  }, []);

  const onAdd = async (e: FormEvent) => {
    e.preventDefault();
    if (foodId === '') return;
    setAdding(true);
    setError('');
    try {
      const isToday = date === todayStr();
      await nutritionApi.addLog({
        foodId: Number(foodId),
        amountGrams: Number(amount),
        mealType,
        loggedAt: isToday ? null : `${date}T12:00:00`,
      });
      loadSummary();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setAdding(false);
    }
  };

  const onDelete = async (id: number) => {
    try {
      await nutritionApi.deleteLog(id);
      loadSummary();
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };

  const handleFoodCreated = (food: Food) => {
    setFoods((prev) => [food, ...prev.filter((f) => f.id !== food.id)]);
    setFoodId(food.id);
  };

  const caloriePct = summary && summary.calorieGoal > 0
    ? Math.min(100, Math.round((summary.totalCalories / summary.calorieGoal) * 100)) : 0;

  return (
    <div>
      <PageHeader
        title="Beslenme"
        subtitle="Öğünlerini kaydet, kalori ve makrolarını takip et"
        action={<Input type="date" value={date} max={todayStr()} onChange={(e) => setDate(e.target.value)} className="w-auto" />}
      />

      <div className="mb-4"><ErrorAlert message={error} /></div>

      {/* Besin ekleme formu */}
      <Card className="mb-6">
        <form onSubmit={onAdd} className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-5 lg:items-end">
          <div className="lg:col-span-2">
            <Field label="Kayıtlı besinlerim" hint={foods.length === 0 ? 'Yukarıdan ara veya barkot ile ekle' : undefined}>
              <Select value={foodId} onChange={(e) => setFoodId(Number(e.target.value))}>
                {foods.map((f) => (
                  <option key={f.id} value={f.id}>{f.name} ({f.caloriesPer100g} kcal/100g)</option>
                ))}
              </Select>
            </Field>
          </div>
          <Field label="Miktar (g)"><Input type="number" value={amount} onChange={(e) => setAmount(Number(e.target.value))} min={1} max={5000} /></Field>
          <Field label="Öğün">
            <Select value={mealType} onChange={(e) => setMealType(e.target.value as MealType)}>
              {mealOrder.map((m) => <option key={m} value={m}>{mealLabels[m]}</option>)}
            </Select>
          </Field>
          <Button type="submit" disabled={adding || foodId === ''}>{adding ? 'Ekleniyor…' : '+ Ekle'}</Button>
        </form>
      </Card>

      <FoodSearch onFoodImported={handleFoodCreated} />

      <BarcodeAdd onFoodCreated={handleFoodCreated} />

      {loading || !summary ? (
        <Spinner />
      ) : (
        <>
          {/* Günlük toplam */}
          <Card className="mb-6">
            <div className="mb-3 flex flex-wrap items-end justify-between gap-4">
              <div>
                <div className="font-label-caps text-label-caps uppercase text-on-surface-variant">Toplam Kalori</div>
                <div className="mt-1 text-2xl font-bold text-on-surface">
                  {Math.round(summary.totalCalories)} <span className="text-base font-normal text-on-surface-variant">/ {summary.calorieGoal} kcal</span>
                </div>
              </div>
              <div className="flex gap-2 text-xs font-semibold">
                <span className="rounded-lg bg-tertiary/10 px-3 py-1.5 text-tertiary">P {Math.round(summary.totalProtein)}g</span>
                <span className="rounded-lg bg-primary/10 px-3 py-1.5 text-primary">K {Math.round(summary.totalCarbs)}g</span>
                <span className="rounded-lg bg-amber-500/10 px-3 py-1.5 text-amber-400">Y {Math.round(summary.totalFat)}g</span>
              </div>
            </div>
            <div className="h-2.5 w-full overflow-hidden rounded-full bg-white/5">
              <div className="h-full rounded-full bg-primary transition-all" style={{ width: `${caloriePct}%` }} />
            </div>
          </Card>

          {/* Öğünler */}
          <div className="space-y-4">
            {mealOrder.map((meal) => {
              const group = summary.meals.find((m) => m.mealType === meal);
              if (!group) return null;
              const mi = mealIcons[meal];
              return (
                <Card key={meal}>
                  <div className="mb-3 flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <div className={`flex h-10 w-10 items-center justify-center rounded-xl ${mi.chip}`}>
                        <Icon name={mi.icon} className="text-xl" />
                      </div>
                      <h3 className="font-semibold text-on-surface">{mealLabels[meal]}</h3>
                    </div>
                    <span className="text-body-sm font-bold text-on-surface">{Math.round(group.calories)} kcal</span>
                  </div>
                  {group.items.length === 0 ? (
                    <EmptyState message="Bu öğüne henüz besin eklenmedi." />
                  ) : (
                    <ul className="divide-y divide-white/5">
                      {group.items.map((item) => (
                        <li key={item.id} className="flex items-center justify-between py-2.5">
                          <div className="min-w-0">
                            <div className="font-medium text-on-surface">{item.foodName}</div>
                            <div className="text-xs text-on-surface-variant">
                              {item.amountGrams} g • {Math.round(item.calories)} kcal • P{Math.round(item.protein)} K{Math.round(item.carbs)} Y{Math.round(item.fat)}
                            </div>
                          </div>
                          <button onClick={() => onDelete(item.id)} className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg text-on-surface-variant transition-colors hover:bg-error/10 hover:text-error" title="Sil">
                            <Icon name="delete" className="text-lg" />
                          </button>
                        </li>
                      ))}
                    </ul>
                  )}
                </Card>
              );
            })}
          </div>
        </>
      )}
    </div>
  );
}
