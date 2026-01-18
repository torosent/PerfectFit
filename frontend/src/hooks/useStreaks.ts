/**
 * useStreaks Hook
 *
 * Hook for accessing streak data and using streak freeze tokens.
 */

import { useEffect } from 'react';
import { useGamificationStore } from '@/lib/stores/gamification-store';
import { useIsAuthenticated, useIsAuthInitialized } from '@/lib/stores/auth-store';

export function useStreaks() {
  const streak = useGamificationStore((s) => s.status?.streak);
  const useFreeze = useGamificationStore((s) => s.useStreakFreeze);
  const isLoading = useGamificationStore((s) => s.isLoading);
  const fetchGamificationStatus = useGamificationStore((s) => s.fetchGamificationStatus);
  const isAuthenticated = useIsAuthenticated();
  const isAuthInitialized = useIsAuthInitialized();

  useEffect(() => {
    if (!isAuthInitialized || !isAuthenticated) {
      return;
    }

    fetchGamificationStatus();
  }, [isAuthInitialized, isAuthenticated, fetchGamificationStatus]);

  return {
    currentStreak: streak?.currentStreak ?? 0,
    longestStreak: streak?.longestStreak ?? 0,
    freezeTokens: streak?.freezeTokens ?? 0,
    isAtRisk: streak?.isAtRisk ?? false,
    resetTime: streak?.resetTime ? new Date(streak.resetTime) : null,
    useFreeze,
    isLoading,
  };
}
