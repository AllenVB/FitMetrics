import type {
  ButtonHTMLAttributes, InputHTMLAttributes, ReactNode, SelectHTMLAttributes, TextareaHTMLAttributes,
} from 'react';

/** Material Symbols Outlined icon. */
export function Icon({ name, className = '', filled = false }: {
  name: string; className?: string; filled?: boolean;
}) {
  return <span className={`material-symbols-outlined${filled ? ' filled' : ''} ${className}`}>{name}</span>;
}

export function Card({ className = '', children }: { className?: string; children: ReactNode }) {
  return (
    <div className={`clean-card rounded-2xl p-6 transition-colors hover:border-primary/20 ${className}`}>
      {children}
    </div>
  );
}

export function PageHeader({ title, subtitle, action }: {
  title: string; subtitle?: string; action?: ReactNode;
}) {
  return (
    <div className="mb-6 flex flex-wrap items-end justify-between gap-4">
      <div>
        <h1 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-on-surface">{title}</h1>
        {subtitle && <p className="mt-1 text-body-lg text-on-surface-variant">{subtitle}</p>}
      </div>
      {action}
    </div>
  );
}

const accentMap: Record<string, { border: string; chip: string; text: string }> = {
  primary: { border: 'border-l-primary', chip: 'bg-primary/10 text-primary', text: 'text-primary' },
  brand: { border: 'border-l-primary', chip: 'bg-primary/10 text-primary', text: 'text-primary' },
  blue: { border: 'border-l-blue-400', chip: 'bg-blue-500/10 text-blue-400', text: 'text-blue-400' },
  green: { border: 'border-l-tertiary', chip: 'bg-tertiary/10 text-tertiary', text: 'text-tertiary' },
  tertiary: { border: 'border-l-tertiary', chip: 'bg-tertiary/10 text-tertiary', text: 'text-tertiary' },
  secondary: { border: 'border-l-secondary', chip: 'bg-secondary/10 text-secondary', text: 'text-secondary' },
  violet: { border: 'border-l-secondary', chip: 'bg-secondary/10 text-secondary', text: 'text-secondary' },
  amber: { border: 'border-l-amber-400', chip: 'bg-amber-500/10 text-amber-400', text: 'text-amber-400' },
  rose: { border: 'border-l-error', chip: 'bg-error/10 text-error', text: 'text-error' },
};

export function StatCard({ icon, label, value, sub, accent = 'primary' }: {
  icon: ReactNode; label: string; value: ReactNode; sub?: ReactNode; accent?: keyof typeof accentMap;
}) {
  const a = accentMap[accent] ?? accentMap.primary;
  return (
    <div className={`clean-card rounded-2xl p-5 border-l-4 ${a.border} transition-colors hover:bg-surface-container-high`}>
      <div className={`mb-4 flex h-11 w-11 items-center justify-center rounded-xl text-xl ${a.chip}`}>
        {icon}
      </div>
      <div className="font-label-caps text-label-caps uppercase text-on-surface-variant">{label}</div>
      <div className="mt-1 truncate text-2xl font-bold text-on-surface">{value}</div>
      {sub && <div className="mt-0.5 text-xs text-on-surface-variant">{sub}</div>}
    </div>
  );
}

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost';
}

export function Button({ variant = 'primary', className = '', ...props }: ButtonProps) {
  const variants = {
    primary: 'bg-primary text-on-primary font-bold hover:brightness-110 disabled:bg-primary/40 disabled:text-on-primary/60',
    secondary: 'clean-card text-on-surface hover:bg-surface-container-high',
    danger: 'bg-error text-on-error font-bold hover:brightness-110',
    ghost: 'text-on-surface-variant hover:bg-white/5 hover:text-on-surface',
  };
  return (
    <button
      className={`rounded-xl px-4 py-2.5 text-body-sm font-semibold transition-all active:scale-95 disabled:cursor-not-allowed disabled:active:scale-100 ${variants[variant]} ${className}`}
      {...props}
    />
  );
}

export function Field({ label, children, hint }: { label: string; children: ReactNode; hint?: string }) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-body-sm font-medium text-on-surface-variant">{label}</span>
      {children}
      {hint && <span className="mt-1 block text-xs text-on-surface-variant/70">{hint}</span>}
    </label>
  );
}

const fieldClasses =
  'w-full rounded-xl border border-white/10 bg-surface-container px-3.5 py-2.5 text-body-sm text-on-surface placeholder:text-on-surface-variant/50 outline-none transition-colors focus:border-primary focus:ring-2 focus:ring-primary/20';

export function Input(props: InputHTMLAttributes<HTMLInputElement>) {
  return <input className={fieldClasses} {...props} />;
}

export function Select({ className = '', ...props }: SelectHTMLAttributes<HTMLSelectElement>) {
  return <select className={`${fieldClasses} appearance-none ${className}`} {...props} />;
}

export function Textarea(props: TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return <textarea className={fieldClasses} {...props} />;
}

export function Spinner({ label = 'Yükleniyor…' }: { label?: string }) {
  return (
    <div className="flex items-center justify-center gap-3 py-16 text-on-surface-variant">
      <span className="material-symbols-outlined animate-spin text-primary">progress_activity</span>
      <span className="text-body-sm">{label}</span>
    </div>
  );
}

export function ErrorAlert({ message }: { message: string }) {
  if (!message) return null;
  return (
    <div className="flex items-center gap-2 rounded-xl border border-error/30 bg-error/10 px-4 py-3 text-body-sm text-error">
      <span className="material-symbols-outlined text-base">error</span>
      {message}
    </div>
  );
}

export function EmptyState({ message }: { message: string }) {
  return (
    <div className="rounded-2xl border border-dashed border-white/10 bg-surface-container-low px-4 py-12 text-center text-body-sm text-on-surface-variant">
      {message}
    </div>
  );
}
