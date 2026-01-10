/**
 * useGamification Hook
 *
 * Main hook for fetching and accessing gamification data.
 * Automatically fetches gamification status when authenticated.
 */

import { useEffect } from 'react';
import { useGamificationStore } from '@/lib/stores/gamification-store';
import { useAuthStore } from '@/lib/stores/auth-store';

export function useGamification() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const status = useGamificationStore((s) => s.status);
  const isLoading = useGamificationStore((s) => s.isLoading);
  const error = useGamificationStore((s) => s.error);
  const fetchGamificationStatus = useGamificationStore((s) => s.fetchGamificationStatus);

  useEffect(() => {
    if (isAuthenticated && !status) {
      fetchGamificationStatus();
    }
  }, [isAuthenticated, status, fetchGamificationStatus]);

  return { status, isLoading, error };
}
