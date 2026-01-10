/**
 * useCosmetics Hook
 *
 * Hook for fetching and filtering cosmetics by type.
 */

import { useEffect } from 'react';
import { useGamificationStore } from '@/lib/stores/gamification-store';
import type { CosmeticType } from '@/types/gamification';

export function useCosmetics(type?: CosmeticType) {
  const cosmetics = useGamificationStore((s) => s.cosmetics);
  const equippedCosmetics = useGamificationStore((s) => s.equippedCosmetics);
  const isLoading = useGamificationStore((s) => s.isLoading);
  const fetchCosmetics = useGamificationStore((s) => s.fetchCosmetics);
  const equipCosmetic = useGamificationStore((s) => s.equipCosmetic);

  useEffect(() => {
    fetchCosmetics(type);
  }, [type, fetchCosmetics]);

  const ownedCosmetics = cosmetics.filter((c) => c.isOwned);
  const byType = {
    boardThemes: cosmetics.filter((c) => c.type === 'BoardTheme'),
    avatarFrames: cosmetics.filter((c) => c.type === 'AvatarFrame'),
    badges: cosmetics.filter((c) => c.type === 'Badge'),
  };

  return {
    cosmetics,
    ownedCosmetics,
    byType,
    equippedCosmetics,
    equipCosmetic,
    isLoading,
  };
}
