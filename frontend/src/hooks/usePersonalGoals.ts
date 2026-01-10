/**
 * usePersonalGoals Hook
 *
 * Hook for fetching and filtering personal goals.
 */

import { useEffect } from 'react';
import { useGamificationStore } from '@/lib/stores/gamification-store';

export function usePersonalGoals() {
  const personalGoals = useGamificationStore((s) => s.personalGoals);
  const isLoading = useGamificationStore((s) => s.isLoading);
  const fetchPersonalGoals = useGamificationStore((s) => s.fetchPersonalGoals);

  useEffect(() => {
    fetchPersonalGoals();
  }, [fetchPersonalGoals]);

  const activeGoals = personalGoals.filter((g) => !g.isCompleted);
  const completedGoals = personalGoals.filter((g) => g.isCompleted);

  return {
    goals: personalGoals,
    activeGoals,
    completedGoals,
    isLoading,
  };
}
