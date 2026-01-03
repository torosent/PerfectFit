/**
 * DeleteUserModal Component Tests
 *
 * Tests for the single user delete confirmation modal.
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { DeleteUserModal } from '@/components/admin/DeleteUserModal';
import { deleteUser } from '@/lib/api/admin-client';
import type { AdminUser } from '@/types';

// Mock the admin client
jest.mock('@/lib/api/admin-client', () => ({
  deleteUser: jest.fn(),
}));

// Mock the auth store
const mockToken = 'test-admin-token';
jest.mock('@/lib/stores/auth-store', () => ({
  useToken: () => mockToken,
}));

const mockDeleteUser = deleteUser as jest.MockedFunction<typeof deleteUser>;

// Helper to create mock user
function createMockUser(overrides: Partial<AdminUser> = {}): AdminUser {
  return {
    id: 1,
    email: 'user@example.com',
    displayName: 'Test User',
    username: 'testuser',
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

describe('DeleteUserModal', () => {
  const mockOnClose = jest.fn();
  const mockOnSuccess = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('renders user info', () => {
    it('should display user information in the modal', () => {
      const mockUser = createMockUser({
        displayName: 'John Doe',
        email: 'john@example.com',
      });

      render(
        <DeleteUserModal
          user={mockUser}
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      expect(screen.getByText('John Doe')).toBeInTheDocument();
      expect(screen.getByText('john@example.com')).toBeInTheDocument();
    });

    it('should show warning message about deletion', () => {
      const mockUser = createMockUser();

      render(
        <DeleteUserModal
          user={mockUser}
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      expect(screen.getByText(/permanently delete/i)).toBeInTheDocument();
    });

    it('should not render when isOpen is false', () => {
      const mockUser = createMockUser();

      render(
        <DeleteUserModal
          user={mockUser}
          isOpen={false}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });

    it('should render dialog when isOpen is true', () => {
      const mockUser = createMockUser();

      render(
        <DeleteUserModal
          user={mockUser}
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });
  });

  describe('calls API on confirm', () => {
    it('should call deleteUser API when confirm button is clicked', async () => {
      const mockUser = createMockUser({ id: 42 });
      mockDeleteUser.mockResolvedValueOnce(undefined);

      render(
        <DeleteUserModal
          user={mockUser}
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /confirm|delete/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(mockDeleteUser).toHaveBeenCalledWith(42, mockToken);
      });
    });

    it('should call onSuccess and onClose after successful deletion', async () => {
      const mockUser = createMockUser();
      mockDeleteUser.mockResolvedValueOnce(undefined);

      render(
        <DeleteUserModal
          user={mockUser}
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /confirm|delete/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(mockOnSuccess).toHaveBeenCalled();
        expect(mockOnClose).toHaveBeenCalled();
      });
    });

    it('should show error message when deletion fails', async () => {
      const mockUser = createMockUser();
      mockDeleteUser.mockRejectedValueOnce(new Error('Failed to delete user'));

      render(
        <DeleteUserModal
          user={mockUser}
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /confirm|delete/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText(/failed to delete/i)).toBeInTheDocument();
      });

      expect(mockOnSuccess).not.toHaveBeenCalled();
      expect(mockOnClose).not.toHaveBeenCalled();
    });

    it('should show loading state while deleting', async () => {
      const mockUser = createMockUser();
      // Create a pending promise
      let resolvePromise: () => void;
      const pendingPromise = new Promise<void>((resolve) => {
        resolvePromise = resolve;
      });
      mockDeleteUser.mockReturnValueOnce(pendingPromise);

      render(
        <DeleteUserModal
          user={mockUser}
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /confirm|delete/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(confirmButton).toBeDisabled();
      });

      resolvePromise!();
    });
  });

  describe('cancel behavior', () => {
    it('should call onClose when cancel button is clicked', () => {
      const mockUser = createMockUser();

      render(
        <DeleteUserModal
          user={mockUser}
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const cancelButton = screen.getByRole('button', { name: /cancel/i });
      fireEvent.click(cancelButton);

      expect(mockOnClose).toHaveBeenCalled();
    });
  });
});
