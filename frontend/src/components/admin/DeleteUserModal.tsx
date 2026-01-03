'use client';

import { useState, useCallback } from 'react';
import { deleteUser } from '@/lib/api/admin-client';
import { useToken } from '@/lib/stores/auth-store';
import type { AdminUser } from '@/types';

export interface DeleteUserModalProps {
  /** User to delete */
  user: AdminUser;
  /** Whether the modal is open */
  isOpen: boolean;
  /** Callback to close the modal */
  onClose: () => void;
  /** Callback when deletion is successful */
  onSuccess: () => void;
}

/**
 * DeleteUserModal - Confirmation modal for deleting a single user.
 * 
 * Shows user information and warning message.
 * Calls deleteUser API on confirmation.
 */
export function DeleteUserModal({ user, isOpen, onClose, onSuccess }: DeleteUserModalProps) {
  const token = useToken();
  const [isDeleting, setIsDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleDelete = useCallback(async () => {
    if (!token) {
      setError('You must be logged in to delete users');
      return;
    }

    setIsDeleting(true);
    setError(null);

    try {
      await deleteUser(user.id, token);
      onSuccess();
      onClose();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to delete user';
      setError(message);
    } finally {
      setIsDeleting(false);
    }
  }, [token, user.id, onSuccess, onClose]);

  if (!isOpen) {
    return null;
  }

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="delete-user-title"
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
          border: '1px solid rgba(239, 68, 68, 0.3)',
        }}
      >
        {/* Header */}
        <div
          className="px-6 py-4"
          style={{
            borderBottom: '1px solid rgba(239, 68, 68, 0.2)',
          }}
        >
          <h2
            id="delete-user-title"
            className="text-xl font-semibold text-white"
          >
            Delete User
          </h2>
        </div>

        {/* Content */}
        <div className="px-6 py-4 space-y-4">
          {/* User info */}
          <div className="p-4 rounded-lg" style={{ backgroundColor: 'rgba(10, 37, 64, 0.6)' }}>
            <div className="flex items-center gap-3">
              {user.avatar && (
                <span className="text-2xl">{user.avatar}</span>
              )}
              <div>
                <p className="text-white font-medium">
                  {user.displayName || user.username || 'Unknown User'}
                </p>
                <p className="text-gray-400 text-sm">
                  {user.email || 'No email'}
                </p>
              </div>
            </div>
          </div>

          {/* Warning message */}
          <div className="p-4 rounded-lg bg-red-500/10 border border-red-500/30">
            <p className="text-red-300 text-sm">
              <strong>Warning:</strong> This will permanently delete this user account.
              All game data and scores associated with this account will be lost.
              This action cannot be undone.
            </p>
          </div>

          {/* Error message */}
          {error && (
            <div className="p-3 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400 text-sm">
              {error}
            </div>
          )}
        </div>

        {/* Footer */}
        <div
          className="px-6 py-4 flex justify-end gap-3"
          style={{
            borderTop: '1px solid rgba(239, 68, 68, 0.2)',
          }}
        >
          <button
            type="button"
            onClick={onClose}
            disabled={isDeleting}
            className="px-4 py-2 rounded-lg text-gray-300 hover:text-white hover:bg-white/10 transition-colors disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={handleDelete}
            disabled={isDeleting}
            className="px-4 py-2 rounded-lg bg-red-600 hover:bg-red-500 text-white font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isDeleting ? 'Deleting...' : 'Delete'}
          </button>
        </div>
      </div>
    </div>
  );
}
