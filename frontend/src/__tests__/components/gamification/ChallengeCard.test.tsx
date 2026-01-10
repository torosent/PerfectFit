/**
 * ChallengeCard Component Tests
 *
 * Tests for individual challenge display with progress.
 */

import React from 'react';
import { render, screen } from '@testing-library/react';
import { ChallengeCard } from '@/components/gamification/ChallengeCard';
import type { Challenge } from '@/types/gamification';

// Mock motion/react to avoid animation issues in tests
jest.mock('motion/react', () => {
  const React = require('react');
  return {
    motion: {
      div: React.forwardRef(({ children, ...props }: React.PropsWithChildren<object>, ref: React.Ref<HTMLDivElement>) => 
        React.createElement('div', { ...props, ref }, children)),
      span: React.forwardRef(({ children, ...props }: React.PropsWithChildren<object>, ref: React.Ref<HTMLSpanElement>) => 
        React.createElement('span', { ...props, ref }, children)),
    },
    AnimatePresence: ({ children }: React.PropsWithChildren) => React.createElement(React.Fragment, null, children),
  };
});

describe('ChallengeCard', () => {
  const mockDailyChallenge: Challenge = {
    id: 1,
    name: 'Score Master',
    description: 'Score 10,000 points in a single game',
    type: 'Daily',
    targetValue: 10000,
    currentProgress: 5000,
    xpReward: 100,
    isCompleted: false,
    endsAt: new Date(Date.now() + 12 * 60 * 60 * 1000).toISOString(), // 12 hours from now
  };

  const mockWeeklyChallenge: Challenge = {
    id: 2,
    name: 'Combo Champion',
    description: 'Achieve a 10x combo in any game',
    type: 'Weekly',
    targetValue: 10,
    currentProgress: 10,
    xpReward: 500,
    isCompleted: true,
    endsAt: new Date(Date.now() + 5 * 24 * 60 * 60 * 1000).toISOString(), // 5 days from now
  };

  describe('renders challenge info', () => {
    it('should display challenge name', () => {
      render(<ChallengeCard challenge={mockDailyChallenge} />);
      expect(screen.getByText('Score Master')).toBeInTheDocument();
    });

    it('should display challenge description', () => {
      render(<ChallengeCard challenge={mockDailyChallenge} />);
      expect(screen.getByText('Score 10,000 points in a single game')).toBeInTheDocument();
    });

    it('should display XP reward', () => {
      render(<ChallengeCard challenge={mockDailyChallenge} />);
      expect(screen.getByText(/100\s*XP/i)).toBeInTheDocument();
    });
  });

  describe('shows progress bar', () => {
    it('should display progress bar with correct percentage', () => {
      render(<ChallengeCard challenge={mockDailyChallenge} />);
      const progressBar = screen.getByRole('progressbar');
      expect(progressBar).toBeInTheDocument();
      expect(progressBar).toHaveAttribute('aria-valuenow', '50');
    });

    it('should show progress text', () => {
      render(<ChallengeCard challenge={mockDailyChallenge} />);
      expect(screen.getByText(/5,?000.*\/.*10,?000/)).toBeInTheDocument();
    });

    it('should show 100% for completed challenge', () => {
      render(<ChallengeCard challenge={mockWeeklyChallenge} />);
      const progressBar = screen.getByRole('progressbar');
      expect(progressBar).toHaveAttribute('aria-valuenow', '100');
    });
  });

  describe('shows completion state', () => {
    it('should show checkmark for completed challenge', () => {
      render(<ChallengeCard challenge={mockWeeklyChallenge} />);
      expect(screen.getByTestId('challenge-completed')).toBeInTheDocument();
    });

    it('should not show checkmark for incomplete challenge', () => {
      render(<ChallengeCard challenge={mockDailyChallenge} />);
      expect(screen.queryByTestId('challenge-completed')).not.toBeInTheDocument();
    });

    it('should apply completed styling', () => {
      render(<ChallengeCard challenge={mockWeeklyChallenge} />);
      expect(screen.getByTestId('challenge-card')).toHaveClass('completed');
    });
  });

  describe('shows time remaining', () => {
    it('should display time remaining for daily challenge', () => {
      render(<ChallengeCard challenge={mockDailyChallenge} />);
      // Should show hours remaining
      expect(screen.getByText(/\d+h/i)).toBeInTheDocument();
    });

    it('should display days remaining for weekly challenge', () => {
      render(<ChallengeCard challenge={mockWeeklyChallenge} />);
      // Should show days remaining
      expect(screen.getByText(/\d+d/i)).toBeInTheDocument();
    });
  });

  describe('daily vs weekly visual distinction', () => {
    it('should have daily badge for daily challenges', () => {
      render(<ChallengeCard challenge={mockDailyChallenge} />);
      expect(screen.getByText('Daily')).toBeInTheDocument();
    });

    it('should have weekly badge for weekly challenges', () => {
      render(<ChallengeCard challenge={mockWeeklyChallenge} />);
      expect(screen.getByText('Weekly')).toBeInTheDocument();
    });

    it('should apply different styling for daily challenges', () => {
      render(<ChallengeCard challenge={mockDailyChallenge} />);
      expect(screen.getByTestId('challenge-type-badge')).toHaveClass('daily');
    });

    it('should apply different styling for weekly challenges', () => {
      render(<ChallengeCard challenge={mockWeeklyChallenge} />);
      expect(screen.getByTestId('challenge-type-badge')).toHaveClass('weekly');
    });
  });

  describe('accessibility', () => {
    it('should have proper aria-label for challenge', () => {
      render(<ChallengeCard challenge={mockDailyChallenge} />);
      expect(screen.getByLabelText(/score master.*50.*complete/i)).toBeInTheDocument();
    });

    it('should have accessible progress bar', () => {
      render(<ChallengeCard challenge={mockDailyChallenge} />);
      const progressBar = screen.getByRole('progressbar');
      expect(progressBar).toHaveAttribute('aria-valuemin', '0');
      expect(progressBar).toHaveAttribute('aria-valuemax', '100');
    });
  });
});
