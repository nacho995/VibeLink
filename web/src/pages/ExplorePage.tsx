import { useState, useEffect, useCallback, useRef } from 'react';
import { Film, Tv, Gamepad2, Search, Heart, X, Star, Check, Loader2 } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { api } from '../lib/api';
import { getContentTypeLabel, getContentTypeColor, cn } from '../lib/utils';
import type { ContentItem } from '../types';

const TABS = [
  { key: 'movies' as const, label: 'Pelis', icon: Film, color: '#e50914', fetchFn: (p: number) => api.discoverMovies(p) },
  { key: 'series' as const, label: 'Series', icon: Tv, color: '#7b2fbe', fetchFn: (p: number) => api.discoverSeries(p) },
  { key: 'games' as const, label: 'Juegos', icon: Gamepad2, color: '#00d4ff', fetchFn: (p: number) => api.discoverGames(p) },
];

type LikeStatus = 'none' | 'liked' | 'disliked' | 'loading';

export function ExplorePage() {
  const [activeTab, setActiveTab] = useState(0);
  const [items, setItems] = useState<ContentItem[]>([]);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);

  // Track like status per externalId
  const [likeMap, setLikeMap] = useState<Record<string, LikeStatus>>({});

  // Search
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<ContentItem[] | null>(null);
  const [searching, setSearching] = useState(false);
  const searchTimeout = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Detail modal
  const [selectedItem, setSelectedItem] = useState<ContentItem | null>(null);

  const tab = TABS[activeTab];
  const scrollRef = useRef<HTMLDivElement>(null);

  const loadContent = useCallback(async (fetchFn: (p: number) => Promise<ContentItem[]>, p: number, reset: boolean) => {
    if (reset) setLoading(true);
    else setLoadingMore(true);
    try {
      const data = await fetchFn(p);
      if (data.length === 0) setHasMore(false);
      setItems(prev => reset ? data : [...prev, ...data]);
    } catch (err) {
      console.error('Error loading content:', err);
    } finally {
      setLoading(false);
      setLoadingMore(false);
    }
  }, []);

  // Reset on tab change
  useEffect(() => {
    setItems([]);
    setPage(1);
    setHasMore(true);
    setSearchQuery('');
    setSearchResults(null);
    setLikeMap({});
    loadContent(tab.fetchFn, 1, true);
  }, [activeTab, tab.fetchFn, loadContent]);

  // Infinite scroll
  function handleScroll() {
    const el = scrollRef.current;
    if (!el || loadingMore || !hasMore || searchResults !== null) return;
    if (el.scrollTop + el.clientHeight >= el.scrollHeight - 300) {
      const nextPage = page + 1;
      setPage(nextPage);
      loadContent(tab.fetchFn, nextPage, false);
    }
  }

  // Debounced search
  useEffect(() => {
    if (searchTimeout.current) clearTimeout(searchTimeout.current);
    const q = searchQuery.trim();
    if (!q) {
      setSearchResults(null);
      return;
    }
    searchTimeout.current = setTimeout(async () => {
      setSearching(true);
      try {
        const data = await api.searchContent(q);
        setSearchResults(data);
      } catch {
        setSearchResults([]);
      } finally {
        setSearching(false);
      }
    }, 400);
    return () => { if (searchTimeout.current) clearTimeout(searchTimeout.current); };
  }, [searchQuery]);

  async function handleLike(item: ContentItem, liked: boolean) {
    const key = item.externalId;
    if (likeMap[key] === 'liked' || likeMap[key] === 'disliked' || likeMap[key] === 'loading') return;

    setLikeMap(prev => ({ ...prev, [key]: 'loading' }));
    try {
      await api.likeExternal({
        externalId: item.externalId,
        title: item.title,
        imageUrl: item.imageUrl,
        state: liked ? 0 : 1,
      });
      setLikeMap(prev => ({ ...prev, [key]: liked ? 'liked' : 'disliked' }));
    } catch {
      // Si ya existia el like, marcarlo como liked
      setLikeMap(prev => ({ ...prev, [key]: liked ? 'liked' : 'disliked' }));
    }
  }

  const displayItems = searchResults !== null ? searchResults : items;
  const isInitialLoad = loading && items.length === 0;

  return (
    <div className="flex flex-col h-full min-h-[calc(100dvh-7rem)]">

      {/* Header: tabs + search */}
      <div className="px-4 pt-3 pb-2 flex-shrink-0">
        {/* Tabs */}
        <div className="flex gap-2 mb-3">
          {TABS.map((t, i) => (
            <button
              key={t.key}
              onClick={() => setActiveTab(i)}
              className={cn(
                'flex-1 flex items-center justify-center gap-1.5 py-2.5 rounded-xl text-sm font-semibold transition-all border',
                i === activeTab
                  ? 'border-transparent text-white'
                  : 'bg-bg-card border-border-subtle text-text-muted hover:text-text-secondary',
              )}
              style={i === activeTab ? { backgroundColor: `${t.color}20`, borderColor: t.color, color: t.color } : undefined}
            >
              <t.icon size={16} />
              {t.label}
            </button>
          ))}
        </div>

        {/* Search */}
        <div className="relative">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-text-muted" />
          <input
            type="text"
            value={searchQuery}
            onChange={e => setSearchQuery(e.target.value)}
            placeholder="Buscar pelis, series, juegos..."
            className="w-full bg-bg-card border border-border-subtle rounded-xl pl-9 pr-9 py-2.5 text-sm text-text-primary placeholder-text-muted focus:outline-none focus:border-vibe-purple transition-colors"
          />
          {searchQuery && (
            <button
              onClick={() => setSearchQuery('')}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-text-muted hover:text-text-primary"
            >
              <X size={14} />
            </button>
          )}
        </div>
      </div>

      {/* Content grid */}
      <div
        ref={scrollRef}
        onScroll={handleScroll}
        className="flex-1 overflow-y-auto px-4 pb-24 no-scrollbar"
      >
        {isInitialLoad || searching ? (
          <div className="flex flex-col items-center justify-center py-20">
            <div className="w-10 h-10 border-4 border-vibe-purple/30 border-t-vibe-purple rounded-full animate-spin" />
            <p className="text-text-muted text-sm mt-4">
              {searching ? 'Buscando...' : 'Cargando...'}
            </p>
          </div>
        ) : displayItems.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-20">
            <Search size={48} className="text-text-muted mb-4" />
            <p className="text-text-secondary text-sm text-center">
              {searchResults !== null
                ? 'No se encontraron resultados. Proba con otro termino.'
                : 'No hay contenido disponible.'}
            </p>
          </div>
        ) : (
          <>
            <div className="grid grid-cols-2 gap-3">
              {displayItems.map((item) => {
                const status = likeMap[item.externalId] || 'none';
                return (
                  <ContentCard
                    key={item.externalId}
                    item={item}
                    status={status}
                    onLike={() => handleLike(item, true)}
                    onDislike={() => handleLike(item, false)}
                    onTap={() => setSelectedItem(item)}
                  />
                );
              })}
            </div>

            {/* Load more indicator */}
            {loadingMore && (
              <div className="flex justify-center py-6">
                <Loader2 size={24} className="text-vibe-purple animate-spin" />
              </div>
            )}
          </>
        )}
      </div>

      {/* Detail modal */}
      <AnimatePresence>
        {selectedItem && (
          <DetailModal
            item={selectedItem}
            status={likeMap[selectedItem.externalId] || 'none'}
            onLike={() => handleLike(selectedItem, true)}
            onDislike={() => handleLike(selectedItem, false)}
            onClose={() => setSelectedItem(null)}
          />
        )}
      </AnimatePresence>
    </div>
  );
}

