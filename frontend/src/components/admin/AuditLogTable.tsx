'use client';

import { useState, useEffect, useCallback } from 'react';
import { getAuditLogs } from '@/lib/api/admin-client';
import { useToken } from '@/lib/stores/auth-store';
import type { AuditLog, PaginatedResponse } from '@/types';

export interface AuditLogTableProps {
  /** Trigger to refresh the table data */
  refreshTrigger?: number;
}

/**
 * Format timestamp to a readable format with time
 */
function formatTimestamp(timestamp: string): string {
  const date = new Date(timestamp);
  return date.toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  });
}

/**
 * Get action badge color based on action type
 * Handles both backend enum values (DeleteUser) and display values (delete_user)
 */
function getActionColor(action: string): { bg: string; text: string } {
  const normalizedAction = action.toLowerCase().replace(/_/g, '');
  switch (normalizedAction) {
    case 'deleteuser':
      return { bg: 'rgba(239, 68, 68, 0.2)', text: '#f87171' };
    case 'bulkdeleteguests':
    case 'bulkdeleteusers':
      return { bg: 'rgba(239, 68, 68, 0.2)', text: '#f87171' };
    case 'updateuser':
      return { bg: 'rgba(59, 130, 246, 0.2)', text: '#60a5fa' };
    default:
      return { bg: 'rgba(100, 116, 139, 0.2)', text: '#94a3b8' };
  }
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
            <div className="h-4 w-36 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-32 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-6 w-24 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-32 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
          <td className="py-3 px-4">
            <div className="h-4 w-48 rounded animate-pulse" style={{ backgroundColor: 'rgba(20, 184, 166, 0.15)' }} />
          </td>
        </tr>
      ))}
    </>
  );
}

/**
 * AuditLogTable - Paginated table displaying admin audit logs.
 * 
 * Features:
 * - Paginated audit log list with prev/next controls
 * - Formatted timestamps
 * - Action type badges
 * - Loading and error states
 */
export function AuditLogTable({ refreshTrigger }: AuditLogTableProps) {
  const token = useToken();
  const [logs, setLogs] = useState<PaginatedResponse<AuditLog> | null>(null);
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const pageSize = 20;

  const fetchLogs = useCallback(async () => {
    if (!token) return;

    setIsLoading(true);
    setError(null);

    try {
      const response = await getAuditLogs(token, page, pageSize);
      setLogs(response);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to fetch audit logs';
      setError(message);
    } finally {
      setIsLoading(false);
    }
  }, [token, page, pageSize]);

  // Fetch logs on mount and when page changes
  useEffect(() => {
    fetchLogs();
  }, [fetchLogs, refreshTrigger]);

  const handlePrevPage = () => {
    if (page > 1) {
      setPage(page - 1);
    }
  };

  const handleNextPage = () => {
    if (logs && page < logs.totalPages) {
      setPage(page + 1);
    }
  };

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h3 className="text-lg font-semibold text-white">Audit Logs</h3>
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
        <table className="w-full min-w-[900px]">
          <thead>
            <tr className="border-b" style={{ borderColor: 'rgba(56, 97, 140, 0.4)', background: 'rgba(10, 37, 64, 0.6)' }}>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Timestamp
              </th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Admin
              </th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Action
              </th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Target User
              </th>
              <th className="py-3 px-4 text-left text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Details
              </th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <TableSkeleton />
            ) : !logs || logs.items.length === 0 ? (
              <tr>
                <td colSpan={5} className="py-12 text-center text-gray-500">
                  No audit logs found.
                </td>
              </tr>
            ) : (
              logs.items.map((log, index) => {
                const actionColor = getActionColor(log.action);
                return (
                  <tr
                    key={log.id}
                    className="transition-colors"
                    style={{
                      borderBottom: '1px solid rgba(56, 97, 140, 0.3)',
                      background: index % 2 === 0 ? 'rgba(10, 37, 64, 0.3)' : 'transparent',
                    }}
                  >
                    <td className="py-3 px-4 text-gray-400 text-sm whitespace-nowrap">
                      {formatTimestamp(log.timestamp)}
                    </td>
                    <td className="py-3 px-4 text-white text-sm">
                      <div>
                        <span className="text-gray-500 text-xs">#{log.adminUserId}</span>
                        {log.adminEmail && (
                          <p className="text-gray-300 text-xs truncate max-w-[150px]">
                            {log.adminEmail}
                          </p>
                        )}
                      </div>
                    </td>
                    <td className="py-3 px-4">
                      <span
                        className="text-xs px-2 py-1 rounded-full font-medium"
                        style={{
                          background: actionColor.bg,
                          color: actionColor.text,
                        }}
                      >
                        {log.action.replace(/_/g, ' ')}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-sm">
                      {log.targetUserId ? (
                        <div>
                          <span className="text-gray-500 text-xs">#{log.targetUserId}</span>
                          {log.targetUserEmail && (
                            <p className="text-gray-300 text-xs truncate max-w-[150px]">
                              {log.targetUserEmail}
                            </p>
                          )}
                        </div>
                      ) : (
                        <span className="text-gray-500">-</span>
                      )}
                    </td>
                    <td className="py-3 px-4 text-gray-400 text-sm">
                      {log.details || <span className="text-gray-500">-</span>}
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination controls */}
      {logs && logs.totalPages > 0 && (
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
            Page {page} of {logs.totalPages}
          </span>
          <button
            onClick={handleNextPage}
            disabled={page >= logs.totalPages}
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
