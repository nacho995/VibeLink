import { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Heart, Sparkles, Users, Percent } from 'lucide-react';
import { SwipeCard } from '../components/swipe/SwipeCard';
import { Button } from '../components/ui/Button';
import { api } from '../lib/api';
import { useAuthStore } from '../stores/authStore';
import type { MatchCandidate } from '../types';

export function DiscoverPage() {
  const userId = useAuthStore(s => s.userId);
  const [candidates, setCandidates] = useState<MatchCandidate[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [loading, setLoading] = useState(true);
  const [matchPopup, setMatchPopup] = useState<MatchCandidate | null>(null);

  useEffect(() => {
    if (!userId) return;
    loadCandidates();
  }, [userId]);

  async function loadCandidates() {
    if (!userId) return;
    setLoading(true);
    try {
      const data = await api.getCompatiblePeople(userId);
      setCandidates(data.filter(c => c.compatibilidad > 0));
    } catch (err) {
      console.error('Error loading candidates:', err);
    } finally {
      setLoading(false);
    }
  }

  async function handleSwipe(liked: boolean) {
    if (!userId) return;
    const candidate = candidates[currentIndex];
    if (!candidate) return;

    try {
      const result = await api.swipePerson({
        userId,
        matchingUserId: candidate.usuario.id,
        state: liked ? 0 : 1,
      });

      // If backend returns a Match object, it's a match!
      if (result && typeof result === 'object' && 'id' in (result as Record<string, unknown>)) {
        setMatchPopup(candidate);
      }
    } catch {
      // ignore
    }

    setCurrentIndex(prev => prev + 1);
  }

  const currentCandidate = candidates[currentIndex];

  if (loading) {
    return (
      <div className="flex-1 flex items-center justify-center h-full min-h-[60vh]">
        <div className="flex flex-col items-center gap-4">
          <div className="w-12 h-12 border-4 border-vibe-purple/30 border-t-vibe-purple rounded-full animate-spin" />
          <p className="text-text-muted text-sm">Buscando personas compatibles...</p>
        </div>
      </div>
    );
  }

  if (!currentCandidate) {
    return (
      <div className="flex-1 flex flex-col items-center justify-center h-full min-h-[60vh] px-6">
        <Users size={64} className="text-text-muted mb-4" />
        <h2 className="text-xl font-bold text-text-primary mb-2">No hay mas personas</h2>
        <p className="text-text-secondary text-center text-sm mb-6">
          Vuelve mas tarde o dale like a mas contenido para mejorar tus matches.
        </p>
        <Button variant="secondary" onClick={loadCandidates}>Refrescar</Button>
      </div>
    );
  }

  return (
    <>
      <div className="flex-1 flex items-center justify-center px-4 pb-24 pt-4">
        <SwipeCard
          key={currentCandidate.usuario.id}
          imageUrl={currentCandidate.usuario.avatarUrl}
          title={currentCandidate.usuario.username}
          subtitle={currentCandidate.usuario.bio || undefined}
          onSwipeRight={() => handleSwipe(true)}
          onSwipeLeft={() => handleSwipe(false)}
          overlayContent={
            <div>
              <h2 className="text-2xl font-bold text-white">{currentCandidate.usuario.username}</h2>
              {currentCandidate.usuario.bio && (
                <p className="text-sm text-white/70 mt-1 line-clamp-2">{currentCandidate.usuario.bio}</p>
              )}
              <div className="flex items-center gap-2 mt-3">
                <div className="flex items-center gap-1.5 bg-vibe-purple/30 backdrop-blur-sm px-3 py-1.5 rounded-full">
                  <Percent size={14} className="text-vibe-cyan" />
                  <span className="text-sm font-bold text-white">{currentCandidate.compatibilidad}% compatible</span>
                </div>
              </div>
            </div>
          }
        />
      </div>

      {/* Match popup */}
      <AnimatePresence>
        {matchPopup && (
          <motion.div
            className="fixed inset-0 z-[100] flex items-center justify-center bg-black/80 backdrop-blur-sm px-6"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={() => setMatchPopup(null)}
          >
            <motion.div
              className="bg-bg-card border border-border-subtle rounded-3xl p-8 max-w-sm w-full text-center"
              initial={{ scale: 0.5, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.5, opacity: 0 }}
              transition={{ type: 'spring', duration: 0.6 }}
              onClick={e => e.stopPropagation()}
            >
              <motion.div
                animate={{ scale: [1, 1.2, 1] }}
                transition={{ repeat: Infinity, duration: 1.5 }}
              >
                <Heart size={56} className="text-vibe-red mx-auto mb-4" fill="#e50914" />
              </motion.div>
              <h2 className="text-3xl font-black gradient-text mb-2">It's a Match!</h2>
              <p className="text-text-secondary text-sm mb-6">
                Tu y <span className="text-vibe-cyan font-semibold">{matchPopup.usuario.username}</span> tienen gustos compatibles
              </p>
              <div className="flex items-center justify-center gap-1.5 mb-6">
                <Sparkles size={16} className="text-match-gold" />
                <span className="text-lg font-bold text-match-gold">{matchPopup.compatibilidad}%</span>
              </div>
              <div className="flex gap-3">
                <Button variant="secondary" className="flex-1" onClick={() => setMatchPopup(null)}>
                  Seguir
                </Button>
                <Button className="flex-1" onClick={() => { setMatchPopup(null); window.location.href = '/chat'; }}>
                  Chatear
                </Button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
}
