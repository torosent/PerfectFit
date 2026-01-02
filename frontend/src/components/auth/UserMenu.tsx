'use client';

import { useState, useRef, useEffect } from 'react';
import { useAuthStore, useUser, useIsGuest } from '@/lib/stores/auth-store';

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
function getAvatarColor(userId: string): string {
  const colors = [
    'bg-blue-500',
    'bg-green-500',
    'bg-purple-500',
    'bg-pink-500',
    'bg-indigo-500',
    'bg-cyan-500',
    'bg-orange-500',
    'bg-teal-500',
  ];
  
  // Simple hash based on user id
  let hash = 0;
  for (let i = 0; i < userId.length; i++) {
    hash = ((hash << 5) - hash) + userId.charCodeAt(i);
    hash = hash & hash;
  }
  
  return colors[Math.abs(hash) % colors.length];
}

/**
 * User menu dropdown component
 * Shows user avatar, name, stats, and logout option
 */
export function UserMenu() {
  const user = useUser();
  const isGuest = useIsGuest();
  const logout = useAuthStore((state) => state.logout);
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  // Close menu when clicking outside
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  // Close menu on escape key
  useEffect(() => {
    function handleEscape(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setIsOpen(false);
      }
    }

    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, []);

  if (!user) return null;

  const initials = getInitials(user.displayName);
  const avatarColor = getAvatarColor(user.id);

  const handleLogout = async () => {
    setIsOpen(false);
    await logout();
  };

  return (
    <div className="relative" ref={menuRef}>
      {/* Avatar Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className={`
          flex items-center gap-2 p-1 rounded-full
          hover:bg-white/10 transition-colors
          focus:outline-none focus:ring-2 focus:ring-blue-500/50
        `}
        aria-expanded={isOpen}
        aria-haspopup="true"
        aria-label="User menu"
      >
        <div
          className={`
            w-9 h-9 rounded-full flex items-center justify-center
            ${avatarColor} text-white font-semibold text-sm
            ring-2 ring-white/20
          `}
        >
          {initials}
        </div>
        <svg
          className={`w-4 h-4 text-gray-400 transition-transform ${isOpen ? 'rotate-180' : ''}`}
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
          aria-hidden="true"
        >
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
        </svg>
      </button>

      {/* Dropdown Menu */}
      {isOpen && (
        <div
          className={`
            absolute right-0 mt-2 w-64
            bg-gray-800 rounded-lg shadow-xl
            border border-gray-700
            py-2 z-50
            animate-in fade-in slide-in-from-top-2 duration-200
          `}
          role="menu"
          aria-orientation="vertical"
        >
          {/* User Info */}
          <div className="px-4 py-3 border-b border-gray-700">
            <p className="font-semibold text-white truncate">{user.displayName}</p>
            {user.email && (
              <p className="text-sm text-gray-400 truncate">{user.email}</p>
            )}
            {isGuest && (
              <span className="inline-block mt-1 px-2 py-0.5 text-xs bg-yellow-600/20 text-yellow-500 rounded">
                Guest
              </span>
            )}
          </div>

          {/* Stats */}
          <div className="px-4 py-3 border-b border-gray-700">
            <div className="flex justify-between text-sm">
              <span className="text-gray-400">High Score</span>
              <span className="text-white font-medium">
                {user.highScore.toLocaleString()}
              </span>
            </div>
            <div className="flex justify-between text-sm mt-1">
              <span className="text-gray-400">Games Played</span>
              <span className="text-white font-medium">
                {user.gamesPlayed.toLocaleString()}
              </span>
            </div>
          </div>

          {/* Actions */}
          <div className="py-1">
            <button
              onClick={handleLogout}
              className={`
                w-full px-4 py-2 text-left text-sm
                text-red-400 hover:text-red-300 hover:bg-gray-700/50
                transition-colors flex items-center gap-2
              `}
              role="menuitem"
            >
              <svg
                className="w-4 h-4"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                aria-hidden="true"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"
                />
              </svg>
              Sign out
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
