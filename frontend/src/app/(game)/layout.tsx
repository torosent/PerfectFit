'use client';

import { useEffect } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import {
  useAuthStore,
  useIsAuthenticated,
  useIsAuthInitialized,
} from '@/lib/stores/auth-store';
import { UserMenu } from '@/components/auth/UserMenu';

/**
 * Game layout with header
 * Includes game title and auth controls
 */
export default function GameLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const isAuthenticated = useIsAuthenticated();
  const isInitialized = useIsAuthInitialized();
  const initializeAuth = useAuthStore((state) => state.initializeAuth);

  // Initialize auth on mount
  useEffect(() => {
    initializeAuth();
  }, [initializeAuth]);

  return (
    <div className="min-h-screen game-background">
      {/* Header */}
      <header className="relative z-20 border-b border-white/10">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            {/* Logo */}
            <Link
              href="/play"
              className="flex items-center gap-2 hover:opacity-80 transition-opacity"
            >
              <div className="w-8 h-8 bg-gradient-to-br from-blue-500 to-purple-600 rounded-lg flex items-center justify-center">
                <svg
                  className="w-5 h-5 text-white"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                  aria-hidden="true"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z"
                  />
                </svg>
              </div>
              <span className="text-xl font-bold text-white">
                Perfect<span className="text-blue-400">Fit</span>
              </span>
            </Link>

            {/* Navigation */}
            <nav className="hidden sm:flex items-center gap-6">
              <Link
                href="/play"
                className={`text-sm font-medium transition-colors ${
                  pathname === '/play'
                    ? 'text-white'
                    : 'text-gray-400 hover:text-white'
                }`}
              >
                Play
              </Link>
              <Link
                href="/leaderboard"
                className={`text-sm font-medium transition-colors ${
                  pathname === '/leaderboard'
                    ? 'text-white'
                    : 'text-gray-400 hover:text-white'
                }`}
              >
                🏆 Leaderboard
              </Link>
            </nav>

            {/* Auth Controls */}
            <div className="flex items-center gap-4">
              {isInitialized && (
                <>
                  {isAuthenticated ? (
                    <UserMenu />
                  ) : (
                    <Link
                      href="/login"
                      className={`
                        py-2 px-4 text-sm font-medium
                        bg-blue-600 hover:bg-blue-500 text-white
                        rounded-lg transition-colors
                      `}
                    >
                      Sign In
                    </Link>
                  )}
                </>
              )}
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="relative z-10">{children}</main>
    </div>
  );
}
