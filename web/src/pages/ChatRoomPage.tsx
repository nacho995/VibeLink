import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Send } from 'lucide-react';
import { motion } from 'framer-motion';
import { api } from '../lib/api';
import { useAuthStore } from '../stores/authStore';
import { cn } from '../lib/utils';
import type { Message } from '../types';

export function ChatRoomPage() {
  const { matchUserId } = useParams<{ matchUserId: string }>();
  const navigate = useNavigate();
  const userId = useAuthStore(s => s.userId);
  const otherUserId = Number(matchUserId);

  const [messages, setMessages] = useState<Message[]>([]);
  const [newMessage, setNewMessage] = useState('');
  const [sending, setSending] = useState(false);
  const [otherUsername, setOtherUsername] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const pollRef = useRef<ReturnType<typeof setInterval>>(undefined);

  useEffect(() => {
    if (!userId || !otherUserId) return;

    // Load other user's info
    api.getUser(otherUserId)
      .then(u => setOtherUsername(u.username))
      .catch(() => setOtherUsername(`User ${otherUserId}`));

    // Load messages
    loadMessages();

    // Poll for new messages every 3 seconds
    pollRef.current = setInterval(loadMessages, 3000);
    return () => clearInterval(pollRef.current);
  }, [userId, otherUserId]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  async function loadMessages() {
    if (!userId || !otherUserId) return;
    try {
      const msgs = await api.getMessages(userId, otherUserId);
      setMessages(msgs);
    } catch {
      // ignore
    }
  }

  async function handleSend(e: React.FormEvent) {
    e.preventDefault();
    if (!newMessage.trim() || !userId || sending) return;

    setSending(true);
    try {
      await api.sendMessage({
        userId,
        matchingUserId: otherUserId,
        message: newMessage.trim(),
      });
      setNewMessage('');
      await loadMessages();
    } catch {
      // ignore
    } finally {
      setSending(false);
    }
  }

  return (
    <div className="flex flex-col h-dvh bg-bg-primary">
      {/* Header */}
      <header className="glass flex items-center gap-3 px-4 py-3 z-10">
        <button
          onClick={() => navigate('/chat')}
          className="p-1.5 rounded-xl hover:bg-bg-elevated transition-colors"
        >
          <ArrowLeft size={22} className="text-text-primary" />
        </button>
        <div className="w-9 h-9 rounded-full bg-gradient-to-br from-vibe-purple to-vibe-cyan flex items-center justify-center text-white text-sm font-bold">
          {otherUsername.charAt(0).toUpperCase() || '?'}
        </div>
        <div>
          <h2 className="font-semibold text-text-primary text-sm">{otherUsername}</h2>
          <p className="text-[10px] text-text-muted">Online</p>
        </div>
      </header>

      {/* Messages */}
      <div className="flex-1 overflow-y-auto px-4 py-4 space-y-3 no-scrollbar">
        {messages.length === 0 && (
          <div className="text-center text-text-muted text-sm py-12">
            Empieza la conversacion!
          </div>
        )}
        {messages.map((msg, i) => {
          const isMine = msg.userId === userId;
          return (
            <motion.div
              key={msg.id}
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: i * 0.02 }}
              className={cn('flex', isMine ? 'justify-end' : 'justify-start')}
            >
              <div className={cn(
                'max-w-[75%] px-4 py-2.5 rounded-2xl text-sm',
                isMine
                  ? 'bg-gradient-to-r from-vibe-purple to-vibe-cyan text-white rounded-br-sm'
                  : 'bg-bg-elevated text-text-primary border border-border-subtle rounded-bl-sm',
              )}>
                <p className="break-words">{msg.message}</p>
                <p className={cn(
                  'text-[10px] mt-1',
                  isMine ? 'text-white/50' : 'text-text-muted',
                )}>
                  {new Date(msg.date).toLocaleTimeString('es', { hour: '2-digit', minute: '2-digit' })}
                </p>
              </div>
            </motion.div>
          );
        })}
        <div ref={messagesEndRef} />
      </div>

      {/* Input */}
      <form onSubmit={handleSend} className="glass px-4 py-3 flex items-center gap-3">
        <input
          value={newMessage}
          onChange={e => setNewMessage(e.target.value)}
          placeholder="Escribe un mensaje..."
          className="flex-1 bg-bg-input border border-border-subtle rounded-xl px-4 py-2.5 text-sm text-text-primary placeholder-text-muted focus:outline-none focus:border-vibe-purple transition-colors"
        />
        <button
          type="submit"
          disabled={!newMessage.trim() || sending}
          className="w-10 h-10 rounded-xl bg-gradient-to-r from-vibe-purple to-vibe-cyan flex items-center justify-center text-white disabled:opacity-40 active:scale-90 transition-all"
        >
          <Send size={18} />
        </button>
      </form>
    </div>
  );
}
