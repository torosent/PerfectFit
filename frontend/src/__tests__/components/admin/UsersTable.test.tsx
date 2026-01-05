/**
 * UsersTable Component Tests
 *
 * Tests for the admin users table that displays paginated users
 * with delete functionality.
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { UsersTable } from '@/components/admin/UsersTable';
import { getUsers } from '@/lib/api/admin-client';
import type { AdminUser, PaginatedResponse } from '@/types';

// Mock the admin client
jest.mock('@/lib/api/admin-client', () => ({
  getUsers: jest.fn(),
}));

// Mock the auth store
const mockToken = 'test-admin-token';
jest.mock('@/lib/stores/auth-store', () => ({
  useToken: () => mockToken,
}));

const mockGetUsers = getUsers as jest.MockedFunction<typeof getUsers>;

// Helper to create mock users
function createMockUser(overrides: Partial<AdminUser> = {}): AdminUser {
  return {
    id: 1,
    email: 'user@example.com',
    displayName: 'Test User',
    avatar: '😎',
    provider: 'google',
    role: 'User',
    createdAt: '2024-01-01T00:00:00Z',
    lastLoginAt: '2024-01-15T00:00:00Z',
    highScore: 1000,
    gamesPlayed: 10,
    isDeleted: false,
    deletedAt: null,
    ...overrides,
  };
}

function createMockPaginatedResponse(
  users: AdminUser[],
  page: number = 1,
  totalPages: number = 1
): PaginatedResponse<AdminUser> {
  return {
    items: users,
    page,
    pageSize: 20,
    totalCount: users.length,
    totalPages,
  };
}

describe('UsersTable', () => {
  const mockOnDeleteClick = jest.fn();
  const mockOnBulkDeleteClick = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('renders users', () => {
    it('should display users in a table', async () => {
      const mockUsers = [
        createMockUser({ id: 1, email: 'user1@test.com', displayName: 'User One' }),
        createMockUser({ id: 2, email: 'user2@test.com', displayName: 'User Two' }),
      ];
      mockGetUsers.mockResolvedValueOnce(createMockPaginatedResponse(mockUsers));

      render(
        <UsersTable
          onDeleteClick={mockOnDeleteClick}
          onBulkDeleteClick={mockOnBulkDeleteClick}
        />
      );

      await waitFor(() => {
        expect(screen.getByText('User One')).toBeInTheDocument();
        expect(screen.getByText('User Two')).toBeInTheDocument();
      });

      expect(screen.getByText('user1@test.com')).toBeInTheDocument();
      expect(screen.getByText('user2@test.com')).toBeInTheDocument();
    });

    it('should display column headers', async () => {
      mockGetUsers.mockResolvedValueOnce(createMockPaginatedResponse([createMockUser()]));

      render(
        <UsersTable
          onDeleteClick={mockOnDeleteClick}
          onBulkDeleteClick={mockOnBulkDeleteClick}
        />
      );

      await waitFor(() => {
        expect(screen.getByText(/email/i)).toBeInTheDocument();
      });

      expect(screen.getByText(/display name/i)).toBeInTheDocument();
      expect(screen.getByText(/provider/i)).toBeInTheDocument();
      expect(screen.getByText(/role/i)).toBeInTheDocument();
    });

    it('should show deleted indicator for soft-deleted users', async () => {
      const mockUsers = [
        createMockUser({ id: 1, displayName: 'Active User', isDeleted: false }),
        createMockUser({ id: 2, displayName: 'SoftDeleted User', isDeleted: true, deletedAt: '2024-01-20T00:00:00Z' }),
      ];
      mockGetUsers.mockResolvedValueOnce(createMockPaginatedResponse(mockUsers));

      render(
        <UsersTable
          onDeleteClick={mockOnDeleteClick}
          onBulkDeleteClick={mockOnBulkDeleteClick}
        />
      );

      await waitFor(() => {
        expect(screen.getByText('SoftDeleted User')).toBeInTheDocument();
      });

      // Should show a deleted badge indicator
      const deletedBadges = screen.getAllByText(/deleted/i);
      // One badge text for the deleted user row
      expect(deletedBadges.length).toBeGreaterThan(0);
    });
  });

  describe('pagination works', () => {
    it('should show pagination info', async () => {
      const mockUsers = [createMockUser()];
      mockGetUsers.mockResolvedValueOnce(createMockPaginatedResponse(mockUsers, 1, 3));

      render(
        <UsersTable
          onDeleteClick={mockOnDeleteClick}
          onBulkDeleteClick={mockOnBulkDeleteClick}
        />
      );

      await waitFor(() => {
        expect(screen.getByText(/page 1 of 3/i)).toBeInTheDocument();
      });
    });

    it('should call API with next page when Next button is clicked', async () => {
      const mockUsers = [createMockUser()];
      mockGetUsers
        .mockResolvedValueOnce(createMockPaginatedResponse(mockUsers, 1, 3))
        .mockResolvedValueOnce(createMockPaginatedResponse(mockUsers, 2, 3));

      render(
        <UsersTable
          onDeleteClick={mockOnDeleteClick}
          onBulkDeleteClick={mockOnBulkDeleteClick}
        />
      );

      await waitFor(() => {
        expect(screen.getByText(/page 1 of 3/i)).toBeInTheDocument();
      });

      const nextButton = screen.getByRole('button', { name: /next/i });
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(mockGetUsers).toHaveBeenCalledWith(mockToken, 2, 20);
      });
    });

    it('should disable Previous button on first page', async () => {
      const mockUsers = [createMockUser()];
      mockGetUsers.mockResolvedValueOnce(createMockPaginatedResponse(mockUsers, 1, 3));

      render(
        <UsersTable
          onDeleteClick={mockOnDeleteClick}
          onBulkDeleteClick={mockOnBulkDeleteClick}
        />
      );

      await waitFor(() => {
        const prevButton = screen.getByRole('button', { name: /previous/i });
        expect(prevButton).toBeDisabled();
      });
    });

    it('should disable Next button on last page', async () => {
      const mockUsers = [createMockUser()];
      // Return page 1 of 1 (only one page, so next should be disabled)
      mockGetUsers.mockResolvedValueOnce(createMockPaginatedResponse(mockUsers, 1, 1));

      render(
        <UsersTable
          onDeleteClick={mockOnDeleteClick}
          onBulkDeleteClick={mockOnBulkDeleteClick}
        />
      );

      await waitFor(() => {
        const nextButton = screen.getByRole('button', { name: /next/i });
        expect(nextButton).toBeDisabled();
      });
    });
  });

  describe('delete button triggers modal', () => {
    it('should call onDeleteClick with user when delete button is clicked', async () => {
      const mockUser = createMockUser({ id: 1, displayName: 'User One' });
      mockGetUsers.mockResolvedValueOnce(createMockPaginatedResponse([mockUser]));

      render(
        <UsersTable
          onDeleteClick={mockOnDeleteClick}
          onBulkDeleteClick={mockOnBulkDeleteClick}
        />
      );

      await waitFor(() => {
        expect(screen.getByText('User One')).toBeInTheDocument();
      });

      // Use the more specific aria-label to find the row delete button
      const deleteButton = screen.getByRole('button', { name: /delete user one/i });
      fireEvent.click(deleteButton);

      expect(mockOnDeleteClick).toHaveBeenCalledWith(mockUser);
    });
  });

  describe('bulk delete button', () => {
    it('should have a Bulk Delete Guests button', async () => {
      mockGetUsers.mockResolvedValueOnce(createMockPaginatedResponse([createMockUser()]));

      render(
        <UsersTable
          onDeleteClick={mockOnDeleteClick}
          onBulkDeleteClick={mockOnBulkDeleteClick}
        />
      );

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /bulk delete guests/i })).toBeInTheDocument();
      });
    });

    it('should call onBulkDeleteClick when Bulk Delete Guests button is clicked', async () => {
      mockGetUsers.mockResolvedValueOnce(createMockPaginatedResponse([createMockUser()]));

      render(
        <UsersTable
          onDeleteClick={mockOnDeleteClick}
          onBulkDeleteClick={mockOnBulkDeleteClick}
        />
      );

      await waitFor(() => {
        expect(screen.getByText('Test User')).toBeInTheDocument();
      });

      const bulkDeleteButton = screen.getByRole('button', { name: /bulk delete guests/i });
      fireEvent.click(bulkDeleteButton);

      expect(mockOnBulkDeleteClick).toHaveBeenCalled();
    });
  });

  describe('loading state', () => {
    it('should show loading skeleton while fetching users', () => {
      // Never resolve the promise to stay in loading state
      mockGetUsers.mockReturnValueOnce(new Promise(() => {}));

      render(
        <UsersTable
          onDeleteClick={mockOnDeleteClick}
          onBulkDeleteClick={mockOnBulkDeleteClick}
        />
      );

      // Check for skeleton loading indicators (animate-pulse elements)
      const skeletonElements = document.querySelectorAll('.animate-pulse');
      expect(skeletonElements.length).toBeGreaterThan(0);
    });
  });

  describe('error state', () => {
    it('should show error message when API fails', async () => {
      mockGetUsers.mockRejectedValueOnce(new Error('Failed to fetch users'));

      render(
        <UsersTable
          onDeleteClick={mockOnDeleteClick}
          onBulkDeleteClick={mockOnBulkDeleteClick}
        />
      );

      await waitFor(() => {
        expect(screen.getByText(/failed to fetch users/i)).toBeInTheDocument();
      });
    });
  });

  describe('refresh function', () => {
    it('should refetch users when refresh is called', async () => {
      const mockUsers = [createMockUser()];
      mockGetUsers
        .mockResolvedValueOnce(createMockPaginatedResponse(mockUsers))
        .mockResolvedValueOnce(createMockPaginatedResponse(mockUsers));

      const { rerender } = render(
        <UsersTable
          onDeleteClick={mockOnDeleteClick}
          onBulkDeleteClick={mockOnBulkDeleteClick}
          refreshTrigger={0}
        />
      );

      await waitFor(() => {
        expect(mockGetUsers).toHaveBeenCalledTimes(1);
      });

      // Trigger refresh
      rerender(
        <UsersTable
          onDeleteClick={mockOnDeleteClick}
          onBulkDeleteClick={mockOnBulkDeleteClick}
          refreshTrigger={1}
        />
      );

      await waitFor(() => {
        expect(mockGetUsers).toHaveBeenCalledTimes(2);
      });
    });
  });
});
