'use client';

import { memo } from 'react';
import { motion, AnimatePresence } from 'motion/react';

export interface StreakDisplayProps {
  /** Current consecutive days played */
  currentStreak: number;
  /** Longest streak ever achieved */
  longestStreak: number;
  /** Number of streak freeze tokens available */
  freezeTokens: number;
  /** Whether the streak is at risk of being lost */
  isAtRisk: boolean;
  /** Compact mode for header display */
  compact?: boolean;
  /** When the streak will reset (optional) */
  resetTime?: Date | null;
}

/**
 * Get streak tier for visual intensity
 */
function getStreakTier(streak: number): 'none' | 'low' | 'medium' | 'high' | 'epic' {
  if (streak >= 30) return 'epic';
  if (streak >= 14) return 'high';
  if (streak >= 7) return 'medium';
  if (streak >= 1) return 'low';
  return 'none';
}

/**
 * Get glow color based on streak tier
 */
function getGlowColor(tier: string, isAtRisk: boolean): string {
  if (isAtRisk) return 'rgba(239, 68, 68, 0.5)'; // Red glow
  switch (tier) {
    case 'epic': return 'rgba(168, 85, 247, 0.5)'; // Purple
    case 'high': return 'rgba(249, 115, 22, 0.5)'; // Orange
    case 'medium': return 'rgba(251, 191, 36, 0.5)'; // Yellow
    case 'low': return 'rgba(251, 191, 36, 0.3)'; // Light yellow
    default: return 'transparent';
  }
}

/**
 * Animated fire streak counter component
 * Shows current streak with fire emoji and visual effects
 */
function StreakDisplayComponent({
  currentStreak,
  longestStreak,
  freezeTokens,
  isAtRisk,
  compact = false,
}: StreakDisplayProps) {
  const tier = getStreakTier(currentStreak);
  const glowColor = getGlowColor(tier, isAtRisk);
  const isActive = currentStreak > 0;

  if (compact) {
    return (
      <div
        data-testid="streak-display-compact"
        className="flex items-center gap-2"
        aria-label={`Current streak: ${currentStreak} days`}
      >
        <motion.div
          className="flex items-center gap-1"
          animate={isActive && !isAtRisk ? { scale: [1, 1.05, 1] } : {}}
          transition={{ duration: 1.5, repeat: Infinity, ease: 'easeInOut' }}
        >
          <span className="text-lg">🔥</span>
          <span className="font-bold text-white tabular-nums">{currentStreak}</span>
        </motion.div>
        
        {isAtRisk && (
          <motion.div
            data-testid="streak-at-risk"
            role="alert"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className="text-xs text-red-400 font-medium"
          >
            At Risk!
          </motion.div>
        )}
        
        <div className="flex items-center gap-1 text-sm text-gray-400">
          <span>❄️</span>
          <span data-testid="freeze-token-count">{freezeTokens}</span>
        </div>
      </div>
    );
  }

  return (
    <motion.div
      data-testid="streak-display-full"
      className="relative p-4 rounded-xl bg-white/5 backdrop-blur-md"
      style={{
        boxShadow: isActive ? `0 0 20px ${glowColor}` : 'none',
      }}
      aria-label={`Current streak: ${currentStreak} days`}
    >
      {/* Main streak display */}
      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          {/* Fire icon with animation */}
          <motion.div
            className="relative"
            animate={isActive && !isAtRisk ? {
              scale: [1, 1.1, 1],
              rotate: [0, 2, -2, 0],
            } : {}}
            transition={{ duration: 0.8, repeat: Infinity, ease: 'easeInOut' }}
          >
            <span className="text-4xl">🔥</span>
            {tier === 'epic' && (
              <motion.div
                className="absolute inset-0 flex items-center justify-center"
                animate={{ opacity: [0.5, 1, 0.5] }}
                transition={{ duration: 1, repeat: Infinity }}
              >
                <span className="text-4xl blur-sm">🔥</span>
              </motion.div>
            )}
          </motion.div>
          
          {/* Streak count */}
          <div>
            <motion.p
              key={currentStreak}
              initial={{ scale: 0.8, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              className="text-3xl font-bold text-white tabular-nums"
            >
              {currentStreak}
            </motion.p>
            <p className="text-sm text-gray-400">day streak</p>
          </div>
        </div>

        {/* Freeze tokens */}
        <div className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white/5">
          <span className="text-xl">❄️</span>
          <div>
            <p className="text-lg font-semibold text-white tabular-nums" data-testid="freeze-token-count">
              {freezeTokens}
            </p>
            <p className="text-xs text-gray-400">freezes</p>
          </div>
        </div>
      </div>

      {/* At risk warning */}
      <AnimatePresence>
        {isAtRisk && (
          <motion.div
            data-testid="streak-at-risk"
            role="alert"
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            exit={{ opacity: 0, height: 0 }}
            className="mt-3 flex items-center gap-2 px-3 py-2 rounded-lg bg-red-500/20 border border-red-500/30"
          >
            <span className="text-red-400">⚠️</span>
            <p className="text-sm text-red-400 font-medium">
              Streak at risk! Play today to keep it.
            </p>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Best streak */}
      <div className="mt-3 flex items-center gap-2 text-sm text-gray-400">
        <span>🏆</span>
        <span>Best: <span className="text-white font-medium">{longestStreak}</span> days</span>
      </div>
    </motion.div>
  );
}

export const StreakDisplay = memo(StreakDisplayComponent);
StreakDisplay.displayName = 'StreakDisplay';
