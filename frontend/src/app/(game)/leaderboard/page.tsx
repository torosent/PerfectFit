'use client';

import { useEffect, useState, useCallback } from 'react';
import Link from 'next/link';
import { motion } from 'motion/react';
import { LeaderboardTable, PersonalStats } from '@/components/leaderboard';
import { useAuthStore, useIsAuthenticated, useUser } from '@/lib/stores/auth-store';
import {
  getTopScores,
  getUserStats,
  type LeaderboardEntry,
  type UserStats,
} from '@/lib/api/leaderboard-client';
import { staggerContainerVariants, staggerItemVariants } from '@/lib/animations';

/**
 * Leaderboard page displaying top scores and personal stats
 */
export default function LeaderboardPage() {
  const isAuthenticated = useIsAuthenticated();
  const user = useUser();
  const token = useAuthStore((state) => state.token);

  const [entries, setEntries] = useState<LeaderboardEntry[]>([]);
  const [userStats, setUserStats] = useState<UserStats | null>(null);
  const [isLoadingEntries, setIsLoadingEntries] = useState(true);
  const [isLoadingStats, setIsLoadingStats] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Fetch leaderboard entries
  const fetchLeaderboard = useCallback(async () => {
    setIsLoadingEntries(true);
    setError(null);
    try {
      const data = await getTopScores(100);
      setEntries(data);
    } catch (err) {
      console.error('Failed to fetch leaderboard:', err);
      setError('Failed to load leaderboard. Please try again.');
    } finally {
      setIsLoadingEntries(false);
    }
  }, []);

  // Fetch user stats when authenticated
  const fetchUserStats = useCallback(async () => {
    if (!token) return;
    
    setIsLoadingStats(true);
    try {
      const data = await getUserStats(token);
      setUserStats(data);
    } catch (err) {
      console.error('Failed to fetch user stats:', err);
      // Don't show error for stats, just don't display them
    } finally {
      setIsLoadingStats(false);
    }
  }, [token]);

  // Load data on mount
  useEffect(() => {
    fetchLeaderboard();
  }, [fetchLeaderboard]);

  // Load user stats when authenticated
  useEffect(() => {
    if (isAuthenticated && token) {
      fetchUserStats();
    } else {
      setUserStats(null);
    }
  }, [isAuthenticated, token, fetchUserStats]);

  return (
    <div className="min-h-screen text-white p-4 sm:p-8">
      <div className="max-w-4xl mx-auto">
        <motion.div
          variants={staggerContainerVariants}
          initial="hidden"
          animate="visible"
          className="space-y-6"
        >
          {/* Header */}
          <motion.div
            variants={staggerItemVariants}
            className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4"
          >
            <div>
              <h1 className="text-3xl font-bold flex items-center gap-3">
                <span className="text-4xl">🏆</span>
                Leaderboard
              </h1>
              <p className="text-gray-400 mt-1">
                Top 100 players worldwide
              </p>
            </div>
            <Link
              href="/play"
              className="inline-flex items-center justify-center gap-2 py-2 px-4 bg-blue-600 hover:bg-blue-500 text-white font-medium rounded-lg transition-colors"
            >
              <svg
                className="w-5 h-5"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z"
                />
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
              Play Now
            </Link>
          </motion.div>

          {/* Personal Stats (if authenticated) */}
          {isAuthenticated && userStats && !isLoadingStats && (
            <motion.div variants={staggerItemVariants}>
              <PersonalStats stats={userStats} />
            </motion.div>
          )}

          {/* Personal Stats Loading */}
          {isAuthenticated && isLoadingStats && (
            <motion.div
              variants={staggerItemVariants}
              className="bg-gray-800/50 backdrop-blur-sm border border-gray-700 rounded-xl p-6"
            >
              <div className="h-6 bg-gray-700 rounded w-32 mb-4 animate-pulse" />
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                {Array.from({ length: 4 }).map((_, i) => (
                  <div key={i} className="text-center">
                    <div className="h-4 bg-gray-700 rounded w-16 mx-auto mb-2 animate-pulse" />
                    <div className="h-8 bg-gray-700 rounded w-20 mx-auto animate-pulse" />
                  </div>
                ))}
              </div>
            </motion.div>
          )}

          {/* Sign in prompt for guests */}
          {!isAuthenticated && (
            <motion.div
              variants={staggerItemVariants}
              className="bg-gradient-to-r from-blue-900/30 to-purple-900/30 border border-blue-700/50 rounded-xl p-4 flex flex-col sm:flex-row items-center justify-between gap-4"
            >
              <div className="flex items-center gap-3">
                <svg
                  className="w-6 h-6 text-blue-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                  aria-hidden="true"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
                <p className="text-gray-300">
                  Sign in to see your personal stats and track your progress!
                </p>
              </div>
              <Link
                href="/login"
                className="whitespace-nowrap py-2 px-4 bg-blue-600 hover:bg-blue-500 text-white font-medium rounded-lg transition-colors"
              >
                Sign In
              </Link>
            </motion.div>
          )}

          {/* Error Message */}
          {error && (
            <motion.div
              variants={staggerItemVariants}
              className="bg-red-900/50 border border-red-500 text-red-200 px-4 py-3 rounded-lg text-center"
              role="alert"
            >
              <p>{error}</p>
              <button
                onClick={fetchLeaderboard}
                className="mt-2 text-sm underline hover:no-underline"
              >
                Try again
              </button>
            </motion.div>
          )}

          {/* Leaderboard Table */}
          <motion.div variants={staggerItemVariants}>
            <LeaderboardTable
              entries={entries}
              currentUserId={user?.id}
              isLoading={isLoadingEntries}
            />
          </motion.div>

          {/* Refresh Button */}
          <motion.div
            variants={staggerItemVariants}
            className="flex justify-center"
          >
            <button
              onClick={() => {
                fetchLeaderboard();
                if (isAuthenticated) fetchUserStats();
              }}
              disabled={isLoadingEntries}
              className="flex items-center gap-2 py-2 px-4 bg-gray-700 hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed text-gray-300 font-medium rounded-lg transition-colors"
            >
              <svg
                className={`w-4 h-4 ${isLoadingEntries ? 'animate-spin' : ''}`}
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
                />
              </svg>
              Refresh
            </button>
          </motion.div>
        </motion.div>
      </div>
    </div>
  );
}
