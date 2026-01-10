/**
 * ChallengeTemplatesTable Component Tests
 *
 * Tests for the admin challenge templates table that displays paginated templates
 * with inline editing, active toggle, create, and delete functionality.
 */

import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ChallengeTemplatesTable } from '@/components/admin/ChallengeTemplatesTable';
import * as adminGamificationClient from '@/lib/api/admin-gamification-client';
import { AdminApiError } from '@/lib/api/admin-client';
import type { AdminChallengeTemplate, PaginatedResponse } from '@/types';

// Mock the API client
jest.mock('@/lib/api/admin-gamification-client');

// Mock the auth store
const mockToken = 'test-admin-token';
jest.mock('@/lib/stores/auth-store', () => ({
  useToken: () => mockToken,
}));

const mockGetAdminChallengeTemplates = adminGamificationClient.getAdminChallengeTemplates as jest.MockedFunction<
  typeof adminGamificationClient.getAdminChallengeTemplates
>;
const mockCreateAdminChallengeTemplate = adminGamificationClient.createAdminChallengeTemplate as jest.MockedFunction<
  typeof adminGamificationClient.createAdminChallengeTemplate
>;
const mockUpdateAdminChallengeTemplate = adminGamificationClient.updateAdminChallengeTemplate as jest.MockedFunction<
  typeof adminGamificationClient.updateAdminChallengeTemplate
>;
const mockDeleteAdminChallengeTemplate = adminGamificationClient.deleteAdminChallengeTemplate as jest.MockedFunction<
  typeof adminGamificationClient.deleteAdminChallengeTemplate
>;

// Helper to create mock challenge templates
function createMockChallengeTemplate(overrides: Partial<AdminChallengeTemplate> = {}): AdminChallengeTemplate {
  return {
    id: 1,
    name: 'Daily Challenge',
    description: 'Complete 5 games today',
    type: 'Daily',
    targetValue: 5,
    xpReward: 50,
    isActive: true,
    ...overrides,
  };
}

function createMockPaginatedResponse(
  templates: AdminChallengeTemplate[],
  page: number = 1,
  totalPages: number = 1
): PaginatedResponse<AdminChallengeTemplate> {
  return {
    items: templates,
    page,
    pageSize: 10,
    totalCount: templates.length,
    totalPages,
  };
}

