import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { UserProfile } from '@/types';
import * as authClient from '@/lib/api/auth-client';
import type { RegisterResponse } from '@/lib/api/auth-client';

/**
 * Auth store state interface
 */
export interface AuthState {
  user: UserProfile | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isInitialized: boolean;
  error: string | null;
  lockoutEnd: string | null;
}

/**
 * Auth store actions interface
 */
export interface AuthActions {
  login: (token: string, user: UserProfile) => void;
  logout: () => Promise<void>;
  setUser: (user: UserProfile) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  initializeAuth: () => Promise<void>;
  loginAsGuest: () => Promise<void>;
  clearError: () => void;
  localLogin: (email: string, password: string) => Promise<void>;
  localRegister: (email: string, password: string, displayName: string, avatar?: string) => Promise<RegisterResponse>;
}

/**
 * Combined auth store interface
 */
export type AuthStore = AuthState & AuthActions;

/**
 * Initial auth state
 */
const initialState: AuthState = {
  user: null,
  token: null,
  isAuthenticated: false,
  isLoading: false,
  isInitialized: false,
  error: null,
  lockoutEnd: null,
};

/**
 * Zustand store for authentication state management
 * Uses persist middleware to store auth state in localStorage
 */
export const useAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      ...initialState,

      /**
       * Login with token and user profile
       */
      login: (token: string, user: UserProfile) => {
        set({
          token,
          user,
          isAuthenticated: true,
          isLoading: false,
          error: null,
        });
      },

      /**
       * Logout and clear auth state
       */
      logout: async () => {
        const { token } = get();
        
        // Clear state immediately for better UX
        set({
          ...initialState,
          isInitialized: true,
        });

        // Notify backend (fire and forget)
        if (token) {
          authClient.logout(token);
        }
      },

      /**
       * Update the current user profile
       */
      setUser: (user: UserProfile) => {
        set({ user });
      },

      /**
       * Set loading state
       */
      setLoading: (loading: boolean) => {
        set({ isLoading: loading });
      },

      /**
       * Set error message
       */
      setError: (error: string | null) => {
        set({ error, isLoading: false });
      },

      /**
       * Clear error message
       */
      clearError: () => {
        set({ error: null });
      },

      /**
       * Initialize auth state on app load
       * Validates stored token and fetches current user
       */
      initializeAuth: async () => {
        const { token, isInitialized } = get();
        
        // Skip if already initialized
        if (isInitialized) return;

        set({ isLoading: true });

        if (!token) {
          set({ isLoading: false, isInitialized: true });
          return;
        }

        try {
          // Validate token by fetching current user
          const user = await authClient.getCurrentUser(token);
          set({
            user,
            isAuthenticated: true,
            isLoading: false,
            isInitialized: true,
            error: null,
          });
        } catch (err) {
          // Token is invalid, clear auth state
          console.warn('Auth initialization failed:', err);
          set({
            ...initialState,
            isInitialized: true,
          });
        }
      },

      /**
       * Login as a guest user
       */
      loginAsGuest: async () => {
        set({ isLoading: true, error: null });

        try {
          const { token, user } = await authClient.createGuestSession();
          set({
            token,
            user,
            isAuthenticated: true,
            isLoading: false,
            error: null,
          });
        } catch (err) {
          const message = err instanceof Error ? err.message : 'Failed to create guest session';
          set({
            error: message,
            isLoading: false,
          });
          throw err;
        }
      },

      /**
       * Login with email and password
       */
      localLogin: async (email: string, password: string) => {
        set({ isLoading: true, error: null, lockoutEnd: null });

        try {
          const response = await authClient.login(email, password);
          
          if (!response.success) {
            set({
              error: response.error || 'Login failed',
              lockoutEnd: response.lockoutEnd || null,
              isLoading: false,
            });
            throw new Error(response.error || 'Login failed');
          }

          if (response.token && response.user) {
            set({
              token: response.token,
              user: response.user,
              isAuthenticated: true,
              isLoading: false,
              error: null,
              lockoutEnd: null,
            });
          }
        } catch (err) {
          const message = err instanceof Error ? err.message : 'Login failed';
          // Only set error if not already set by the response handling
          const currentState = get();
          if (!currentState.error) {
            set({
              error: message,
              isLoading: false,
            });
          }
          throw err;
        }
      },

      /**
       * Register a new user with email and password
       */
      localRegister: async (email: string, password: string, displayName: string, avatar?: string): Promise<RegisterResponse> => {
        set({ isLoading: true, error: null });

        try {
          const response = await authClient.register(email, password, displayName, avatar);
          set({ isLoading: false });
          return response;
        } catch (err) {
          const message = err instanceof Error ? err.message : 'Registration failed';
          set({
            error: message,
            isLoading: false,
          });
          return {
            success: false,
            error: message,
          };
        }
      },
    }),
    {
      name: 'perfectfit-auth',
      storage: createJSONStorage(() => localStorage),
      // Only persist these fields
      partialize: (state) => ({
        token: state.token,
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);

/**
 * Selector hooks for specific parts of auth state
 */
export const useUser = () => useAuthStore((state) => state.user);
export const useToken = () => useAuthStore((state) => state.token);
export const useIsAuthenticated = () => useAuthStore((state) => state.isAuthenticated);
export const useIsAuthLoading = () => useAuthStore((state) => state.isLoading);
export const useIsAuthInitialized = () => useAuthStore((state) => state.isInitialized);
export const useAuthError = () => useAuthStore((state) => state.error);
export const useLockoutEnd = () => useAuthStore((state) => state.lockoutEnd);
export const useIsGuest = () => useAuthStore((state) => state.user?.provider === 'guest');
export const useIsAdmin = () => useAuthStore((state) => state.user?.role === 'Admin');
