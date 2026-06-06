import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { getErrorMessage } from '../api/client';
import { Button, ErrorAlert, Field, Icon, Input } from '../components/ui';

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await login(email, password);
      navigate('/');
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-4">
      <div className="w-full max-w-md clean-card rounded-2xl p-8 shadow-2xl">
        <div className="mb-6 flex flex-col items-center text-center">
          <div className="mb-3 flex h-12 w-12 items-center justify-center rounded-xl bg-primary">
            <Icon name="ecg_heart" className="text-2xl text-on-primary" />
          </div>
          <div className="ai-gradient-text text-3xl font-bold">FitMetrics</div>
          <p className="mt-1 text-body-sm text-on-surface-variant">Hesabına giriş yap</p>
        </div>

        <form onSubmit={onSubmit} className="space-y-4">
          <Field label="E-posta">
            <Input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required placeholder="ornek@mail.com" />
          </Field>
          <Field label="Parola">
            <Input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required placeholder="••••••••" />
          </Field>
          <ErrorAlert message={error} />
          <Button type="submit" className="w-full" disabled={loading}>
            {loading ? 'Giriş yapılıyor…' : 'Giriş Yap'}
          </Button>
        </form>

        <p className="mt-6 text-center text-body-sm text-on-surface-variant">
          Hesabın yok mu?{' '}
          <Link to="/register" className="font-semibold text-primary hover:underline">Kayıt ol</Link>
        </p>
      </div>
    </div>
  );
}