describe('ChallengeTemplatesTable', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // Reset mock implementations to prevent cross-test pollution
    mockGetAdminChallengeTemplates.mockReset();
    mockCreateAdminChallengeTemplate.mockReset();
    mockUpdateAdminChallengeTemplate.mockReset();
    mockDeleteAdminChallengeTemplate.mockReset();
  });

  describe('loading state', () => {
    it('renders loading skeleton initially', () => {
      // Never resolve the promise to stay in loading state
      mockGetAdminChallengeTemplates.mockReturnValueOnce(new Promise(() => {}));

      render(<ChallengeTemplatesTable />);

      // Check for skeleton loading indicators (animate-pulse elements)
      const skeletonElements = document.querySelectorAll('.animate-pulse');
      expect(skeletonElements.length).toBeGreaterThan(0);
    });
  });

  describe('renders templates', () => {
    it('renders templates after loading', async () => {
      const mockTemplates = [
        createMockChallengeTemplate({ id: 1, name: 'Daily Challenge', description: 'Play 5 games' }),
        createMockChallengeTemplate({ id: 2, name: 'Weekly Marathon', description: 'Play 20 games', type: 'Weekly' }),
      ];
      mockGetAdminChallengeTemplates.mockResolvedValueOnce(createMockPaginatedResponse(mockTemplates));

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByText('Daily Challenge')).toBeInTheDocument();
        expect(screen.getByText('Weekly Marathon')).toBeInTheDocument();
      });

      expect(screen.getByText('Play 5 games')).toBeInTheDocument();
      expect(screen.getByText('Play 20 games')).toBeInTheDocument();
    });

    it('displays column headers', async () => {
      mockGetAdminChallengeTemplates.mockResolvedValueOnce(
        createMockPaginatedResponse([createMockChallengeTemplate()])
      );

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByText(/name/i)).toBeInTheDocument();
      });

      expect(screen.getByText(/description/i)).toBeInTheDocument();
      expect(screen.getByText(/type/i)).toBeInTheDocument();
      expect(screen.getByText(/target/i)).toBeInTheDocument();
      expect(screen.getByText(/xp reward/i)).toBeInTheDocument();
      expect(screen.getByText(/active/i)).toBeInTheDocument();
    });

    it('displays type badges', async () => {
      const mockTemplates = [
        createMockChallengeTemplate({ type: 'Daily' }),
        createMockChallengeTemplate({ id: 2, type: 'Weekly' }),
      ];
      mockGetAdminChallengeTemplates.mockResolvedValueOnce(
        createMockPaginatedResponse(mockTemplates)
      );

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByText('Daily')).toBeInTheDocument();
        expect(screen.getByText('Weekly')).toBeInTheDocument();
      });
    });

    it('displays XP reward with XP suffix', async () => {
      const mockTemplate = createMockChallengeTemplate({ xpReward: 100 });
      mockGetAdminChallengeTemplates.mockResolvedValueOnce(
        createMockPaginatedResponse([mockTemplate])
      );

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByText('100 XP')).toBeInTheDocument();
      });
    });
  });

  describe('pagination', () => {
    it('handles pagination', async () => {
      mockGetAdminChallengeTemplates.mockResolvedValueOnce(
        createMockPaginatedResponse([createMockChallengeTemplate()], 1, 3)
      );

      render(<ChallengeTemplatesTable />);

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
      const mockTemplates = [createMockChallengeTemplate()];
      mockGetAdminChallengeTemplates
        .mockResolvedValueOnce(createMockPaginatedResponse(mockTemplates, 1, 3))
        .mockResolvedValueOnce(createMockPaginatedResponse(mockTemplates, 2, 3));

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByText(/page 1 of 3/i)).toBeInTheDocument();
      });

      const nextButton = screen.getByRole('button', { name: /next/i });
      fireEvent.click(nextButton);

      await waitFor(() => {
        expect(mockGetAdminChallengeTemplates).toHaveBeenCalledWith(mockToken, 2, 10);
      });
    });
  });

  describe('inline editing', () => {
    it('inline editing works', async () => {
      const mockTemplate = createMockChallengeTemplate({ id: 1, name: 'Test Challenge' });
      mockGetAdminChallengeTemplates
        .mockResolvedValueOnce(createMockPaginatedResponse([mockTemplate]))
        .mockResolvedValueOnce(createMockPaginatedResponse([{ ...mockTemplate, name: 'Updated Challenge' }]));
      mockUpdateAdminChallengeTemplate.mockResolvedValueOnce({ ...mockTemplate, name: 'Updated Challenge' });

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByText('Test Challenge')).toBeInTheDocument();
      });

      // Click to edit
      fireEvent.click(screen.getByText('Test Challenge'));

      const input = screen.getByDisplayValue('Test Challenge');
      expect(input).toBeInTheDocument();

      fireEvent.change(input, { target: { value: 'Updated Challenge' } });
      fireEvent.keyDown(input, { key: 'Enter', code: 'Enter' });

      await waitFor(() => {
        expect(mockUpdateAdminChallengeTemplate).toHaveBeenCalledWith(
          1,
          expect.objectContaining({ name: 'Updated Challenge' }),
          mockToken
        );
      });
    });

    it('cancels editing on Escape', async () => {
      const mockTemplate = createMockChallengeTemplate({ name: 'Original Name' });
      mockGetAdminChallengeTemplates.mockResolvedValueOnce(
        createMockPaginatedResponse([mockTemplate])
      );

      render(<ChallengeTemplatesTable />);

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
  });

  describe('active/inactive toggle', () => {
    it('active/inactive toggle works', async () => {
      const mockTemplate = createMockChallengeTemplate({ id: 1, isActive: true });
      mockGetAdminChallengeTemplates
        .mockResolvedValueOnce(createMockPaginatedResponse([mockTemplate]))
        .mockResolvedValueOnce(createMockPaginatedResponse([{ ...mockTemplate, isActive: false }]));
      mockUpdateAdminChallengeTemplate.mockResolvedValueOnce({ ...mockTemplate, isActive: false });

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByText('Daily Challenge')).toBeInTheDocument();
      });

      // Find the toggle button (aria-label: Deactivate for active templates)
      const toggleButton = screen.getByRole('button', { name: /deactivate/i });
      fireEvent.click(toggleButton);

      await waitFor(() => {
        expect(mockUpdateAdminChallengeTemplate).toHaveBeenCalledWith(
          1,
          expect.objectContaining({ isActive: false }),
          mockToken
        );
      });
    });

    it('toggle shows correct aria-label for inactive templates', async () => {
      const mockTemplate = createMockChallengeTemplate({ isActive: false });
      mockGetAdminChallengeTemplates.mockResolvedValueOnce(
        createMockPaginatedResponse([mockTemplate])
      );

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /activate/i })).toBeInTheDocument();
      });
    });
  });

  describe('create template', () => {
    it('creates new template', async () => {
      const mockTemplate = createMockChallengeTemplate();
      mockGetAdminChallengeTemplates
        .mockResolvedValueOnce(createMockPaginatedResponse([]))
        .mockResolvedValueOnce(createMockPaginatedResponse([mockTemplate]));
      mockCreateAdminChallengeTemplate.mockResolvedValueOnce(mockTemplate);

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByText(/no challenge templates found/i)).toBeInTheDocument();
      });

      const addButton = screen.getByRole('button', { name: /add template/i });
      fireEvent.click(addButton);

      await waitFor(() => {
        expect(screen.getByPlaceholderText('Name')).toBeInTheDocument();
      });

      // Fill in the form
      fireEvent.change(screen.getByPlaceholderText('Name'), { target: { value: 'New Challenge' } });
      fireEvent.change(screen.getByPlaceholderText('Description'), { target: { value: 'New description' } });

      // Click Save
      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(mockCreateAdminChallengeTemplate).toHaveBeenCalledWith(
          expect.objectContaining({
            name: 'New Challenge',
            description: 'New description',
          }),
          mockToken
        );
      });
    });

    it('cancels create when Cancel clicked', async () => {
      mockGetAdminChallengeTemplates.mockResolvedValueOnce(
        createMockPaginatedResponse([createMockChallengeTemplate()])
      );

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByText('Daily Challenge')).toBeInTheDocument();
      });

      const addButton = screen.getByRole('button', { name: /add template/i });
      fireEvent.click(addButton);

      await waitFor(() => {
        expect(screen.getByPlaceholderText('Name')).toBeInTheDocument();
      });

      // Click Cancel
      const cancelButton = screen.getByRole('button', { name: /cancel/i });
      fireEvent.click(cancelButton);

      // Create form should be hidden
      await waitFor(() => {
        expect(screen.queryByPlaceholderText('Name')).not.toBeInTheDocument();
      });
    });
  });

  describe('delete template', () => {
    it('delete with confirmation', async () => {
      const mockTemplate = createMockChallengeTemplate({ id: 1 });
      mockGetAdminChallengeTemplates
        .mockResolvedValueOnce(createMockPaginatedResponse([mockTemplate]))
        .mockResolvedValueOnce(createMockPaginatedResponse([]));
      mockDeleteAdminChallengeTemplate.mockResolvedValueOnce(undefined);

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByText('Daily Challenge')).toBeInTheDocument();
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
        expect(mockDeleteAdminChallengeTemplate).toHaveBeenCalledWith(1, mockToken);
      });
    });

    it('delete handles 409 conflict', async () => {
      const mockTemplate = createMockChallengeTemplate({ id: 1 });
      mockGetAdminChallengeTemplates.mockResolvedValueOnce(
        createMockPaginatedResponse([mockTemplate])
      );
      mockDeleteAdminChallengeTemplate.mockRejectedValueOnce(
        new AdminApiError('Template has active challenges', 409)
      );

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByText('Daily Challenge')).toBeInTheDocument();
      });

      const deleteButton = screen.getByRole('button', { name: /delete/i });
      fireEvent.click(deleteButton);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /confirm/i })).toBeInTheDocument();
      });

      const confirmButton = screen.getByRole('button', { name: /confirm/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText(/active challenges and cannot be deleted/i)).toBeInTheDocument();
      });
    });

    it('cancels delete when Cancel clicked', async () => {
      const mockTemplate = createMockChallengeTemplate();
      mockGetAdminChallengeTemplates.mockResolvedValueOnce(
        createMockPaginatedResponse([mockTemplate])
      );

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByText('Daily Challenge')).toBeInTheDocument();
      });

      const deleteButton = screen.getByRole('button', { name: /delete/i });
      fireEvent.click(deleteButton);

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /confirm/i })).toBeInTheDocument();
      });

      // Click Cancel
      const cancelButtons = screen.getAllByRole('button', { name: /cancel/i });
      const deleteCancelButton = cancelButtons[cancelButtons.length - 1]; // Last cancel button in delete confirmation
      fireEvent.click(deleteCancelButton);

      // Should hide confirmation
      await waitFor(() => {
        expect(screen.queryByRole('button', { name: /confirm/i })).not.toBeInTheDocument();
      });
    });
  });

  describe('error handling', () => {
    it('shows error message on API failure', async () => {
      mockGetAdminChallengeTemplates.mockRejectedValueOnce(new Error('Failed to fetch templates'));

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByText(/failed to fetch templates/i)).toBeInTheDocument();
      });
    });

    it('shows error when create fails', async () => {
      mockGetAdminChallengeTemplates.mockResolvedValueOnce(
        createMockPaginatedResponse([])
      );
      mockCreateAdminChallengeTemplate.mockRejectedValueOnce(new Error('Create failed'));

      render(<ChallengeTemplatesTable />);

      await waitFor(() => {
        expect(screen.getByText(/no challenge templates found/i)).toBeInTheDocument();
      });

      const addButton = screen.getByRole('button', { name: /add template/i });
      fireEvent.click(addButton);

      await waitFor(() => {
        expect(screen.getByPlaceholderText('Name')).toBeInTheDocument();
      });

      fireEvent.change(screen.getByPlaceholderText('Name'), { target: { value: 'Test' } });

      const saveButton = screen.getByRole('button', { name: /save/i });
      fireEvent.click(saveButton);

      await waitFor(() => {
        expect(screen.getByText(/create failed/i)).toBeInTheDocument();
      });
    });
  });

  describe('refresh functionality', () => {
    it('refetches templates when refreshTrigger changes', async () => {
      const mockTemplates = [createMockChallengeTemplate()];
      mockGetAdminChallengeTemplates
        .mockResolvedValueOnce(createMockPaginatedResponse(mockTemplates))
        .mockResolvedValueOnce(createMockPaginatedResponse(mockTemplates));

      const { rerender } = render(<ChallengeTemplatesTable refreshTrigger={0} />);

      await waitFor(() => {
        expect(mockGetAdminChallengeTemplates).toHaveBeenCalledTimes(1);
      });

      // Trigger refresh
      rerender(<ChallengeTemplatesTable refreshTrigger={1} />);

      await waitFor(() => {
        expect(mockGetAdminChallengeTemplates).toHaveBeenCalledTimes(2);
      });
    });
  });
});
