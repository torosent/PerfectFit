import type { UserProfile } from '@/types';
import { API_BASE_URL } from './index';

/**
 * Custom error class for authentication errors
 */
export class AuthError extends Error {
  constructor(
    message: string,
    public statusCode?: number,
    public code?: string
  ) {
    super(message);
    this.name = 'AuthError';
  }
}

/**
 * Supported OAuth providers
 */
export type OAuthProvider = 'google' | 'microsoft' | 'facebook';

/**
 * Get the OAuth authorization URL for a provider
 * @param provider - The OAuth provider to authenticate with
 * @returns The URL to redirect the user to
 */
export function getOAuthUrl(provider: OAuthProvider): string {
  const callbackUrl = typeof window !== 'undefined' 
    ? `${window.location.origin}/callback`
    : '';
  
  return `${API_BASE_URL}/api/auth/${provider}?redirect=${encodeURIComponent(callbackUrl)}`;
}

/**
 * Get the current authenticated user's profile
 * @param token - JWT authentication token
 * @returns The user's profile
 */
export async function getCurrentUser(token: string): Promise<UserProfile> {
  const response = await fetch(`${API_BASE_URL}/api/auth/me`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new AuthError('Session expired. Please login again.', 401, 'TOKEN_EXPIRED');
    }
    throw new AuthError('Failed to get user profile', response.status);
  }

  return response.json();
}

/**
 * Refresh the authentication token
 * @param token - Current JWT token
 * @returns New token
 */
export async function refreshToken(token: string): Promise<{ token: string }> {
  const response = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new AuthError('Session expired. Please login again.', 401, 'TOKEN_EXPIRED');
    }
    throw new AuthError('Failed to refresh token', response.status);
  }

  return response.json();
}

/**
 * Create a guest session for anonymous play
 * @returns Token and guest user profile
 */
export async function createGuestSession(): Promise<{ token: string; user: UserProfile }> {
  const response = await fetch(`${API_BASE_URL}/api/auth/guest`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
  });

  if (!response.ok) {
    throw new AuthError('Failed to create guest session', response.status);
  }

  return response.json();
}

/**
 * Logout the current user (invalidate session on backend)
 * @param token - JWT authentication token
 */
export async function logout(token: string): Promise<void> {
  try {
    await fetch(`${API_BASE_URL}/api/auth/logout`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
  } catch {
    // Ignore errors on logout - client will clear local state anyway
  }
}

/**
 * Helper to create authorization headers
 * @param token - JWT token (optional)
 * @returns Headers object with Authorization if token provided
 */
export function getAuthHeaders(token?: string | null): HeadersInit {
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
  };
  
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  
  return headers;
}
