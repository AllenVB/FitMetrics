import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { workoutApi } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, EmptyState, ErrorAlert, Field, Icon, Input, PageHeader, Spinner } from '../components/ui';
import { categoryLabels, formatDate, muscleLabels } from '../lib/labels';
import type { Exercise, MuscleGroup, WorkoutLog } from '../types';

// Her kas grubuna ikon eşlemesi
const muscleIcons: Record<MuscleGroup, string> = {
  Chest:      'self_improvement',
  Back:       'accessibility_new',
  Legs:       'directions_run',
  Shoulders:  'sports_martial_arts',
  Arms:       'sports_handball',
  Core:       'sports_gymnastics',
  Cardio:     'favorite',
  FullBody:   'fitness_center',
};

// Görünüm sırası
const MUSCLE_ORDER: MuscleGroup[] = [
  'Chest', 'Back', 'Legs', 'Shoulders', 'Arms', 'Core', 'Cardio', 'FullBody',
];

export default function Workouts() {
  const [exercises, setExercises]       = useState<Exercise[]>([]);
  const [logs, setLogs]                 = useState<WorkoutLog[]>([]);
  const [loading, setLoading]           = useState(true);
  const [error, setError]               = useState('');

  // ---- Seçim adımları ----
  const [selectedMuscle, setSelectedMuscle] = useState<MuscleGroup | null>(null);
  const [exerciseId, setExerciseId]         = useState<number | ''>('');

  // ---- Form alanları ----
  const [sets, setSets]         = useState('');
  const [reps, setReps]         = useState('');
  const [weight, setWeight]     = useState('');
  const [duration, setDuration] = useState('');
  const [adding, setAdding]     = useState(false);

  const selectedExercise = exercises.find((e) => e.id === exerciseId);
  const isCardio         = selectedExercise?.category === 'Cardio';

  // Seçili kas grubuna ait egzersizler
  const filteredExercises = selectedMuscle
    ? exercises.filter((e) => e.muscleGroup === selectedMuscle)
    : [];

  const loadLogs = useCallback(() => {
    setLoading(true);
    workoutApi.getLogs()
      .then(setLogs)
      .catch((e) => setError(getErrorMessage(e)))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => { loadLogs(); }, [loadLogs]);
  useEffect(() => { workoutApi.getExercises().then(setExercises).catch((e) => setError(getErrorMessage(e))); }, []);

  // Kas grubu seçilince egzersiz sıfırla
  const handleMuscleSelect = (m: MuscleGroup) => {
    setSelectedMuscle(m);
    setExerciseId('');
  };

  const onAdd = async (e: FormEvent) => {
    e.preventDefault();
    if (exerciseId === '') return;
    setAdding(true);
    setError('');
    try {
      await workoutApi.addLog({
        exerciseId: Number(exerciseId),
        durationMinutes: duration === '' ? null : Number(duration),
        sets:            sets    === '' ? null : Number(sets),
        reps:            reps    === '' ? null : Number(reps),
        weightKg:        weight  === '' ? null : Number(weight),
      });
      setSets(''); setReps(''); setWeight(''); setDuration('');
      loadLogs();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setAdding(false);
    }
  };

  const onDelete = async (id: number) => {
    try {
      await workoutApi.deleteLog(id);
      loadLogs();
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };

  // Güne göre grupla
  const grouped: Record<string, WorkoutLog[]> = {};
  for (const log of logs) {
    const day = log.performedAt.slice(0, 10);
    (grouped[day] ??= []).push(log);
  }
  const days = Object.keys(grouped).sort().reverse();

  return (
    <div>
      <PageHeader title="Antrenman" subtitle="Çalışmak istediğin bölgeyi seç, egzersizini ekle" />

      <div className="mb-4"><ErrorAlert message={error} /></div>

      {/* ── ADIM 1: Kas Grubu Seçimi ── */}
      <Card className="mb-4">
        <p className="mb-3 text-xs font-semibold uppercase tracking-widest text-on-surface-variant">
          1 · Bölge Seç
        </p>
        <div className="grid grid-cols-4 gap-2 sm:grid-cols-8">
          {MUSCLE_ORDER.map((m) => {
            const active = selectedMuscle === m;
            return (
              <button
                key={m}
                type="button"
                onClick={() => handleMuscleSelect(m)}
                className={`flex flex-col items-center gap-1.5 rounded-xl px-2 py-3 text-center transition-all active:scale-95 ${
                  active
                    ? 'bg-primary text-on-primary shadow-lg shadow-primary/20'
                    : 'bg-surface-container text-on-surface-variant hover:bg-surface-container-high hover:text-on-surface'
                }`}
              >
                <Icon name={muscleIcons[m]} className="text-2xl" filled={active} />
                <span className="text-[10px] font-medium leading-tight">{muscleLabels[m]}</span>
              </button>
            );
          })}
        </div>
      </Card>

      {/* ── ADIM 2: Egzersiz Seçimi ── */}
      {selectedMuscle && (
        <Card className="mb-4">
          <p className="mb-3 text-xs font-semibold uppercase tracking-widest text-on-surface-variant">
            2 · Egzersiz Seç — <span className="text-primary">{muscleLabels[selectedMuscle]}</span>
          </p>
          <div className="flex flex-wrap gap-2">
            {filteredExercises.map((ex) => {
              const active = exerciseId === ex.id;
              return (
                <button
                  key={ex.id}
                  type="button"
                  onClick={() => setExerciseId(ex.id)}
                  className={`flex items-center gap-2 rounded-xl border px-3 py-2 text-sm font-medium transition-all active:scale-95 ${
                    active
                      ? 'border-primary bg-primary/10 text-primary'
                      : 'border-white/10 bg-surface-container text-on-surface-variant hover:border-primary/30 hover:text-on-surface'
                  }`}
                >
                  <Icon
                    name={ex.category === 'Cardio' ? 'directions_run' : 'fitness_center'}
                    className="text-base"
                    filled={active}
                  />
                  {ex.name}
                  <span className={`text-xs ${active ? 'text-primary/70' : 'text-on-surface-variant/50'}`}>
                    {ex.caloriesBurnedPerMinute} kcal/dk
                  </span>
                </button>
              );
            })}
          </div>
        </Card>
      )}

      {/* ── ADIM 3: Detay Formu ── */}
      {exerciseId !== '' && (
        <Card className="mb-6">
          <p className="mb-3 text-xs font-semibold uppercase tracking-widest text-on-surface-variant">
            3 · Detayları Gir
          </p>
          <form onSubmit={onAdd} className="grid grid-cols-2 gap-4 lg:grid-cols-5 lg:items-end">
            {isCardio ? (
              <div className="col-span-2 lg:col-span-2">
              <Field label="Süre (dk)">
                <Input type="number" value={duration} onChange={(e) => setDuration(e.target.value)} min={1} max={600} placeholder="30" />
              </Field>
            </div>
            ) : (
              <>
                <Field label="Set">
                  <Input type="number" value={sets} onChange={(e) => setSets(e.target.value)} min={1} max={50} placeholder="4" />
                </Field>
                <Field label="Tekrar">
                  <Input type="number" value={reps} onChange={(e) => setReps(e.target.value)} min={1} max={1000} placeholder="10" />
                </Field>
                <Field label="Ağırlık (kg)">
                  <Input type="number" step="0.5" value={weight} onChange={(e) => setWeight(e.target.value)} min={0} placeholder="80" />
                </Field>
              </>
            )}
            <div className="col-span-2 lg:col-span-2 lg:col-start-4">
              <Button type="submit" className="w-full" disabled={adding}>
                {adding ? 'Kaydediliyor…' : '+ Antrenmanı Kaydet'}
              </Button>
            </div>
          </form>
          <p className="mt-2 text-xs text-on-surface-variant/70">
            {isCardio
              ? 'Kardiyo egzersizlerinde süre giriniz; kalori otomatik hesaplanır.'
              : 'Kuvvet egzersizlerinde set/tekrar/ağırlık giriniz; kalori otomatik hesaplanır.'}
          </p>
        </Card>
      )}

      {/* ── Geçmiş ── */}
      <p className="mb-3 text-xs font-semibold uppercase tracking-widest text-on-surface-variant">
        Antrenman Geçmişi
      </p>

      {loading ? (
        <Spinner />
      ) : days.length === 0 ? (
        <EmptyState message="Henüz antrenman kaydın yok. Yukarıdan bölge seçerek başla!" />
      ) : (
        <div className="space-y-4">
          {days.map((day) => (
            <Card key={day}>
              <h3 className="mb-3 font-semibold text-on-surface">{formatDate(day)}</h3>
              <ul className="divide-y divide-white/5">
                {grouped[day].map((log) => (
                  <li key={log.id} className="flex items-center justify-between py-2.5">
                    <div className="flex items-center gap-3">
                      <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-primary/10">
                        <Icon
                          name={muscleIcons[log.muscleGroup]}
                          className="text-lg text-primary"
                          filled
                        />
                      </div>
                      <div>
                        <div className="flex flex-wrap items-center gap-1.5 font-medium text-on-surface">
                          {log.exerciseName}
                          <span className="rounded bg-white/5 px-1.5 py-0.5 text-xs text-on-surface-variant">
                            {categoryLabels[log.category]} · {muscleLabels[log.muscleGroup]}
                          </span>
                        </div>
                        <div className="text-xs text-on-surface-variant">
                          {log.durationMinutes ? `${log.durationMinutes} dk` : ''}
                          {log.sets ? `${log.sets} set × ${log.reps ?? '-'} tekrar` : ''}
                          {log.weightKg ? ` @ ${log.weightKg} kg` : ''}
                          {' · '}{Math.round(log.caloriesBurned)} kcal
                        </div>
                      </div>
                    </div>
                    <button
                      onClick={() => onDelete(log.id)}
                      className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg text-on-surface-variant transition-colors hover:bg-error/10 hover:text-error"
                      title="Sil"
                    >
                      <Icon name="delete" className="text-lg" />
                    </button>
                  </li>
                ))}
              </ul>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
