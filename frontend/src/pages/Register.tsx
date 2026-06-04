import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { getErrorMessage } from '../api/client';
import { Button, ErrorAlert, Field, Input, Select } from '../components/ui';
import { activityOptions, genderOptions, goalOptions } from '../lib/labels';
import type { ActivityLevel, Gender, GoalType } from '../types';

export default function Register() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [age, setAge] = useState(25);
  const [gender, setGender] = useState<Gender>('Male');
  const [heightCm, setHeightCm] = useState(175);
  const [currentWeightKg, setCurrentWeightKg] = useState(75);
  const [activityLevel, setActivityLevel] = useState<ActivityLevel>('Moderate');
  const [goalType, setGoalType] = useState<GoalType>('MaintainWeight');
  const [targetWeightKg, setTargetWeightKg] = useState('');

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await register({
        fullName, email, password,
        age: Number(age), gender, heightCm: Number(heightCm),
        currentWeightKg: Number(currentWeightKg), activityLevel, goalType,
        targetWeightKg: targetWeightKg === '' ? null : Number(targetWeightKg),
      });
      navigate('/');
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-slate-900 to-brand-900 p-4">
      <div className="w-full max-w-2xl rounded-2xl bg-white p-8 shadow-2xl">
        <div className="mb-6 text-center">
          <div className="text-3xl font-bold text-slate-800">
            <span className="text-brand-500">⚡</span> FitMetrics
          </div>
          <p className="mt-1 text-sm text-slate-500">Hesap oluştur ve hedefini belirle</p>
        </div>

        <form onSubmit={onSubmit} className="space-y-4">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label="Ad Soyad">
              <Input value={fullName} onChange={(e) => setFullName(e.target.value)} required placeholder="Adınız Soyadınız" />
            </Field>
            <Field label="E-posta">
              <Input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required placeholder="ornek@mail.com" />
            </Field>
            <Field label="Parola" hint="En az 6 karakter">
              <Input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={6} />
            </Field>
            <Field label="Yaş">
              <Input type="number" value={age} onChange={(e) => setAge(Number(e.target.value))} required min={10} max={120} />
            </Field>
            <Field label="Cinsiyet">
              <Select value={gender} onChange={(e) => setGender(e.target.value as Gender)}>
                {genderOptions.map(([value, label]) => <option key={value} value={value}>{label}</option>)}
              </Select>
            </Field>
            <Field label="Aktivite Seviyesi">
              <Select value={activityLevel} onChange={(e) => setActivityLevel(e.target.value as ActivityLevel)}>
                {activityOptions.map(([value, label]) => <option key={value} value={value}>{label}</option>)}
              </Select>
            </Field>
            <Field label="Boy (cm)">
              <Input type="number" value={heightCm} onChange={(e) => setHeightCm(Number(e.target.value))} required min={50} max={260} />
            </Field>
            <Field label="Kilo (kg)">
              <Input type="number" step="0.1" value={currentWeightKg} onChange={(e) => setCurrentWeightKg(Number(e.target.value))} required min={20} max={400} />
            </Field>
            <Field label="Hedef">
              <Select value={goalType} onChange={(e) => setGoalType(e.target.value as GoalType)}>
                {goalOptions.map(([value, label]) => <option key={value} value={value}>{label}</option>)}
              </Select>
            </Field>
            <Field label="Hedef Kilo (kg)" hint="İsteğe bağlı">
              <Input type="number" step="0.1" value={targetWeightKg} onChange={(e) => setTargetWeightKg(e.target.value)} placeholder="örn. 80" />
            </Field>
          </div>

          <ErrorAlert message={error} />
          <Button type="submit" className="w-full" disabled={loading}>
            {loading ? 'Hesap oluşturuluyor…' : 'Kayıt Ol'}
          </Button>
        </form>

        <p className="mt-6 text-center text-sm text-slate-500">
          Zaten hesabın var mı?{' '}
          <Link to="/login" className="font-semibold text-brand-600 hover:underline">Giriş yap</Link>
        </p>
      </div>
    </div>
  );
}
