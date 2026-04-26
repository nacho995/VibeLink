import { useState } from 'react';
import { Wrench, RefreshCw, WifiOff } from 'lucide-react';

interface MaintenancePageProps {
  onRetry?: () => void;
}

export function MaintenancePage({ onRetry }: MaintenancePageProps) {
  const [checking, setChecking] = useState(false);

  async function handleRetry() {
    setChecking(true);
    try {
      const res = await fetch('/health', { signal: AbortSignal.timeout(5000) });
      if (res.ok) {
        // Backend is back, reload or reset error boundary
        if (onRetry) onRetry();
        else window.location.reload();
        return;
      }
    } catch {
      // still down
    }
    setChecking(false);
  }

  const isOffline = typeof navigator !== 'undefined' && !navigator.onLine;

  return (
    <div className="min-h-dvh flex flex-col items-center justify-center bg-bg-primary px-6 text-center">

      {/* Animated icon */}
      <div className="relative mb-8">
        <div className="w-24 h-24 rounded-full bg-gradient-to-br from-vibe-red/20 via-vibe-purple/20 to-vibe-cyan/20 flex items-center justify-center animate-pulse-glow">
          <div className="w-20 h-20 rounded-full bg-bg-card flex items-center justify-center">
            {isOffline ? (
              <WifiOff size={36} className="text-vibe-red" />
            ) : (
              <Wrench size={36} className="text-vibe-purple" />
            )}
          </div>
        </div>

        {/* Floating dots */}
        <div className="absolute -top-2 -right-2 w-4 h-4 rounded-full bg-vibe-red animate-bounce" style={{ animationDelay: '0s' }} />
        <div className="absolute -bottom-1 -left-3 w-3 h-3 rounded-full bg-vibe-cyan animate-bounce" style={{ animationDelay: '0.3s' }} />
        <div className="absolute top-1/2 -right-4 w-2.5 h-2.5 rounded-full bg-vibe-purple animate-bounce" style={{ animationDelay: '0.6s' }} />
      </div>

      {/* Logo */}
      <h1 className="text-3xl font-black gradient-text mb-3">VibeLink</h1>

      {/* Message */}
      <h2 className="text-lg font-bold text-text-primary mb-2">
        {isOffline ? 'Sin conexion' : 'Estamos en mantenimiento'}
      </h2>
      <p className="text-sm text-text-secondary max-w-xs mb-8 leading-relaxed">
        {isOffline
          ? 'Parece que no tenes conexion a internet. Revisa tu WiFi o datos moviles e intenta de nuevo.'
          : 'Estamos mejorando VibeLink para vos. Volvemos en unos minutos con todo funcionando.'}
      </p>

      {/* Status indicator */}
      <div className="flex items-center gap-2 mb-8 px-4 py-2.5 bg-bg-card border border-border-subtle rounded-full">
        <div className={`w-2 h-2 rounded-full ${isOffline ? 'bg-dislike-red' : 'bg-match-gold'} animate-pulse`} />
        <span className="text-xs text-text-muted">
          {isOffline ? 'Sin conexion' : 'Mantenimiento en curso'}
        </span>
      </div>

      {/* Retry button */}
      <button
        onClick={handleRetry}
        disabled={checking}
        className="flex items-center gap-2 px-6 py-3 rounded-xl bg-gradient-to-r from-vibe-red via-vibe-purple to-vibe-cyan text-white font-semibold shadow-lg shadow-vibe-purple/30 hover:shadow-vibe-purple/50 transition-all active:scale-95 disabled:opacity-60"
      >
        <RefreshCw size={18} className={checking ? 'animate-spin' : ''} />
        {checking ? 'Verificando...' : 'Reintentar'}
      </button>

      {/* Footer */}
      <p className="mt-12 text-[10px] text-text-muted">
        Si el problema persiste, contactanos en soporte@govibelink.com
      </p>
    </div>
  );
}
