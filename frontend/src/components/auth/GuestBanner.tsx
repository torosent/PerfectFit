'use client';

import { useState, useSyncExternalStore } from 'react';
import { useRouter } from 'next/navigation';
import { useIsGuest, useIsAuthenticated } from '@/lib/stores/auth-store';

/**
 * Props for GuestBanner component
 */
interface GuestBannerProps {
  /** Storage key for dismissal state */
  storageKey?: string;
  /** Class name for additional styling */
  className?: string;
}

/**
 * Hook to safely check if we're on the client
 */
function useIsMounted() {
  return useSyncExternalStore(
    () => () => {},
    () => true,
    () => false
  );
}

/**
 * Banner component shown to guest users
 * Prompts them to sign in to save their scores
 * Dismissible and remembers dismissal in localStorage
 */
export function GuestBanner({
  storageKey = 'perfectfit-guest-banner-dismissed',
  className = '',
}: GuestBannerProps) {
  const router = useRouter();
  const isAuthenticated = useIsAuthenticated();
  const isGuest = useIsGuest();
  const isMounted = useIsMounted();
  const [isDismissed, setIsDismissed] = useState(() => {
    // Initialize from localStorage if on client
    if (typeof window !== 'undefined') {
      return localStorage.getItem(storageKey) === 'true';
    }
    return true; // Default to hidden on server
  });

  // Don't show if not mounted, not a guest, not authenticated, or dismissed
  if (!isMounted || !isAuthenticated || !isGuest || isDismissed) {
    return null;
  }

  const handleDismiss = () => {
    localStorage.setItem(storageKey, 'true');
    setIsDismissed(true);
  };

  const handleSignIn = () => {
    router.push('/login');
  };

  return (
    <div
      className={`
        bg-gradient-to-r from-blue-600/20 to-purple-600/20
        border border-blue-500/30 rounded-lg
        p-4 flex items-center justify-between gap-4
        animate-in fade-in slide-in-from-top-2 duration-300
        ${className}
      `}
      role="alert"
    >
      <div className="flex items-center gap-3">
        <div className="flex-shrink-0">
          <svg
            className="w-5 h-5 text-blue-400"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
        </div>
        <p className="text-sm text-gray-200">
          <span className="font-medium">Playing as guest.</span>{' '}
          <span className="text-gray-400">
            Sign in to save your scores to the leaderboard!
          </span>
        </p>
      </div>

      <div className="flex items-center gap-2 flex-shrink-0">
        <button
          onClick={handleSignIn}
          className={`
            px-3 py-1.5 text-sm font-medium
            bg-blue-600 hover:bg-blue-500 text-white
            rounded-md transition-colors
          `}
        >
          Sign In
        </button>
        <button
          onClick={handleDismiss}
          className={`
            p-1.5 text-gray-400 hover:text-white
            hover:bg-white/10 rounded-md transition-colors
          `}
          aria-label="Dismiss banner"
        >
          <svg
            className="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M6 18L18 6M6 6l12 12"
            />
          </svg>
        </button>
      </div>
    </div>
  );
}