/* ======= Content Card Component ======= */

function ContentCard({ item, status, onLike, onDislike, onTap }: {
  item: ContentItem;
  status: LikeStatus;
  onLike: () => void;
  onDislike: () => void;
  onTap: () => void;
}) {
  const acted = status === 'liked' || status === 'disliked';

  return (
    <motion.div
      layout
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: 1, scale: 1 }}
      className={cn(
        'relative bg-bg-card border rounded-xl overflow-hidden transition-all',
        acted ? 'border-border-subtle opacity-60' : 'border-border-subtle',
      )}
    >
      {/* Image */}
      <button onClick={onTap} className="w-full aspect-[2/3] relative block">
        {item.imageUrl ? (
          <img
            src={item.imageUrl}
            alt={item.title}
            className="w-full h-full object-cover"
            loading="lazy"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center bg-bg-elevated">
            <Star size={32} className="text-text-muted" />
          </div>
        )}

        {/* Gradient overlay */}
        <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-transparent to-transparent" />

        {/* Badge */}
        <span
          className="absolute top-2 left-2 px-2 py-0.5 rounded-full text-[10px] font-bold text-white"
          style={{ backgroundColor: getContentTypeColor(item.type) }}
        >
          {getContentTypeLabel(item.type)}
        </span>

        {/* Rating */}
        {item.rating > 0 && (
          <div className="absolute top-2 right-2 flex items-center gap-0.5 bg-black/60 backdrop-blur-sm px-1.5 py-0.5 rounded-full">
            <Star size={10} className="text-match-gold fill-match-gold" />
            <span className="text-[10px] font-bold text-match-gold">{item.rating.toFixed(1)}</span>
          </div>
        )}

        {/* Title overlay */}
        <div className="absolute bottom-0 left-0 right-0 p-2.5">
          <h3 className="text-sm font-bold text-white leading-tight line-clamp-2">{item.title}</h3>
          {item.year && (
            <p className="text-[10px] text-white/60 mt-0.5">{item.year}</p>
          )}
        </div>

        {/* Liked/disliked stamp */}
        {status === 'liked' && (
          <div className="absolute inset-0 bg-like-green/20 flex items-center justify-center">
            <div className="bg-like-green rounded-full p-2">
              <Check size={24} className="text-white" />
            </div>
          </div>
        )}
        {status === 'disliked' && (
          <div className="absolute inset-0 bg-dislike-red/20 flex items-center justify-center">
            <div className="bg-dislike-red rounded-full p-2">
              <X size={24} className="text-white" />
            </div>
          </div>
        )}
      </button>

      {/* Action buttons */}
      {!acted && (
        <div className="flex border-t border-border-subtle">
          <button
            onClick={onDislike}
            disabled={status === 'loading'}
            className="flex-1 flex items-center justify-center gap-1 py-2.5 text-text-muted hover:text-dislike-red hover:bg-dislike-red/5 transition-colors text-xs font-medium"
          >
            <X size={14} />
            Nope
          </button>
          <div className="w-px bg-border-subtle" />
          <button
            onClick={onLike}
            disabled={status === 'loading'}
            className="flex-1 flex items-center justify-center gap-1 py-2.5 text-text-muted hover:text-like-green hover:bg-like-green/5 transition-colors text-xs font-medium"
          >
            {status === 'loading' ? (
              <Loader2 size={14} className="animate-spin" />
            ) : (
              <Heart size={14} />
            )}
            Like
          </button>
        </div>
      )}
    </motion.div>
  );
}

