/**
 * usePersonalGoals Hook
 *
 * Hook for fetching and filtering personal goals.
 */

import { useEffect } from 'react';
import { useGamificationStore } from '@/lib/stores/gamification-store';
import { useIsAuthenticated, useIsAuthInitialized } from '@/lib/stores/auth-store';

export function usePersonalGoals() {
  const personalGoals = useGamificationStore((s) => s.personalGoals);
  const isLoading = useGamificationStore((s) => s.isLoading);
  const fetchPersonalGoals = useGamificationStore((s) => s.fetchPersonalGoals);
  const isAuthenticated = useIsAuthenticated();
  const isAuthInitialized = useIsAuthInitialized();

  useEffect(() => {
    if (!isAuthInitialized || !isAuthenticated) {
      return;
    }

    fetchPersonalGoals();
  }, [isAuthInitialized, isAuthenticated, fetchPersonalGoals]);

  const activeGoals = personalGoals.filter((g) => !g.isCompleted);
  const completedGoals = personalGoals.filter((g) => g.isCompleted);

  return {
    goals: personalGoals,
    activeGoals,
    completedGoals,
    isLoading,
  };
}
