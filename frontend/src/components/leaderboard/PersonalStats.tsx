'use client';

import { memo } from 'react';
import { motion } from 'motion/react';
import { staggerContainerVariants, staggerItemVariants } from '@/lib/animations';
import { RankBadge } from './RankBadge';
import type { UserStats } from '@/lib/api/leaderboard-client';

export interface PersonalStatsProps {
  /** User statistics to display */
  stats: UserStats;
}

/**
 * Format a number with locale-specific separators
 */
function formatNumber(value: number): string {
  return value.toLocaleString();
}

/**
 * Format a date as relative time (e.g., "2 hours ago")
 */
function formatRelativeTime(dateString: string): string {
  // Ensure the date string is treated as UTC if no timezone specified
  // .NET DateTime.UtcNow serializes as "2026-01-03T12:00:00" without 'Z'
  // We need to append 'Z' if not present to ensure proper UTC parsing
  let normalizedDateString = dateString;
  if (dateString && !dateString.endsWith('Z') && !dateString.includes('+') && !dateString.includes('-', 10)) {
    normalizedDateString = dateString + 'Z';
  }
  
  const date = new Date(normalizedDateString);
  
  // Handle invalid dates
  if (isNaN(date.getTime())) {
    return 'unknown';
  }
  
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  
  // Handle future dates (clock skew) - show as "just now"
  if (diffMs < 0) {
    return 'just now';
  }
  
  const diffSeconds = Math.floor(diffMs / 1000);
  const diffMinutes = Math.floor(diffSeconds / 60);
  const diffHours = Math.floor(diffMinutes / 60);
  const diffDays = Math.floor(diffHours / 24);
  const diffWeeks = Math.floor(diffDays / 7);
  const diffMonths = Math.floor(diffDays / 30);

  if (diffSeconds < 60) return 'just now';
  if (diffMinutes < 60) return `${diffMinutes} minute${diffMinutes === 1 ? '' : 's'} ago`;
  if (diffHours < 24) return `${diffHours} hour${diffHours === 1 ? '' : 's'} ago`;
  if (diffDays < 7) return `${diffDays} day${diffDays === 1 ? '' : 's'} ago`;
  if (diffWeeks < 4) return `${diffWeeks} week${diffWeeks === 1 ? '' : 's'} ago`;
  return `${diffMonths} month${diffMonths === 1 ? '' : 's'} ago`;
}

/**
 * Displays personal statistics panel for authenticated users
 * Shows high score, global rank, games played, and best game details
 */
function PersonalStatsComponent({ stats }: PersonalStatsProps) {
  const { highScore, gamesPlayed, globalRank, bestGame } = stats;

  return (
    <motion.div
      variants={staggerContainerVariants}
      initial="hidden"
      animate="visible"
      className="backdrop-blur-sm rounded-xl p-6"
      style={{ background: 'rgba(13, 36, 61, 0.85)', border: '1px solid rgba(56, 97, 140, 0.4)' }}
    >
      <motion.h2
        variants={staggerItemVariants}
        className="text-lg font-semibold text-white mb-4 flex items-center gap-2"
      >
        <svg
          className="w-5 h-5"
          style={{ color: '#2dd4bf' }}
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
          aria-hidden="true"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
          />
        </svg>
        Your Stats
      </motion.h2>

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        {/* High Score */}
        <motion.div variants={staggerItemVariants} className="text-center">
          <p className="text-xs font-medium text-gray-400 uppercase tracking-wider mb-1">
            High Score
          </p>
          <motion.p
            className="text-2xl sm:text-3xl font-bold text-yellow-400 tabular-nums"
            animate={{
              textShadow: [
                '0 0 5px rgba(251, 191, 36, 0.2)',
                '0 0 15px rgba(251, 191, 36, 0.4)',
                '0 0 5px rgba(251, 191, 36, 0.2)',
              ],
            }}
            transition={{
              duration: 3,
              repeat: Infinity,
              ease: 'easeInOut',
            }}
          >
            {formatNumber(highScore)}
          </motion.p>
        </motion.div>

        {/* Global Rank */}
        <motion.div variants={staggerItemVariants} className="text-center">
          <p className="text-xs font-medium text-gray-400 uppercase tracking-wider mb-1">
            Global Rank
          </p>
          <div className="flex items-center justify-center gap-2">
            {globalRank !== null ? (
              <>
                {globalRank <= 3 ? (
                  <RankBadge rank={globalRank} size="lg" />
                ) : (
                  <p className="text-2xl sm:text-3xl font-bold text-white tabular-nums">
                    #{formatNumber(globalRank)}
                  </p>
                )}
              </>
            ) : (
              <p className="text-lg text-gray-500">Not ranked</p>
            )}
          </div>
        </motion.div>

        {/* Games Played */}
        <motion.div variants={staggerItemVariants} className="text-center">
          <p className="text-xs font-medium text-gray-400 uppercase tracking-wider mb-1">
            Games Played
          </p>
          <p className="text-2xl sm:text-3xl font-bold tabular-nums" style={{ color: '#2dd4bf' }}>
            {formatNumber(gamesPlayed)}
          </p>
        </motion.div>

        {/* Best Game Lines/Combo */}
        <motion.div variants={staggerItemVariants} className="text-center">
          <p className="text-xs font-medium text-gray-400 uppercase tracking-wider mb-1">
            Best Lines
          </p>
          <p className="text-2xl sm:text-3xl font-bold tabular-nums" style={{ color: '#0ea5e9' }}>
            {bestGame ? formatNumber(bestGame.linesCleared) : '-'}
          </p>
        </motion.div>
      </div>

      {/* Best Game Details */}
      {bestGame && (
        <motion.div
          variants={staggerItemVariants}
          className="mt-4 pt-4"
          style={{ borderTop: '1px solid rgba(56, 97, 140, 0.4)' }}
        >
          <p className="text-xs text-gray-400 text-center">
            Best game achieved {formatRelativeTime(bestGame.achievedAt)} with a{' '}
            <span className="font-medium" style={{ color: '#fbbf24' }}>
              {bestGame.maxCombo}x combo
            </span>
          </p>
        </motion.div>
      )}
    </motion.div>
  );
}

export const PersonalStats = memo(PersonalStatsComponent);
PersonalStats.displayName = 'PersonalStats';
