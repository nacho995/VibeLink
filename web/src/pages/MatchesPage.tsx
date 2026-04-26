import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Heart, MessageCircle, Users } from 'lucide-react';
import { api } from '../lib/api';
import { useAuthStore } from '../stores/authStore';
import { formatTimeAgo } from '../lib/utils';
import type { MatchInfo } from '../types';

export function MatchesPage() {
  const userId = useAuthStore(s => s.userId);
  const navigate = useNavigate();
  const [matches, setMatches] = useState<MatchInfo[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!userId) return;
    api.getMyMatches(userId)
      .then(setMatches)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [userId]);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="w-10 h-10 border-4 border-vibe-purple/30 border-t-vibe-purple rounded-full animate-spin" />
      </div>
    );
  }

  if (matches.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh] px-6">
        <Users size={56} className="text-text-muted mb-4" />
        <h2 className="text-xl font-bold text-text-primary mb-2">Sin matches aun</h2>
        <p className="text-text-secondary text-sm text-center">
          Sigue haciendo swipe para encontrar personas con tus mismos gustos.
        </p>
      </div>
    );
  }

  return (
    <div className="px-4 py-4">
      <h2 className="text-lg font-bold text-text-primary mb-4 flex items-center gap-2">
        <Heart size={20} className="text-vibe-red" fill="#e50914" />
        Tus Matches ({matches.length})
      </h2>

      <div className="space-y-3">
        {matches.map((match, i) => (
          <motion.div
            key={match.matchId}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: i * 0.05 }}
            onClick={() => navigate(`/chat/${match.anotherUser.id}`)}
            className="flex items-center gap-4 p-4 bg-bg-card border border-border-subtle rounded-2xl hover:border-vibe-purple/30 transition-all cursor-pointer active:scale-[0.98]"
          >
            {/* Avatar */}
            <div className="w-14 h-14 rounded-full bg-gradient-to-br from-vibe-purple to-vibe-cyan flex items-center justify-center text-white font-bold text-lg shrink-0 overflow-hidden">
              {match.anotherUser.avatarUrl ? (
                <img src={match.anotherUser.avatarUrl} alt="" className="w-full h-full object-cover" />
              ) : (
                match.anotherUser.username.charAt(0).toUpperCase()
              )}
            </div>

            {/* Info */}
            <div className="flex-1 min-w-0">
              <h3 className="font-semibold text-text-primary truncate">{match.anotherUser.username}</h3>
              {match.anotherUser.bio && (
                <p className="text-sm text-text-secondary truncate">{match.anotherUser.bio}</p>
              )}
              <p className="text-xs text-text-muted mt-0.5">Match {formatTimeAgo(match.matchDate)}</p>
            </div>

            {/* Chat icon */}
            <MessageCircle size={20} className="text-text-muted shrink-0" />
          </motion.div>
        ))}
      </div>
    </div>
  );
}
