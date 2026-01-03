'use client';

import { useState, useCallback } from 'react';
import { bulkDeleteGuests } from '@/lib/api/admin-client';
import { useToken } from '@/lib/stores/auth-store';

export interface BulkDeleteModalProps {
  /** Whether the modal is open */
  isOpen: boolean;
  /** Callback to close the modal */
  onClose: () => void;
  /** Callback when deletion is successful */
  onSuccess: () => void;
}

/**
 * BulkDeleteModal - Confirmation modal for bulk deleting all guest users.
 * 
 * Shows warning about deleting all guest users.
 * Displays count of deleted users after success.
 * Calls bulkDeleteGuests API on confirmation.
 */
export function BulkDeleteModal({ isOpen, onClose, onSuccess }: BulkDeleteModalProps) {
  const token = useToken();
  const [isDeleting, setIsDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [deletedCount, setDeletedCount] = useState<number | null>(null);

  const handleDelete = useCallback(async () => {
    if (!token) {
      setError('You must be logged in to delete users');
      return;
    }

    setIsDeleting(true);
    setError(null);

    try {
      const response = await bulkDeleteGuests(token);
      setDeletedCount(response.deletedCount);
      onSuccess();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to delete guest users';
      setError(message);
    } finally {
      setIsDeleting(false);
    }
  }, [token, onSuccess]);

  const handleClose = useCallback(() => {
    setDeletedCount(null);
    setError(null);
    onClose();
  }, [onClose]);

  if (!isOpen) {
    return null;
  }

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="bulk-delete-title"
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
    >
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-black/60 backdrop-blur-sm"
        onClick={handleClose}
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
            id="bulk-delete-title"
            className="text-xl font-semibold text-white"
          >
            Bulk Delete Guest Users
          </h2>
        </div>

        {/* Content */}
        <div className="px-6 py-4 space-y-4">
          {deletedCount !== null ? (
            // Success state
            <div className="p-4 rounded-lg bg-teal-500/10 border border-teal-500/30">
              <p className="text-teal-300 text-center">
                <span className="text-2xl font-bold block mb-2">{deletedCount}</span>
                guest {deletedCount === 1 ? 'account' : 'accounts'} deleted successfully
              </p>
            </div>
          ) : (
            <>
              {/* Warning message */}
              <div className="p-4 rounded-lg bg-red-500/10 border border-red-500/30">
                <p className="text-red-300 text-sm">
                  <strong>Warning:</strong> This will permanently delete ALL guest user accounts.
                  All game data and scores associated with guest accounts will be lost.
                  This action cannot be undone.
                </p>
              </div>

              <p className="text-gray-400 text-sm">
                Guest accounts are temporary accounts created when users play without signing in.
                Deleting them helps clean up the database and remove inactive anonymous players.
              </p>
            </>
          )}

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
          {deletedCount !== null ? (
            <button
              type="button"
              onClick={handleClose}
              className="px-4 py-2 rounded-lg text-white font-medium transition-colors"
              style={{
                background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)',
              }}
            >
              Done
            </button>
          ) : (
            <>
              <button
                type="button"
                onClick={handleClose}
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
                {isDeleting ? 'Deleting...' : 'Delete All Guests'}
              </button>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
