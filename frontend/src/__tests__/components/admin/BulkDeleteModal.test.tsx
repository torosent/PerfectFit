/**
 * BulkDeleteModal Component Tests
 *
 * Tests for the bulk delete guest users confirmation modal.
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BulkDeleteModal } from '@/components/admin/BulkDeleteModal';
import { bulkDeleteGuests } from '@/lib/api/admin-client';

// Mock the admin client
jest.mock('@/lib/api/admin-client', () => ({
  bulkDeleteGuests: jest.fn(),
}));

// Mock the auth store
const mockToken = 'test-admin-token';
jest.mock('@/lib/stores/auth-store', () => ({
  useToken: () => mockToken,
}));

const mockBulkDeleteGuests = bulkDeleteGuests as jest.MockedFunction<typeof bulkDeleteGuests>;

describe('BulkDeleteModal', () => {
  const mockOnClose = jest.fn();
  const mockOnSuccess = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('modal rendering', () => {
    it('should not render when isOpen is false', () => {
      render(
        <BulkDeleteModal
          isOpen={false}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });

    it('should render dialog when isOpen is true', () => {
      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    it('should display warning message about bulk deletion', () => {
      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      expect(screen.getByText(/warning/i)).toBeInTheDocument();
      expect(screen.getByText(/permanently delete ALL guest user accounts/i)).toBeInTheDocument();
      expect(screen.getByText(/cannot be undone/i)).toBeInTheDocument();
    });

    it('should display explanation about guest accounts', () => {
      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      expect(screen.getByText(/temporary accounts/i)).toBeInTheDocument();
    });

    it('should display modal title', () => {
      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      expect(screen.getByText('Bulk Delete Guest Users')).toBeInTheDocument();
    });
  });

  describe('API call on confirm', () => {
    it('should call bulkDeleteGuests API when confirm button is clicked', async () => {
      mockBulkDeleteGuests.mockResolvedValueOnce({
        deletedCount: 5,
        message: 'Successfully deleted 5 guests',
      });

      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /delete all guests/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(mockBulkDeleteGuests).toHaveBeenCalledWith(mockToken);
      });
    });

    it('should call onSuccess after successful deletion', async () => {
      mockBulkDeleteGuests.mockResolvedValueOnce({
        deletedCount: 10,
        message: 'Successfully deleted 10 guests',
      });

      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /delete all guests/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(mockOnSuccess).toHaveBeenCalled();
      });
    });

    it('should show loading state while deleting', async () => {
      let resolvePromise: (value: { deletedCount: number; message: string }) => void;
      const pendingPromise = new Promise<{ deletedCount: number; message: string }>((resolve) => {
        resolvePromise = resolve;
      });
      mockBulkDeleteGuests.mockReturnValueOnce(pendingPromise);

      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /delete all guests/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText('Deleting...')).toBeInTheDocument();
        expect(confirmButton).toBeDisabled();
      });

      resolvePromise!({ deletedCount: 1, message: 'Deleted' });
    });
  });

  describe('success state', () => {
    it('should display deleted count after successful deletion', async () => {
      mockBulkDeleteGuests.mockResolvedValueOnce({
        deletedCount: 15,
        message: 'Successfully deleted 15 guests',
      });

      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /delete all guests/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText('15')).toBeInTheDocument();
        expect(screen.getByText(/accounts deleted successfully/i)).toBeInTheDocument();
      });
    });

    it('should display singular form for one deleted account', async () => {
      mockBulkDeleteGuests.mockResolvedValueOnce({
        deletedCount: 1,
        message: 'Successfully deleted 1 guest',
      });

      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /delete all guests/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText('1')).toBeInTheDocument();
        expect(screen.getByText(/account deleted successfully/i)).toBeInTheDocument();
      });
    });

    it('should show Done button after successful deletion', async () => {
      mockBulkDeleteGuests.mockResolvedValueOnce({
        deletedCount: 5,
        message: 'Successfully deleted 5 guests',
      });

      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /delete all guests/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /done/i })).toBeInTheDocument();
      });
    });
  });

  describe('error handling', () => {
    it('should show error message when deletion fails', async () => {
      mockBulkDeleteGuests.mockRejectedValueOnce(new Error('Failed to delete guest users'));

      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /delete all guests/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText(/failed to delete guest users/i)).toBeInTheDocument();
      });

      expect(mockOnSuccess).not.toHaveBeenCalled();
    });

    it('should show generic error message for non-Error exceptions', async () => {
      mockBulkDeleteGuests.mockRejectedValueOnce('Some unknown error');

      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /delete all guests/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText(/failed to delete guest users/i)).toBeInTheDocument();
      });
    });

    it('should not call onSuccess on error', async () => {
      mockBulkDeleteGuests.mockRejectedValueOnce(new Error('API Error'));

      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /delete all guests/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText(/api error/i)).toBeInTheDocument();
      });

      expect(mockOnSuccess).not.toHaveBeenCalled();
    });
  });

  describe('cancel behavior', () => {
    it('should call onClose when cancel button is clicked', () => {
      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const cancelButton = screen.getByRole('button', { name: /cancel/i });
      fireEvent.click(cancelButton);

      expect(mockOnClose).toHaveBeenCalled();
    });

    it('should call onClose when backdrop is clicked', () => {
      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      // Find backdrop and click it
      const dialog = screen.getByRole('dialog');
      const backdrop = dialog.parentElement?.querySelector('.bg-black\\/60');
      if (backdrop) {
        fireEvent.click(backdrop);
      }

      expect(mockOnClose).toHaveBeenCalled();
    });

    it('should call onClose when Done button is clicked after success', async () => {
      mockBulkDeleteGuests.mockResolvedValueOnce({
        deletedCount: 5,
        message: 'Success',
      });

      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /delete all guests/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /done/i })).toBeInTheDocument();
      });

      const doneButton = screen.getByRole('button', { name: /done/i });
      fireEvent.click(doneButton);

      expect(mockOnClose).toHaveBeenCalled();
    });

    it('should disable cancel button while deleting', async () => {
      let resolvePromise: (value: { deletedCount: number; message: string }) => void;
      const pendingPromise = new Promise<{ deletedCount: number; message: string }>((resolve) => {
        resolvePromise = resolve;
      });
      mockBulkDeleteGuests.mockReturnValueOnce(pendingPromise);

      render(
        <BulkDeleteModal
          isOpen={true}
          onClose={mockOnClose}
          onSuccess={mockOnSuccess}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /delete all guests/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        const cancelButton = screen.getByRole('button', { name: /cancel/i });
        expect(cancelButton).toBeDisabled();
      });

      resolvePromise!({ deletedCount: 1, message: 'Done' });
    });
  });
});
