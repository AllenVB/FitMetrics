import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function ProtectedRoute() {
  const { user, loading } = useAuth();

  if (loading) {
    return (
      <div className="flex h-screen items-center justify-center gap-3 bg-background text-on-surface-variant">
        <span className="material-symbols-outlined animate-spin text-primary">progress_activity</span>
        <span className="text-body-sm">Yükleniyor…</span>
      </div>
    );
  }

  if (!user) return <Navigate to="/login" replace />;

  return <Outlet />;
}
