/**
 * AchievementsTable Component Tests
 *
 * Tests for the admin achievements table that displays paginated achievements
 * with inline editing, create, and delete functionality.
 */

import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { AchievementsTable } from '@/components/admin/AchievementsTable';
import * as adminGamificationClient from '@/lib/api/admin-gamification-client';
import { AdminApiError } from '@/lib/api/admin-client';
import type { AdminAchievement, PaginatedResponse } from '@/types';

// Mock the API client
jest.mock('@/lib/api/admin-gamification-client');

// Mock the auth store
const mockToken = 'test-admin-token';
jest.mock('@/lib/stores/auth-store', () => ({
  useToken: () => mockToken,
}));

const mockGetAdminAchievements = adminGamificationClient.getAdminAchievements as jest.MockedFunction<
  typeof adminGamificationClient.getAdminAchievements
>;
const mockCreateAdminAchievement = adminGamificationClient.createAdminAchievement as jest.MockedFunction<
  typeof adminGamificationClient.createAdminAchievement
>;
const mockUpdateAdminAchievement = adminGamificationClient.updateAdminAchievement as jest.MockedFunction<
  typeof adminGamificationClient.updateAdminAchievement
>;
const mockDeleteAdminAchievement = adminGamificationClient.deleteAdminAchievement as jest.MockedFunction<
  typeof adminGamificationClient.deleteAdminAchievement
>;

// Helper to create mock achievements
function createMockAchievement(overrides: Partial<AdminAchievement> = {}): AdminAchievement {
  return {
    id: 1,
    name: 'First Victory',
    description: 'Win your first game',
    category: 'Games',
    iconUrl: '/icons/first-victory.svg',
    unlockCondition: '{"Type":"TotalWins","Value":1}',
    rewardType: 'XPBoost',
    rewardValue: 100,
    isSecret: false,
    displayOrder: 1,
    rewardCosmeticCode: null,
    ...overrides,
  };
}

function createMockPaginatedResponse(
  achievements: AdminAchievement[],
  page: number = 1,
  totalPages: number = 1
): PaginatedResponse<AdminAchievement> {
  return {
    items: achievements,
    page,
    pageSize: 10,
    totalCount: achievements.length,
    totalPages,
  };
}

