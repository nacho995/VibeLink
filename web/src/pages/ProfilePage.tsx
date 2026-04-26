import { useState, useEffect, useRef } from 'react';
import { LogOut, Crown, Save, Camera, Calendar, User as UserIcon, X, Loader2, ImagePlus } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { useAuthStore } from '../stores/authStore';
import { api } from '../lib/api';
import { uploadAvatar } from '../lib/cloudinary';
import { Button } from '../components/ui/Button';
import { cn } from '../lib/utils';

export function ProfilePage() {
  const { user, userId, logout, loadUser } = useAuthStore();
  const [editing, setEditing] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Form state
  const [bio, setBio] = useState('');
  const [avatarUrl, setAvatarUrl] = useState('');
  const [gender, setGender] = useState(0);
  const [dateOfBirth, setDateOfBirth] = useState('');

  // Avatar upload
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [uploading, setUploading] = useState(false);
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);

  // Sync form with user data
  useEffect(() => {
    if (user) {
      setBio(user.bio || '');
      setAvatarUrl(user.avatarUrl || '');
      setGender(user.gender);
      setAvatarPreview(null);
      if (user.dateOfBirth && user.dateOfBirth !== '0001-01-01T00:00:00') {
        const d = new Date(user.dateOfBirth);
        if (d.getFullYear() > 1900) {
          setDateOfBirth(d.toISOString().split('T')[0]);
        }
      }
    }
  }, [user]);

  function startEditing() {
    setError('');
    setSuccess('');
    setEditing(true);
  }

  function cancelEditing() {
    if (user) {
      setBio(user.bio || '');
      setAvatarUrl(user.avatarUrl || '');
      setGender(user.gender);
      setAvatarPreview(null);
      if (user.dateOfBirth && user.dateOfBirth !== '0001-01-01T00:00:00') {
        const d = new Date(user.dateOfBirth);
        if (d.getFullYear() > 1900) {
          setDateOfBirth(d.toISOString().split('T')[0]);
        }
      }
    }
    setEditing(false);
    setError('');
  }

  async function handleFileSelect(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate
    if (!file.type.startsWith('image/')) {
      setError('Solo se permiten imagenes');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      setError('La imagen no puede pesar mas de 5MB');
      return;
    }

    // Local preview
    const localUrl = URL.createObjectURL(file);
    setAvatarPreview(localUrl);
    setError('');

    // Upload to Cloudinary
    setUploading(true);
    try {
      const url = await uploadAvatar(file);
      setAvatarUrl(url);
      setAvatarPreview(null);
    } catch {
      setError('Error al subir la imagen. Intenta de nuevo.');
      setAvatarPreview(null);
    } finally {
      setUploading(false);
      // Reset file input
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  }

  async function handleSave() {
    if (!userId || !user) return;
    if (uploading) return;
    setError('');
    setSuccess('');
    setSaving(true);

    try {
      await api.updateProfile(userId, {
        gender,
        dateOfBirth: dateOfBirth ? new Date(dateOfBirth).toISOString() : new Date().toISOString(),
        bio: bio.trim() || null,
        avatarUrl: avatarUrl.trim() || null,
      });
      await loadUser();
      setEditing(false);
      setSuccess('Perfil actualizado');
      setTimeout(() => setSuccess(''), 3000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al guardar');
    } finally {
      setSaving(false);
    }
  }

  if (!user) return null;

  // Show: local preview > edited cloudinary url > saved url
  const displayAvatar = avatarPreview || (editing ? avatarUrl.trim() : user.avatarUrl);

  return (
    <div className="px-4 py-6 max-w-md mx-auto pb-24">

      {/* Hidden file input */}
      <input
        ref={fileInputRef}
        type="file"
        accept="image/*"
        onChange={handleFileSelect}
        className="hidden"
      />

      {/* ===== Avatar section ===== */}
      <div className="flex flex-col items-center mb-6">
        <div className="relative mb-4">
          {/* Avatar circle with gradient border */}
          <div className="w-28 h-28 rounded-full bg-gradient-to-br from-vibe-red via-vibe-purple to-vibe-cyan p-[3px]">
            <div className="w-full h-full rounded-full bg-bg-card flex items-center justify-center overflow-hidden relative">
              {displayAvatar ? (
                <img
                  src={displayAvatar}
                  alt={user.username}
                  className="w-full h-full object-cover"
                  onError={(e) => { (e.target as HTMLImageElement).style.display = 'none'; }}
                />
              ) : (
                <span className="text-4xl font-bold gradient-text">
                  {user.username.charAt(0).toUpperCase()}
                </span>
              )}

              {/* Upload spinner overlay */}
              {uploading && (
                <div className="absolute inset-0 bg-black/60 flex items-center justify-center">
                  <Loader2 size={28} className="text-white animate-spin" />
                </div>
              )}
            </div>
          </div>

          {/* Premium badge */}
          {user.isPremium && !editing && (
            <div className="absolute -bottom-1 -right-1 w-8 h-8 rounded-full bg-match-gold flex items-center justify-center shadow-lg shadow-match-gold/30">
              <Crown size={16} className="text-black" />
            </div>
          )}

          {/* Camera button when editing */}
          {editing && (
            <button
              onClick={() => fileInputRef.current?.click()}
              disabled={uploading}
              className="absolute bottom-0 right-0 w-9 h-9 rounded-full bg-vibe-purple flex items-center justify-center shadow-lg hover:bg-vibe-cyan transition-colors disabled:opacity-50"
            >
              <Camera size={16} className="text-white" />
            </button>
          )}
        </div>

        <h2 className="text-xl font-bold text-text-primary">{user.username}</h2>
        <p className="text-sm text-text-secondary">{user.email}</p>
        {user.isPremium && (
          <span className="mt-2 px-3 py-1 rounded-full bg-match-gold/10 text-match-gold text-xs font-bold">
            Premium
          </span>
        )}
      </div>

      {/* ===== Success / Error messages ===== */}
      <AnimatePresence>
        {success && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0 }}
            className="mb-4 px-4 py-3 bg-like-green/10 border border-like-green/20 rounded-xl text-sm text-like-green text-center"
          >
            {success}
          </motion.div>
        )}
        {error && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0 }}
            className="mb-4 px-4 py-3 bg-dislike-red/10 border border-dislike-red/20 rounded-xl text-sm text-dislike-red text-center"
          >
            {error}
          </motion.div>
        )}
      </AnimatePresence>

      {/* ===== Stats bar ===== */}
      <div className="grid grid-cols-3 gap-3 mb-6">
        <div className="bg-bg-card border border-border-subtle rounded-xl p-3 text-center">
          <p className="text-xl font-bold text-vibe-cyan">{user.swipes}</p>
          <p className="text-[10px] text-text-muted mt-0.5">Swipes</p>
        </div>
        <div className="bg-bg-card border border-border-subtle rounded-xl p-3 text-center">
          <p className="text-xl font-bold text-vibe-purple">
            {user.gender === 0 ? 'Hombre' : 'Mujer'}
          </p>
          <p className="text-[10px] text-text-muted mt-0.5">Genero</p>
        </div>
        <div className="bg-bg-card border border-border-subtle rounded-xl p-3 text-center">
          <p className="text-xl font-bold text-vibe-red">
            {user.dateOfBirth && user.dateOfBirth !== '0001-01-01T00:00:00'
              ? `${Math.floor((Date.now() - new Date(user.dateOfBirth).getTime()) / 31557600000)}`
              : '--'}
          </p>
          <p className="text-[10px] text-text-muted mt-0.5">Edad</p>
        </div>
      </div>

      {/* ===== Edit / View toggle button ===== */}
      {!editing ? (
        <Button variant="secondary" className="w-full mb-6" onClick={startEditing}>
          <UserIcon size={16} /> Editar perfil
        </Button>
      ) : (
        <div className="flex gap-3 mb-6">
          <Button variant="ghost" className="flex-1" onClick={cancelEditing}>
            <X size={16} /> Cancelar
          </Button>
          <Button className="flex-1" onClick={handleSave} loading={saving || uploading}>
            <Save size={16} /> {uploading ? 'Subiendo...' : 'Guardar'}
          </Button>
        </div>
      )}

      {/* ===== Profile fields ===== */}
      <div className="space-y-4 mb-6">

        {/* Avatar upload */}
        <div className="bg-bg-card border border-border-subtle rounded-xl p-4">
          <label className="block text-xs font-semibold text-text-muted uppercase tracking-wider mb-2">
            Foto de perfil
          </label>
          {editing ? (
            <button
              onClick={() => fileInputRef.current?.click()}
              disabled={uploading}
              className="w-full flex items-center justify-center gap-2 py-3 rounded-lg border-2 border-dashed border-border-subtle text-text-muted hover:border-vibe-purple hover:text-vibe-purple transition-colors disabled:opacity-50"
            >
              {uploading ? (
                <>
                  <Loader2 size={18} className="animate-spin" />
                  <span className="text-sm">Subiendo imagen...</span>
                </>
              ) : (
                <>
                  <ImagePlus size={18} />
                  <span className="text-sm">
                    {avatarUrl ? 'Cambiar foto' : 'Elegir foto'}
                  </span>
                </>
              )}
            </button>
          ) : (
            <p className="text-sm text-text-secondary truncate">
              {user.avatarUrl ? 'Foto configurada' : 'Sin foto de perfil'}
            </p>
          )}
        </div>

        {/* Bio */}
        <div className="bg-bg-card border border-border-subtle rounded-xl p-4">
          <label className="block text-xs font-semibold text-text-muted uppercase tracking-wider mb-2">
            Bio
          </label>
          {editing ? (
            <textarea
              value={bio}
              onChange={e => setBio(e.target.value)}
              placeholder="Cuenta algo sobre ti, tus pelis favoritas, series, juegos..."
              rows={3}
              maxLength={300}
              className="w-full bg-bg-input border border-border-subtle rounded-lg px-3 py-2.5 text-sm text-text-primary placeholder-text-muted focus:outline-none focus:border-vibe-purple transition-colors resize-none"
            />
          ) : (
            <p className="text-sm text-text-secondary">
              {user.bio || 'Sin bio. Toca "Editar perfil" para agregar una.'}
            </p>
          )}
          {editing && (
            <p className="text-[10px] text-text-muted mt-1 text-right">{bio.length}/300</p>
          )}
        </div>

        {/* Gender */}
        <div className="bg-bg-card border border-border-subtle rounded-xl p-4">
          <label className="block text-xs font-semibold text-text-muted uppercase tracking-wider mb-2">
            Genero
          </label>
          {editing ? (
            <div className="flex gap-3">
              <button
                type="button"
                onClick={() => setGender(0)}
                className={cn(
                  'flex-1 py-2.5 rounded-lg text-sm font-semibold transition-all border',
                  gender === 0
                    ? 'bg-vibe-purple/20 border-vibe-purple text-vibe-purple'
                    : 'bg-bg-input border-border-subtle text-text-muted hover:border-text-muted',
                )}
              >
                Hombre
              </button>
              <button
                type="button"
                onClick={() => setGender(1)}
                className={cn(
                  'flex-1 py-2.5 rounded-lg text-sm font-semibold transition-all border',
                  gender === 1
                    ? 'bg-vibe-cyan/20 border-vibe-cyan text-vibe-cyan'
                    : 'bg-bg-input border-border-subtle text-text-muted hover:border-text-muted',
                )}
              >
                Mujer
              </button>
            </div>
          ) : (
            <p className="text-sm text-text-secondary">
              {user.gender === 0 ? 'Hombre' : 'Mujer'}
            </p>
          )}
        </div>

        {/* Date of birth */}
        <div className="bg-bg-card border border-border-subtle rounded-xl p-4">
          <label className="block text-xs font-semibold text-text-muted uppercase tracking-wider mb-2">
            <Calendar size={12} className="inline mr-1" />
            Fecha de nacimiento
          </label>
          {editing ? (
            <input
              type="date"
              value={dateOfBirth}
              onChange={e => setDateOfBirth(e.target.value)}
              max={new Date().toISOString().split('T')[0]}
              min="1950-01-01"
              className="w-full bg-bg-input border border-border-subtle rounded-lg px-3 py-2.5 text-sm text-text-primary focus:outline-none focus:border-vibe-purple transition-colors [color-scheme:dark]"
            />
          ) : (
            <p className="text-sm text-text-secondary">
              {user.dateOfBirth && user.dateOfBirth !== '0001-01-01T00:00:00'
                ? new Date(user.dateOfBirth).toLocaleDateString('es-ES', {
                    day: 'numeric', month: 'long', year: 'numeric'
                  })
                : 'No configurada'}
            </p>
          )}
        </div>
      </div>

      {/* ===== Premium upsell ===== */}
      {!user.isPremium && (
        <div className="bg-gradient-to-r from-vibe-red/10 via-vibe-purple/10 to-vibe-cyan/10 border border-vibe-purple/20 rounded-xl p-5 mb-6">
          <div className="flex items-center gap-2 mb-2">
            <Crown size={20} className="text-match-gold" />
            <h3 className="font-bold text-text-primary">VibeLink Premium</h3>
          </div>
          <ul className="text-sm text-text-secondary space-y-1.5 mb-4">
            <li className="flex items-center gap-2">
              <span className="w-1.5 h-1.5 rounded-full bg-vibe-cyan" />
              Swipes ilimitados
            </li>
            <li className="flex items-center gap-2">
              <span className="w-1.5 h-1.5 rounded-full bg-vibe-purple" />
              Ve quien te dio like
            </li>
            <li className="flex items-center gap-2">
              <span className="w-1.5 h-1.5 rounded-full bg-vibe-red" />
              Acceso prioritario a matches
            </li>
          </ul>
          <Button
            size="md"
            className="w-full"
            onClick={async () => {
              if (!userId) return;
              try {
                const { url } = await api.createCheckout(userId);
                window.location.href = url;
              } catch (err) {
                console.error(err);
              }
            }}
          >
            <Crown size={16} /> Hazte Premium - 9.99 EUR
          </Button>
        </div>
      )}

      {/* ===== Logout ===== */}
      <Button variant="danger" className="w-full" onClick={logout}>
        <LogOut size={16} /> Cerrar sesion
      </Button>
    </div>
  );
}
