import { useEffect } from 'react';
import { Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from './stores/authStore';
import { AppLayout } from './components/layout/AppLayout';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { OnboardingPage } from './pages/OnboardingPage';
import { DiscoverPage } from './pages/DiscoverPage';
import { MatchesPage } from './pages/MatchesPage';
import { ChatListPage } from './pages/ChatListPage';
import { ChatRoomPage } from './pages/ChatRoomPage';
import { ProfilePage } from './pages/ProfilePage';
import { ExplorePage } from './pages/ExplorePage';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading, hasCompletedOnboarding } = useAuthStore();
  const location = useLocation();

  if (isLoading) {
    return (
      <div className="min-h-dvh flex items-center justify-center bg-bg-primary">
        <div className="flex flex-col items-center gap-4">
          <div className="w-12 h-12 border-4 border-vibe-purple/30 border-t-vibe-purple rounded-full animate-spin" />
          <p className="text-sm text-text-muted">Cargando...</p>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) return <Navigate to="/login" replace />;

  // Redirect to onboarding if not completed (except if already there)
  if (!hasCompletedOnboarding && location.pathname !== '/onboarding') {
    return <Navigate to="/onboarding" replace />;
  }

  return <>{children}</>;
}

function PublicRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading, hasCompletedOnboarding } = useAuthStore();

  if (isLoading) {
    return (
      <div className="min-h-dvh flex items-center justify-center bg-bg-primary">
        <div className="w-12 h-12 border-4 border-vibe-purple/30 border-t-vibe-purple rounded-full animate-spin" />
      </div>
    );
  }

  if (isAuthenticated) {
    return <Navigate to={hasCompletedOnboarding ? '/discover' : '/onboarding'} replace />;
  }

  return <>{children}</>;
}

export default function App() {
  const initialize = useAuthStore(s => s.initialize);

  useEffect(() => {
    initialize();
  }, [initialize]);

  return (
    <Routes>
      {/* Public routes */}
      <Route path="/login" element={<PublicRoute><LoginPage /></PublicRoute>} />
      <Route path="/register" element={<PublicRoute><RegisterPage /></PublicRoute>} />

      {/* Onboarding */}
      <Route path="/onboarding" element={
        <ProtectedRoute><OnboardingPage /></ProtectedRoute>
      } />

      {/* Main app with bottom nav */}
      <Route element={<ProtectedRoute><AppLayout /></ProtectedRoute>}>
        <Route path="/discover" element={<DiscoverPage />} />
        <Route path="/explore" element={<ExplorePage />} />
        <Route path="/matches" element={<MatchesPage />} />
        <Route path="/chat" element={<ChatListPage />} />
        <Route path="/chat/:matchUserId" element={<ChatRoomPage />} />
        <Route path="/profile" element={<ProfilePage />} />
      </Route>

      {/* Default redirect */}
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}