describe('AchievementsTable', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // Reset mock implementations to prevent cross-test pollution
    mockGetAdminAchievements.mockReset();
    mockCreateAdminAchievement.mockReset();
    mockUpdateAdminAchievement.mockReset();
    mockDeleteAdminAchievement.mockReset();
  });

  describe('loading state', () => {
    it('renders loading skeleton initially', () => {
      // Never resolve the promise to stay in loading state
      mockGetAdminAchievements.mockReturnValueOnce(new Promise(() => {}));

      render(<AchievementsTable />);

      // Check for skeleton loading indicators (animate-pulse elements)
      const skeletonElements = document.querySelectorAll('.animate-pulse');
      expect(skeletonElements.length).toBeGreaterThan(0);
    });
  });

  describe('renders achievements', () => {
    it('renders achievements after loading', async () => {
      const mockAchievements = [
        createMockAchievement({ id: 1, name: 'First Victory', description: 'Win your first game' }),
        createMockAchievement({ id: 2, name: 'High Scorer', description: 'Score 1000 points' }),
      ];
      mockGetAdminAchievements.mockResolvedValueOnce(createMockPaginatedResponse(mockAchievements));

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText('First Victory')).toBeInTheDocument();
        expect(screen.getByText('High Scorer')).toBeInTheDocument();
      });

      expect(screen.getByText('Win your first game')).toBeInTheDocument();
      expect(screen.getByText('Score 1000 points')).toBeInTheDocument();
    });

    it('displays column headers', async () => {
      mockGetAdminAchievements.mockResolvedValueOnce(
        createMockPaginatedResponse([createMockAchievement()])
      );

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText(/name/i)).toBeInTheDocument();
      });

      expect(screen.getByText(/description/i)).toBeInTheDocument();
      expect(screen.getByText(/category/i)).toBeInTheDocument();
      expect(screen.getByText(/reward/i)).toBeInTheDocument();
    });

    it('displays category badge', async () => {
      const mockAchievement = createMockAchievement({ category: 'Score' });
      mockGetAdminAchievements.mockResolvedValueOnce(
        createMockPaginatedResponse([mockAchievement])
      );

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText('Score')).toBeInTheDocument();
      });
    });

    it('displays secret badge for secret achievements', async () => {
      const mockAchievement = createMockAchievement({ isSecret: true });
      mockGetAdminAchievements.mockResolvedValueOnce(
        createMockPaginatedResponse([mockAchievement])
      );

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText('Secret')).toBeInTheDocument();
      });
    });
  });

  describe('pagination', () => {
    it('shows pagination info', async () => {
      mockGetAdminAchievements.mockResolvedValueOnce(
        createMockPaginatedResponse([createMockAchievement()], 1, 3)
      );

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText(/page 1 of 3/i)).toBeInTheDocument();
      });
    });

    it('handles next page navigation', async () => {
      const mockAchievements = [createMockAchievement()];
      mockGetAdminAchievements
        .mockResolvedValueOnce(createMockPaginatedResponse(mockAchievements, 1, 3))
        .mockResolvedValueOnce(createMockPaginatedResponse(mockAchievements, 2, 3));

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText(/page 1 of 3/i)).toBeInTheDocument();
      });

      const nextButton = screen.getByRole('button', { name: /next/i });
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(mockGetAdminAchievements).toHaveBeenCalledWith(mockToken, 2, 10);
      });
    });

    it('handles previous page navigation', async () => {
      const mockAchievements = [createMockAchievement()];
      mockGetAdminAchievements
        .mockResolvedValueOnce(createMockPaginatedResponse(mockAchievements, 1, 3))
        .mockResolvedValueOnce(createMockPaginatedResponse(mockAchievements, 2, 3))
        .mockResolvedValueOnce(createMockPaginatedResponse(mockAchievements, 1, 3));

      render(<AchievementsTable />);

      // Start on page 1
      await waitFor(() => {
        expect(screen.getByText(/page 1 of 3/i)).toBeInTheDocument();
      });

      // Go to page 2
      const nextButton = screen.getByRole('button', { name: /next/i });
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(screen.getByText(/page 2 of 3/i)).toBeInTheDocument();
      });

      // Go back to page 1
      const prevButton = screen.getByRole('button', { name: /previous/i });
      fireEvent.click(prevButton);

      await waitFor(() => {
        expect(mockGetAdminAchievements).toHaveBeenCalledTimes(3);
      });
    });

    it('disables Previous button on first page', async () => {
      mockGetAdminAchievements.mockResolvedValueOnce(
        createMockPaginatedResponse([createMockAchievement()], 1, 3)
      );

      render(<AchievementsTable />);

      await waitFor(() => {
        const prevButton = screen.getByRole('button', { name: /previous/i });
        expect(prevButton).toBeDisabled();
      });
    });

    it('disables Next button on last page', async () => {
      mockGetAdminAchievements.mockResolvedValueOnce(
        createMockPaginatedResponse([createMockAchievement()], 1, 1)
      );

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText(/page 1 of 1/i)).toBeInTheDocument();
      });
      
      const nextButton = screen.getByRole('button', { name: /next/i });
      expect(nextButton).toBeDisabled();
    });
  });

  describe('inline editing', () => {
    it('enters edit mode when clicking on a cell', async () => {
      const mockAchievement = createMockAchievement({ name: 'Test Achievement' });
      mockGetAdminAchievements.mockResolvedValueOnce(
        createMockPaginatedResponse([mockAchievement])
      );

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText('Test Achievement')).toBeInTheDocument();
      });

      // Click on the name to edit
      fireEvent.click(screen.getByText('Test Achievement'));

      // Should show input field in edit mode
      const input = screen.getByDisplayValue('Test Achievement');
      expect(input).toBeInTheDocument();
      expect(input.tagName).toBe('INPUT');
    });

    it('saves on Enter key', async () => {
      const mockAchievement = createMockAchievement({ id: 1, name: 'Test Achievement' });
      mockGetAdminAchievements
        .mockResolvedValueOnce(createMockPaginatedResponse([mockAchievement]))
        .mockResolvedValueOnce(createMockPaginatedResponse([{ ...mockAchievement, name: 'Updated Name' }]));
      mockUpdateAdminAchievement.mockResolvedValueOnce({ ...mockAchievement, name: 'Updated Name' });

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText('Test Achievement')).toBeInTheDocument();
      });

      // Click to edit
      fireEvent.click(screen.getByText('Test Achievement'));

      const input = screen.getByDisplayValue('Test Achievement');
      fireEvent.change(input, { target: { value: 'Updated Name' } });
      fireEvent.keyDown(input, { key: 'Enter', code: 'Enter' });

      await waitFor(() => {
        expect(mockUpdateAdminAchievement).toHaveBeenCalledWith(
          1,
          expect.objectContaining({ name: 'Updated Name' }),
          mockToken
        );
      });
    });

    it('cancels on Escape key', async () => {
      const mockAchievement = createMockAchievement({ name: 'Original Name' });
      mockGetAdminAchievements.mockResolvedValueOnce(
        createMockPaginatedResponse([mockAchievement])
      );

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText('Original Name')).toBeInTheDocument();
      });

      // Click to edit
      fireEvent.click(screen.getByText('Original Name'));

      const input = screen.getByDisplayValue('Original Name');
      fireEvent.change(input, { target: { value: 'New Name' } });
      fireEvent.keyDown(input, { key: 'Escape', code: 'Escape' });

      // Should revert to original name and exit edit mode
      await waitFor(() => {
        expect(screen.getByText('Original Name')).toBeInTheDocument();
        expect(screen.queryByDisplayValue('New Name')).not.toBeInTheDocument();
      });
    });
  });

  describe('JSON validation', () => {
    it('shows error for invalid UnlockCondition JSON', async () => {
      const mockAchievement = createMockAchievement();
      mockGetAdminAchievements.mockResolvedValueOnce(
        createMockPaginatedResponse([mockAchievement])
      );

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText('First Victory')).toBeInTheDocument();
      });

      // Click to edit
      fireEvent.click(screen.getByText('First Victory'));

      // Find the unlock condition textarea and enter invalid JSON
      const textarea = screen.getByDisplayValue('{"Type":"TotalWins","Value":1}');
      fireEvent.change(textarea, { target: { value: 'invalid json{' } });

      // Click Save button
      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        // Look specifically for the error message div
        expect(screen.getByText('Invalid JSON in unlock condition')).toBeInTheDocument();
      });

      // Should not call update API
      expect(mockUpdateAdminAchievement).not.toHaveBeenCalled();
    });
  });

  describe('create achievement', () => {
    it('shows create form when Add button clicked', async () => {
      mockGetAdminAchievements.mockResolvedValueOnce(
        createMockPaginatedResponse([createMockAchievement()])
      );

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText('First Victory')).toBeInTheDocument();
      });

      const addButton = screen.getByRole('button', { name: /add achievement/i });
      fireEvent.click(addButton);

      // Should show a new row with input fields
      await waitFor(() => {
        expect(screen.getByPlaceholderText('Name')).toBeInTheDocument();
        expect(screen.getByPlaceholderText('Description')).toBeInTheDocument();
      });
    });

    it('creates new achievement when Save clicked', async () => {
      const mockAchievement = createMockAchievement();
      mockGetAdminAchievements
        .mockResolvedValueOnce(createMockPaginatedResponse([]))
        .mockResolvedValueOnce(createMockPaginatedResponse([mockAchievement]));
      mockCreateAdminAchievement.mockResolvedValueOnce(mockAchievement);

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText(/no achievements found/i)).toBeInTheDocument();
      });

      const addButton = screen.getByRole('button', { name: /add achievement/i });
      fireEvent.click(addButton);

      await waitFor(() => {
        expect(screen.getByPlaceholderText('Name')).toBeInTheDocument();
      });

      // Fill in the form
      fireEvent.change(screen.getByPlaceholderText('Name'), { target: { value: 'New Achievement' } });
      fireEvent.change(screen.getByPlaceholderText('Description'), { target: { value: 'Test desc' } });

      // Click Save
      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(mockCreateAdminAchievement).toHaveBeenCalledWith(
          expect.objectContaining({
            name: 'New Achievement',
            description: 'Test desc',
          }),
          mockToken
        );
      });
    });
  });

  describe('delete achievement', () => {
    it('shows confirmation dialog when delete clicked', async () => {
      const mockAchievement = createMockAchievement({ name: 'Test Achievement' });
      mockGetAdminAchievements.mockResolvedValueOnce(
        createMockPaginatedResponse([mockAchievement])
      );

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText('Test Achievement')).toBeInTheDocument();
      });

      const deleteButton = screen.getByRole('button', { name: /delete/i });
      fireEvent.click(deleteButton);

      // Should show confirmation buttons
      await waitFor(() => {
        expect(screen.getByRole('button', { name: /confirm/i })).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument();
      });
    });

    it('deletes achievement on confirm', async () => {
      const mockAchievement = createMockAchievement({ id: 1 });
      mockGetAdminAchievements
        .mockResolvedValueOnce(createMockPaginatedResponse([mockAchievement]))
        .mockResolvedValueOnce(createMockPaginatedResponse([]));
      mockDeleteAdminAchievement.mockResolvedValueOnce(undefined);

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText('First Victory')).toBeInTheDocument();
      });

      const deleteButton = screen.getByRole('button', { name: /delete/i });
      fireEvent.click(deleteButton);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /confirm/i })).toBeInTheDocument();
      });

      const confirmButton = screen.getByRole('button', { name: /confirm/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(mockDeleteAdminAchievement).toHaveBeenCalledWith(1, mockToken);
      });
    });

    it('handles 409 conflict (entity in use)', async () => {
      const mockAchievement = createMockAchievement({ id: 1 });
      mockGetAdminAchievements.mockResolvedValueOnce(
        createMockPaginatedResponse([mockAchievement])
      );
      mockDeleteAdminAchievement.mockRejectedValueOnce(
        new AdminApiError('Achievement is in use', 409)
      );

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText('First Victory')).toBeInTheDocument();
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
  });

  describe('error handling', () => {
    it('shows error message on API failure', async () => {
      mockGetAdminAchievements.mockRejectedValueOnce(new Error('Failed to fetch achievements'));

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText(/failed to fetch achievements/i)).toBeInTheDocument();
      });
    });

    it('shows error when update fails', async () => {
      const mockAchievement = createMockAchievement({ name: 'Test Achievement' });
      mockGetAdminAchievements.mockResolvedValueOnce(
        createMockPaginatedResponse([mockAchievement])
      );
      mockUpdateAdminAchievement.mockRejectedValueOnce(new Error('Update failed'));

      render(<AchievementsTable />);

      await waitFor(() => {
        expect(screen.getByText('Test Achievement')).toBeInTheDocument();
      });

      // Click to edit
      fireEvent.click(screen.getByText('Test Achievement'));

      const input = screen.getByDisplayValue('Test Achievement');
      fireEvent.change(input, { target: { value: 'Updated' } });
      fireEvent.keyDown(input, { key: 'Enter', code: 'Enter' });

      await waitFor(() => {
        expect(screen.getByText(/update failed/i)).toBeInTheDocument();
      });
    });
  });

  describe('refresh functionality', () => {
    it('refetches achievements when refreshTrigger changes', async () => {
      const mockAchievements = [createMockAchievement()];
      mockGetAdminAchievements
        .mockResolvedValueOnce(createMockPaginatedResponse(mockAchievements))
        .mockResolvedValueOnce(createMockPaginatedResponse(mockAchievements));

      const { rerender } = render(<AchievementsTable refreshTrigger={0} />);

      await waitFor(() => {
        expect(mockGetAdminAchievements).toHaveBeenCalledTimes(1);
      });

      // Trigger refresh
      rerender(<AchievementsTable refreshTrigger={1} />);

      await waitFor(() => {
        expect(mockGetAdminAchievements).toHaveBeenCalledTimes(2);
      });
    });
  });
});
