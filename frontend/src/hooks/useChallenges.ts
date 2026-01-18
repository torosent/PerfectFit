/**
 * useChallenges Hook
 *
 * Hook for fetching and filtering challenges by type.
 */

import { useEffect } from 'react';
import { useGamificationStore } from '@/lib/stores/gamification-store';
import { useIsAuthenticated, useIsAuthInitialized } from '@/lib/stores/auth-store';
import type { ChallengeType } from '@/types/gamification';

export function useChallenges(type?: ChallengeType) {
  const challenges = useGamificationStore((s) => s.challenges);
  const isLoading = useGamificationStore((s) => s.isLoading);
  const fetchChallenges = useGamificationStore((s) => s.fetchChallenges);
  const isAuthenticated = useIsAuthenticated();
  const isAuthInitialized = useIsAuthInitialized();

  useEffect(() => {
    if (!isAuthInitialized || !isAuthenticated) {
      return;
    }

    fetchChallenges(type);
  }, [type, isAuthInitialized, isAuthenticated, fetchChallenges]);

  const dailyChallenges = challenges.filter((c) => c.type === 'Daily');
  const weeklyChallenges = challenges.filter((c) => c.type === 'Weekly');
  const completedCount = challenges.filter((c) => c.isCompleted).length;

  return {
    challenges,
    dailyChallenges,
    weeklyChallenges,
    completedCount,
    totalCount: challenges.length,
    isLoading,
  };
}
