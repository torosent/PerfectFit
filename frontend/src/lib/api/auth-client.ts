import type { UserProfile } from '@/types';
import { API_BASE_URL } from './constants';

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
export type OAuthProvider = 'microsoft';

/**
 * Response from login endpoint
 */
export interface LoginResponse {
  success: boolean;
  token?: string;
  user?: UserProfile;
  error?: string;
  lockoutEnd?: string;
}

/**
 * Response from registration endpoint
 */
export interface RegisterResponse {
  success: boolean;
  message?: string;
  error?: string;
}

/**
 * Response from email verification endpoint
 */
export interface VerifyEmailResponse {
  success: boolean;
  error?: string;
}

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
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ token }),
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

/**
 * Register a new user with email and password
 * @param email - User's email address
 * @param password - User's password
 * @param displayName - User's display name
 * @param avatar - Optional emoji avatar
 * @returns Registration result
 */
export async function register(
  email: string,
  password: string,
  displayName: string,
  avatar?: string
): Promise<RegisterResponse> {
  const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ email, password, displayName, avatar }),
  });

  const data = await response.json();
  
  if (!response.ok) {
    return {
      success: false,
      error: data.errorMessage || data.error || data.message || 'Registration failed',
    };
  }

  return {
    success: true,
    message: data.message || 'Registration successful. Please check your email to verify your account.',
  };
}

/**
 * Login with email and password
 * @param email - User's email address
 * @param password - User's password
 * @returns Login result with token and user profile
 */
export async function login(
  email: string,
  password: string
): Promise<LoginResponse> {
  const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ email, password }),
  });

  const data = await response.json();
  
  if (!response.ok) {
    return {
      success: false,
      error: data.errorMessage || data.error || data.message || 'Login failed',
      lockoutEnd: data.lockoutEnd,
    };
  }

  return {
    success: true,
    token: data.token,
    user: data.user,
  };
}

/**
 * Verify email address with verification token
 * @param email - User's email address
 * @param token - Email verification token
 * @returns Verification result
 */
export async function verifyEmail(
  email: string,
  token: string
): Promise<VerifyEmailResponse> {
  const response = await fetch(`${API_BASE_URL}/api/auth/verify-email`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ email, token }),
  });

  const data = await response.json();
  
  if (!response.ok) {
    return {
      success: false,
      error: data.errorMessage || data.error || data.message || 'Email verification failed',
    };
  }

  return {
    success: true,
  };
}
