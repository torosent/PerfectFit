'use client';

import { useCallback, useMemo } from 'react';

export interface HapticFeedback {
  /** Whether the Vibration API is supported on this device */
  isSupported: boolean;
  /** Light tap feedback (10ms) - for piece pickup */
  lightTap: () => void;
  /** Medium tap feedback (20ms) - for successful placement */
  mediumTap: () => void;
  /** Celebratory pattern [50, 30, 50] - for line clears */
  lineClear: () => void;
  /** Long pattern [100, 50, 100, 50, 100] - for game over */
  gameOver: () => void;
  /** Success feedback (30ms) - for general success actions */
  success: () => void;
  /** Error pattern [50, 100, 50] - for invalid actions */
  error: () => void;
}

/**
 * Hook for triggering haptic feedback on mobile devices.
 * 
 * Uses the Vibration API (navigator.vibrate) which is supported on:
 * - Android Chrome, Firefox, Opera
 * - NOT supported on iOS Safari (fails gracefully)
 * 
 * All functions are safe to call even when vibration is not supported.
 * 
 * @example
 * ```tsx
 * const { lightTap, mediumTap, lineClear, gameOver } = useHaptics();
 * 
 * // On piece pickup
 * lightTap();
 * 
 * // On successful placement
 * mediumTap();
 * 
 * // On line clear
 * lineClear();
 * 
 * // On game over
 * gameOver();
 * ```
 */
export function useHaptics(): HapticFeedback {
  const isSupported = useMemo(() => {
    if (typeof navigator === 'undefined') return false;
    return 'vibrate' in navigator && typeof navigator.vibrate === 'function';
  }, []);

  const vibrate = useCallback((pattern: number | number[]) => {
    if (isSupported) {
      try {
        navigator.vibrate(pattern);
      } catch {
        // Silently fail - vibration not critical
        // Can happen due to browser restrictions or permissions
      }
    }
  }, [isSupported]);

  const lightTap = useCallback(() => vibrate(10), [vibrate]);
  const mediumTap = useCallback(() => vibrate(20), [vibrate]);
  const lineClear = useCallback(() => vibrate([50, 30, 50]), [vibrate]);
  const gameOver = useCallback(() => vibrate([100, 50, 100, 50, 100]), [vibrate]);
  const success = useCallback(() => vibrate(30), [vibrate]);
  const error = useCallback(() => vibrate([50, 100, 50]), [vibrate]);

  return {
    isSupported,
    lightTap,
    mediumTap,
    lineClear,
    gameOver,
    success,
    error,
  };
}
