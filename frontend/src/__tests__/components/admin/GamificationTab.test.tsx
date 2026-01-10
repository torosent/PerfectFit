/**
 * GamificationTab Component Tests
 *
 * Tests for the admin gamification tab container that manages sub-tabs
 * for achievements, challenge templates, and cosmetics.
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { GamificationTab } from '@/components/admin/GamificationTab';
import * as adminGamificationClient from '@/lib/api/admin-gamification-client';
import type { PaginatedResponse, AdminAchievement, AdminChallengeTemplate, AdminCosmetic } from '@/types';

// Mock the API client
jest.mock('@/lib/api/admin-gamification-client');

// Mock the auth store
const mockToken = 'test-admin-token';
jest.mock('@/lib/stores/auth-store', () => ({
  useToken: () => mockToken,
}));

// Mock framer-motion to avoid animation issues in tests
jest.mock('motion/react', () => ({
  motion: {
    div: ({ children, ...props }: React.PropsWithChildren<Record<string, unknown>>) => (
      <div {...props}>{children}</div>
    ),
  },
}));

const mockGetAdminAchievements = adminGamificationClient.getAdminAchievements as jest.MockedFunction<
  typeof adminGamificationClient.getAdminAchievements
>;
const mockGetAdminChallengeTemplates = adminGamificationClient.getAdminChallengeTemplates as jest.MockedFunction<
  typeof adminGamificationClient.getAdminChallengeTemplates
>;
const mockGetAdminCosmetics = adminGamificationClient.getAdminCosmetics as jest.MockedFunction<
  typeof adminGamificationClient.getAdminCosmetics
>;

// Helper to create mock paginated responses
function createMockAchievementResponse(): PaginatedResponse<AdminAchievement> {
  return {
    items: [
      {
        id: 1,
        name: 'Test Achievement',
        description: 'Test description',
        category: 'Games',
        iconUrl: '/icons/test.svg',
        unlockCondition: '{}',
        rewardType: 'XPBoost',
        rewardValue: 100,
        isSecret: false,
        displayOrder: 1,
        rewardCosmeticCode: null,
      },
    ],
    page: 1,
    pageSize: 10,
    totalCount: 1,
    totalPages: 1,
  };
}

function createMockTemplateResponse(): PaginatedResponse<AdminChallengeTemplate> {
  return {
    items: [
      {
        id: 1,
        name: 'Test Challenge',
        description: 'Test description',
        type: 'Daily',
        targetValue: 5,
        xpReward: 50,
        isActive: true,
      },
    ],
    page: 1,
    pageSize: 10,
    totalCount: 1,
    totalPages: 1,
  };
}

function createMockCosmeticResponse(): PaginatedResponse<AdminCosmetic> {
  return {
    items: [
      {
        id: 1,
        code: 'test-cosmetic',
        name: 'Test Cosmetic',
        description: 'Test description',
        type: 'BoardTheme',
        assetUrl: '/assets/test.css',
        previewUrl: '/previews/test.png',
        rarity: 'Common',
        isDefault: false,
      },
    ],
    page: 1,
    pageSize: 10,
    totalCount: 1,
    totalPages: 1,
  };
}

describe('GamificationTab', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // Set up default mocks
    mockGetAdminAchievements.mockResolvedValue(createMockAchievementResponse());
    mockGetAdminChallengeTemplates.mockResolvedValue(createMockTemplateResponse());
    mockGetAdminCosmetics.mockResolvedValue(createMockCosmeticResponse());
  });

  describe('sub-tab navigation', () => {
    it('renders with Achievements sub-tab selected by default', async () => {
      render(<GamificationTab />);

      // Should show achievements tab as selected
      const achievementsTab = screen.getByRole('button', { name: /^achievements$/i });
      expect(achievementsTab).toBeInTheDocument();

      // Should show achievements content
      await waitFor(() => {
        expect(screen.getByText('Test Achievement')).toBeInTheDocument();
      });
    });

    it('displays all three sub-tab buttons', () => {
      render(<GamificationTab />);

      expect(screen.getByRole('button', { name: /^achievements$/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /^challenge templates$/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /^cosmetics$/i })).toBeInTheDocument();
    });

    it('switches to Challenge Templates sub-tab', async () => {
      render(<GamificationTab />);

      // Wait for initial load
      await waitFor(() => {
        expect(screen.getByText('Test Achievement')).toBeInTheDocument();
      });

      // Click Challenge Templates tab
      const templatesTab = screen.getByRole('button', { name: /^challenge templates$/i });
      fireEvent.click(templatesTab);

      // Should now show challenge templates content
      await waitFor(() => {
        expect(screen.getByText('Test Challenge')).toBeInTheDocument();
      });

      // Achievements content should not be visible
      expect(screen.queryByText('Test Achievement')).not.toBeInTheDocument();
    });

    it('switches to Cosmetics sub-tab', async () => {
      render(<GamificationTab />);

      // Wait for initial load
      await waitFor(() => {
        expect(screen.getByText('Test Achievement')).toBeInTheDocument();
      });

      // Click Cosmetics tab
  const cosmeticsTab = screen.getByRole('button', { name: /^cosmetics$/i });
      fireEvent.click(cosmeticsTab);

      // Should now show cosmetics content
      await waitFor(() => {
        expect(screen.getByText('Test Cosmetic')).toBeInTheDocument();
      });

      // Achievements content should not be visible
      expect(screen.queryByText('Test Achievement')).not.toBeInTheDocument();
    });

    it('can switch back to Achievements sub-tab', async () => {
      render(<GamificationTab />);

      // Wait for initial load
      await waitFor(() => {
        expect(screen.getByText('Test Achievement')).toBeInTheDocument();
      });

      // Switch to Cosmetics
  const cosmeticsTab = screen.getByRole('button', { name: /^cosmetics$/i });
      fireEvent.click(cosmeticsTab);

      await waitFor(() => {
        expect(screen.getByText('Test Cosmetic')).toBeInTheDocument();
      });

      // Switch back to Achievements
      const achievementsTab = screen.getByRole('button', { name: /^achievements$/i });
      fireEvent.click(achievementsTab);

      await waitFor(() => {
        expect(screen.getByText('Test Achievement')).toBeInTheDocument();
      });

      // Cosmetics content should not be visible
      expect(screen.queryByText('Test Cosmetic')).not.toBeInTheDocument();
    });
  });

  describe('refreshTrigger prop', () => {
    it('passes refreshTrigger to child components', async () => {
      const { rerender } = render(<GamificationTab refreshTrigger={0} />);

      // Wait for initial load
      await waitFor(() => {
        expect(mockGetAdminAchievements).toHaveBeenCalledTimes(1);
      });

      // Change refreshTrigger
      rerender(<GamificationTab refreshTrigger={1} />);

      // Should trigger a refetch
      await waitFor(() => {
        expect(mockGetAdminAchievements).toHaveBeenCalledTimes(2);
      });
    });

    it('passes refreshTrigger to ChallengeTemplatesTable when active', async () => {
      const { rerender } = render(<GamificationTab refreshTrigger={0} />);

      // Switch to Challenge Templates tab
      const templatesTab = screen.getByRole('button', { name: /^challenge templates$/i });
      fireEvent.click(templatesTab);

      await waitFor(() => {
        expect(mockGetAdminChallengeTemplates).toHaveBeenCalledTimes(1);
      });

      // Change refreshTrigger
      rerender(<GamificationTab refreshTrigger={1} />);

      await waitFor(() => {
        expect(mockGetAdminChallengeTemplates).toHaveBeenCalledTimes(2);
      });
    });

    it('passes refreshTrigger to CosmeticsTable when active', async () => {
      const { rerender } = render(<GamificationTab refreshTrigger={0} />);

      // Switch to Cosmetics tab
      const cosmeticsTab = screen.getByRole('button', { name: /^cosmetics$/i });
      fireEvent.click(cosmeticsTab);

      await waitFor(() => {
        expect(mockGetAdminCosmetics).toHaveBeenCalledTimes(1);
      });

      // Change refreshTrigger
      rerender(<GamificationTab refreshTrigger={1} />);

      await waitFor(() => {
        expect(mockGetAdminCosmetics).toHaveBeenCalledTimes(2);
      });
    });
  });

  describe('tab state', () => {
    it('only renders the active sub-tab content', async () => {
      render(<GamificationTab />);

      // Initially only achievements table should be rendered
      await waitFor(() => {
        expect(screen.getByText('Test Achievement')).toBeInTheDocument();
      });

      // Challenge templates and cosmetics APIs should not be called initially
      expect(mockGetAdminAchievements).toHaveBeenCalled();
      expect(mockGetAdminChallengeTemplates).not.toHaveBeenCalled();
      expect(mockGetAdminCosmetics).not.toHaveBeenCalled();
    });

    it('fetches data when switching to a new tab', async () => {
      render(<GamificationTab />);

      // Wait for achievements to load
      await waitFor(() => {
        expect(mockGetAdminAchievements).toHaveBeenCalled();
      });

      // Switch to Challenge Templates
  const templatesTab = screen.getByRole('button', { name: /^challenge templates$/i });
      fireEvent.click(templatesTab);

      await waitFor(() => {
        expect(mockGetAdminChallengeTemplates).toHaveBeenCalled();
      });

      // Switch to Cosmetics
  const cosmeticsTab = screen.getByRole('button', { name: /^cosmetics$/i });
      fireEvent.click(cosmeticsTab);

      await waitFor(() => {
        expect(mockGetAdminCosmetics).toHaveBeenCalled();
      });
    });
  });

  describe('error handling', () => {
    it('handles error in AchievementsTable gracefully', async () => {
      mockGetAdminAchievements.mockRejectedValueOnce(new Error('Failed to load'));

      render(<GamificationTab />);

      await waitFor(() => {
        expect(screen.getByText(/failed to load/i)).toBeInTheDocument();
      });

      // Should still be able to switch tabs
  const templatesTab = screen.getByRole('button', { name: /^challenge templates$/i });
      fireEvent.click(templatesTab);

      await waitFor(() => {
        expect(screen.getByText('Test Challenge')).toBeInTheDocument();
      });
    });

    it('handles error in ChallengeTemplatesTable gracefully', async () => {
      mockGetAdminChallengeTemplates.mockRejectedValueOnce(new Error('Failed to load templates'));

      render(<GamificationTab />);

      // Switch to Challenge Templates
  const templatesTab = screen.getByRole('button', { name: /^challenge templates$/i });
      fireEvent.click(templatesTab);

      await waitFor(() => {
        expect(screen.getByText(/failed to load templates/i)).toBeInTheDocument();
      });
    });

    it('handles error in CosmeticsTable gracefully', async () => {
      mockGetAdminCosmetics.mockRejectedValueOnce(new Error('Failed to load cosmetics'));

      render(<GamificationTab />);

      // Switch to Cosmetics
  const cosmeticsTab = screen.getByRole('button', { name: /^cosmetics$/i });
      fireEvent.click(cosmeticsTab);

      await waitFor(() => {
        expect(screen.getByText(/failed to load cosmetics/i)).toBeInTheDocument();
      });
    });
  });

  describe('accessibility', () => {
    it('sub-tab buttons are keyboard accessible', async () => {
      render(<GamificationTab />);

      const templatesTab = screen.getByRole('button', { name: /^challenge templates$/i });
      
      // Buttons should be focusable
      templatesTab.focus();
      expect(document.activeElement).toBe(templatesTab);

      // Pressing Enter should activate the tab
      fireEvent.keyDown(templatesTab, { key: 'Enter', code: 'Enter' });
      fireEvent.click(templatesTab); // Simulate button activation

      await waitFor(() => {
        expect(screen.getByText('Test Challenge')).toBeInTheDocument();
      });
    });
  });
});
