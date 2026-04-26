import { motion, useMotionValue, useTransform, type PanInfo } from 'framer-motion';
import { useState } from 'react';
import { Heart, X, Star } from 'lucide-react';

interface SwipeCardProps {
  imageUrl: string | null;
  title: string;
  subtitle?: string;
  badge?: string;
  badgeColor?: string;
  rating?: number;
  description?: string;
  overlayContent?: React.ReactNode;
  onSwipeLeft: () => void;
  onSwipeRight: () => void;
}

export function SwipeCard({
  imageUrl, title, subtitle, badge, badgeColor, rating,
  description, overlayContent, onSwipeLeft, onSwipeRight,
}: SwipeCardProps) {
  const x = useMotionValue(0);
  const rotate = useTransform(x, [-200, 200], [-15, 15]);
  const likeOpacity = useTransform(x, [0, 100], [0, 1]);
  const nopeOpacity = useTransform(x, [-100, 0], [1, 0]);
  const [exiting, setExiting] = useState<'left' | 'right' | null>(null);

  function handleDragEnd(_: unknown, info: PanInfo) {
    const threshold = 100;
    if (info.offset.x > threshold) {
      setExiting('right');
      setTimeout(onSwipeRight, 300);
    } else if (info.offset.x < -threshold) {
      setExiting('left');
      setTimeout(onSwipeLeft, 300);
    }
  }

  function handleButtonSwipe(dir: 'left' | 'right') {
    setExiting(dir);
    setTimeout(dir === 'right' ? onSwipeRight : onSwipeLeft, 300);
  }

  return (
    <div className="relative w-full max-w-sm mx-auto aspect-[3/4]">
      <motion.div
        className="swipe-card absolute inset-0 rounded-2xl overflow-hidden shadow-2xl shadow-black/50 cursor-grab active:cursor-grabbing"
        style={{ x, rotate }}
        drag="x"
        dragConstraints={{ left: 0, right: 0 }}
        dragElastic={0.7}
        onDragEnd={handleDragEnd}
        animate={
          exiting === 'right' ? { x: 500, opacity: 0, rotate: 20 }
          : exiting === 'left' ? { x: -500, opacity: 0, rotate: -20 }
          : {}
        }
        transition={{ duration: 0.3, ease: 'easeOut' }}
      >
        {/* Image */}
        <div className="absolute inset-0 bg-bg-card">
          {imageUrl ? (
            <img
              src={imageUrl}
              alt={title}
              className="w-full h-full object-cover"
              draggable={false}
            />
          ) : (
            <div className="w-full h-full flex items-center justify-center bg-gradient-to-br from-bg-card to-bg-elevated">
              <Star size={64} className="text-text-muted" />
            </div>
          )}
        </div>

        {/* Gradient overlay */}
        <div className="absolute inset-0 bg-gradient-to-t from-black/90 via-black/30 to-transparent" />

        {/* LIKE stamp */}
        <motion.div
          className="absolute top-8 left-6 border-4 border-like-green text-like-green font-black text-3xl px-4 py-1 rounded-lg -rotate-12 z-10"
          style={{ opacity: likeOpacity }}
        >
          LIKE
        </motion.div>

        {/* NOPE stamp */}
        <motion.div
          className="absolute top-8 right-6 border-4 border-dislike-red text-dislike-red font-black text-3xl px-4 py-1 rounded-lg rotate-12 z-10"
          style={{ opacity: nopeOpacity }}
        >
          NOPE
        </motion.div>

        {/* Badge */}
        {badge && (
          <div
            className="absolute top-4 left-4 px-3 py-1 rounded-full text-xs font-bold text-white z-10"
            style={{ backgroundColor: badgeColor || '#7b2fbe' }}
          >
            {badge}
          </div>
        )}

        {/* Rating */}
        {rating != null && rating > 0 && (
          <div className="absolute top-4 right-4 flex items-center gap-1 bg-black/60 backdrop-blur-sm px-2.5 py-1 rounded-full z-10">
            <Star size={12} className="text-match-gold fill-match-gold" />
            <span className="text-xs font-bold text-match-gold">{rating.toFixed(1)}</span>
          </div>
        )}

        {/* Bottom info */}
        <div className="absolute bottom-0 left-0 right-0 p-5 z-10">
          {overlayContent || (
            <>
              <h2 className="text-2xl font-bold text-white leading-tight">{title}</h2>
              {subtitle && <p className="text-sm text-white/70 mt-1">{subtitle}</p>}
              {description && (
                <p className="text-xs text-white/50 mt-2 line-clamp-3">{description}</p>
              )}
            </>
          )}
        </div>
      </motion.div>

      {/* Action buttons */}
      <div className="absolute -bottom-16 left-0 right-0 flex items-center justify-center gap-6">
        <button
          onClick={() => handleButtonSwipe('left')}
          className="w-14 h-14 rounded-full bg-bg-elevated border-2 border-dislike-red/30 flex items-center justify-center text-dislike-red hover:bg-dislike-red/10 hover:border-dislike-red transition-all active:scale-90"
        >
          <X size={28} strokeWidth={3} />
        </button>
        <button
          onClick={() => handleButtonSwipe('right')}
          className="w-16 h-16 rounded-full bg-gradient-to-r from-like-green to-vibe-cyan flex items-center justify-center text-white shadow-lg shadow-like-green/30 hover:shadow-like-green/50 transition-all active:scale-90"
        >
          <Heart size={30} strokeWidth={2.5} fill="white" />
        </button>
      </div>
    </div>
  );
}
