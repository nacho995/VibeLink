// === Enums as const objects (TypeScript erasableSyntaxOnly compatible) ===
export const Gender = { Male: 0, Female: 1 } as const;
export const ContentType = { Movie: 0, Series: 1, Game: 2 } as const;
export const SwipeState = { Like: 0, Dislike: 1 } as const;
export const LikeState = { Liked: 0, Disliked: 1 } as const;

// === User ===
export interface User {
  id: number;
  username: string;
  email: string;
  avatarUrl: string | null;
  codigoInvitacion: string | null;
  fechaRegistro: string;
  gender: number;
  dateOfBirth: string;
  bio: string | null;
  isPremium: boolean;
  swipes: number;
}

// === Content (from Discover API) ===
export interface ContentItem {
  externalId: string;
  title: string;
  type: number;
  imageUrl: string | null;
  backdropUrl: string | null;
  description: string;
  rating: number;
  year: number | null;
  genres: string[];
  platforms: string[] | null;
}

// === Matching ===
export interface MatchCandidate {
  usuario: {
    id: number;
    username: string;
    avatarUrl: string | null;
    bio: string | null;
    gender: number;
    dateOfBirth: string;
  };
  compatibilidad: number;
}

export interface MatchInfo {
  matchId: number;
  matchDate: string;
  anotherUser: {
    id: number;
    username: string;
    avatarUrl: string | null;
    bio: string | null;
  };
}

// === Chat ===
export interface Message {
  id: number;
  userId: number;
  matchingUserId: number;
  message: string;
  date: string;
}

// === Request DTOs ===
export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface ExternalLikeRequest {
  externalId: string;
  title: string;
  imageUrl?: string | null;
  state: number;
}

export interface PeopleSwipeRequest {
  userId: number;
  matchingUserId: number;
  state: number;
}

export interface SendMessageRequest {
  userId: number;
  matchingUserId: number;
  message: string;
}

export interface ProfileUpdateRequest {
  avatarUrl?: string | null;
  gender: number;
  dateOfBirth: string;
  bio?: string | null;
}
