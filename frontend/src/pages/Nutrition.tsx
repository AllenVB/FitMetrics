import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { nutritionApi } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, EmptyState, ErrorAlert, Field, Input, PageHeader, Select, Spinner } from '../components/ui';
import BarcodeAdd from '../components/BarcodeAdd';
import { mealLabels, mealOrder } from '../lib/labels';
import type { DailyNutritionSummary, Food, MealType } from '../types';

function todayStr() {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

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
        title="🍽️ Beslenme"
        subtitle="Öğünlerini kaydet, kalori ve makrolarını takip et"
        action={<Input type="date" value={date} max={todayStr()} onChange={(e) => setDate(e.target.value)} className="w-auto" />}
      />

      <div className="mb-4"><ErrorAlert message={error} /></div>

      {/* Besin ekleme formu */}
      <Card className="mb-6">
        <form onSubmit={onAdd} className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-5 lg:items-end">
          <div className="lg:col-span-2">
            <Field label="Besin">
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

      <BarcodeAdd onFoodCreated={handleFoodCreated} />

      {loading || !summary ? (
        <Spinner />
      ) : (
        <>
          {/* Günlük toplam */}
          <Card className="mb-6">
            <div className="mb-2 flex items-end justify-between">
              <div>
                <div className="text-sm text-slate-500">Toplam Kalori</div>
                <div className="text-2xl font-bold text-slate-800">
                  {Math.round(summary.totalCalories)} <span className="text-base font-normal text-slate-400">/ {summary.calorieGoal} kcal</span>
                </div>
              </div>
              <div className="flex gap-4 text-sm">
                <span className="text-brand-600">P {Math.round(summary.totalProtein)}g</span>
                <span className="text-blue-600">K {Math.round(summary.totalCarbs)}g</span>
                <span className="text-amber-600">Y {Math.round(summary.totalFat)}g</span>
              </div>
            </div>
            <div className="h-2.5 w-full overflow-hidden rounded-full bg-slate-100">
              <div className="h-full rounded-full bg-brand-500 transition-all" style={{ width: `${caloriePct}%` }} />
            </div>
          </Card>

          {/* Öğünler */}
          <div className="space-y-4">
            {mealOrder.map((meal) => {
              const group = summary.meals.find((m) => m.mealType === meal);
              if (!group) return null;
              return (
                <Card key={meal}>
                  <div className="mb-3 flex items-center justify-between">
                    <h3 className="font-semibold text-slate-800">{mealLabels[meal]}</h3>
                    <span className="text-sm text-slate-500">{Math.round(group.calories)} kcal</span>
                  </div>
                  {group.items.length === 0 ? (
                    <EmptyState message="Bu öğüne henüz besin eklenmedi." />
                  ) : (
                    <ul className="divide-y divide-slate-100">
                      {group.items.map((item) => (
                        <li key={item.id} className="flex items-center justify-between py-2.5">
                          <div>
                            <div className="font-medium text-slate-700">{item.foodName}</div>
                            <div className="text-xs text-slate-400">
                              {item.amountGrams} g • {Math.round(item.calories)} kcal • P{Math.round(item.protein)} K{Math.round(item.carbs)} Y{Math.round(item.fat)}
                            </div>
                          </div>
                          <button onClick={() => onDelete(item.id)} className="rounded-md px-2 py-1 text-sm text-rose-500 hover:bg-rose-50">
                            Sil
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
