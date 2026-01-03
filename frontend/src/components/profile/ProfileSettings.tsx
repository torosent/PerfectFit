'use client';

import { useState, useEffect, useCallback } from 'react';
import { EmojiPicker } from './EmojiPicker';
import { updateProfile } from '@/lib/api/profile-client';
import { useUser, useToken, useAuthStore } from '@/lib/stores/auth-store';
import type { UserProfile } from '@/types';

export interface ProfileSettingsProps {
  /** Whether the modal is open */
  isOpen: boolean;
  /** Callback to close the modal */
  onClose: () => void;
}

/**
 * Profile settings modal for editing username and avatar.
 * Uses the updateProfile API to save changes.
 */
export function ProfileSettings({ isOpen, onClose }: ProfileSettingsProps) {
  const user = useUser();
  const token = useToken();
  const setUser = useAuthStore((state) => state.setUser);

  const [username, setUsername] = useState(user?.username || '');
  const [avatar, setAvatar] = useState(user?.avatar || '');
  const [error, setError] = useState('');
  const [suggestedUsername, setSuggestedUsername] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  // Reset form when modal opens/closes or user changes
  useEffect(() => {
    if (isOpen) {
      setUsername(user?.username || '');
      setAvatar(user?.avatar || '');
      setError('');
      setSuggestedUsername(null);
    }
  }, [isOpen, user?.username, user?.avatar]);

  const handleSubmit = useCallback(async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!token) {
      setError('You must be logged in to update your profile');
      return;
    }

    setIsLoading(true);
    setError('');
    setSuggestedUsername(null);

    try {
      const response = await updateProfile({ username, avatar }, token);

      if (response.success && response.profile) {
        // Update the user in the store
        if (user && setUser) {
          const updatedUser: UserProfile = {
            ...user,
            username: response.profile.username,
            avatar: response.profile.avatar,
          };
          setUser(updatedUser);
        }
        onClose();
      } else {
        setError(response.errorMessage || 'Failed to update profile');
        if (response.suggestedUsername) {
          setSuggestedUsername(response.suggestedUsername);
        }
      }
    } catch {
      setError('An unexpected error occurred');
    } finally {
      setIsLoading(false);
    }
  }, [username, avatar, token, user, setUser, onClose]);

  const handleUseSuggestion = useCallback(() => {
    if (suggestedUsername) {
      setUsername(suggestedUsername);
      setSuggestedUsername(null);
      setError('');
    }
  }, [suggestedUsername]);

  if (!isOpen) {
    return null;
  }

  return (
    <div 
      role="dialog"
      aria-modal="true"
      aria-labelledby="profile-settings-title"
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
    >
      {/* Backdrop */}
      <div 
        className="absolute inset-0 bg-black/60 backdrop-blur-sm"
        onClick={onClose}
      />

      {/* Modal */}
      <div 
        className="relative w-full max-w-md rounded-xl shadow-2xl animate-in fade-in zoom-in-95 duration-200"
        style={{ 
          backgroundColor: '#0d243d', 
          border: '1px solid rgba(20, 184, 166, 0.3)' 
        }}
      >
        {/* Header */}
        <div 
          className="px-6 py-4"
          style={{ 
            borderBottom: '1px solid rgba(20, 184, 166, 0.2)' 
          }}
        >
          <h2 
            id="profile-settings-title"
            className="text-xl font-semibold text-white"
          >
            Edit Profile
          </h2>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="px-6 py-4 space-y-6">
          {/* Username field */}
          <div>
            <label 
              htmlFor="username"
              className="block text-sm font-medium text-gray-300 mb-2"
            >
              Username
            </label>
            <input
              id="username"
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="w-full px-4 py-2 rounded-lg bg-gray-800/50 text-white border border-gray-700 focus:border-teal-500 focus:ring-2 focus:ring-teal-500/30 focus:outline-none transition-colors"
              placeholder="Enter your username"
              disabled={isLoading}
              maxLength={30}
            />
          </div>

          {/* Avatar picker */}
          <div>
            <label className="block text-sm font-medium text-gray-300 mb-2">
              Avatar
            </label>
            <div className="flex items-center gap-4 mb-3">
              <div 
                className="w-14 h-14 rounded-full flex items-center justify-center text-3xl"
                style={{ backgroundColor: 'rgba(20, 184, 166, 0.2)' }}
              >
                {avatar || '👤'}
              </div>
              <span className="text-gray-400 text-sm">
                Select an emoji below
              </span>
            </div>
            <EmojiPicker selected={avatar} onSelect={setAvatar} />
          </div>

          {/* Error message */}
          {error && (
            <div className="p-3 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400 text-sm">
              {error}
              {suggestedUsername && (
                <div className="mt-2">
                  <span className="text-gray-400">Try: </span>
                  <button
                    type="button"
                    onClick={handleUseSuggestion}
                    className="text-teal-400 hover:text-teal-300 underline"
                    aria-label={`Use ${suggestedUsername}`}
                  >
                    {suggestedUsername}
                  </button>
                </div>
              )}
            </div>
          )}
        </form>

        {/* Footer */}
        <div 
          className="px-6 py-4 flex justify-end gap-3"
          style={{ 
            borderTop: '1px solid rgba(20, 184, 166, 0.2)' 
          }}
        >
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 rounded-lg text-gray-300 hover:text-white hover:bg-white/10 transition-colors"
            disabled={isLoading}
          >
            Cancel
          </button>
          <button
            type="submit"
            onClick={handleSubmit}
            disabled={isLoading}
            className="px-4 py-2 rounded-lg text-white font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            style={{ 
              background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)',
            }}
          >
            {isLoading ? 'Saving...' : 'Save'}
          </button>
        </div>
      </div>
    </div>
  );
}
