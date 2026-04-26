import type {
  User, ContentItem, MatchCandidate, MatchInfo, Message,
  RegisterRequest, LoginRequest, ExternalLikeRequest,
  PeopleSwipeRequest, SendMessageRequest, ProfileUpdateRequest,
} from '../types';

const BASE_URL = import.meta.env.VITE_API_URL || '';

class ApiClient {
  private token: string | null = null;

  setToken(token: string | null) {
    this.token = token;
    if (token) localStorage.setItem('vibelink_token', token);
    else localStorage.removeItem('vibelink_token');
  }

  getToken() {
    if (!this.token) this.token = localStorage.getItem('vibelink_token');
    return this.token;
  }

  private async request<T>(path: string, options: RequestInit = {}): Promise<T> {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...options.headers as Record<string, string>,
    };

    const token = this.getToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;

    const res = await fetch(`${BASE_URL}${path}`, { ...options, headers });

    if (res.status === 401) {
      this.setToken(null);
      window.location.href = '/login';
      throw new Error('Unauthorized');
    }

    if (!res.ok) {
      const text = await res.text();
      throw new Error(text || `Error ${res.status}`);
    }

    const contentType = res.headers.get('content-type');
    if (contentType?.includes('application/json')) return res.json();
    const text = await res.text();
    return text as unknown as T;
  }

  // Auth
  async register(data: RegisterRequest): Promise<User> {
    return this.request('/api/auth/register', { method: 'POST', body: JSON.stringify(data) });
  }

  async login(data: LoginRequest): Promise<string> {
    return this.request('/api/auth/login', { method: 'POST', body: JSON.stringify(data) });
  }

  // Users
  async getUser(id: number): Promise<User> {
    return this.request(`/api/users/${id}`);
  }

  async updateProfile(id: number, data: ProfileUpdateRequest): Promise<User> {
    return this.request(`/api/users/${id}`, { method: 'PUT', body: JSON.stringify(data) });
  }

  // Discover
  async discoverMovies(page = 1): Promise<ContentItem[]> {
    return this.request(`/api/discover/movies?page=${page}`);
  }

  async discoverSeries(page = 1): Promise<ContentItem[]> {
    return this.request(`/api/discover/series?page=${page}`);
  }

  async discoverGames(page = 1): Promise<ContentItem[]> {
    return this.request(`/api/discover/games?page=${page}`);
  }

  async searchContent(q: string, page = 1): Promise<ContentItem[]> {
    return this.request(`/api/discover/search?q=${encodeURIComponent(q)}&page=${page}`);
  }

  // Content Likes (Onboarding + content swipe)
  async likeExternal(data: ExternalLikeRequest): Promise<{ message: string; contentId: number }> {
    return this.request('/api/userlikes/external', { method: 'POST', body: JSON.stringify(data) });
  }

  // User Likes count (for onboarding check)
  async getUserLikesCount(userId: number): Promise<{ count: number }> {
    return this.request(`/api/userlikes/count/${userId}`);
  }

  // People Swipe
  async swipePerson(data: PeopleSwipeRequest): Promise<unknown> {
    return this.request('/api/swipe', { method: 'POST', body: JSON.stringify(data) });
  }

  // Matching
  async getCompatiblePeople(userId: number): Promise<MatchCandidate[]> {
    return this.request(`/api/matching/${userId}`);
  }

  async getMyMatches(userId: number): Promise<MatchInfo[]> {
    return this.request(`/api/matching/mymatch/${userId}`);
  }

  // Chat
  async getMessages(userId: number, matchingUserId: number): Promise<Message[]> {
    return this.request(`/api/chat?userId=${userId}&matchingUserId=${matchingUserId}`);
  }

  async sendMessage(data: SendMessageRequest): Promise<Message> {
    return this.request('/api/chat', { method: 'POST', body: JSON.stringify(data) });
  }

  // Payment
  async createCheckout(userId: number): Promise<{ url: string }> {
    return this.request(`/api/payment/create-checkout/${userId}`, { method: 'POST' });
  }
}

export const api = new ApiClient();
