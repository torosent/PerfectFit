/**
 * useSeasonPass Hook
 *
 * Hook for fetching season pass data and claiming rewards.
 */

import { useEffect } from 'react';
import { useGamificationStore } from '@/lib/stores/gamification-store';
import { useIsAuthenticated, useIsAuthInitialized } from '@/lib/stores/auth-store';

export function useSeasonPass() {
  const seasonPass = useGamificationStore((s) => s.seasonPass);
  const isLoading = useGamificationStore((s) => s.isLoading);
  const fetchSeasonPass = useGamificationStore((s) => s.fetchSeasonPass);
  const claimReward = useGamificationStore((s) => s.claimReward);
  const isAuthenticated = useIsAuthenticated();
  const isAuthInitialized = useIsAuthInitialized();

  useEffect(() => {
    if (!isAuthInitialized || !isAuthenticated) {
      return;
    }

    fetchSeasonPass();
  }, [isAuthInitialized, isAuthenticated, fetchSeasonPass]);

  const claimableRewards = seasonPass?.rewards.filter((r) => r.canClaim && !r.isClaimed) ?? [];
  const nextReward = seasonPass?.rewards.find((r) => !r.canClaim && !r.isClaimed);
  const progressToNextTier = nextReward
    ? ((seasonPass?.currentXP ?? 0) / nextReward.xpRequired) * 100
    : 100;

  return {
    seasonPass,
    currentTier: seasonPass?.currentTier ?? 0,
    currentXP: seasonPass?.currentXP ?? 0,
    claimableRewards,
    nextReward,
    progressToNextTier,
    claimReward,
    isLoading,
  };
}
