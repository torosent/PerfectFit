/**
 * LeaderboardTable Avatar Display Tests
 * 
 * Tests for displaying emoji avatars in the leaderboard table.
 */

import React from 'react';
import { render, screen } from '@testing-library/react';
import { LeaderboardTable } from '@/components/leaderboard/LeaderboardTable';
import type { LeaderboardEntry } from '@/lib/api/leaderboard-client';

describe('LeaderboardTable Avatar Display', () => {
  const mockEntries: LeaderboardEntry[] = [
    {
      rank: 1,
      userId: 1,
      displayName: 'Player One',
      score: 10000,
      linesCleared: 50,
      maxCombo: 5,
      achievedAt: new Date().toISOString(),
      avatar: '🚀',
    },
    {
      rank: 2,
      userId: 2,
      displayName: 'Player Two',
      score: 9000,
      linesCleared: 45,
      maxCombo: 4,
      achievedAt: new Date().toISOString(),
      avatar: '🎮',
    },
    {
      rank: 3,
      userId: 3,
      displayName: 'Player Three',
      score: 8000,
      linesCleared: 40,
      maxCombo: 3,
      achievedAt: new Date().toISOString(),
      // No avatar - should show fallback
    },
  ];

  describe('displays emoji avatar when set', () => {
    it('should display emoji avatar for players who have one', () => {
      render(<LeaderboardTable entries={mockEntries} />);

      // Player One has 🚀 avatar
      expect(screen.getByText('🚀')).toBeInTheDocument();
      
      // Player Two has 🎮 avatar
      expect(screen.getByText('🎮')).toBeInTheDocument();
    });

    it('should display avatar with proper styling', () => {
      render(<LeaderboardTable entries={mockEntries} />);

      const avatarEmoji = screen.getByText('🚀');
      const avatarContainer = avatarEmoji.closest('[data-testid="avatar-container"]');
      expect(avatarContainer).toBeInTheDocument();
    });
  });

  describe('displays fallback when no avatar', () => {
    it('should display initials when player has no avatar', () => {
      render(<LeaderboardTable entries={mockEntries} />);

      // Player Three has no avatar, should show initials "PT"
      expect(screen.getByText('PT')).toBeInTheDocument();
    });

    it('should handle single-word names for initials', () => {
      const entriesWithSingleName: LeaderboardEntry[] = [
        {
          rank: 1,
          userId: 1,
          displayName: 'Anonymous',
          score: 10000,
          linesCleared: 50,
          maxCombo: 5,
          achievedAt: new Date().toISOString(),
          // No avatar
        },
      ];

      render(<LeaderboardTable entries={entriesWithSingleName} />);

      // Should show first two letters "AN"
      expect(screen.getByText('AN')).toBeInTheDocument();
    });
  });

  describe('avatar and name display together', () => {
    it('should display avatar next to player name', () => {
      render(<LeaderboardTable entries={mockEntries} />);

      // Find the row with Player One
      const playerOneName = screen.getByText('Player One');
      const row = playerOneName.closest('tr');
      
      // The row should contain the avatar
      expect(row).toHaveTextContent('🚀');
      expect(row).toHaveTextContent('Player One');
    });
  });

  describe('loading state', () => {
    it('should show skeleton placeholders during loading', () => {
      render(<LeaderboardTable entries={[]} isLoading={true} />);

      // Should not show any avatar emojis during loading
      expect(screen.queryByText('🚀')).not.toBeInTheDocument();
    });
  });

  describe('empty state', () => {
    it('should show empty message when no entries', () => {
      render(<LeaderboardTable entries={[]} />);

      expect(screen.getByText(/no scores yet/i)).toBeInTheDocument();
    });
  });
});