/* ======= Detail Modal Component ======= */

function DetailModal({ item, status, onLike, onDislike, onClose }: {
  item: ContentItem;
  status: LikeStatus;
  onLike: () => void;
  onDislike: () => void;
  onClose: () => void;
}) {
  const acted = status === 'liked' || status === 'disliked';

  return (
    <motion.div
      className="fixed inset-0 z-[100] flex items-end sm:items-center justify-center bg-black/70 backdrop-blur-sm"
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      onClick={onClose}
    >
      <motion.div
        className="bg-bg-card border border-border-subtle rounded-t-3xl sm:rounded-3xl max-w-lg w-full max-h-[85vh] overflow-y-auto no-scrollbar"
        initial={{ y: 100, opacity: 0 }}
        animate={{ y: 0, opacity: 1 }}
        exit={{ y: 100, opacity: 0 }}
        transition={{ type: 'spring', damping: 25 }}
        onClick={e => e.stopPropagation()}
      >
        {/* Image header */}
        <div className="relative aspect-video">
          {(item.backdropUrl || item.imageUrl) ? (
            <img
              src={item.backdropUrl || item.imageUrl || ''}
              alt={item.title}
              className="w-full h-full object-cover rounded-t-3xl"
            />
          ) : (
            <div className="w-full h-full bg-bg-elevated flex items-center justify-center rounded-t-3xl">
              <Star size={48} className="text-text-muted" />
            </div>
          )}
          <div className="absolute inset-0 bg-gradient-to-t from-bg-card via-transparent to-transparent" />

          {/* Close button */}
          <button
            onClick={onClose}
            className="absolute top-3 right-3 w-8 h-8 rounded-full bg-black/50 backdrop-blur-sm flex items-center justify-center text-white hover:bg-black/70 transition-colors"
          >
            <X size={16} />
          </button>

          {/* Badge + Rating */}
          <div className="absolute top-3 left-3 flex items-center gap-2">
            <span
              className="px-2.5 py-1 rounded-full text-xs font-bold text-white"
              style={{ backgroundColor: getContentTypeColor(item.type) }}
            >
              {getContentTypeLabel(item.type)}
            </span>
            {item.rating > 0 && (
              <span className="flex items-center gap-1 bg-black/50 backdrop-blur-sm px-2 py-1 rounded-full">
                <Star size={12} className="text-match-gold fill-match-gold" />
                <span className="text-xs font-bold text-match-gold">{item.rating.toFixed(1)}</span>
              </span>
            )}
          </div>
        </div>

        {/* Info */}
        <div className="p-5 -mt-6 relative">
          <h2 className="text-xl font-bold text-text-primary mb-1">{item.title}</h2>

          <div className="flex items-center gap-2 text-xs text-text-muted mb-3 flex-wrap">
            {item.year && <span>{item.year}</span>}
            {item.genres.length > 0 && (
              <>
                <span className="w-1 h-1 rounded-full bg-text-muted" />
                <span>{item.genres.slice(0, 3).join(', ')}</span>
              </>
            )}
            {item.platforms && item.platforms.length > 0 && (
              <>
                <span className="w-1 h-1 rounded-full bg-text-muted" />
                <span>{item.platforms.slice(0, 3).join(', ')}</span>
              </>
            )}
          </div>

          {item.description && (
            <p className="text-sm text-text-secondary leading-relaxed mb-5">{item.description}</p>
          )}

          {/* Actions */}
          {!acted ? (
            <div className="flex gap-3">
              <button
                onClick={onDislike}
                disabled={status === 'loading'}
                className="flex-1 flex items-center justify-center gap-2 py-3 rounded-xl border-2 border-dislike-red/30 text-dislike-red font-semibold hover:bg-dislike-red/10 transition-all active:scale-95"
              >
                <X size={18} />
                Nope
              </button>
              <button
                onClick={onLike}
                disabled={status === 'loading'}
                className="flex-1 flex items-center justify-center gap-2 py-3 rounded-xl bg-gradient-to-r from-like-green to-vibe-cyan text-white font-semibold shadow-lg shadow-like-green/20 hover:shadow-like-green/40 transition-all active:scale-95"
              >
                {status === 'loading' ? (
                  <Loader2 size={18} className="animate-spin" />
                ) : (
                  <Heart size={18} />
                )}
                Like
              </button>
            </div>
          ) : (
            <div className={cn(
              'flex items-center justify-center gap-2 py-3 rounded-xl font-semibold',
              status === 'liked'
                ? 'bg-like-green/10 text-like-green border border-like-green/20'
                : 'bg-dislike-red/10 text-dislike-red border border-dislike-red/20',
            )}>
              {status === 'liked' ? <Check size={18} /> : <X size={18} />}
              {status === 'liked' ? 'Te gusta' : 'No te interesa'}
            </div>
          )}
        </div>
      </motion.div>
    </motion.div>
  );
}
