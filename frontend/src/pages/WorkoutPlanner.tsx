import { useEffect, useRef, useState } from 'react';
import { workoutApi, workoutPlanApi, type SavePlanDay, type SavePlanExercise } from '../api';
import { getErrorMessage } from '../api/client';
import { Button, Card, ErrorAlert, Icon, PageHeader, Spinner } from '../components/ui';
import { muscleLabels } from '../lib/labels';
import type { Exercise, MuscleGroup, WorkoutPlanDetail, WorkoutPlanExerciseItem, WorkoutPlanSummary } from '../types';

const DAYS = ['Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma', 'Cumartesi', 'Pazar'];
const MUSCLE_ORDER: MuscleGroup[] = ['Chest', 'Back', 'Legs', 'Shoulders', 'Arms', 'Core', 'Cardio', 'FullBody'];

const muscleIcons: Record<MuscleGroup, string> = {
  Chest: 'self_improvement', Back: 'accessibility_new', Legs: 'directions_run',
  Shoulders: 'sports_martial_arts', Arms: 'sports_handball', Core: 'sports_gymnastics',
  Cardio: 'favorite', FullBody: 'fitness_center',
};

type DraftExercise = {
  key: string;
  exerciseId: number;
  name: string;
  muscleGroup: string;
  category: string;
  sets: string;
  reps: string;
  duration: string;
};

type DraftDay = DraftExercise[];
type Draft = DraftDay[];

function emptyDraft(): Draft { return Array.from({ length: 7 }, () => []); }

function planToDraft(plan: WorkoutPlanDetail): Draft {
  const d = emptyDraft();
  for (const day of plan.days) {
    d[day.dayIndex] = day.exercises.map(e => ({
      key: `${e.exerciseId}-${Math.random()}`,
      exerciseId: e.exerciseId,
      name: e.exerciseName,
      muscleGroup: e.muscleGroup,
      category: e.category,
      sets: e.sets?.toString() ?? '',
      reps: e.reps?.toString() ?? '',
      duration: e.durationMinutes?.toString() ?? '',
    }));
  }
  return d;
}

function draftToPayload(name: string, draft: Draft) {
  const days: SavePlanDay[] = draft.map((exList, dayIndex) => ({
    dayIndex,
    exercises: exList.map((ex, i): SavePlanExercise => ({
      exerciseId: ex.exerciseId,
      sets: ex.sets !== '' ? Number(ex.sets) : null,
      reps: ex.reps !== '' ? Number(ex.reps) : null,
      durationMinutes: ex.duration !== '' ? Number(ex.duration) : null,
      sortOrder: i,
    })),
  }));
  return { name, days };
}

// ── Aktif oturum tipi ────────────────────────────────────────────────────────
type SetEntry = { reps: string; weight: string; done: boolean };
type SessionExercise = WorkoutPlanExerciseItem & { setEntries: SetEntry[] };

