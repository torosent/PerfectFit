/**
 * CosmeticsTable Component Tests
 *
 * Tests for the admin cosmetics table that displays paginated cosmetic items
 * with inline editing, rarity badges, create, and delete functionality.
 */

import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { CosmeticsTable } from '@/components/admin/CosmeticsTable';
import * as adminGamificationClient from '@/lib/api/admin-gamification-client';
import { AdminApiError } from '@/lib/api/admin-client';
import type { AdminCosmetic, PaginatedResponse, CosmeticRarity } from '@/types';

// Mock the API client
jest.mock('@/lib/api/admin-gamification-client');

// Mock the auth store
const mockToken = 'test-admin-token';
jest.mock('@/lib/stores/auth-store', () => ({
  useToken: () => mockToken,
}));

const mockGetAdminCosmetics = adminGamificationClient.getAdminCosmetics as jest.MockedFunction<
  typeof adminGamificationClient.getAdminCosmetics
>;
const mockCreateAdminCosmetic = adminGamificationClient.createAdminCosmetic as jest.MockedFunction<
  typeof adminGamificationClient.createAdminCosmetic
>;
const mockUpdateAdminCosmetic = adminGamificationClient.updateAdminCosmetic as jest.MockedFunction<
  typeof adminGamificationClient.updateAdminCosmetic
>;
const mockDeleteAdminCosmetic = adminGamificationClient.deleteAdminCosmetic as jest.MockedFunction<
  typeof adminGamificationClient.deleteAdminCosmetic
>;

// Helper to create mock cosmetics
function createMockCosmetic(overrides: Partial<AdminCosmetic> = {}): AdminCosmetic {
  return {
    id: 1,
    code: 'ocean-theme',
    name: 'Ocean Theme',
    description: 'A calming ocean-themed board',
    type: 'BoardTheme',
    assetUrl: '/themes/ocean.css',
    previewUrl: '/previews/ocean.png',
    rarity: 'Rare',
    isDefault: false,
    ...overrides,
  };
}

function createMockPaginatedResponse(
  cosmetics: AdminCosmetic[],
  page: number = 1,
  totalPages: number = 1
): PaginatedResponse<AdminCosmetic> {
  return {
    items: cosmetics,
    page,
    pageSize: 10,
    totalCount: cosmetics.length,
    totalPages,
  };
}

