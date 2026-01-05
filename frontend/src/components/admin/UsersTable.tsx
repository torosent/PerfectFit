'use client';

import { useState, useEffect, useCallback } from 'react';
import { getUsers } from '@/lib/api/admin-client';
import { useToken } from '@/lib/stores/auth-store';
import type { AdminUser, PaginatedResponse } from '@/types';

export interface UsersTableProps {
  /** Callback when delete button is clicked for a user */
  onDeleteClick: (user: AdminUser) => void;
  /** Callback when bulk delete guests button is clicked */
  onBulkDeleteClick: () => void;
  /** Trigger to refresh the table data */
  refreshTrigger?: number;
}

/**
 * Format date string to a readable format
 */
function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

/**
 * Loading skeleton for table rows
 */
function TableSkeleton() {
  return (
    <>
      {Array.from({ length: 10 }).map((_, i) => (
        <tr key={i} className="border-b border-gray-800">
          <td className="py-3 px-4">
            <div className="h-4 w-8 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-40 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-24 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-16 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-12 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-20 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-8 w-16 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
        </tr>
      ))}
    </>
  );
}

/**
 * UsersTable - Paginated table displaying all users with admin actions.
 * 
 * Features:
 * - Paginated user list with prev/next controls
 * - Delete button per row
 * - Bulk delete guests button
 * - Deleted status indicator for soft-deleted users
 * - Loading and error states
 */
export function UsersTable({ onDeleteClick, onBulkDeleteClick, refreshTrigger }: UsersTableProps) {
  const token = useToken();
  const [users, setUsers] = useState<PaginatedResponse<AdminUser> | null>(null);
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const pageSize = 20;

  const fetchUsers = useCallback(async () => {
    if (!token) return;

    setIsLoading(true);
    setError(null);

    try {
      const response = await getUsers(token, page, pageSize);
      setUsers(response);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to fetch users';
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }, [token, page, pageSize]);

  // Fetch users on mount and when page changes
  useEffect(() => {
    fetchUsers();
  }, [fetchUsers, refreshTrigger]);

  const handlePrevPage = () => {
    if (page > 1) {
      setPage(page - 1);
    }
  };

  const handleNextPage = () => {
    if (users && page < users.totalPages) {
      setPage(page + 1);
    }
  };

  return (
    <div className="space-y-4">
      {/* Header with Bulk Delete button */}
      <div className="flex justify-between items-center">
        <h3 className="text-lg font-semibold text-white">Users</h3>
        <button
          onClick={onBulkDeleteClick}
          className="px-4 py-2 rounded-lg text-red-400 border border-red-500/30 hover:bg-red-500/10 transition-colors text-sm font-medium"
        >
          Bulk Delete Guests
        </button>
      </div>

      {/* Error state */}
      {error && (
        <div className="p-4 rounded-lg bg-red-500/10 border border-red-500/30 text-red-400">
          {error}
        </div>
      )}

      {/* Table */}
      <div
        className="overflow-x-auto rounded-xl backdrop-blur-sm"
        style={{ background: 'rgba(13, 36, 61, 0.85)', border: '1px solid rgba(56, 97, 140, 0.4)' }}
      >
        <table className="w-full min-w-[800px]">
          <thead>
            <tr className="border-b" style={{ borderColor: 'rgba(56, 97, 140, 0.4)', background: 'rgba(10, 37, 64, 0.6)' }}>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">
                ID
              </th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Email
              </th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Display Name
              </th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Provider
              </th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Role
              </th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Created
              </th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <TableSkeleton />
            ) : !users || users.items.length === 0 ? (
              <tr>
                <td colSpan={7} className="py-12 text-center text-gray-500">
                  No users found.
                </td>
              </tr>
            ) : (
              users.items.map((user, index) => (
                <tr
                  key={user.id}
                  className="transition-colors"
                  style={{
                    borderBottom: '1px solid rgba(56, 97, 140, 0.3)',
                    background: index % 2 === 0 ? 'rgba(10, 37, 64, 0.3)' : 'transparent',
                    opacity: user.isDeleted ? 0.6 : 1,
                  }}
                >
                  <td className="py-3 px-4 text-gray-300 text-sm">
                    {user.id}
                  </td>
                  <td className="py-3 px-4 text-white text-sm">
                    {user.email || <span className="text-gray-500">-</span>}
                  </td>
                  <td className="py-3 px-4">
                    <div className="flex items-center gap-2">
                      {user.avatar && (
                        <span className="text-lg">{user.avatar}</span>
                      )}
                      <span className="text-white text-sm">
                        {user.displayName || <span className="text-gray-500">-</span>}
                      </span>
                      {user.isDeleted && (
                        <span className="text-xs px-2 py-0.5 rounded-full bg-red-500/20 text-red-400">
                          Deleted
                        </span>
                      )}
                    </div>
                  </td>
                  <td className="py-3 px-4 text-gray-400 text-sm capitalize">
                    {user.provider}
                  </td>
                  <td className="py-3 px-4">
                    <span
                      className="text-xs px-2 py-1 rounded-full font-medium"
                      style={{
                        background: user.role === 'Admin'
                          ? 'rgba(20, 184, 166, 0.2)'
                          : 'rgba(100, 116, 139, 0.2)',
                        color: user.role === 'Admin' ? '#14b8a6' : '#94a3b8',
                      }}
                    >
                      {user.role}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-gray-400 text-sm">
                    {formatDate(user.createdAt)}
                  </td>
                  <td className="py-3 px-4">
                    <button
                      onClick={() => onDeleteClick(user)}
                      disabled={user.isDeleted}
                      className="px-3 py-1.5 rounded text-sm text-red-400 hover:bg-red-500/10 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                      aria-label={`Delete ${user.displayName || user.email || 'user'}`}
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination controls */}
      {users && users.totalPages > 0 && (
        <div className="flex justify-between items-center">
          <button
            onClick={handlePrevPage}
            disabled={page <= 1}
            className="px-4 py-2 rounded-lg text-gray-300 hover:bg-white/10 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            aria-label="Previous page"
          >
            Previous
          </button>
          <span className="text-gray-400">
            Page {page} of {users.totalPages}
          </span>
          <button
            onClick={handleNextPage}
            disabled={page >= users.totalPages}
            className="px-4 py-2 rounded-lg text-gray-300 hover:bg-white/10 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            aria-label="Next page"
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}