export default function WorkoutPlanner() {
  const [view, setView] = useState<'list' | 'create' | 'edit' | 'session'>('list');
  const [exercises, setExercises] = useState<Exercise[]>([]);
  const [plans, setPlans] = useState<WorkoutPlanSummary[]>([]);
  const [loadingPlans, setLoadingPlans] = useState(true);
  const [expandedPlan, setExpandedPlan] = useState<WorkoutPlanDetail | null>(null);
  const [loadingExpand, setLoadingExpand] = useState(false);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const [planName, setPlanName] = useState('');
  const [draft, setDraft] = useState<Draft>(emptyDraft());
  const [editId, setEditId] = useState<number | null>(null);
  const [activeMusclePicker, setActiveMusclePicker] = useState<MuscleGroup>('Chest');

  // Aktif antrenman oturumu
  const [sessionPlan, setSessionPlan] = useState<WorkoutPlanDetail | null>(null);
  const [sessionDayIdx, setSessionDayIdx] = useState(0);
  const [sessionExercises, setSessionExercises] = useState<SessionExercise[]>([]);
  const [sessionFinishing, setSessionFinishing] = useState(false);
  const [sessionStartTime] = useState<Date>(new Date());

  const dragEx = useRef<Exercise | null>(null);
  const [dragOverDay, setDragOverDay] = useState<number | null>(null);

  const fetchPlans = () => {
    setLoadingPlans(true);
    workoutPlanApi.getAll()
      .then(setPlans)
      .catch(e => setError(getErrorMessage(e)))
      .finally(() => setLoadingPlans(false));
  };

  useEffect(() => {
    workoutApi.getExercises().then(setExercises).catch(e => setError(getErrorMessage(e)));
    fetchPlans();
  }, []);

  const filteredExercises = exercises.filter(e => e.muscleGroup === activeMusclePicker);

  // ── Drag handlers ────────────────────────────────────────────────────────
  const onDragStart = (ex: Exercise) => { dragEx.current = ex; };
  const onDrop = (dayIdx: number) => {
    const ex = dragEx.current;
    if (!ex) return;
    addToDay(dayIdx, ex);
    dragEx.current = null;
    setDragOverDay(null);
  };

  const addToDay = (dayIdx: number, ex: Exercise) => {
    setDraft(prev => {
      const next = prev.map(d => [...d]);
      next[dayIdx] = [...next[dayIdx], {
        key: `${ex.id}-${Date.now()}`,
        exerciseId: ex.id,
        name: ex.name,
        muscleGroup: ex.muscleGroup,
        category: ex.category,
        sets: ex.category === 'Cardio' ? '' : '4',
        reps: ex.category === 'Cardio' ? '' : '10',
        duration: ex.category === 'Cardio' ? '30' : '',
      }];
      return next;
    });
  };

  const removeFromDay = (dayIdx: number, key: string) => {
    setDraft(prev => {
      const next = prev.map(d => [...d]);
      next[dayIdx] = next[dayIdx].filter(e => e.key !== key);
      return next;
    });
  };

  const updateField = (dayIdx: number, key: string, field: 'sets' | 'reps' | 'duration', val: string) => {
    setDraft(prev => {
      const next = prev.map(d => [...d]);
      next[dayIdx] = next[dayIdx].map(e => e.key === key ? { ...e, [field]: val } : e);
      return next;
    });
  };

  // ── Kaydet ──────────────────────────────────────────────────────────────
  const savePlan = async () => {
    if (!planName.trim()) { setError('Program adı gerekli.'); return; }
    setSaving(true); setError('');
    try {
      const payload = draftToPayload(planName, draft);
      if (editId !== null) await workoutPlanApi.update(editId, payload);
      else await workoutPlanApi.create(payload);
      fetchPlans();
      resetForm();
      setView('list');
    } catch (e) { setError(getErrorMessage(e)); }
    finally { setSaving(false); }
  };

  const resetForm = () => { setPlanName(''); setDraft(emptyDraft()); setEditId(null); };

  const startEdit = async (id: number) => {
    setLoadingExpand(true); setError('');
    try {
      const plan = await workoutPlanApi.getById(id);
      setPlanName(plan.name);
      setDraft(planToDraft(plan));
      setEditId(id);
      setView('edit');
    } catch (e) { setError(getErrorMessage(e)); }
    finally { setLoadingExpand(false); }
  };

  const deletePlan = async (id: number) => {
    if (!window.confirm('Bu programı silmek istediğine emin misin?')) return;
    try {
      await workoutPlanApi.remove(id);
      setPlans(p => p.filter(x => x.id !== id));
      if (expandedPlan?.id === id) setExpandedPlan(null);
    } catch (e) { setError(getErrorMessage(e)); }
  };

  const toggleExpand = async (id: number) => {
    if (expandedPlan?.id === id) { setExpandedPlan(null); return; }
    setLoadingExpand(true);
    try { setExpandedPlan(await workoutPlanApi.getById(id)); }
    catch (e) { setError(getErrorMessage(e)); }
    finally { setLoadingExpand(false); }
  };

  // ── Aktif Antrenman ──────────────────────────────────────────────────────
  const startSession = async (planId: number) => {
    setLoadingExpand(true); setError('');
    try {
      const plan = await workoutPlanApi.getById(planId);
      const todayJs = new Date().getDay(); // 0=Pazar
      const todayIdx = todayJs === 0 ? 6 : todayJs - 1; // 0=Pazartesi
      const defaultDay = plan.days.find(d => d.dayIndex === todayIdx) ? todayIdx
        : plan.days.length > 0 ? plan.days[0].dayIndex : 0;

      setSessionPlan(plan);
      setSessionDayIdx(defaultDay);
      buildSessionExercises(plan, defaultDay);
      setView('session');
    } catch (e) { setError(getErrorMessage(e)); }
    finally { setLoadingExpand(false); }
  };

  const buildSessionExercises = (plan: WorkoutPlanDetail, dayIdx: number) => {
    const day = plan.days.find(d => d.dayIndex === dayIdx);
    const exList = day?.exercises ?? [];
    setSessionExercises(exList.map(ex => ({
      ...ex,
      setEntries: Array.from({ length: ex.sets ?? 1 }, () => ({
        reps: ex.reps?.toString() ?? '',
        weight: '',
        done: false,
      })),
    })));
  };

  const updateSetEntry = (exIdx: number, setIdx: number, field: 'reps' | 'weight' | 'done', val: string | boolean) => {
    setSessionExercises(prev => prev.map((ex, ei) =>
      ei !== exIdx ? ex : {
        ...ex,
        setEntries: ex.setEntries.map((s, si) =>
          si !== setIdx ? s : { ...s, [field]: val }
        ),
      }
    ));
  };

  const finishSession = async () => {
    setSessionFinishing(true); setError('');
    try {
      const now = new Date().toISOString();
      await Promise.all(
        sessionExercises
          .filter(ex => ex.setEntries.some(s => s.done))
          .map(ex => {
            const completedSets = ex.setEntries.filter(s => s.done);
            const avgReps = completedSets.length > 0
              ? Math.round(completedSets.reduce((a, s) => a + (Number(s.reps) || 0), 0) / completedSets.length)
              : null;
            const avgWeight = completedSets.some(s => s.weight !== '')
              ? completedSets.reduce((a, s) => a + (Number(s.weight) || 0), 0) / completedSets.length
              : null;
            return workoutApi.addLog({
              exerciseId: ex.exerciseId,
              sets: completedSets.length,
              reps: avgReps,
              weightKg: avgWeight,
              durationMinutes: ex.durationMinutes ?? null,
              performedAt: now,
              notes: `${sessionPlan?.name ?? ''} — ${DAYS[sessionDayIdx]}`,
            });
          })
      );
      setView('list');
      setSessionPlan(null);
    } catch (e) { setError(getErrorMessage(e)); }
    finally { setSessionFinishing(false); }
  };

  const isCreating = view === 'create' || view === 'edit';
  const totalItems = draft.reduce((s, d) => s + d.length, 0);
  const sessionDay = sessionPlan?.days.find(d => d.dayIndex === sessionDayIdx);
  const completedCount = sessionExercises.filter(ex => ex.setEntries.every(s => s.done)).length;

  return (
    <div>
      <PageHeader title="Antrenman Programı" subtitle="7 günlük kişisel programını oluştur ve yönet" />
      <div className="mb-4"><ErrorAlert message={error} /></div>

      {/* ── Sekme çubuğu (oturum modunda gizle) ── */}
      {view !== 'session' && (
        <div className="mb-6 flex gap-2">
          <button
            onClick={() => { setView('list'); resetForm(); }}
            className={`flex items-center gap-2 rounded-xl px-4 py-2 text-sm font-medium transition-all ${!isCreating ? 'bg-primary text-on-primary' : 'bg-surface-container text-on-surface-variant hover:text-on-surface'}`}
          >
            <Icon name="list" className="text-base" /> Programlarım
          </button>
          <button
            onClick={() => { resetForm(); setView('create'); }}
            className={`flex items-center gap-2 rounded-xl px-4 py-2 text-sm font-medium transition-all ${isCreating ? 'bg-primary text-on-primary' : 'bg-surface-container text-on-surface-variant hover:text-on-surface'}`}
          >
            <Icon name="add_circle" className="text-base" />
            {view === 'edit' ? 'Programı Düzenle' : 'Yeni Program'}
          </button>
        </div>
      )}

      {/* ══════════════════════════════════════════════════════════════════ */}
      {/* AKTİF ANTRENMAN OTURUMU                                           */}
      {/* ══════════════════════════════════════════════════════════════════ */}
      {view === 'session' && sessionPlan && (
        <div className="space-y-4">
          {/* Başlık */}
          <div className="flex items-center justify-between">
            <div>
              <h2 className="text-xl font-bold text-on-surface">{sessionPlan.name}</h2>
              <p className="text-sm text-on-surface-variant">
                {completedCount}/{sessionExercises.length} egzersiz tamamlandı
              </p>
            </div>
            <button
              onClick={() => setView('list')}
              className="flex h-9 w-9 items-center justify-center rounded-xl text-on-surface-variant transition-colors hover:bg-white/5 hover:text-on-surface"
            >
              <Icon name="close" className="text-xl" />
            </button>
          </div>

          {/* Gün seçici */}
          <div className="custom-scrollbar flex gap-2 overflow-x-auto pb-1">
            {sessionPlan.days.filter(d => d.exercises.length > 0).map(d => (
              <button
                key={d.dayIndex}
                onClick={() => { setSessionDayIdx(d.dayIndex); buildSessionExercises(sessionPlan, d.dayIndex); }}
                className={`flex shrink-0 flex-col items-center rounded-xl px-4 py-2 text-sm font-semibold transition-all ${
                  sessionDayIdx === d.dayIndex ? 'bg-primary text-on-primary' : 'bg-surface-container text-on-surface-variant hover:text-on-surface'
                }`}
              >
                <span>{DAYS[d.dayIndex]}</span>
                <span className="text-[10px] font-normal opacity-70">{d.exercises.length} egzersiz</span>
              </button>
            ))}
          </div>

          {sessionExercises.length === 0 ? (
            <Card className="py-12 text-center text-on-surface-variant">Bu gün için egzersiz yok.</Card>
          ) : (
            <div className="space-y-3">
              {sessionExercises.map((ex, exIdx) => {
                const allDone = ex.setEntries.every(s => s.done);
                return (
                  <Card key={ex.id} className={`transition-all ${allDone ? 'opacity-60 ring-1 ring-primary/30' : ''}`}>
                    <div className="mb-3 flex items-center gap-3">
                      <div className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-xl ${allDone ? 'bg-primary' : 'bg-primary/10'}`}>
                        {allDone
                          ? <Icon name="check" className="text-on-primary" />
                          : <Icon name={muscleIcons[ex.muscleGroup as MuscleGroup] ?? 'fitness_center'} className="text-primary text-lg" filled />
                        }
                      </div>
                      <div>
                        <p className="font-semibold text-on-surface">{ex.exerciseName}</p>
                        <p className="text-xs text-on-surface-variant">
                          {ex.durationMinutes ? `${ex.durationMinutes} dk` : `Hedef: ${ex.sets ?? '-'} × ${ex.reps ?? '-'} tekrar`}
                        </p>
                      </div>
                    </div>

                    {ex.durationMinutes ? (
                      // Kardio: sadece tek tamamla butonu
                      <button
                        onClick={() => updateSetEntry(exIdx, 0, 'done', !ex.setEntries[0]?.done)}
                        className={`w-full rounded-xl py-2 text-sm font-semibold transition-all ${
                          ex.setEntries[0]?.done ? 'bg-primary/20 text-primary' : 'bg-surface-container text-on-surface-variant hover:bg-primary/10'
                        }`}
                      >
                        {ex.setEntries[0]?.done ? '✓ Tamamlandı' : `${ex.durationMinutes} dk Cardio — Tamamla`}
                      </button>
                    ) : (
                      // Kuvvet: set tablosu
                      <div className="space-y-2">
                        <div className="grid grid-cols-[2rem_1fr_1fr_2.5rem] gap-2 text-[10px] font-bold uppercase tracking-wider text-on-surface-variant">
                          <span>#</span><span>Tekrar</span><span>Ağırlık (kg)</span><span />
                        </div>
                        {ex.setEntries.map((s, si) => (
                          <div key={si} className={`grid grid-cols-[2rem_1fr_1fr_2.5rem] items-center gap-2 rounded-xl px-2 py-1.5 transition-all ${s.done ? 'bg-primary/10' : 'bg-surface-container'}`}>
                            <span className="text-center text-xs font-bold text-on-surface-variant">{si + 1}</span>
                            <input
                              type="number" min={1} max={200}
                              value={s.reps}
                              onChange={e => updateSetEntry(exIdx, si, 'reps', e.target.value)}
                              placeholder={ex.reps?.toString() ?? '—'}
                              className="w-full rounded-lg border border-white/10 bg-background px-2 py-1 text-center text-sm outline-none focus:border-primary"
                            />
                            <input
                              type="number" min={0} max={500} step={0.5}
                              value={s.weight}
                              onChange={e => updateSetEntry(exIdx, si, 'weight', e.target.value)}
                              placeholder="—"
                              className="w-full rounded-lg border border-white/10 bg-background px-2 py-1 text-center text-sm outline-none focus:border-primary"
                            />
                            <button
                              onClick={() => updateSetEntry(exIdx, si, 'done', !s.done)}
                              className={`flex h-8 w-8 items-center justify-center rounded-xl transition-all ${s.done ? 'bg-primary text-on-primary' : 'bg-white/5 text-on-surface-variant hover:bg-primary/20'}`}
                            >
                              <Icon name="check" className="text-base" />
                            </button>
                          </div>
                        ))}
                      </div>
                    )}
                  </Card>
                );
              })}
            </div>
          )}

          <div className="flex gap-3 pb-4">
            <button
              onClick={() => setView('list')}
              className="flex-1 rounded-2xl bg-surface-container py-3 text-sm font-semibold text-on-surface-variant transition-colors hover:bg-white/5"
            >
              Vazgeç
            </button>
            <Button
              onClick={finishSession}
              disabled={sessionFinishing || sessionExercises.every(ex => ex.setEntries.every(s => !s.done))}
              className="flex-[2]"
            >
              {sessionFinishing ? 'Kaydediliyor…' : `Antrenmanı Tamamla (${completedCount}/${sessionExercises.length})`}
            </Button>
          </div>
        </div>
      )}

      {/* ══════════════════════════════════════════════════════════════════ */}
      {/* PROGRAM LİSTESİ                                                   */}
      {/* ══════════════════════════════════════════════════════════════════ */}
      {!isCreating && view !== 'session' && (
        loadingPlans ? <Spinner /> : plans.length === 0 ? (
          <Card className="py-12 text-center">
            <Icon name="calendar_month" className="mb-3 text-5xl text-on-surface-variant/30" />
            <p className="text-on-surface-variant">Henüz programın yok.</p>
            <button onClick={() => setView('create')} className="mt-4 rounded-xl bg-primary px-6 py-2 text-sm font-semibold text-on-primary">
              İlk Programı Oluştur
            </button>
          </Card>
        ) : (
          <div className="space-y-3">
            {plans.map(plan => (
              <Card key={plan.id} className="overflow-hidden">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-semibold text-on-surface">{plan.name}</p>
                    <p className="text-xs text-on-surface-variant">
                      {new Date(plan.createdAt).toLocaleDateString('tr-TR', { day: '2-digit', month: 'long', year: 'numeric' })}
                      {' · '}{plan.totalExercises} egzersiz
                    </p>
                  </div>
                  <div className="flex items-center gap-1">
                    {/* Başlat */}
                    <button
                      onClick={() => startSession(plan.id)}
                      className="flex items-center gap-1.5 rounded-xl bg-primary/10 px-3 py-1.5 text-xs font-semibold text-primary transition-colors hover:bg-primary/20"
                      title="Antrenmanı Başlat"
                    >
                      <Icon name="play_arrow" className="text-base" filled /> Başlat
                    </button>
                    <button onClick={() => toggleExpand(plan.id)} className="flex h-8 w-8 items-center justify-center rounded-lg text-on-surface-variant transition-colors hover:bg-white/5 hover:text-primary" title="Görüntüle">
                      <Icon name={expandedPlan?.id === plan.id ? 'expand_less' : 'expand_more'} className="text-xl" />
                    </button>
                    <button onClick={() => startEdit(plan.id)} className="flex h-8 w-8 items-center justify-center rounded-lg text-on-surface-variant transition-colors hover:bg-white/5 hover:text-primary" title="Düzenle">
                      <Icon name="edit" className="text-lg" />
                    </button>
                    <button onClick={() => deletePlan(plan.id)} className="flex h-8 w-8 items-center justify-center rounded-lg text-on-surface-variant transition-colors hover:bg-error/10 hover:text-error" title="Sil">
                      <Icon name="delete" className="text-lg" />
                    </button>
                  </div>
                </div>

                {expandedPlan?.id === plan.id && (
                  loadingExpand ? (
                    <div className="mt-4 flex justify-center"><Spinner /></div>
                  ) : (
                    <div className="mt-4 grid grid-cols-1 gap-2 sm:grid-cols-7">
                      {DAYS.map((dayName, i) => {
                        const day = expandedPlan.days.find(d => d.dayIndex === i);
                        const exList = day?.exercises ?? [];
                        return (
                          <div key={i} className="rounded-xl bg-surface-container p-3">
                            <p className="mb-2 text-[10px] font-bold uppercase tracking-wider text-primary">{dayName}</p>
                            {exList.length === 0 ? (
                              <p className="text-xs text-on-surface-variant/50">Dinlenme</p>
                            ) : (
                              <ul className="space-y-1">
                                {exList.map(ex => (
                                  <li key={ex.id} className="flex items-start gap-1.5">
                                    <Icon name={muscleIcons[ex.muscleGroup as MuscleGroup] ?? 'fitness_center'} className="mt-0.5 text-xs text-primary" filled />
                                    <div>
                                      <p className="text-xs font-medium leading-tight text-on-surface">{ex.exerciseName}</p>
                                      <p className="text-[10px] text-on-surface-variant">
                                        {ex.durationMinutes ? `${ex.durationMinutes} dk` : ''}
                                        {ex.sets ? `${ex.sets}×${ex.reps ?? '-'}` : ''}
                                      </p>
                                    </div>
                                  </li>
                                ))}
                              </ul>
                            )}
                          </div>
                        );
                      })}
                    </div>
                  )
                )}
              </Card>
            ))}
          </div>
        )
      )}

      {/* ══════════════════════════════════════════════════════════════════ */}
      {/* OLUŞTUR / DÜZENLE                                                 */}
      {/* ══════════════════════════════════════════════════════════════════ */}
      {isCreating && (
        <div className="space-y-5">
          <Card>
            <label className="mb-1 block text-xs font-semibold uppercase tracking-widest text-on-surface-variant">Program Adı</label>
            <input
              value={planName}
              onChange={e => setPlanName(e.target.value)}
              placeholder="Örn. Güç Antrenmanı, Split Program…"
              className="w-full rounded-xl border border-white/10 bg-surface-container px-4 py-2.5 text-sm text-on-surface placeholder:text-on-surface-variant/50 outline-none transition focus:border-primary"
            />
          </Card>

          <div className="grid grid-cols-1 gap-4 lg:grid-cols-[300px_1fr]">
            <Card className="lg:sticky lg:top-24 lg:self-start">
              <p className="mb-3 text-xs font-semibold uppercase tracking-widest text-on-surface-variant">Egzersiz Kütüphanesi</p>
              <p className="mb-3 text-[11px] text-on-surface-variant/70">
                Sürükle → güne bırak &nbsp;|&nbsp; Dokunmatik: <span className="text-primary">+ butonuna</span> bas, gün seç
              </p>
              <div className="custom-scrollbar mb-3 flex gap-1 overflow-x-auto pb-1">
                {MUSCLE_ORDER.map(m => (
                  <button key={m} onClick={() => setActiveMusclePicker(m)}
                    className={`flex shrink-0 items-center gap-1.5 rounded-xl px-2.5 py-1.5 text-xs font-medium transition-all ${activeMusclePicker === m ? 'bg-primary text-on-primary' : 'bg-surface-container text-on-surface-variant hover:bg-surface-container-high'}`}
                  >
                    <Icon name={muscleIcons[m]} className="text-sm" filled={activeMusclePicker === m} />
                    {muscleLabels[m]}
                  </button>
                ))}
              </div>
              <ul className="space-y-1.5">
                {filteredExercises.map(ex => (
                  <li key={ex.id} draggable onDragStart={() => onDragStart(ex)}
                    className="flex cursor-grab items-center justify-between rounded-xl border border-white/5 bg-surface-container px-3 py-2 active:cursor-grabbing hover:border-primary/30"
                  >
                    <span className="text-sm text-on-surface">{ex.name}</span>
                    <DayPickerButton onSelect={dayIdx => addToDay(dayIdx, ex)} />
                  </li>
                ))}
              </ul>
            </Card>

            <div className="space-y-3">
              {DAYS.map((dayName, dayIdx) => (
                <Card key={dayIdx}
                  onDragOver={e => { e.preventDefault(); setDragOverDay(dayIdx); }}
                  onDragLeave={() => setDragOverDay(null)}
                  onDrop={() => onDrop(dayIdx)}
                  className={`min-h-[80px] transition-all ${dragOverDay === dayIdx ? 'border-2 border-dashed border-primary bg-primary/5' : 'border border-white/5'}`}
                >
                  <div className="mb-2 flex items-center justify-between">
                    <span className="text-xs font-bold uppercase tracking-wider text-primary">{dayName}</span>
                    <span className="text-xs text-on-surface-variant">{draft[dayIdx].length} egzersiz</span>
                  </div>
                  {draft[dayIdx].length === 0 ? (
                    <p className="py-4 text-center text-xs text-on-surface-variant/40">Egzersiz sürükle veya + ile ekle</p>
                  ) : (
                    <ul className="space-y-2">
                      {draft[dayIdx].map(ex => (
                        <li key={ex.key} className="flex items-start gap-2 rounded-xl bg-surface-container p-2">
                          <div className="flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-primary/10">
                            <Icon name={muscleIcons[ex.muscleGroup as MuscleGroup] ?? 'fitness_center'} className="text-sm text-primary" filled />
                          </div>
                          <div className="flex flex-1 flex-wrap items-center gap-2">
                            <span className="text-sm font-medium text-on-surface">{ex.name}</span>
                            {ex.category === 'Cardio' ? (
                              <label className="flex items-center gap-1 text-xs text-on-surface-variant">
                                <span>Süre</span>
                                <input type="number" min={1} max={300} value={ex.duration} onChange={e => updateField(dayIdx, ex.key, 'duration', e.target.value)} className="w-14 rounded-lg border border-white/10 bg-background px-2 py-0.5 text-center text-xs outline-none focus:border-primary" placeholder="dk" />
                                <span>dk</span>
                              </label>
                            ) : (
                              <>
                                <label className="flex items-center gap-1 text-xs text-on-surface-variant">
                                  <span>Set</span>
                                  <input type="number" min={1} max={20} value={ex.sets} onChange={e => updateField(dayIdx, ex.key, 'sets', e.target.value)} className="w-12 rounded-lg border border-white/10 bg-background px-2 py-0.5 text-center text-xs outline-none focus:border-primary" placeholder="4" />
                                </label>
                                <label className="flex items-center gap-1 text-xs text-on-surface-variant">
                                  <span>Tekrar</span>
                                  <input type="number" min={1} max={200} value={ex.reps} onChange={e => updateField(dayIdx, ex.key, 'reps', e.target.value)} className="w-12 rounded-lg border border-white/10 bg-background px-2 py-0.5 text-center text-xs outline-none focus:border-primary" placeholder="10" />
                                </label>
                              </>
                            )}
                          </div>
                          <button onClick={() => removeFromDay(dayIdx, ex.key)} className="flex h-6 w-6 shrink-0 items-center justify-center rounded text-on-surface-variant transition-colors hover:bg-error/10 hover:text-error">
                            <Icon name="close" className="text-sm" />
                          </button>
                        </li>
                      ))}
                    </ul>
                  )}
                </Card>
              ))}
            </div>
          </div>

          <div className="flex items-center justify-between rounded-2xl bg-surface-container p-4">
            <span className="text-sm text-on-surface-variant">
              Toplam <span className="font-bold text-on-surface">{totalItems}</span> egzersiz
            </span>
            <div className="flex gap-2">
              <button onClick={() => { resetForm(); setView('list'); }} className="rounded-xl px-4 py-2 text-sm text-on-surface-variant transition-colors hover:bg-white/5">
                İptal
              </button>
              <Button onClick={savePlan} disabled={saving || !planName.trim()}>
                {saving ? 'Kaydediliyor…' : view === 'edit' ? 'Güncelle' : 'Programı Kaydet'}
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function DayPickerButton({ onSelect }: { onSelect: (dayIdx: number) => void }) {
  const [open, setOpen] = useState(false);
  return (
    <div className="relative">
      <button onClick={e => { e.stopPropagation(); setOpen(v => !v); }} className="flex h-6 w-6 items-center justify-center rounded-lg bg-primary/10 text-primary transition-colors hover:bg-primary/20" title="Güne ekle">
        <Icon name="add" className="text-sm" />
      </button>
      {open && (
        <div className="absolute right-0 top-8 z-50 w-40 overflow-hidden rounded-xl border border-white/10 bg-surface-container shadow-xl">
          {DAYS.map((name, i) => (
            <button key={i} onClick={() => { onSelect(i); setOpen(false); }} className="w-full px-3 py-1.5 text-left text-xs text-on-surface transition-colors hover:bg-primary/10 hover:text-primary">
              {name}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
