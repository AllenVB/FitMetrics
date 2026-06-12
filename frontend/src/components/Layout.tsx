import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Icon } from './ui';

const coreNav = [
  { to: '/', label: 'Panel', icon: 'dashboard', end: true },
  { to: '/nutrition', label: 'Beslenme', icon: 'restaurant' },
  { to: '/workouts', label: 'Antrenman', icon: 'fitness_center' },
  { to: '/workout-planner', label: 'Program', icon: 'calendar_month' },
  { to: '/progress', label: 'İlerleme', icon: 'trending_up' },
];

const aiNav = [
  { to: '/ai-coach', label: 'AI Koç', icon: 'smart_toy' },
  { to: '/ai-assistant', label: 'AI Asistan', icon: 'forum' },
  { to: '/insights', label: 'Insights', icon: 'analytics' },
  { to: '/dietitian', label: 'Diyetisyen', icon: 'health_and_safety' },
  { to: '/knowledge', label: 'Bilgi', icon: 'menu_book' },
];

const mobileNav = [...coreNav, { to: '/ai-coach', label: 'AI Koç', icon: 'smart_toy' }, { to: '/profile', label: 'Profil', icon: 'settings' }];

function NavItem({ item }: { item: { to: string; label: string; icon: string; end?: boolean } }) {
  return (
    <NavLink
      to={item.to}
      end={item.end}
      className={({ isActive }) =>
        `flex items-center gap-3 rounded-xl px-4 py-2.5 text-body-sm font-medium transition-all active:scale-[0.98] ${
          isActive
            ? 'bg-primary text-on-primary shadow-lg shadow-primary/10'
            : 'text-on-surface-variant hover:bg-surface-container hover:text-on-surface'
        }`
      }
    >
      {({ isActive }) => (
        <>
          <Icon name={item.icon} className="text-lg" filled={isActive} />
          {item.label}
        </>
      )}
    </NavLink>
  );
}

export default function Layout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  const initial = user?.fullName?.charAt(0).toUpperCase() ?? '?';

  return (
    <div className="min-h-screen bg-background text-on-surface">
      {/* ---- Desktop sidebar ---- */}
      <aside className="fixed left-0 top-0 z-50 hidden h-screen w-[280px] flex-col gap-6 border-r border-white/5 bg-background p-6 md:flex">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary text-on-primary">
            <Icon name="ecg_heart" className="text-2xl" filled />
          </div>
          <div className="flex flex-col leading-tight">
            <span className="font-display-lg text-title-md font-bold ai-gradient-text">FitMetrics</span>
            <span className="font-label-caps text-[10px] uppercase tracking-[0.18em] text-on-surface-variant">
              Sağlık Analitiği
            </span>
          </div>
        </div>

        <nav className="custom-scrollbar flex flex-1 flex-col overflow-y-auto">
          <div className="flex flex-col gap-1">
            {coreNav.map((item) => <NavItem key={item.to} item={item} />)}
          </div>

          <div className="my-3 border-t border-white/5" />

          <p className="mb-2 px-4 text-[10px] font-bold uppercase tracking-widest text-on-surface-variant/50">
            Yapay Zeka
          </p>
          <div className="flex flex-col gap-1">
            {aiNav.map((item) => <NavItem key={item.to} item={item} />)}
          </div>

          <div className="my-3 border-t border-white/5" />

          <NavItem item={{ to: '/profile', label: 'Profil', icon: 'settings' }} />
        </nav>

        <div className="flex items-center gap-3 border-t border-white/5 pt-4">
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary/15 font-semibold text-primary">
            {initial}
          </div>
          <div className="min-w-0 flex-1">
            <p className="truncate text-xs font-bold text-on-surface">{user?.fullName}</p>
            <p className="truncate text-[10px] text-on-surface-variant">{user?.email}</p>
          </div>
          <button
            onClick={handleLogout}
            title="Çıkış"
            className="flex h-8 w-8 items-center justify-center rounded-lg text-on-surface-variant transition-colors hover:bg-white/5 hover:text-error"
          >
            <Icon name="logout" className="text-lg" />
          </button>
        </div>
      </aside>

      {/* ---- Main column ---- */}
      <div className="flex min-h-screen flex-col md:ml-[280px]">
        <header className="fixed right-0 top-0 z-40 flex h-16 w-full items-center justify-between border-b border-white/5 bg-background/80 px-4 backdrop-blur-md md:h-20 md:w-[calc(100%-280px)] md:px-8">
          <div className="flex items-center gap-2 md:hidden">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-on-primary">
              <Icon name="ecg_heart" className="text-lg" filled />
            </div>
            <span className="font-display-lg text-base font-bold ai-gradient-text">FitMetrics</span>
          </div>

          <div className="relative hidden max-w-md flex-1 md:block">
            <Icon name="search" className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-on-surface-variant" />
            <input
              type="text"
              placeholder="Veri, kayıt veya egzersiz ara…"
              className="w-full rounded-xl border border-white/10 bg-surface-container py-2.5 pl-10 pr-4 text-body-sm text-on-surface placeholder:text-on-surface-variant/50 outline-none transition-colors focus:border-primary"
            />
          </div>

          <div className="flex items-center gap-3 md:gap-5">
            <div className="ml-1 flex items-center gap-3">
              <div className="hidden text-right sm:block">
                <p className="text-sm font-semibold leading-none text-on-surface">{user?.fullName}</p>
                <p className="mt-1 font-label-caps text-[10px] uppercase text-on-surface-variant">{user?.email}</p>
              </div>
              <div className="flex h-9 w-9 items-center justify-center rounded-full border-2 border-primary/20 bg-primary/15 font-semibold text-primary">
                {initial}
              </div>
            </div>
          </div>
        </header>

        <main className="flex-1 px-container-padding-mobile pb-28 pt-20 md:px-container-padding-desktop md:pb-12 md:pt-24">
          <Outlet />
        </main>
      </div>

      {/* ---- Mobile bottom nav (core only) ---- */}
      <nav className="fixed bottom-0 left-0 z-50 flex w-full border-t border-white/5 bg-surface-container/95 backdrop-blur-md md:hidden">
        {mobileNav.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            className={({ isActive }) =>
              `flex flex-1 flex-col items-center gap-1 py-2.5 transition-colors ${
                isActive ? 'text-primary' : 'text-on-surface-variant'
              }`
            }
          >
            {({ isActive }) => (
              <>
                <Icon name={item.icon} className="text-xl" filled={isActive} />
                <span className="text-[10px] font-medium">{item.label}</span>
              </>
            )}
          </NavLink>
        ))}
      </nav>
    </div>
  );
}
