'use client';

import { memo, useMemo } from 'react';
import { motion } from 'motion/react';
import { RankBadge } from './RankBadge';
import type { LeaderboardEntry } from '@/lib/api/leaderboard-client';

export interface LeaderboardTableProps {
  /** Leaderboard entries to display */
  entries: LeaderboardEntry[];
  /** Current user ID for highlighting their row */
  currentUserId?: string;
  /** Whether data is loading */
  isLoading?: boolean;
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
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffSeconds = Math.floor(diffMs / 1000);
  const diffMinutes = Math.floor(diffSeconds / 60);
  const diffHours = Math.floor(diffMinutes / 60);
  const diffDays = Math.floor(diffHours / 24);
  const diffWeeks = Math.floor(diffDays / 7);
  const diffMonths = Math.floor(diffDays / 30);

  if (diffSeconds < 60) return 'just now';
  if (diffMinutes < 60) return `${diffMinutes}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 7) return `${diffDays}d ago`;
  if (diffWeeks < 4) return `${diffWeeks}w ago`;
  return `${diffMonths}mo ago`;
}

/**
 * Loading skeleton for table rows
 */
function TableSkeleton() {
  return (
    <>
      {Array.from({ length: 10 }).map((_, i) => (
        <tr key={i} className="border-b border-gray-800">
          <td className="py-3 px-2 sm:px-4">
            <div className="w-8 h-8 bg-gray-700 rounded-full animate-pulse" />
          </td>
          <td className="py-3 px-2 sm:px-4">
            <div className="h-5 bg-gray-700 rounded w-24 animate-pulse" />
          </td>
          <td className="py-3 px-2 sm:px-4 text-right">
            <div className="h-5 bg-gray-700 rounded w-16 ml-auto animate-pulse" />
          </td>
          <td className="py-3 px-2 sm:px-4 text-right hidden sm:table-cell">
            <div className="h-5 bg-gray-700 rounded w-12 ml-auto animate-pulse" />
          </td>
        </tr>
      ))}
    </>
  );
}

/**
 * Row animation variants
 */
const rowVariants = {
  hidden: { opacity: 0, x: -20 },
  visible: (i: number) => ({
    opacity: 1,
    x: 0,
    transition: {
      delay: i * 0.03,
      duration: 0.3,
      ease: 'easeOut' as const,
    },
  }),
};

/**
 * Leaderboard table displaying top scores
 * Features animated row entrance and current user highlighting
 */
function LeaderboardTableComponent({
  entries,
  currentUserId,
  isLoading = false,
}: LeaderboardTableProps) {
  // Memoize the processed entries
  const processedEntries = useMemo(() => {
    return entries.map((entry) => ({
      ...entry,
      isCurrentUser: entry.userId === currentUserId,
      formattedScore: formatNumber(entry.score),
      formattedTime: formatRelativeTime(entry.achievedAt),
    }));
  }, [entries, currentUserId]);

  return (
    <div className="overflow-x-auto rounded-xl border border-gray-700 bg-gray-800/30 backdrop-blur-sm">
      <table className="w-full min-w-[400px]">
        <thead>
          <tr className="border-b border-gray-700 bg-gray-800/50">
            <th className="py-3 px-2 sm:px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider w-16">
              Rank
            </th>
            <th className="py-3 px-2 sm:px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">
              Player
            </th>
            <th className="py-3 px-2 sm:px-4 text-right text-xs font-semibold text-gray-400 uppercase tracking-wider">
              Score
            </th>
            <th className="py-3 px-2 sm:px-4 text-right text-xs font-semibold text-gray-400 uppercase tracking-wider hidden sm:table-cell">
              When
            </th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <TableSkeleton />
          ) : processedEntries.length === 0 ? (
            <tr>
              <td colSpan={4} className="py-12 text-center text-gray-500">
                No scores yet. Be the first to play!
              </td>
            </tr>
          ) : (
            processedEntries.map((entry, index) => (
              <motion.tr
                key={`${entry.userId}-${entry.rank}`}
                custom={index}
                variants={rowVariants}
                initial="hidden"
                animate="visible"
                className={`
                  border-b border-gray-800 transition-colors
                  ${index % 2 === 0 ? 'bg-gray-800/20' : 'bg-transparent'}
                  ${
                    entry.isCurrentUser
                      ? 'bg-blue-900/30 border-blue-700 hover:bg-blue-900/40'
                      : 'hover:bg-gray-700/30'
                  }
                `}
              >
                {/* Rank */}
                <td className="py-3 px-2 sm:px-4">
                  <RankBadge rank={entry.rank} size="sm" />
                </td>

                {/* Player Name */}
                <td className="py-3 px-2 sm:px-4">
                  <div className="flex items-center gap-2">
                    <span
                      className={`
                        font-medium truncate max-w-[150px] sm:max-w-[200px]
                        ${entry.isCurrentUser ? 'text-blue-300' : 'text-white'}
                      `}
                    >
                      {entry.displayName}
                    </span>
                    {entry.isCurrentUser && (
                      <span className="text-xs bg-blue-600 text-white px-2 py-0.5 rounded-full">
                        You
                      </span>
                    )}
                  </div>
                  {/* Mobile: Show time under name */}
                  <span className="text-xs text-gray-500 sm:hidden">
                    {entry.formattedTime}
                  </span>
                </td>

                {/* Score */}
                <td className="py-3 px-2 sm:px-4 text-right">
                  <span
                    className={`
                      font-bold tabular-nums
                      ${entry.rank === 1 ? 'text-yellow-400' : ''}
                      ${entry.rank === 2 ? 'text-gray-300' : ''}
                      ${entry.rank === 3 ? 'text-orange-400' : ''}
                      ${entry.rank > 3 ? 'text-white' : ''}
                      ${entry.isCurrentUser ? 'text-blue-300' : ''}
                    `}
                  >
                    {entry.formattedScore}
                  </span>
                </td>

                {/* Time (hidden on mobile) */}
                <td className="py-3 px-2 sm:px-4 text-right text-gray-400 text-sm hidden sm:table-cell">
                  {entry.formattedTime}
                </td>
              </motion.tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}

export const LeaderboardTable = memo(LeaderboardTableComponent);
LeaderboardTable.displayName = 'LeaderboardTable';
