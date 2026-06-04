import { useState, type FormEvent } from 'react';
import { profileApi } from '../api';
import { getErrorMessage } from '../api/client';
import { useAuth } from '../context/AuthContext';
import {
  Button, Card, ErrorAlert, Field, Input, PageHeader, Select, StatCard,
} from '../components/ui';
import { activityOptions, genderOptions, goalLabels, goalOptions } from '../lib/labels';
import type { ActivityLevel, Gender, GoalType } from '../types';

export default function Profile() {
  const { user, updateUser } = useAuth();

  const [fullName, setFullName] = useState(user!.fullName);
  const [age, setAge] = useState(user!.age);
  const [gender, setGender] = useState<Gender>(user!.gender);
  const [heightCm, setHeightCm] = useState(user!.heightCm);
  const [currentWeightKg, setCurrentWeightKg] = useState(user!.currentWeightKg);
  const [activityLevel, setActivityLevel] = useState<ActivityLevel>(user!.activityLevel);
  const [goalType, setGoalType] = useState<GoalType>(user!.goalType);
  const [targetWeightKg, setTargetWeightKg] = useState(user!.targetWeightKg?.toString() ?? '');

  const [autoGoals, setAutoGoals] = useState(false);
  const [calorieGoal, setCalorieGoal] = useState(user!.dailyCalorieGoal);
  const [proteinGoal, setProteinGoal] = useState(user!.dailyProteinGoal);
  const [waterGoal, setWaterGoal] = useState(user!.dailyWaterGoalMl);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [saved, setSaved] = useState(false);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setSaved(false);
    setLoading(true);
    try {
      const updated = await profileApi.update({
        fullName,
        age: Number(age),
        gender,
        heightCm: Number(heightCm),
        currentWeightKg: Number(currentWeightKg),
        activityLevel,
        goalType,
        targetWeightKg: targetWeightKg === '' ? null : Number(targetWeightKg),
        dailyCalorieGoal: autoGoals ? null : Number(calorieGoal),
        dailyProteinGoal: autoGoals ? null : Number(proteinGoal),
        dailyWaterGoalMl: autoGoals ? null : Number(waterGoal),
      });
      updateUser(updated);
      setCalorieGoal(updated.dailyCalorieGoal);
      setProteinGoal(updated.dailyProteinGoal);
      setWaterGoal(updated.dailyWaterGoalMl);
      setSaved(true);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <PageHeader title="⚙️ Profil" subtitle="Bilgilerini ve hedeflerini güncelle" />

      <div className="mb-6 grid gap-4 sm:grid-cols-3">
        <StatCard icon="🔥" label="Günlük Kalori" value={`${user!.dailyCalorieGoal} kcal`} accent="amber" />
        <StatCard icon="🥩" label="Günlük Protein" value={`${user!.dailyProteinGoal} g`} accent="brand" />
        <StatCard icon="⚖️" label="BMI" value={user!.bmi} sub={goalLabels[user!.goalType]} accent="violet" />
      </div>

      <form onSubmit={onSubmit} className="space-y-6">
        <Card>
          <h2 className="mb-4 font-semibold text-slate-800">Kişisel Bilgiler</h2>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <Field label="Ad Soyad"><Input value={fullName} onChange={(e) => setFullName(e.target.value)} required /></Field>
            <Field label="Yaş"><Input type="number" value={age} onChange={(e) => setAge(Number(e.target.value))} min={10} max={120} /></Field>
            <Field label="Cinsiyet">
              <Select value={gender} onChange={(e) => setGender(e.target.value as Gender)}>
                {genderOptions.map(([v, l]) => <option key={v} value={v}>{l}</option>)}
              </Select>
            </Field>
            <Field label="Boy (cm)"><Input type="number" value={heightCm} onChange={(e) => setHeightCm(Number(e.target.value))} min={50} max={260} /></Field>
            <Field label="Kilo (kg)"><Input type="number" step="0.1" value={currentWeightKg} onChange={(e) => setCurrentWeightKg(Number(e.target.value))} min={20} max={400} /></Field>
            <Field label="Aktivite Seviyesi">
              <Select value={activityLevel} onChange={(e) => setActivityLevel(e.target.value as ActivityLevel)}>
                {activityOptions.map(([v, l]) => <option key={v} value={v}>{l}</option>)}
              </Select>
            </Field>
            <Field label="Hedef">
              <Select value={goalType} onChange={(e) => setGoalType(e.target.value as GoalType)}>
                {goalOptions.map(([v, l]) => <option key={v} value={v}>{l}</option>)}
              </Select>
            </Field>
            <Field label="Hedef Kilo (kg)" hint="İsteğe bağlı">
              <Input type="number" step="0.1" value={targetWeightKg} onChange={(e) => setTargetWeightKg(e.target.value)} />
            </Field>
          </div>
        </Card>

        <Card>
          <div className="mb-4 flex items-center justify-between">
            <h2 className="font-semibold text-slate-800">Günlük Hedefler</h2>
            <label className="flex items-center gap-2 text-sm text-slate-600">
              <input type="checkbox" checked={autoGoals} onChange={(e) => setAutoGoals(e.target.checked)} className="h-4 w-4 rounded border-slate-300" />
              Otomatik hesapla
            </label>
          </div>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <Field label="Kalori (kcal)">
              <Input type="number" value={calorieGoal} onChange={(e) => setCalorieGoal(Number(e.target.value))} disabled={autoGoals} min={800} max={8000} />
            </Field>
            <Field label="Protein (g)">
              <Input type="number" value={proteinGoal} onChange={(e) => setProteinGoal(Number(e.target.value))} disabled={autoGoals} min={20} max={500} />
            </Field>
            <Field label="Su (ml)">
              <Input type="number" value={waterGoal} onChange={(e) => setWaterGoal(Number(e.target.value))} disabled={autoGoals} min={500} max={8000} />
            </Field>
          </div>
          {autoGoals && (
            <p className="mt-3 text-xs text-slate-400">
              Hedefler, vücut bilgilerine göre (Mifflin-St Jeor + aktivite + hedef) otomatik hesaplanacak.
            </p>
          )}
        </Card>

        <div className="flex items-center gap-4">
          <Button type="submit" disabled={loading}>{loading ? 'Kaydediliyor…' : 'Kaydet'}</Button>
          {saved && <span className="text-sm font-medium text-emerald-600">✓ Bilgiler güncellendi</span>}
        </div>
        <ErrorAlert message={error} />
      </form>
    </div>
  );
}
