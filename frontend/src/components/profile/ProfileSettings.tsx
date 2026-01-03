'use client';

import { useState, useEffect, useCallback } from 'react';
import { EmojiPicker } from './EmojiPicker';
import { updateProfile, deleteAccount } from '@/lib/api/profile-client';
import { useUser, useToken, useAuthStore, useIsGuest } from '@/lib/stores/auth-store';
import type { UserProfile } from '@/types';

export interface ProfileSettingsProps {
  /** Whether the modal is open */
  isOpen: boolean;
  /** Callback to close the modal */
  onClose: () => void;
}

/**
 * Profile settings modal for editing display name and avatar.
 * Uses the updateProfile API to save changes.
 */
export function ProfileSettings({ isOpen, onClose }: ProfileSettingsProps) {
  const user = useUser();
  const token = useToken();
  const setUser = useAuthStore((state) => state.setUser);
  const logout = useAuthStore((state) => state.logout);
  const isGuest = useIsGuest();

  const [displayName, setDisplayName] = useState(user?.displayName || '');
  const [avatar, setAvatar] = useState(user?.avatar || '');
  const [showEmojiPicker, setShowEmojiPicker] = useState(false);
  const [error, setError] = useState('');
  const [suggestedDisplayName, setSuggestedDisplayName] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  // Reset form when modal opens/closes or user changes
  useEffect(() => {
    if (isOpen) {
      setDisplayName(user?.displayName || '');
      setAvatar(user?.avatar || '');
      setError('');
      setSuggestedDisplayName(null);
      setShowDeleteConfirm(false);
      setShowEmojiPicker(false);
    }
  }, [isOpen, user?.displayName, user?.avatar]);

  const handleSubmit = useCallback(async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!token) {
      setError('You must be logged in to update your profile');
      return;
    }

    // Build request with only changed fields
    const request: { displayName?: string; avatar?: string } = {};
    
    // Only include displayName if changed (and not a guest)
    if (!isGuest && displayName !== user?.displayName) {
      request.displayName = displayName;
    }
    
    // Only include avatar if changed
    if (avatar !== (user?.avatar || '')) {
      request.avatar = avatar || undefined;
    }
    
    // Don't submit if nothing changed
    if (Object.keys(request).length === 0) {
      onClose();
      return;
    }

    setIsLoading(true);
    setError('');
    setSuggestedDisplayName(null);

    try {
      const response = await updateProfile(request, token);

      if (response.success && response.profile) {
        // Update the user in the store
        if (user && setUser) {
          const updatedUser: UserProfile = {
            ...user,
            displayName: response.profile.displayName,
            avatar: response.profile.avatar,
          };
          setUser(updatedUser);
        }
        onClose();
      } else {
        setError(response.errorMessage || 'Failed to update profile');
        if (response.suggestedDisplayName) {
          setSuggestedDisplayName(response.suggestedDisplayName);
        }
      }
    } catch {
      setError('An unexpected error occurred');
    } finally {
      setIsLoading(false);
    }
  }, [displayName, avatar, token, user, setUser, onClose, isGuest]);

  const handleUseSuggestion = useCallback(() => {
    if (suggestedDisplayName) {
      setDisplayName(suggestedDisplayName);
      setSuggestedDisplayName(null);
      setError('');
    }
  }, [suggestedDisplayName]);

  const handleDeleteAccount = useCallback(async () => {
    if (!token) {
      setError('You must be logged in to delete your account');
      return;
    }

    setIsDeleting(true);
    setError('');

    try {
      const result = await deleteAccount(token);

      if (result.success) {
        onClose();
        await logout();
      } else {
        setError(result.error || 'Failed to delete account');
        setShowDeleteConfirm(false);
      }
    } catch {
      setError('An unexpected error occurred');
      setShowDeleteConfirm(false);
    } finally {
      setIsDeleting(false);
    }
  }, [token, logout, onClose]);

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
          {/* Display Name field */}
          <div>
            <label 
              htmlFor="displayName"
              className="block text-sm font-medium text-gray-300 mb-2"
            >
              Display Name
            </label>
            <input
              id="displayName"
              type="text"
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              className="w-full px-4 py-2 rounded-lg bg-gray-800/50 text-white border border-gray-700 focus:border-teal-500 focus:ring-2 focus:ring-teal-500/30 focus:outline-none transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              placeholder="Enter your display name"
              disabled={isLoading || isGuest}
              maxLength={30}
            />
            {isGuest && (
              <p className="mt-1 text-xs text-gray-400">
                Sign in with Microsoft or create an account to customize your display name.
              </p>
            )}
          </div>

          {/* Avatar picker */}
          <div>
            <label className="block text-sm font-medium text-gray-300 mb-2">
              Avatar
            </label>
            <div className="flex items-center gap-4">
              <div 
                className="w-14 h-14 rounded-full flex items-center justify-center text-3xl"
                style={{ backgroundColor: 'rgba(20, 184, 166, 0.2)' }}
              >
                {avatar || '👤'}
              </div>
              {!isGuest && (
                <button
                  type="button"
                  onClick={() => setShowEmojiPicker(true)}
                  className="px-3 py-1.5 rounded-lg text-sm text-teal-400 border border-teal-500/30 hover:bg-teal-500/10 transition-colors"
                >
                  Change
                </button>
              )}
            </div>
            {isGuest && (
              <p className="mt-2 text-xs text-gray-400">
                Sign in with Microsoft or create an account to customize your avatar.
              </p>
            )}
          </div>

          {/* Emoji Picker Popup */}
          {showEmojiPicker && (
            <div className="fixed inset-0 z-[60] flex items-center justify-center p-4">
              <div 
                className="absolute inset-0 bg-black/60"
                onClick={() => setShowEmojiPicker(false)}
              />
              <div 
                className="relative rounded-xl p-4 shadow-2xl max-w-sm w-full"
                style={{ 
                  backgroundColor: '#0d243d', 
                  border: '1px solid rgba(20, 184, 166, 0.3)' 
                }}
              >
                <div className="flex justify-between items-center mb-3">
                  <h3 className="text-lg font-medium text-white">Select Avatar</h3>
                  <button
                    type="button"
                    onClick={() => setShowEmojiPicker(false)}
                    className="text-gray-400 hover:text-white"
                  >
                    ✕
                  </button>
                </div>
                <EmojiPicker 
                  selected={avatar} 
                  onSelect={(emoji) => {
                    setAvatar(emoji);
                    setShowEmojiPicker(false);
                  }} 
                />
              </div>
            </div>
          )}

          {/* Error message */}
          {error && (
            <div className="p-3 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400 text-sm">
              {error}
              {suggestedDisplayName && (
                <div className="mt-2">
                  <span className="text-gray-400">Try: </span>
                  <button
                    type="button"
                    onClick={handleUseSuggestion}
                    className="text-teal-400 hover:text-teal-300 underline"
                    aria-label={`Use ${suggestedDisplayName}`}
                  >
                    {suggestedDisplayName}
                  </button>
                </div>
              )}
            </div>
          )}

          {/* Delete Account Section - Hidden for guests */}
          {!isGuest && (
            <div 
              className="pt-4 mt-2"
              style={{ borderTop: '1px solid rgba(239, 68, 68, 0.2)' }}
            >
              <h3 className="text-sm font-medium text-red-400 mb-2">Danger Zone</h3>
              {!showDeleteConfirm ? (
                <button
                  type="button"
                  onClick={() => setShowDeleteConfirm(true)}
                  className="w-full px-4 py-2 rounded-lg text-red-400 border border-red-500/30 hover:bg-red-500/10 transition-colors text-sm"
                  disabled={isLoading}
                >
                  Delete Account
                </button>
              ) : (
                <div className="p-3 rounded-lg bg-red-500/10 border border-red-500/30 space-y-3">
                  <p className="text-sm text-red-300">
                    Are you sure? This will permanently delete your account, scores, and all game data. This action cannot be undone.
                  </p>
                  <div className="flex gap-2">
                    <button
                      type="button"
                      onClick={() => setShowDeleteConfirm(false)}
                      className="flex-1 px-3 py-2 rounded-lg text-gray-300 hover:bg-white/10 transition-colors text-sm"
                      disabled={isDeleting}
                    >
                      Cancel
                    </button>
                    <button
                      type="button"
                      onClick={handleDeleteAccount}
                      disabled={isDeleting}
                      className="flex-1 px-3 py-2 rounded-lg bg-red-600 hover:bg-red-500 text-white font-medium transition-colors text-sm disabled:opacity-50"
                    >
                      {isDeleting ? 'Deleting...' : 'Yes, Delete'}
                    </button>
                  </div>
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
            {isGuest ? 'Close' : 'Cancel'}
          </button>
          {!isGuest && (
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
          )}
        </div>
      </div>
    </div>
  );
}