describe('CosmeticsTable', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // Reset mock implementations to prevent cross-test pollution
    mockGetAdminCosmetics.mockReset();
    mockCreateAdminCosmetic.mockReset();
    mockUpdateAdminCosmetic.mockReset();
    mockDeleteAdminCosmetic.mockReset();
  });

  describe('loading state', () => {
    it('renders loading skeleton initially', () => {
      // Never resolve the promise to stay in loading state
      mockGetAdminCosmetics.mockReturnValueOnce(new Promise(() => {}));

      render(<CosmeticsTable />);

      // Check for skeleton loading indicators (animate-pulse elements)
      const skeletonElements = document.querySelectorAll('.animate-pulse');
      expect(skeletonElements.length).toBeGreaterThan(0);
    });
  });

  describe('renders cosmetics', () => {
    it('renders cosmetics after loading', async () => {
      const mockCosmetics = [
        createMockCosmetic({ id: 1, name: 'Ocean Theme', code: 'ocean-theme' }),
        createMockCosmetic({ id: 2, name: 'Fire Theme', code: 'fire-theme', rarity: 'Epic' }),
      ];
      mockGetAdminCosmetics.mockResolvedValueOnce(createMockPaginatedResponse(mockCosmetics));

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText('Ocean Theme')).toBeInTheDocument();
        expect(screen.getByText('Fire Theme')).toBeInTheDocument();
      });

      expect(screen.getByText('ocean-theme')).toBeInTheDocument();
      expect(screen.getByText('fire-theme')).toBeInTheDocument();
    });

    it('displays column headers', async () => {
      mockGetAdminCosmetics.mockResolvedValueOnce(
        createMockPaginatedResponse([createMockCosmetic()])
      );

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText(/code/i)).toBeInTheDocument();
      });

      expect(screen.getByText(/name/i)).toBeInTheDocument();
      expect(screen.getByText(/description/i)).toBeInTheDocument();
      expect(screen.getByText(/type/i)).toBeInTheDocument();
      expect(screen.getByText(/rarity/i)).toBeInTheDocument();
    });

    it('displays default badge for default cosmetics', async () => {
      const mockCosmetic = createMockCosmetic({ isDefault: true });
      mockGetAdminCosmetics.mockResolvedValueOnce(
        createMockPaginatedResponse([mockCosmetic])
      );

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText('Default')).toBeInTheDocument();
      });
    });
  });

  describe('rarity badges', () => {
    it('displays rarity badges with correct colors', async () => {
      const rarities: CosmeticRarity[] = ['Common', 'Rare', 'Epic', 'Legendary'];
      const mockCosmetics = rarities.map((rarity, index) =>
        createMockCosmetic({ id: index + 1, code: `${rarity.toLowerCase()}-item`, name: `${rarity} Item`, rarity })
      );
      mockGetAdminCosmetics.mockResolvedValueOnce(createMockPaginatedResponse(mockCosmetics));

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText('Common')).toBeInTheDocument();
        expect(screen.getByText('Rare')).toBeInTheDocument();
        expect(screen.getByText('Epic')).toBeInTheDocument();
        expect(screen.getByText('Legendary')).toBeInTheDocument();
      });

      // Verify the rarity badges exist (we can't easily test CSS colors in JSDOM)
      const commonBadge = screen.getByText('Common');
      const rareBadge = screen.getByText('Rare');
      const epicBadge = screen.getByText('Epic');
      const legendaryBadge = screen.getByText('Legendary');

      expect(commonBadge).toHaveClass('rounded-full');
      expect(rareBadge).toHaveClass('rounded-full');
      expect(epicBadge).toHaveClass('rounded-full');
      expect(legendaryBadge).toHaveClass('rounded-full');
    });
  });

  describe('pagination', () => {
    it('handles pagination', async () => {
      mockGetAdminCosmetics.mockResolvedValueOnce(
        createMockPaginatedResponse([createMockCosmetic()], 1, 3)
      );

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText(/page 1 of 3/i)).toBeInTheDocument();
      });

      // Previous should be disabled on first page
      const prevButton = screen.getByRole('button', { name: /previous/i });
      expect(prevButton).toBeDisabled();

      // Next should be enabled
      const nextButton = screen.getByRole('button', { name: /next/i });
      expect(nextButton).not.toBeDisabled();
    });

    it('navigates to next page', async () => {
      const mockCosmetics = [createMockCosmetic()];
      mockGetAdminCosmetics
        .mockResolvedValueOnce(createMockPaginatedResponse(mockCosmetics, 1, 3))
        .mockResolvedValueOnce(createMockPaginatedResponse(mockCosmetics, 2, 3));

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText(/page 1 of 3/i)).toBeInTheDocument();
      });

      const nextButton = screen.getByRole('button', { name: /next/i });
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(mockGetAdminCosmetics).toHaveBeenCalledWith(mockToken, 2, 10);
      });
    });

    it('navigates to previous page', async () => {
      const mockCosmetics = [createMockCosmetic()];
      mockGetAdminCosmetics
        .mockResolvedValueOnce(createMockPaginatedResponse(mockCosmetics, 1, 3))
        .mockResolvedValueOnce(createMockPaginatedResponse(mockCosmetics, 2, 3))
        .mockResolvedValueOnce(createMockPaginatedResponse(mockCosmetics, 1, 3));

      render(<CosmeticsTable />);

      // Start on page 1
      await waitFor(() => {
        expect(screen.getByText(/page 1 of 3/i)).toBeInTheDocument();
      });

      // Navigate to page 2
      const nextButton = screen.getByRole('button', { name: /next/i });
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(screen.getByText(/page 2 of 3/i)).toBeInTheDocument();
      });

      // Navigate back to page 1
      const prevButton = screen.getByRole('button', { name: /previous/i });
      fireEvent.click(prevButton);

      await waitFor(() => {
        expect(mockGetAdminCosmetics).toHaveBeenCalledTimes(3);
      });
    });
  });

  describe('inline editing', () => {
    it('inline editing works', async () => {
      const mockCosmetic = createMockCosmetic({ id: 1, name: 'Test Cosmetic' });
      mockGetAdminCosmetics
        .mockResolvedValueOnce(createMockPaginatedResponse([mockCosmetic]))
        .mockResolvedValueOnce(createMockPaginatedResponse([{ ...mockCosmetic, name: 'Updated Cosmetic' }]));
      mockUpdateAdminCosmetic.mockResolvedValueOnce({ ...mockCosmetic, name: 'Updated Cosmetic' });

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText('Test Cosmetic')).toBeInTheDocument();
      });

      // Click to edit
      fireEvent.click(screen.getByText('Test Cosmetic'));

      const input = screen.getByDisplayValue('Test Cosmetic');
      expect(input).toBeInTheDocument();

      fireEvent.change(input, { target: { value: 'Updated Cosmetic' } });
      fireEvent.keyDown(input, { key: 'Enter', code: 'Enter' });

      await waitFor(() => {
        expect(mockUpdateAdminCosmetic).toHaveBeenCalledWith(
          1,
          expect.objectContaining({ name: 'Updated Cosmetic' }),
          mockToken
        );
      });
    });

    it('cancels editing on Escape', async () => {
      const mockCosmetic = createMockCosmetic({ name: 'Original Name' });
      mockGetAdminCosmetics.mockResolvedValueOnce(
        createMockPaginatedResponse([mockCosmetic])
      );

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText('Original Name')).toBeInTheDocument();
      });

      // Click to edit
      fireEvent.click(screen.getByText('Original Name'));

      const input = screen.getByDisplayValue('Original Name');
      fireEvent.change(input, { target: { value: 'Changed Name' } });
      fireEvent.keyDown(input, { key: 'Escape', code: 'Escape' });

      // Should revert to original name
      await waitFor(() => {
        expect(screen.getByText('Original Name')).toBeInTheDocument();
        expect(screen.queryByDisplayValue('Changed Name')).not.toBeInTheDocument();
      });
    });

    it('saves with Save button', async () => {
      const mockCosmetic = createMockCosmetic({ id: 1, name: 'Test Cosmetic' });
      mockGetAdminCosmetics
        .mockResolvedValueOnce(createMockPaginatedResponse([mockCosmetic]))
        .mockResolvedValueOnce(createMockPaginatedResponse([{ ...mockCosmetic, name: 'Updated Cosmetic' }]));
      mockUpdateAdminCosmetic.mockResolvedValueOnce({ ...mockCosmetic, name: 'Updated Cosmetic' });

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText('Test Cosmetic')).toBeInTheDocument();
      });

      // Click to edit
      fireEvent.click(screen.getByText('Test Cosmetic'));

      const input = screen.getByDisplayValue('Test Cosmetic');
      fireEvent.change(input, { target: { value: 'Updated Cosmetic' } });

      // Click Save button
      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(mockUpdateAdminCosmetic).toHaveBeenCalledWith(
          1,
          expect.objectContaining({ name: 'Updated Cosmetic' }),
          mockToken
        );
      });
    });
  });

  describe('create cosmetic', () => {
    it('creates new cosmetic', async () => {
      const mockCosmetic = createMockCosmetic();
      mockGetAdminCosmetics
        .mockResolvedValueOnce(createMockPaginatedResponse([]))
        .mockResolvedValueOnce(createMockPaginatedResponse([mockCosmetic]));
      mockCreateAdminCosmetic.mockResolvedValueOnce(mockCosmetic);

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText(/no cosmetics found/i)).toBeInTheDocument();
      });

      const addButton = screen.getByRole('button', { name: /add cosmetic/i });
      fireEvent.click(addButton);

      await waitFor(() => {
        expect(screen.getByPlaceholderText('unique-code')).toBeInTheDocument();
      });

      // Fill in the form
      fireEvent.change(screen.getByPlaceholderText('unique-code'), { target: { value: 'new-cosmetic' } });
      fireEvent.change(screen.getByPlaceholderText('Name'), { target: { value: 'New Cosmetic' } });
      fireEvent.change(screen.getByPlaceholderText('Description'), { target: { value: 'A new cosmetic item' } });

      // Click Save
      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(mockCreateAdminCosmetic).toHaveBeenCalledWith(
          expect.objectContaining({
            code: 'new-cosmetic',
            name: 'New Cosmetic',
            description: 'A new cosmetic item',
          }),
          mockToken
        );
      });
    });

    it('shows create form when Add button clicked', async () => {
      mockGetAdminCosmetics.mockResolvedValueOnce(
        createMockPaginatedResponse([createMockCosmetic()])
      );

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText('Ocean Theme')).toBeInTheDocument();
      });

      const addButton = screen.getByRole('button', { name: /add cosmetic/i });
      fireEvent.click(addButton);

      await waitFor(() => {
        expect(screen.getByPlaceholderText('unique-code')).toBeInTheDocument();
        expect(screen.getByPlaceholderText('Name')).toBeInTheDocument();
        expect(screen.getByPlaceholderText('Description')).toBeInTheDocument();
      });
    });

    it('cancels create when Cancel clicked', async () => {
      mockGetAdminCosmetics.mockResolvedValueOnce(
        createMockPaginatedResponse([createMockCosmetic()])
      );

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText('Ocean Theme')).toBeInTheDocument();
      });

      const addButton = screen.getByRole('button', { name: /add cosmetic/i });
      fireEvent.click(addButton);

      await waitFor(() => {
        expect(screen.getByPlaceholderText('Name')).toBeInTheDocument();
      });

      // Click Cancel
      const cancelButton = screen.getByRole('button', { name: /cancel/i });
      fireEvent.click(cancelButton);

      // Create form should be hidden
      await waitFor(() => {
        expect(screen.queryByPlaceholderText('unique-code')).not.toBeInTheDocument();
      });
    });
  });

  describe('delete cosmetic', () => {
    it('delete with confirmation', async () => {
      const mockCosmetic = createMockCosmetic({ id: 1 });
      mockGetAdminCosmetics
        .mockResolvedValueOnce(createMockPaginatedResponse([mockCosmetic]))
        .mockResolvedValueOnce(createMockPaginatedResponse([]));
      mockDeleteAdminCosmetic.mockResolvedValueOnce(undefined);

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText('Ocean Theme')).toBeInTheDocument();
      });

      const deleteButton = screen.getByRole('button', { name: /delete/i });
      fireEvent.click(deleteButton);

      // Should show confirmation
      await waitFor(() => {
        expect(screen.getByRole('button', { name: /confirm/i })).toBeInTheDocument();
      });

      const confirmButton = screen.getByRole('button', { name: /confirm/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(mockDeleteAdminCosmetic).toHaveBeenCalledWith(1, mockToken);
      });
    });

    it('delete handles 409 conflict', async () => {
      const mockCosmetic = createMockCosmetic({ id: 1 });
      mockGetAdminCosmetics.mockResolvedValueOnce(
        createMockPaginatedResponse([mockCosmetic])
      );
      mockDeleteAdminCosmetic.mockRejectedValueOnce(
        new AdminApiError('Cosmetic is in use', 409)
      );

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText('Ocean Theme')).toBeInTheDocument();
      });

      const deleteButton = screen.getByRole('button', { name: /delete/i });
      fireEvent.click(deleteButton);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /confirm/i })).toBeInTheDocument();
      });

      const confirmButton = screen.getByRole('button', { name: /confirm/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText(/in use and cannot be deleted/i)).toBeInTheDocument();
      });
    });

    it('cancels delete when Cancel clicked', async () => {
      const mockCosmetic = createMockCosmetic();
      mockGetAdminCosmetics.mockResolvedValueOnce(
        createMockPaginatedResponse([mockCosmetic])
      );

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText('Ocean Theme')).toBeInTheDocument();
      });

      const deleteButton = screen.getByRole('button', { name: /delete/i });
      fireEvent.click(deleteButton);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /confirm/i })).toBeInTheDocument();
      });

      // Click Cancel
      const cancelButtons = screen.getAllByRole('button', { name: /cancel/i });
      const deleteCancelButton = cancelButtons[cancelButtons.length - 1];
      fireEvent.click(deleteCancelButton);

      // Should hide confirmation
      await waitFor(() => {
        expect(screen.queryByRole('button', { name: /confirm/i })).not.toBeInTheDocument();
      });
    });
  });

  describe('error handling', () => {
    it('shows error message on API failure', async () => {
      mockGetAdminCosmetics.mockRejectedValueOnce(new Error('Failed to fetch cosmetics'));

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText(/failed to fetch cosmetics/i)).toBeInTheDocument();
      });
    });

    it('shows error when update fails', async () => {
      const mockCosmetic = createMockCosmetic({ name: 'Test Cosmetic' });
      mockGetAdminCosmetics.mockResolvedValueOnce(
        createMockPaginatedResponse([mockCosmetic])
      );
      mockUpdateAdminCosmetic.mockRejectedValueOnce(new Error('Update failed'));

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText('Test Cosmetic')).toBeInTheDocument();
      });

      // Click to edit
      fireEvent.click(screen.getByText('Test Cosmetic'));

      const input = screen.getByDisplayValue('Test Cosmetic');
      fireEvent.change(input, { target: { value: 'Updated' } });
      fireEvent.keyDown(input, { key: 'Enter', code: 'Enter' });

      await waitFor(() => {
        expect(screen.getByText(/update failed/i)).toBeInTheDocument();
      });
    });

    it('shows error when create fails', async () => {
      mockGetAdminCosmetics.mockResolvedValueOnce(
        createMockPaginatedResponse([])
      );
      mockCreateAdminCosmetic.mockRejectedValueOnce(new Error('Create failed'));

      render(<CosmeticsTable />);

      await waitFor(() => {
        expect(screen.getByText(/no cosmetics found/i)).toBeInTheDocument();
      });

      const addButton = screen.getByRole('button', { name: /add cosmetic/i });
      fireEvent.click(addButton);

      await waitFor(() => {
        expect(screen.getByPlaceholderText('Name')).toBeInTheDocument();
      });

      fireEvent.change(screen.getByPlaceholderText('unique-code'), { target: { value: 'test' } });
      fireEvent.change(screen.getByPlaceholderText('Name'), { target: { value: 'Test' } });

      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(screen.getByText(/create failed/i)).toBeInTheDocument();
      });
    });
  });

  describe('refresh functionality', () => {
    it('refetches cosmetics when refreshTrigger changes', async () => {
      const mockCosmetics = [createMockCosmetic()];
      mockGetAdminCosmetics
        .mockResolvedValueOnce(createMockPaginatedResponse(mockCosmetics))
        .mockResolvedValueOnce(createMockPaginatedResponse(mockCosmetics));

      const { rerender } = render(<CosmeticsTable refreshTrigger={0} />);

      await waitFor(() => {
        expect(mockGetAdminCosmetics).toHaveBeenCalledTimes(1);
      });

      // Trigger refresh
      rerender(<CosmeticsTable refreshTrigger={1} />);

      await waitFor(() => {
        expect(mockGetAdminCosmetics).toHaveBeenCalledTimes(2);
      });
    });
  });
});
