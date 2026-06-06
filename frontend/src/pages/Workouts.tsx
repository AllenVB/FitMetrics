import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { workoutApi } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, EmptyState, ErrorAlert, Field, Icon, Input, PageHeader, Select, Spinner } from '../components/ui';
import { categoryLabels, formatDate, muscleLabels } from '../lib/labels';
import type { Exercise, WorkoutLog } from '../types';

export default function Workouts() {
  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [logs, setLogs] = useState<WorkoutLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [exerciseId, setExerciseId] = useState<number | ''>('');
  const [sets, setSets] = useState('');
  const [reps, setReps] = useState('');
  const [weight, setWeight] = useState('');
  const [duration, setDuration] = useState('');
  const [adding, setAdding] = useState(false);

  const selectedExercise = exercises.find((e) => e.id === exerciseId);
  const isCardio = selectedExercise?.category === 'Cardio';

  const loadLogs = useCallback(() => {
    setLoading(true);
    workoutApi.getLogs()
      .then(setLogs)
      .catch((e) => setError(getErrorMessage(e)))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => { loadLogs(); }, [loadLogs]);

  useEffect(() => {
    workoutApi.getExercises()
      .then((list) => {
        setExercises(list);
        if (list.length > 0) setExerciseId(list[0].id);
      })
      .catch((e) => setError(getErrorMessage(e)));
  }, []);

  const onAdd = async (e: FormEvent) => {
    e.preventDefault();
    if (exerciseId === '') return;
    setAdding(true);
    setError('');
    try {
      await workoutApi.addLog({
        exerciseId: Number(exerciseId),
        durationMinutes: duration === '' ? null : Number(duration),
        sets: sets === '' ? null : Number(sets),
        reps: reps === '' ? null : Number(reps),
        weightKg: weight === '' ? null : Number(weight),
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
      <PageHeader title="Antrenman" subtitle="Egzersizlerini kaydet, hacmini ve yakılan kaloriyi izle" />

      <div className="mb-4"><ErrorAlert message={error} /></div>

      <Card className="mb-6">
        <form onSubmit={onAdd} className="grid grid-cols-2 gap-4 lg:grid-cols-6 lg:items-end">
          <div className="col-span-2">
            <Field label="Egzersiz">
              <Select value={exerciseId} onChange={(e) => setExerciseId(Number(e.target.value))}>
                {exercises.map((ex) => (
                  <option key={ex.id} value={ex.id}>{ex.name} — {muscleLabels[ex.muscleGroup]}</option>
                ))}
              </Select>
            </Field>
          </div>
          {isCardio ? (
            <Field label="Süre (dk)"><Input type="number" value={duration} onChange={(e) => setDuration(e.target.value)} min={1} max={600} placeholder="30" /></Field>
          ) : (
            <>
              <Field label="Set"><Input type="number" value={sets} onChange={(e) => setSets(e.target.value)} min={1} max={50} placeholder="4" /></Field>
              <Field label="Tekrar"><Input type="number" value={reps} onChange={(e) => setReps(e.target.value)} min={1} max={1000} placeholder="10" /></Field>
              <Field label="Ağırlık (kg)"><Input type="number" step="0.5" value={weight} onChange={(e) => setWeight(e.target.value)} min={0} placeholder="80" /></Field>
            </>
          )}
          <Button type="submit" disabled={adding || exerciseId === ''}>{adding ? 'Ekleniyor…' : '+ Ekle'}</Button>
        </form>
        <p className="mt-2 text-xs text-on-surface-variant/70">
          {isCardio ? 'Kardiyo için süre giriniz.' : 'Kuvvet için set/tekrar/ağırlık giriniz. Yakılan kalori otomatik hesaplanır.'}
        </p>
      </Card>

      {loading ? (
        <Spinner />
      ) : days.length === 0 ? (
        <EmptyState message="Henüz antrenman kaydın yok. Yukarıdan ekleyerek başla!" />
      ) : (
        <div className="space-y-4">
          {days.map((day) => (
            <Card key={day}>
              <h3 className="mb-3 font-semibold text-on-surface">{formatDate(day)}</h3>
              <ul className="divide-y divide-white/5">
                {grouped[day].map((log) => (
                  <li key={log.id} className="flex items-center justify-between py-2.5">
                    <div>
                      <div className="font-medium text-on-surface">
                        {log.exerciseName}
                        <span className="ml-2 rounded bg-white/5 px-1.5 py-0.5 text-xs text-on-surface-variant">
                          {categoryLabels[log.category]} • {muscleLabels[log.muscleGroup]}
                        </span>
                      </div>
                      <div className="text-xs text-on-surface-variant">
                        {log.durationMinutes ? `${log.durationMinutes} dk` : ''}
                        {log.sets ? `${log.sets} set × ${log.reps ?? '-'} tekrar` : ''}
                        {log.weightKg ? ` @ ${log.weightKg} kg` : ''}
                        {' • '}{Math.round(log.caloriesBurned)} kcal
                      </div>
                    </div>
                    <button onClick={() => onDelete(log.id)} className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg text-on-surface-variant transition-colors hover:bg-error/10 hover:text-error" title="Sil">
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
