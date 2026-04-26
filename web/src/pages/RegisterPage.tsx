import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { Gamepad2, Film, Tv } from 'lucide-react';

export function RegisterPage() {
  const navigate = useNavigate();
  const register = useAuthStore(s => s.register);
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    if (password.length < 6) {
      setError('La password debe tener al menos 6 caracteres');
      return;
    }
    setLoading(true);
    try {
      await register(username, email, password);
      navigate('/onboarding');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al registrarse');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="min-h-dvh flex flex-col items-center justify-center px-6 bg-bg-primary relative overflow-hidden">
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute -top-40 -left-40 w-96 h-96 bg-vibe-cyan/5 rounded-full blur-3xl" />
        <div className="absolute -bottom-40 -right-40 w-96 h-96 bg-vibe-red/5 rounded-full blur-3xl" />
      </div>

      <div className="w-full max-w-sm relative z-10 animate-fade-in">
        <div className="text-center mb-10">
          <div className="flex items-center justify-center gap-3 mb-4">
            <Film className="text-vibe-red" size={28} />
            <Tv className="text-vibe-purple" size={28} />
            <Gamepad2 className="text-vibe-cyan" size={28} />
          </div>
          <h1 className="text-4xl font-black gradient-text mb-2">Unete</h1>
          <p className="text-text-secondary text-sm">Descubre personas con tu mismo gusto</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            label="Username"
            placeholder="tuNombre"
            value={username}
            onChange={e => setUsername(e.target.value)}
            required
          />
          <Input
            label="Email"
            type="email"
            placeholder="tu@email.com"
            value={email}
            onChange={e => setEmail(e.target.value)}
            required
          />
          <Input
            label="Password"
            type="password"
            placeholder="Min 6 caracteres"
            value={password}
            onChange={e => setPassword(e.target.value)}
            required
            minLength={6}
          />

          {error && (
            <div className="bg-dislike-red/10 border border-dislike-red/20 rounded-xl px-4 py-3 text-sm text-dislike-red">
              {error}
            </div>
          )}

          <Button type="submit" loading={loading} className="w-full" size="lg">
            Crear cuenta
          </Button>
        </form>

        <p className="text-center text-sm text-text-secondary mt-6">
          Ya tienes cuenta?{' '}
          <Link to="/login" className="text-vibe-purple font-semibold hover:text-vibe-cyan transition-colors">
            Inicia sesion
          </Link>
        </p>
      </div>
    </div>
  );
}
