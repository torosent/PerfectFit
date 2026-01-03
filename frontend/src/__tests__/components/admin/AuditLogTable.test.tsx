/**
 * AuditLogTable Component Tests
 *
 * Tests for the admin audit log table component.
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { AuditLogTable } from '@/components/admin/AuditLogTable';
import { getAuditLogs } from '@/lib/api/admin-client';
import type { AuditLog, PaginatedResponse } from '@/types';

// Mock the admin client
jest.mock('@/lib/api/admin-client', () => ({
  getAuditLogs: jest.fn(),
}));

// Mock the auth store
const mockToken = 'test-admin-token';
jest.mock('@/lib/stores/auth-store', () => ({
  useToken: () => mockToken,
}));

const mockGetAuditLogs = getAuditLogs as jest.MockedFunction<typeof getAuditLogs>;

// Helper to create mock audit logs
function createMockAuditLog(overrides: Partial<AuditLog> = {}): AuditLog {
  return {
    id: 'log-1',
    adminUserId: 1,
    adminEmail: 'admin@example.com',
    action: 'DeleteUser',
    targetUserId: 42,
    targetUserEmail: 'user@example.com',
    details: 'Deleted user for policy violation',
    timestamp: '2024-01-15T10:30:00Z',
    ...overrides,
  };
}

// Helper to create paginated response
function createMockPaginatedResponse(
  items: AuditLog[],
  page: number = 1,
  totalCount?: number
): PaginatedResponse<AuditLog> {
  const total = totalCount ?? items.length;
  return {
    items,
    page,
    pageSize: 20,
    totalCount: total,
    totalPages: Math.ceil(total / 20),
  };
}

describe('AuditLogTable', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('log display', () => {
    it('should display audit logs with correct information', async () => {
      const mockLogs = [
        createMockAuditLog({
          id: 'log-1',
          adminEmail: 'admin@example.com',
          action: 'DeleteUser',
          targetUserEmail: 'deleted@example.com',
          details: 'User violated terms',
        }),
      ];
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse(mockLogs));

      render(<AuditLogTable />);

      await waitFor(() => {
        expect(screen.getByText('admin@example.com')).toBeInTheDocument();
        expect(screen.getByText('deleted@example.com')).toBeInTheDocument();
        expect(screen.getByText('User violated terms')).toBeInTheDocument();
      });
    });

    it('should display formatted timestamps', async () => {
      const mockLogs = [
        createMockAuditLog({
          timestamp: '2024-06-15T14:30:45Z',
        }),
      ];
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse(mockLogs));

      render(<AuditLogTable />);

      await waitFor(() => {
        // Should show formatted date - format depends on locale but should include June 15
        expect(screen.getByText(/Jun/)).toBeInTheDocument();
        expect(screen.getByText(/15/)).toBeInTheDocument();
      });
    });

    it('should display admin user ID', async () => {
      const mockLogs = [
        createMockAuditLog({
          adminUserId: 123,
          adminEmail: null,
        }),
      ];
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse(mockLogs));

      render(<AuditLogTable />);

      await waitFor(() => {
        expect(screen.getByText('#123')).toBeInTheDocument();
      });
    });

    it('should display target user ID', async () => {
      const mockLogs = [
        createMockAuditLog({
          targetUserId: 456,
          targetUserEmail: null,
        }),
      ];
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse(mockLogs));

      render(<AuditLogTable />);

      await waitFor(() => {
        expect(screen.getByText('#456')).toBeInTheDocument();
      });
    });

    it('should show dash when target user is null', async () => {
      const mockLogs = [
        createMockAuditLog({
          targetUserId: null,
          targetUserEmail: null,
        }),
      ];
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse(mockLogs));

      render(<AuditLogTable />);

      await waitFor(() => {
        // Should show "-" for no target user
        const cells = screen.getAllByText('-');
        expect(cells.length).toBeGreaterThan(0);
      });
    });

    it('should show dash when details is null', async () => {
      const mockLogs = [
        createMockAuditLog({
          details: null,
        }),
      ];
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse(mockLogs));

      render(<AuditLogTable />);

      await waitFor(() => {
        const cells = screen.getAllByText('-');
        expect(cells.length).toBeGreaterThan(0);
      });
    });

    it('should show empty state when no logs exist', async () => {
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse([]));

      render(<AuditLogTable />);

      await waitFor(() => {
        expect(screen.getByText(/no audit logs found/i)).toBeInTheDocument();
      });
    });

    it('should display action as badge with formatted text', async () => {
      const mockLogs = [
        createMockAuditLog({
          action: 'BulkDeleteGuests',
        }),
      ];
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse(mockLogs));

      render(<AuditLogTable />);

      await waitFor(() => {
        // Action should be displayed with underscores replaced by spaces
        expect(screen.getByText(/BulkDeleteGuests/)).toBeInTheDocument();
      });
    });
  });

  describe('pagination', () => {
    it('should display page information', async () => {
      const mockLogs = Array.from({ length: 20 }, (_, i) =>
        createMockAuditLog({ id: `log-${i}` })
      );
      mockGetAuditLogs.mockResolvedValueOnce(
        createMockPaginatedResponse(mockLogs, 1, 45)
      );

      render(<AuditLogTable />);

      await waitFor(() => {
        expect(screen.getByText(/page 1 of 3/i)).toBeInTheDocument();
      });
    });

    it('should call API with next page when next button is clicked', async () => {
      const mockLogs = Array.from({ length: 20 }, (_, i) =>
        createMockAuditLog({ id: `log-${i}` })
      );
      mockGetAuditLogs.mockResolvedValue(
        createMockPaginatedResponse(mockLogs, 1, 45)
      );

      render(<AuditLogTable />);

      await waitFor(() => {
        expect(screen.getByText(/page 1 of 3/i)).toBeInTheDocument();
      });

      const nextButton = screen.getByRole('button', { name: /next/i });
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(mockGetAuditLogs).toHaveBeenCalledWith(mockToken, 2, 20);
      });
    });

    it('should call API with previous page when previous button is clicked', async () => {
      const mockLogs = Array.from({ length: 20 }, (_, i) =>
        createMockAuditLog({ id: `log-${i}` })
      );
      
      // First call returns page 1
      mockGetAuditLogs.mockResolvedValueOnce(
        createMockPaginatedResponse(mockLogs, 1, 45)
      );
      // Second call returns page 2
      mockGetAuditLogs.mockResolvedValueOnce({
        ...createMockPaginatedResponse(mockLogs, 2, 45),
        page: 2,
      });
      // Third call returns page 1 again
      mockGetAuditLogs.mockResolvedValueOnce(
        createMockPaginatedResponse(mockLogs, 1, 45)
      );

      render(<AuditLogTable />);

      await waitFor(() => {
        expect(screen.getByText(/page 1 of 3/i)).toBeInTheDocument();
      });

      // Go to page 2
      const nextButton = screen.getByRole('button', { name: /next/i });
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(mockGetAuditLogs).toHaveBeenCalledWith(mockToken, 2, 20);
      });

      // Go back to page 1
      const prevButton = screen.getByRole('button', { name: /previous/i });
      fireEvent.click(prevButton);

      await waitFor(() => {
        expect(mockGetAuditLogs).toHaveBeenCalledWith(mockToken, 1, 20);
      });
    });

    it('should disable previous button on first page', async () => {
      const mockLogs = [createMockAuditLog()];
      mockGetAuditLogs.mockResolvedValueOnce(
        createMockPaginatedResponse(mockLogs, 1, 45)
      );

      render(<AuditLogTable />);

      await waitFor(() => {
        const prevButton = screen.getByRole('button', { name: /previous/i });
        expect(prevButton).toBeDisabled();
      });
    });

    it('should disable next button on last page', async () => {
      const mockLogs = [createMockAuditLog()];
      // Start with page 1 of 1 to ensure next button is disabled
      mockGetAuditLogs.mockResolvedValueOnce({
        items: mockLogs,
        page: 1,
        pageSize: 20,
        totalCount: 1,
        totalPages: 1,
      });

      render(<AuditLogTable />);

      await waitFor(() => {
        const nextButton = screen.getByRole('button', { name: /next/i });
        expect(nextButton).toBeDisabled();
      });
    });
  });

  describe('loading state', () => {
    it('should show loading skeleton while fetching', async () => {
      let resolvePromise: (value: PaginatedResponse<AuditLog>) => void;
      const pendingPromise = new Promise<PaginatedResponse<AuditLog>>((resolve) => {
        resolvePromise = resolve;
      });
      mockGetAuditLogs.mockReturnValueOnce(pendingPromise);

      render(<AuditLogTable />);

      // Loading skeletons should be visible
      const skeletons = document.querySelectorAll('.animate-pulse');
      expect(skeletons.length).toBeGreaterThan(0);

      // Resolve the promise
      resolvePromise!(createMockPaginatedResponse([createMockAuditLog()]));

      await waitFor(() => {
        expect(document.querySelectorAll('.animate-pulse').length).toBe(0);
      });
    });
  });

  describe('error state', () => {
    it('should show error message when API call fails', async () => {
      mockGetAuditLogs.mockRejectedValueOnce(new Error('Failed to fetch audit logs'));

      render(<AuditLogTable />);

      await waitFor(() => {
        expect(screen.getByText(/failed to fetch audit logs/i)).toBeInTheDocument();
      });
    });

    it('should show generic error message for non-Error exceptions', async () => {
      mockGetAuditLogs.mockRejectedValueOnce('Unknown error');

      render(<AuditLogTable />);

      await waitFor(() => {
        expect(screen.getByText(/failed to fetch audit logs/i)).toBeInTheDocument();
      });
    });
  });

  describe('action badge colors', () => {
    it('should show red color for DeleteUser action', async () => {
      const mockLogs = [
        createMockAuditLog({
          action: 'DeleteUser',
        }),
      ];
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse(mockLogs));

      render(<AuditLogTable />);

      await waitFor(() => {
        const badge = screen.getByText(/DeleteUser/);
        // Red color for delete actions
        expect(badge).toHaveStyle({ color: '#f87171' });
      });
    });

    it('should show red color for BulkDeleteGuests action', async () => {
      const mockLogs = [
        createMockAuditLog({
          action: 'BulkDeleteGuests',
        }),
      ];
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse(mockLogs));

      render(<AuditLogTable />);

      await waitFor(() => {
        const badge = screen.getByText(/BulkDeleteGuests/);
        expect(badge).toHaveStyle({ color: '#f87171' });
      });
    });

    it('should show red color for BulkDeleteUsers action', async () => {
      const mockLogs = [
        createMockAuditLog({
          action: 'BulkDeleteUsers',
        }),
      ];
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse(mockLogs));

      render(<AuditLogTable />);

      await waitFor(() => {
        const badge = screen.getByText(/BulkDeleteUsers/);
        expect(badge).toHaveStyle({ color: '#f87171' });
      });
    });

    it('should show blue color for UpdateUser action', async () => {
      const mockLogs = [
        createMockAuditLog({
          action: 'UpdateUser',
        }),
      ];
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse(mockLogs));

      render(<AuditLogTable />);

      await waitFor(() => {
        const badge = screen.getByText(/UpdateUser/);
        expect(badge).toHaveStyle({ color: '#60a5fa' });
      });
    });

    it('should show gray color for unknown actions', async () => {
      const mockLogs = [
        createMockAuditLog({
          action: 'SomeUnknownAction',
        }),
      ];
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse(mockLogs));

      render(<AuditLogTable />);

      await waitFor(() => {
        const badge = screen.getByText(/SomeUnknownAction/);
        expect(badge).toHaveStyle({ color: '#94a3b8' });
      });
    });

    it('should handle lowercase action names correctly', async () => {
      const mockLogs = [
        createMockAuditLog({
          action: 'deleteuser',
        }),
      ];
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse(mockLogs));

      render(<AuditLogTable />);

      await waitFor(() => {
        const badge = screen.getByText(/deleteuser/);
        // Should still map to red color
        expect(badge).toHaveStyle({ color: '#f87171' });
      });
    });

    it('should handle action names with underscores correctly', async () => {
      const mockLogs = [
        createMockAuditLog({
          action: 'delete_user',
        }),
      ];
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse(mockLogs));

      render(<AuditLogTable />);

      await waitFor(() => {
        // Action text should have underscores replaced with spaces in display
        const badge = screen.getByText(/delete user/i);
        expect(badge).toHaveStyle({ color: '#f87171' });
      });
    });
  });

  describe('refresh trigger', () => {
    it('should refetch logs when refreshTrigger changes', async () => {
      const mockLogs = [createMockAuditLog()];
      mockGetAuditLogs.mockResolvedValue(createMockPaginatedResponse(mockLogs));

      const { rerender } = render(<AuditLogTable refreshTrigger={0} />);

      await waitFor(() => {
        expect(mockGetAuditLogs).toHaveBeenCalledTimes(1);
      });

      rerender(<AuditLogTable refreshTrigger={1} />);

      await waitFor(() => {
        expect(mockGetAuditLogs).toHaveBeenCalledTimes(2);
      });
    });
  });

  describe('table structure', () => {
    it('should render table headers', async () => {
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse([]));

      render(<AuditLogTable />);

      await waitFor(() => {
        expect(screen.getByText(/timestamp/i)).toBeInTheDocument();
        expect(screen.getByText(/admin/i)).toBeInTheDocument();
        expect(screen.getByText(/action/i)).toBeInTheDocument();
        expect(screen.getByText(/target user/i)).toBeInTheDocument();
        expect(screen.getByText(/details/i)).toBeInTheDocument();
      });
    });

    it('should render component title', async () => {
      mockGetAuditLogs.mockResolvedValueOnce(createMockPaginatedResponse([]));

      render(<AuditLogTable />);

      await waitFor(() => {
        expect(screen.getByText('Audit Logs')).toBeInTheDocument();
      });
    });
  });
});
