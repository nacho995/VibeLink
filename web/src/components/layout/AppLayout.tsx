import { Outlet, NavLink, useLocation } from 'react-router-dom';
import { Flame, Compass, Users, MessageCircle, User } from 'lucide-react';
import { cn } from '../../lib/utils';

const navItems = [
  { to: '/discover', icon: Flame, label: 'Personas' },
  { to: '/explore', icon: Compass, label: 'Contenido' },
  { to: '/matches', icon: Users, label: 'Matches' },
  { to: '/chat', icon: MessageCircle, label: 'Chat' },
  { to: '/profile', icon: User, label: 'Perfil' },
];

export function AppLayout() {
  const location = useLocation();
  const isChatRoom = location.pathname.match(/^\/chat\/\d+/);

  return (
    <div className="min-h-dvh flex flex-col bg-bg-primary">
      {!isChatRoom && (
        <header className="glass sticky top-0 z-50 px-4 py-3 flex items-center justify-center">
          <h1 className="text-xl font-bold gradient-text tracking-tight">VibeLink</h1>
        </header>
      )}

      <main className="flex-1 overflow-hidden">
        <Outlet />
      </main>

      {!isChatRoom && (
        <nav className="glass sticky bottom-0 z-50 flex items-center justify-around px-2 py-2">
          {navItems.map(({ to, icon: Icon, label }) => {
            const active = to === '/chat'
              ? location.pathname.startsWith('/chat')
              : location.pathname === to;

            return (
              <NavLink
                key={to}
                to={to}
                className={cn(
                  'flex flex-col items-center gap-0.5 px-4 py-1.5 rounded-xl transition-all duration-200',
                  active ? 'text-vibe-purple' : 'text-text-muted hover:text-text-secondary',
                )}
              >
                <Icon size={22} strokeWidth={active ? 2.5 : 1.5} />
                <span className="text-[10px] font-medium">{label}</span>
              </NavLink>
            );
          })}
        </nav>
      )}
    </div>
  );
}
