import { cn } from '../../lib/utils';
import type { InputHTMLAttributes } from 'react';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
}

export function Input({ label, error, className, ...props }: InputProps) {
  return (
    <div className="space-y-1.5">
      {label && (
        <label className="block text-sm text-text-secondary font-medium">{label}</label>
      )}
      <input
        className={cn(
          'w-full px-4 py-3 bg-bg-input border border-border-subtle rounded-xl',
          'text-text-primary placeholder-text-muted',
          'focus:outline-none focus:border-vibe-purple focus:ring-1 focus:ring-vibe-purple/50',
          'transition-all duration-200',
          error && 'border-dislike-red focus:border-dislike-red focus:ring-dislike-red/50',
          className,
        )}
        {...props}
      />
      {error && <p className="text-sm text-dislike-red">{error}</p>}
    </div>
  );
}
