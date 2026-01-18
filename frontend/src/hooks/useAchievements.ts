/**
 * useAchievements Hook
 *
 * Hook for fetching and categorizing achievements.
 */

import { useEffect } from 'react';
import { useGamificationStore } from '@/lib/stores/gamification-store';
import { useIsAuthenticated, useIsAuthInitialized } from '@/lib/stores/auth-store';

export function useAchievements() {
  const achievements = useGamificationStore((s) => s.achievements);
  const isLoading = useGamificationStore((s) => s.isLoading);
  const fetchAchievements = useGamificationStore((s) => s.fetchAchievements);
  const isAuthenticated = useIsAuthenticated();
  const isAuthInitialized = useIsAuthInitialized();

  useEffect(() => {
    if (!isAuthInitialized || !isAuthenticated) {
      return;
    }

    fetchAchievements();
  }, [isAuthInitialized, isAuthenticated, fetchAchievements]);

  const unlockedCount = achievements.filter((a) => a.isUnlocked).length;
  const byCategory = {
    score: achievements.filter((a) => a.category === 'Score'),
    streak: achievements.filter((a) => a.category === 'Streak'),
    games: achievements.filter((a) => a.category === 'Games'),
    challenge: achievements.filter((a) => a.category === 'Challenge'),
    special: achievements.filter((a) => a.category === 'Special'),
  };

  return {
    achievements,
    unlockedCount,
    totalCount: achievements.length,
    byCategory,
    isLoading,
  };
}
