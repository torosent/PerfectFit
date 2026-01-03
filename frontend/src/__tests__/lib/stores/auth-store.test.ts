/**
 * Auth Store Tests
 * 
 * Tests for local authentication methods in the auth store
 */

import { act } from '@testing-library/react';
import type { LoginResponse, RegisterResponse } from '@/lib/api/auth-client';

// Create mock functions
const mockLogin = jest.fn<Promise<LoginResponse>, [string, string]>();
const mockRegister = jest.fn<Promise<RegisterResponse>, [string, string, string]>();
const mockGetCurrentUser = jest.fn();
const mockLogout = jest.fn();
const mockCreateGuestSession = jest.fn();

// Mock the auth client module
jest.mock('@/lib/api/auth-client', () => ({
  login: (...args: [string, string]) => mockLogin(...args),
  register: (...args: [string, string, string]) => mockRegister(...args),
  getCurrentUser: (...args: unknown[]) => mockGetCurrentUser(...args),
  logout: (...args: unknown[]) => mockLogout(...args),
  createGuestSession: () => mockCreateGuestSession(),
  AuthError: class AuthError extends Error {
    constructor(message: string, public statusCode?: number, public code?: string) {
      super(message);
      this.name = 'AuthError';
    }
  },
  getOAuthUrl: jest.fn(),
  refreshToken: jest.fn(),
  getAuthHeaders: jest.fn(),
}));

// Import store after mocking
import { useAuthStore } from '@/lib/stores/auth-store';

describe('Auth Store - Local Auth Methods', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // Reset store state
    act(() => {
      useAuthStore.setState({
        user: null,
        token: null,
        isAuthenticated: false,
        isLoading: false,
        isInitialized: false,
        error: null,
      });
    });
  });

  describe('localLogin', () => {
    it('calls API and stores token on success', async () => {
      const mockUser = {
        id: 'user-123',
        displayName: 'Test User',
        email: 'test@example.com',
        provider: 'local' as const,
        highScore: 0,
        gamesPlayed: 0,
      };
      const mockToken = 'jwt-token-123';

      mockLogin.mockResolvedValueOnce({
        success: true,
        token: mockToken,
        user: mockUser,
      });

      await act(async () => {
        await useAuthStore.getState().localLogin('test@example.com', 'password123');
      });

      expect(mockLogin).toHaveBeenCalledWith('test@example.com', 'password123');
      expect(useAuthStore.getState().token).toBe(mockToken);
      expect(useAuthStore.getState().user).toEqual(mockUser);
      expect(useAuthStore.getState().isAuthenticated).toBe(true);
      expect(useAuthStore.getState().error).toBeNull();
    });

    it('throws error on failure', async () => {
      mockLogin.mockResolvedValueOnce({
        success: false,
        error: 'Invalid email or password',
      });

      await expect(
        act(async () => {
          await useAuthStore.getState().localLogin('test@example.com', 'wrongpassword');
        })
      ).rejects.toThrow('Invalid email or password');

      expect(useAuthStore.getState().token).toBeNull();
      expect(useAuthStore.getState().isAuthenticated).toBe(false);
      expect(useAuthStore.getState().error).toBe('Invalid email or password');
    });

    it('handles lockout response', async () => {
      const lockoutEnd = new Date(Date.now() + 15 * 60 * 1000).toISOString();
      mockLogin.mockResolvedValueOnce({
        success: false,
        error: 'Account is locked',
        lockoutEnd,
      });

      await expect(
        act(async () => {
          await useAuthStore.getState().localLogin('test@example.com', 'wrongpassword');
        })
      ).rejects.toThrow('Account is locked');

      expect(useAuthStore.getState().error).toBe('Account is locked');
    });

    it('sets loading state during login', async () => {
      let resolveLogin: (value: LoginResponse) => void;
      const pendingPromise = new Promise<LoginResponse>((resolve) => {
        resolveLogin = resolve;
      });
      mockLogin.mockReturnValueOnce(pendingPromise);

      const loginPromise = act(async () => {
        await useAuthStore.getState().localLogin('test@example.com', 'password123');
      });

      // Should be loading during the request
      expect(useAuthStore.getState().isLoading).toBe(true);

      // Resolve the promise
      resolveLogin!({
        success: true,
        token: 'token',
        user: {
          id: 'user-123',
          displayName: 'Test User',
          email: 'test@example.com',
          provider: 'local',
          highScore: 0,
          gamesPlayed: 0,
        },
      });

      await loginPromise;

      // Should not be loading after completion
      expect(useAuthStore.getState().isLoading).toBe(false);
    });
  });

  describe('localRegister', () => {
    it('calls API correctly', async () => {
      mockRegister.mockResolvedValueOnce({
        success: true,
        message: 'Registration successful. Please check your email to verify your account.',
      });

      await act(async () => {
        const result = await useAuthStore.getState().localRegister(
          'newuser@example.com',
          'securePass123!',
          'New User'
        );
        expect(result.success).toBe(true);
        expect(result.message).toContain('Registration successful');
      });

      expect(mockRegister).toHaveBeenCalledWith(
        'newuser@example.com',
        'securePass123!',
        'New User',
        undefined
      );
    });

    it('returns error on registration failure', async () => {
      mockRegister.mockResolvedValueOnce({
        success: false,
        error: 'Email already registered',
      });

      await act(async () => {
        const result = await useAuthStore.getState().localRegister(
          'existing@example.com',
          'password123',
          'Existing User'
        );
        expect(result.success).toBe(false);
        expect(result.error).toBe('Email already registered');
      });
    });

    it('returns error for weak password', async () => {
      mockRegister.mockResolvedValueOnce({
        success: false,
        error: 'Password does not meet requirements',
      });

      await act(async () => {
        const result = await useAuthStore.getState().localRegister(
          'user@example.com',
          'weak',
          'User'
        );
        expect(result.success).toBe(false);
        expect(result.error).toBe('Password does not meet requirements');
      });
    });
  });
});
