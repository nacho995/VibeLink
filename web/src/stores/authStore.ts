import { create } from 'zustand';
import type { User } from '../types';
import { api } from '../lib/api';

const MIN_LIKES_FOR_ONBOARDING = 9; // 3 pelis + 3 series + 3 juegos

function parseJwt(token: string): Record<string, string> {
  const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
  return JSON.parse(atob(base64));
}

interface AuthState {
  user: User | null;
  userId: number | null;
  token: string | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  hasCompletedOnboarding: boolean;

  login: (email: string, password: string) => Promise<void>;
  register: (username: string, email: string, password: string) => Promise<void>;
  logout: () => void;
  loadUser: () => Promise<void>;
  setOnboardingComplete: () => void;
  checkOnboarding: (uid: number) => Promise<boolean>;
  initialize: () => Promise<void>;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  userId: null,
  token: null,
  isLoading: true,
  isAuthenticated: false,
  hasCompletedOnboarding: false,

  checkOnboarding: async (uid: number) => {
    try {
      const { count } = await api.getUserLikesCount(uid);
      const completed = count >= MIN_LIKES_FOR_ONBOARDING;
      if (completed) {
        localStorage.setItem('vibelink_onboarded', 'true');
      }
      return completed;
    } catch {
      // Fallback to localStorage if request fails
      return localStorage.getItem('vibelink_onboarded') === 'true';
    }
  },

  initialize: async () => {
    const token = localStorage.getItem('vibelink_token');
    if (!token) {
      set({ isLoading: false });
      return;
    }

    try {
      api.setToken(token);
      const claims = parseJwt(token);
      const exp = parseInt(claims.exp) * 1000;
      if (Date.now() > exp) throw new Error('Token expired');

      const userId = parseInt(claims.nameid);
      const user = await api.getUser(userId);

      // Check onboarding status from backend
      const onboarded = await get().checkOnboarding(userId);

      set({
        token, userId, user,
        isAuthenticated: true,
        hasCompletedOnboarding: onboarded,
        isLoading: false,
      });
    } catch {
      localStorage.removeItem('vibelink_token');
      api.setToken(null);
      set({ isLoading: false });
    }
  },

  login: async (email, password) => {
    const token = await api.login({ email, password });
    api.setToken(token);
    const claims = parseJwt(token);
    const userId = parseInt(claims.nameid);
    const user = await api.getUser(userId);

    // Check onboarding from backend
    const onboarded = await get().checkOnboarding(userId);

    set({
      token, userId, user,
      isAuthenticated: true,
      hasCompletedOnboarding: onboarded,
    });
  },

  register: async (username, email, password) => {
    await api.register({ username, email, password, confirmPassword: password });
    await get().login(email, password);
  },

  logout: () => {
    api.setToken(null);
    localStorage.removeItem('vibelink_onboarded');
    set({
      user: null, userId: null, token: null,
      isAuthenticated: false, hasCompletedOnboarding: false,
    });
  },

  loadUser: async () => {
    const { userId } = get();
    if (!userId) return;
    const user = await api.getUser(userId);
    set({ user });
  },

  setOnboardingComplete: () => {
    localStorage.setItem('vibelink_onboarded', 'true');
    set({ hasCompletedOnboarding: true });
  },
}));
