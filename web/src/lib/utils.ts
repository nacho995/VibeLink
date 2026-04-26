export function getContentTypeLabel(type: number): string {
  switch (type) {
    case 0: return 'Movie';
    case 1: return 'Series';
    case 2: return 'Game';
    default: return 'Unknown';
  }
}

export function getContentTypeColor(type: number): string {
  switch (type) {
    case 0: return '#e50914';   // Netflix red
    case 1: return '#7b2fbe';   // Steam purple
    case 2: return '#00d4ff';   // PlayStation cyan
    default: return '#8888a0';
  }
}

export function getContentTypeIcon(type: number): string {
  switch (type) {
    case 0: return 'film';
    case 1: return 'tv';
    case 2: return 'gamepad-2';
    default: return 'circle';
  }
}

export function formatTimeAgo(dateStr: string): string {
  const date = new Date(dateStr);
  const now = new Date();
  const diff = now.getTime() - date.getTime();
  const minutes = Math.floor(diff / 60000);
  const hours = Math.floor(diff / 3600000);
  const days = Math.floor(diff / 86400000);

  if (minutes < 1) return 'ahora';
  if (minutes < 60) return `hace ${minutes}m`;
  if (hours < 24) return `hace ${hours}h`;
  if (days < 7) return `hace ${days}d`;
  return date.toLocaleDateString('es');
}

export function cn(...classes: (string | undefined | null | false)[]): string {
  return classes.filter(Boolean).join(' ');
}
