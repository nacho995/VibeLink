import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { MessageCircle } from 'lucide-react';
import { api } from '../lib/api';
import { useAuthStore } from '../stores/authStore';
import type { MatchInfo } from '../types';

export function ChatListPage() {
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
        <MessageCircle size={56} className="text-text-muted mb-4" />
        <h2 className="text-xl font-bold text-text-primary mb-2">Sin conversaciones</h2>
        <p className="text-text-secondary text-sm text-center">
          Cuando tengas un match, podras chatear aqui.
        </p>
      </div>
    );
  }

  return (
    <div className="px-4 py-4">
      <h2 className="text-lg font-bold text-text-primary mb-4 flex items-center gap-2">
        <MessageCircle size={20} className="text-vibe-cyan" />
        Conversaciones
      </h2>

      <div className="space-y-2">
        {matches.map((match, i) => (
          <motion.div
            key={match.matchId}
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: i * 0.04 }}
            onClick={() => navigate(`/chat/${match.anotherUser.id}`)}
            className="flex items-center gap-3 p-3 rounded-2xl hover:bg-bg-elevated transition-all cursor-pointer active:scale-[0.98]"
          >
            <div className="w-12 h-12 rounded-full bg-gradient-to-br from-vibe-red to-vibe-purple flex items-center justify-center text-white font-bold shrink-0 overflow-hidden">
              {match.anotherUser.avatarUrl ? (
                <img src={match.anotherUser.avatarUrl} alt="" className="w-full h-full object-cover" />
              ) : (
                match.anotherUser.username.charAt(0).toUpperCase()
              )}
            </div>
            <div className="flex-1 min-w-0">
              <h3 className="font-semibold text-text-primary text-sm">{match.anotherUser.username}</h3>
              <p className="text-xs text-text-muted">Toca para chatear</p>
            </div>
          </motion.div>
        ))}
      </div>
    </div>
  );
}
