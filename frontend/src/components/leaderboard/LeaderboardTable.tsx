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
 * Get initials from display name
 */
function getInitials(displayName: string): string {
  const parts = displayName.trim().split(/\s+/);
  if (parts.length === 1) {
    return parts[0].substring(0, 2).toUpperCase();
  }
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

/**
 * Get background color based on user id (consistent per user)
 */
function getAvatarColor(identifier: string | undefined): string {
  const colors = [
    '#14b8a6', // teal
    '#06b6d4', // cyan
    '#0ea5e9', // sky
    '#10b981', // emerald
    '#6366f1', // indigo
    '#3b82f6', // blue
    '#f59e0b', // amber
    '#f43f5e', // rose
  ];
  
  // Handle undefined/null identifiers
  if (!identifier) {
    return colors[0];
  }
  
  // Simple hash based on identifier (displayName)
  let hash = 0;
  for (let i = 0; i < identifier.length; i++) {
    hash = ((hash << 5) - hash) + identifier.charCodeAt(i);
    hash = hash & hash;
  }
  
  return colors[Math.abs(hash) % colors.length];
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
            <div className="w-8 h-8 rounded-full animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.2)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-5 rounded w-24 animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4 text-right">
            <div className="h-5 rounded w-16 ml-auto animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.2)' }} />
          </td>
          <td className="py-3 px-4 text-right hidden sm:table-cell">
            <div className="h-5 rounded w-12 ml-auto animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
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
      isCurrentUser: entry.displayName === currentUserId, // Compare by displayName since userId is not returned
      formattedScore: formatNumber(entry.score),
      formattedTime: formatRelativeTime(entry.achievedAt),
      initials: getInitials(entry.displayName),
      avatarColor: getAvatarColor(entry.displayName),
    }));
  }, [entries, currentUserId]);

  return (
    <div className="overflow-x-auto rounded-xl backdrop-blur-sm" style={{ background: 'rgba(13, 36, 61, 0.85)', border: '1px solid rgba(56, 97, 140, 0.4)' }}>
      <table className="w-full min-w-[400px]">
        <thead>
          <tr className="border-b" style={{ borderColor: 'rgba(56, 97, 140, 0.4)', background: 'rgba(10, 37, 64, 0.6)' }}>
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
                className="transition-colors"
                style={{
                  borderBottom: '1px solid rgba(56, 97, 140, 0.3)',
                  background: entry.isCurrentUser 
                    ? 'rgba(20, 184, 166, 0.15)' 
                    : index % 2 === 0 
                      ? 'rgba(10, 37, 64, 0.3)' 
                      : 'transparent',
                }}
              >
                {/* Rank */}
                <td className="py-3 px-2 sm:px-4">
                  <RankBadge rank={entry.rank} size="sm" />
                </td>

                {/* Player Name with Avatar */}
                <td className="py-3 px-2 sm:px-4">
                  <div className="flex items-center gap-2">
                    {/* Avatar */}
                    <div 
                      data-testid="avatar-container"
                      className="w-7 h-7 rounded-full flex items-center justify-center flex-shrink-0 text-sm font-semibold"
                      style={{ 
                        backgroundColor: entry.avatar ? 'rgba(20, 184, 166, 0.2)' : entry.avatarColor,
                        color: entry.avatar ? undefined : '#ffffff'
                      }}
                    >
                      {entry.avatar || entry.initials}
                    </div>
                    <span
                      className="font-medium truncate max-w-[120px] sm:max-w-[170px]"
                      style={{ color: entry.isCurrentUser ? '#2dd4bf' : '#ffffff' }}
                    >
                      {entry.displayName}
                    </span>
                    {entry.isCurrentUser && (
                      <span className="text-xs text-white px-2 py-0.5 rounded-full" style={{ background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)' }}>
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
                    className="font-bold tabular-nums"
                    style={{
                      color: entry.isCurrentUser 
                        ? '#2dd4bf' 
                        : entry.rank === 1 
                          ? '#fbbf24' 
                          : entry.rank === 2 
                            ? '#d1d5db' 
                            : entry.rank === 3 
                              ? '#fb923c' 
                              : '#ffffff'
                    }}
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
