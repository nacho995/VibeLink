import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { Film, Tv, Gamepad2, ChevronRight, Sparkles } from 'lucide-react';
import { SwipeCard } from '../components/swipe/SwipeCard';
import { Button } from '../components/ui/Button';
import { api } from '../lib/api';
import { useAuthStore } from '../stores/authStore';
import { getContentTypeLabel, getContentTypeColor } from '../lib/utils';
import type { ContentItem } from '../types';

type Phase = 'movies' | 'series' | 'games' | 'done';

const PHASES: { key: Phase; label: string; icon: typeof Film; fetchFn: (p: number) => Promise<ContentItem[]> }[] = [
  { key: 'movies', label: 'Peliculas', icon: Film, fetchFn: p => api.discoverMovies(p) },
  { key: 'series', label: 'Series', icon: Tv, fetchFn: p => api.discoverSeries(p) },
  { key: 'games', label: 'Videojuegos', icon: Gamepad2, fetchFn: p => api.discoverGames(p) },
];

const MIN_LIKES_PER_PHASE = 3;

export function OnboardingPage() {
  const navigate = useNavigate();
  const setOnboardingComplete = useAuthStore(s => s.setOnboardingComplete);

  const [phaseIndex, setPhaseIndex] = useState(0);
  const [items, setItems] = useState<ContentItem[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [likesInPhase, setLikesInPhase] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const phase = phaseIndex < PHASES.length ? PHASES[phaseIndex] : null;

  const loadContent = useCallback(async (fetchFn: (p: number) => Promise<ContentItem[]>, p: number) => {
    setLoading(true);
    try {
      const data = await fetchFn(p);
      setItems(prev => p === 1 ? data : [...prev, ...data]);
    } catch (err) {
      console.error('Error loading content:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (phase) {
      setCurrentIndex(0);
      setItems([]);
      setLikesInPhase(0);
      setPage(1);
      loadContent(phase.fetchFn, 1);
    }
  }, [phaseIndex, phase, loadContent]);

  // Load more when running low
  useEffect(() => {
    if (phase && currentIndex >= items.length - 3 && items.length > 0 && !loading) {
      const nextPage = page + 1;
      setPage(nextPage);
      loadContent(phase.fetchFn, nextPage);
    }
  }, [currentIndex, items.length, phase, page, loading, loadContent]);

  async function handleSwipe(liked: boolean) {
    const item = items[currentIndex];
    if (!item) return;

    try {
      await api.likeExternal({
        externalId: item.externalId,
        title: item.title,
        imageUrl: item.imageUrl,
        state: liked ? 0 : 1,
      });
    } catch {
      // ignore swipe errors (e.g. duplicate)
    }

    if (liked) setLikesInPhase(prev => prev + 1);
    setCurrentIndex(prev => prev + 1);
  }

  function handleNextPhase() {
    if (phaseIndex < PHASES.length - 1) {
      setPhaseIndex(prev => prev + 1);
    } else {
      setOnboardingComplete();
      navigate('/discover');
    }
  }

  // Phase complete check
  const phaseComplete = likesInPhase >= MIN_LIKES_PER_PHASE;

  if (phaseIndex >= PHASES.length) {
    return (
      <div className="min-h-dvh flex flex-col items-center justify-center px-6 bg-bg-primary">
        <Sparkles size={64} className="text-match-gold mb-6" />
        <h1 className="text-3xl font-black gradient-text mb-3">Listo!</h1>
        <p className="text-text-secondary text-center mb-8">Tu perfil de gustos esta configurado. Ahora a encontrar gente compatible.</p>
        <Button size="lg" onClick={() => { setOnboardingComplete(); navigate('/discover'); }}>
          Empezar a descubrir
        </Button>
      </div>
    );
  }

  const currentItem = items[currentIndex];

  return (
    <div className="min-h-dvh flex flex-col bg-bg-primary">
      {/* Progress header */}
      <div className="px-4 py-4">
        <div className="flex items-center gap-3 mb-4">
          {PHASES.map((p, i) => (
            <div key={p.key} className="flex-1">
              <div className={`h-1.5 rounded-full transition-all duration-500 ${
                i < phaseIndex ? 'bg-vibe-purple' :
                i === phaseIndex ? 'bg-gradient-to-r from-vibe-red to-vibe-purple' :
                'bg-border-subtle'
              }`} />
            </div>
          ))}
        </div>

        {phase && (
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <phase.icon size={20} style={{ color: getContentTypeColor(phaseIndex) }} />
              <h2 className="text-lg font-bold text-text-primary">{phase.label}</h2>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-xs text-text-muted">
                {likesInPhase}/{MIN_LIKES_PER_PHASE} likes
              </span>
              {phaseComplete && (
                <button
                  onClick={handleNextPhase}
                  className="flex items-center gap-1 text-sm font-semibold text-vibe-cyan hover:text-vibe-purple transition-colors"
                >
                  Siguiente <ChevronRight size={16} />
                </button>
              )}
            </div>
          </div>
        )}
      </div>

      {/* Swipe area */}
      <div className="flex-1 flex items-center justify-center px-4 pb-24">
        {loading && !currentItem ? (
          <div className="flex flex-col items-center gap-4">
            <div className="w-12 h-12 border-4 border-vibe-purple/30 border-t-vibe-purple rounded-full animate-spin" />
            <p className="text-text-muted text-sm">Cargando contenido...</p>
          </div>
        ) : currentItem ? (
          <SwipeCard
            key={currentItem.externalId}
            imageUrl={currentItem.imageUrl}
            title={currentItem.title}
            subtitle={currentItem.year ? `${currentItem.year}` : undefined}
            badge={getContentTypeLabel(currentItem.type)}
            badgeColor={getContentTypeColor(currentItem.type)}
            rating={currentItem.rating}
            description={currentItem.description}
            onSwipeRight={() => handleSwipe(true)}
            onSwipeLeft={() => handleSwipe(false)}
          />
        ) : (
          <div className="text-center">
            <p className="text-text-secondary mb-4">No hay mas contenido</p>
            {phaseComplete && (
              <Button onClick={handleNextPhase}>Siguiente fase</Button>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
