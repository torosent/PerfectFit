/**
 * SeasonPassTrack Component Tests
 *
 * Tests for the horizontal season pass progress track.
 */

import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { SeasonPassTrack } from '@/components/gamification/SeasonPassTrack';
import type { SeasonPassInfo, SeasonReward } from '@/types/gamification';

// Mock motion/react to avoid animation issues in tests
jest.mock('motion/react', () => {
  const React = require('react');
  return {
    motion: {
      div: React.forwardRef(({ children, ...props }: React.PropsWithChildren<object>, ref: React.Ref<HTMLDivElement>) => 
        React.createElement('div', { ...props, ref }, children)),
      button: React.forwardRef(({ children, onClick, ...props }: React.PropsWithChildren<{ onClick?: () => void }>, ref: React.Ref<HTMLButtonElement>) => 
        React.createElement('button', { onClick, ...props, ref }, children)),
      span: React.forwardRef(({ children, ...props }: React.PropsWithChildren<object>, ref: React.Ref<HTMLSpanElement>) => 
        React.createElement('span', { ...props, ref }, children)),
    },
    AnimatePresence: ({ children }: React.PropsWithChildren) => React.createElement(React.Fragment, null, children),
  };
});

describe('SeasonPassTrack', () => {
  const mockRewards: SeasonReward[] = [
    { id: 1, tier: 1, rewardType: 'XPBoost', rewardValue: 10, xpRequired: 100, isClaimed: true, canClaim: false },
    { id: 2, tier: 2, rewardType: 'Cosmetic', rewardValue: 1, xpRequired: 250, isClaimed: false, canClaim: true },
    { id: 3, tier: 3, rewardType: 'StreakFreeze', rewardValue: 1, xpRequired: 500, isClaimed: false, canClaim: false },
    { id: 4, tier: 4, rewardType: 'Cosmetic', rewardValue: 2, xpRequired: 800, isClaimed: false, canClaim: false },
    { id: 5, tier: 5, rewardType: 'XPBoost', rewardValue: 25, xpRequired: 1200, isClaimed: false, canClaim: false },
  ];

  const mockSeasonPass: SeasonPassInfo = {
    seasonId: 1,
    seasonName: 'Ocean Depths',
    seasonNumber: 1,
    currentXP: 300,
    currentTier: 2,
    endsAt: new Date(Date.now() + 20 * 24 * 60 * 60 * 1000).toISOString(), // 20 days from now
    rewards: mockRewards,
  };

  describe('renders all tiers', () => {
    it('should display all tier rewards', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      
      // Should show all 5 tier numbers (use data-testid to find each tier)
      expect(screen.getByTestId('reward-tier-1')).toBeInTheDocument();
      expect(screen.getByTestId('reward-tier-2')).toBeInTheDocument();
      expect(screen.getByTestId('reward-tier-3')).toBeInTheDocument();
      expect(screen.getByTestId('reward-tier-4')).toBeInTheDocument();
      expect(screen.getByTestId('reward-tier-5')).toBeInTheDocument();
    });

    it('should display season name', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      expect(screen.getByText('Ocean Depths')).toBeInTheDocument();
    });

    it('should display season number', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      expect(screen.getByText(/season 1/i)).toBeInTheDocument();
    });
  });

  describe('shows current progress', () => {
    it('should display current XP', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      expect(screen.getByText(/300/)).toBeInTheDocument();
    });

    it('should display current tier', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      expect(screen.getByTestId('current-tier')).toHaveTextContent('2');
    });

    it('should show progress bar between tiers', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      expect(screen.getByTestId('tier-progress-bar')).toBeInTheDocument();
    });

    it('should indicate current position on track', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      const currentTierIndicator = screen.getByTestId('current-tier-indicator');
      expect(currentTierIndicator).toBeInTheDocument();
    });
  });

  describe('reward states', () => {
    it('should show claimed state for claimed rewards', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      const claimedReward = screen.getByTestId('reward-tier-1');
      expect(claimedReward).toHaveClass('claimed');
    });

    it('should show unclaimed state for claimable rewards', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      const unclaimedReward = screen.getByTestId('reward-tier-2');
      expect(unclaimedReward).toHaveClass('claimable');
    });

    it('should show locked state for locked rewards', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      const lockedReward = screen.getByTestId('reward-tier-3');
      expect(lockedReward).toHaveClass('locked');
    });
  });

  describe('claim button works', () => {
    it('should call onClaimReward when claim button is clicked', () => {
      const mockOnClaimReward = jest.fn();
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={mockOnClaimReward} />);
      
      const claimButton = screen.getByTestId('claim-button-2');
      fireEvent.click(claimButton);
      
      expect(mockOnClaimReward).toHaveBeenCalledWith('2');
    });

    it('should show claim button only for claimable rewards', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      
      expect(screen.getByTestId('claim-button-2')).toBeInTheDocument();
      expect(screen.queryByTestId('claim-button-1')).not.toBeInTheDocument(); // Already claimed
      expect(screen.queryByTestId('claim-button-3')).not.toBeInTheDocument(); // Locked
    });
  });

  describe('disabled when already claimed', () => {
    it('should not show claim button for claimed rewards', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      
      expect(screen.queryByTestId('claim-button-1')).not.toBeInTheDocument();
    });

    it('should show checkmark for claimed rewards', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      
      expect(screen.getByTestId('claimed-checkmark-1')).toBeInTheDocument();
    });
  });

  describe('shows XP requirements', () => {
    it('should display XP required for each tier', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      
      expect(screen.getByText(/100\s*XP/i)).toBeInTheDocument();
      expect(screen.getByText(/250\s*XP/i)).toBeInTheDocument();
    });
  });

  describe('accessibility', () => {
    it('should have proper aria-label for track', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      expect(screen.getByLabelText(/season pass.*tier 2/i)).toBeInTheDocument();
    });

    it('should have accessible buttons', () => {
      render(<SeasonPassTrack seasonPass={mockSeasonPass} onClaimReward={jest.fn()} />);
      expect(screen.getByRole('button', { name: /claim tier 2/i })).toBeInTheDocument();
    });
  });
});
