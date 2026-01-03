'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { LoginButton, GuestButton } from '@/components/auth/LoginButton';
import {
  useAuthStore,
  useIsAuthenticated,
  useIsAuthLoading,
  useAuthError,
} from '@/lib/stores/auth-store';

/**
 * Login page with OAuth and guest options
 * Redirects to /play if already authenticated
 */
export default function LoginPage() {
  const router = useRouter();
  const isAuthenticated = useIsAuthenticated();
  const isLoading = useIsAuthLoading();
  const error = useAuthError();
  const loginAsGuest = useAuthStore((state) => state.loginAsGuest);
  const clearError = useAuthStore((state) => state.clearError);
  const initializeAuth = useAuthStore((state) => state.initializeAuth);

  // Initialize auth and redirect if already authenticated
  useEffect(() => {
    initializeAuth();
  }, [initializeAuth]);

  useEffect(() => {
    if (isAuthenticated) {
      router.push('/play');
    }
  }, [isAuthenticated, router]);

  const handleGuestLogin = async () => {
    try {
      await loginAsGuest();
      router.push('/play');
    } catch {
      // Error is handled by the store
    }
  };

  return (
    <div className="min-h-screen game-background flex items-center justify-center p-4">
      {/* Animated background elements */}
      <div className="fixed inset-0 overflow-hidden pointer-events-none" aria-hidden="true">
        <div className="absolute top-1/4 left-1/4 w-64 h-64 rounded-full blur-3xl animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.1)' }} />
        <div className="absolute bottom-1/4 right-1/4 w-96 h-96 rounded-full blur-3xl animate-pulse" style={{ backgroundColor: 'rgba(14, 165, 233, 0.1)', animationDelay: '1s' }} />
        <div className="absolute top-1/2 left-1/2 w-48 h-48 rounded-full blur-3xl animate-pulse" style={{ backgroundColor: 'rgba(34, 211, 238, 0.05)', animationDelay: '2s' }} />
      </div>

      {/* Login card */}
      <div className="relative z-10 w-full max-w-md">
        <div className="glass-panel rounded-2xl p-8 shadow-2xl">
          {/* Logo/Title */}
          <div className="text-center mb-8">
            <h1 className="text-4xl font-bold text-white mb-2">
              Perfect<span style={{ color: '#2dd4bf' }}>Fit</span>
            </h1>
            <p className="text-gray-400">
              A block puzzle game
            </p>
          </div>

          {/* Error display */}
          {error && (
            <div
              className="mb-6 p-4 bg-red-900/50 border border-red-500 rounded-lg text-red-200 text-sm"
              role="alert"
            >
              <div className="flex items-start gap-2">
                <svg
                  className="w-5 h-5 flex-shrink-0 mt-0.5"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                  aria-hidden="true"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
                <div>
                  <p>{error}</p>
                  <button
                    onClick={clearError}
                    className="mt-1 text-xs underline hover:no-underline"
                  >
                    Dismiss
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* OAuth buttons */}
          <div className="space-y-3">
            <LoginButton provider="google" disabled={isLoading} />
            <LoginButton provider="microsoft" disabled={isLoading} />
            <LoginButton provider="facebook" disabled={isLoading} />
          </div>

          {/* Divider */}
          <div className="relative my-6">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-600" />
            </div>
            <div className="relative flex justify-center text-sm">
              <span className="px-3 text-gray-400" style={{ backgroundColor: '#0d243d' }}>or</span>
            </div>
          </div>

          {/* Guest button */}
          <GuestButton
            onClick={handleGuestLogin}
            disabled={isLoading}
            isLoading={isLoading}
          />

          {/* Footer text */}
          <p className="mt-6 text-center text-xs text-gray-500">
            By continuing, you agree to our terms of service and privacy policy.
          </p>
        </div>

        {/* Features preview */}
        <div className="mt-8 grid grid-cols-3 gap-4 text-center">
          <div className="glass-panel rounded-lg p-4">
            <div className="text-2xl mb-1">🎮</div>
            <p className="text-xs text-gray-400">Endless puzzle fun</p>
          </div>
          <div className="glass-panel rounded-lg p-4">
            <div className="text-2xl mb-1">🏆</div>
            <p className="text-xs text-gray-400">Global leaderboard</p>
          </div>
          <div className="glass-panel rounded-lg p-4">
            <div className="text-2xl mb-1">📊</div>
            <p className="text-xs text-gray-400">Track your stats</p>
          </div>
        </div>
      </div>
    </div>
  );
}
