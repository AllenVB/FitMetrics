import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const navItems = [
  { to: '/', label: 'Panel', icon: '📊', end: true },
  { to: '/nutrition', label: 'Beslenme', icon: '🍽️' },
  { to: '/workouts', label: 'Antrenman', icon: '💪' },
  { to: '/progress', label: 'İlerleme', icon: '📈' },
  { to: '/insights', label: 'AI Insights', icon: '🧠' },
  { to: '/ai-coach', label: 'AI Koç', icon: '🤖' },
  { to: '/profile', label: 'Profil', icon: '⚙️' },
];

export default function Layout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="flex min-h-screen bg-slate-100">
      {/* Sidebar */}
      <aside className="hidden w-64 flex-col bg-slate-900 text-slate-200 md:flex">
        <div className="flex items-center gap-2 px-6 py-5 text-xl font-bold text-white">
          <span className="text-brand-400">⚡</span> FitMetrics
        </div>
        <nav className="flex-1 space-y-1 px-3">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) =>
                `flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition ${
                  isActive
                    ? 'bg-brand-600 text-white'
                    : 'text-slate-300 hover:bg-slate-800 hover:text-white'
                }`
              }
            >
              <span className="text-lg">{item.icon}</span>
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="px-6 py-4 text-xs text-slate-500">
          FitMetrics © 2026
        </div>
      </aside>

      {/* Ana içerik */}
      <div className="flex flex-1 flex-col">
        <header className="flex items-center justify-between border-b border-slate-200 bg-white px-6 py-3.5">
          <div className="flex items-center gap-2 text-lg font-bold text-slate-800 md:hidden">
            <span className="text-brand-500">⚡</span> FitMetrics
          </div>
          <div className="ml-auto flex items-center gap-4">
            <div className="text-right">
              <div className="text-sm font-semibold text-slate-800">{user?.fullName}</div>
              <div className="text-xs text-slate-500">{user?.email}</div>
            </div>
            <div className="flex h-9 w-9 items-center justify-center rounded-full bg-brand-100 font-semibold text-brand-700">
              {user?.fullName?.charAt(0).toUpperCase()}
            </div>
            <button
              onClick={handleLogout}
              className="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-600 transition hover:bg-slate-100"
            >
              Çıkış
            </button>
          </div>
        </header>

        {/* Mobil navigasyon */}
        <nav className="flex gap-1 overflow-x-auto border-b border-slate-200 bg-white px-3 py-2 md:hidden">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) =>
                `whitespace-nowrap rounded-lg px-3 py-1.5 text-sm font-medium ${
                  isActive ? 'bg-brand-600 text-white' : 'text-slate-600'
                }`
              }
            >
              {item.icon} {item.label}
            </NavLink>
          ))}
        </nav>

        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
