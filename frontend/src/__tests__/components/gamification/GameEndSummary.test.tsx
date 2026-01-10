/**
 * GameEndSummary Component Tests
 *
 * Tests for the post-game gamification recap component.
 */

import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { GameEndSummary } from '@/components/gamification/GameEndSummary';
import type { GameEndGamification, StreakInfo } from '@/types/gamification';

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
      h2: React.forwardRef(({ children, ...props }: React.PropsWithChildren<object>, ref: React.Ref<HTMLHeadingElement>) => 
        React.createElement('h2', { ...props, ref }, children)),
      h3: React.forwardRef(({ children, ...props }: React.PropsWithChildren<object>, ref: React.Ref<HTMLHeadingElement>) => 
        React.createElement('h3', { ...props, ref }, children)),
      p: React.forwardRef(({ children, ...props }: React.PropsWithChildren<object>, ref: React.Ref<HTMLParagraphElement>) => 
        React.createElement('p', { ...props, ref }, children)),
      li: React.forwardRef(({ children, ...props }: React.PropsWithChildren<object>, ref: React.Ref<HTMLLIElement>) => 
        React.createElement('li', { ...props, ref }, children)),
    },
    AnimatePresence: ({ children }: React.PropsWithChildren) => React.createElement(React.Fragment, null, children),
  };
});

describe('GameEndSummary', () => {
  const mockStreakMaintained: StreakInfo = {
    currentStreak: 5,
    longestStreak: 10,
    freezeTokens: 2,
    isAtRisk: false,
    resetTime: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
  };

  const mockStreakIncreased: StreakInfo = {
    currentStreak: 6,
    longestStreak: 10,
    freezeTokens: 2,
    isAtRisk: false,
    resetTime: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
  };

  const mockGameEndData: GameEndGamification = {
    streak: mockStreakMaintained,
    challengeUpdates: [
      { challengeId: 1, challengeName: 'Score 5000 points', newProgress: 75, justCompleted: false, xpEarned: null },
      { challengeId: 2, challengeName: 'Clear 10 lines', newProgress: 100, justCompleted: true, xpEarned: 50 },
    ],
    newAchievements: [
      {
        achievementId: 1,
        name: 'First Victory',
        description: 'Win your first game',
        iconUrl: '/icons/first-victory.png',
        rewardType: 'XPBoost',
        rewardValue: 25,
      },
    ],
    seasonProgress: {
      xpEarned: 150,
      totalXP: 500,
      newTier: 3,
      tierUp: true,
      newRewardsCount: 1,
    },
    goalUpdates: [
      { goalId: 1, description: 'Beat your average score', newProgress: 100, justCompleted: true },
    ],
  };

  const mockGameEndNoProgress: GameEndGamification = {
    streak: mockStreakMaintained,
    challengeUpdates: [],
    newAchievements: [],
    seasonProgress: {
      xpEarned: 50,
      totalXP: 350,
      newTier: 2,
      tierUp: false,
      newRewardsCount: 0,
    },
    goalUpdates: [],
  };

  describe('displays streak update', () => {
    it('should display current streak', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByText('5')).toBeInTheDocument();
      expect(screen.getByText(/day streak/i)).toBeInTheDocument();
    });

    it('should indicate streak maintained when same', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByTestId('streak-maintained')).toBeInTheDocument();
    });

    it('should indicate streak increased', () => {
      const dataWithIncrease = { ...mockGameEndData, streak: mockStreakIncreased };
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={dataWithIncrease}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByTestId('streak-increased')).toBeInTheDocument();
    });

    it('should indicate streak broken when decreased to 0', () => {
      const brokenStreak: StreakInfo = {
        ...mockStreakMaintained,
        currentStreak: 0,
      };
      const dataWithBrokenStreak = { ...mockGameEndData, streak: brokenStreak };
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={dataWithBrokenStreak}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByTestId('streak-broken')).toBeInTheDocument();
    });
  });

  describe('shows XP earned', () => {
    it('should display total XP earned', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByText(/\+150\s*XP/i)).toBeInTheDocument();
    });

    it('should show tier up notification when applicable', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByText(/tier 3/i)).toBeInTheDocument();
      expect(screen.getByTestId('tier-up')).toBeInTheDocument();
    });

    it('should not show tier up when no tier change', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndNoProgress}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.queryByTestId('tier-up')).not.toBeInTheDocument();
    });
  });

  describe('lists achievements unlocked', () => {
    it('should display newly unlocked achievements', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByText('First Victory')).toBeInTheDocument();
    });

    it('should show achievement count', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByText(/1.*achievement/i)).toBeInTheDocument();
    });

    it('should not show achievements section when none unlocked', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndNoProgress}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.queryByTestId('achievements-section')).not.toBeInTheDocument();
    });
  });

  describe('displays challenge progress', () => {
    it('should show challenge progress updates', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByText(/score 5000 points/i)).toBeInTheDocument();
      expect(screen.getByText(/75%/)).toBeInTheDocument();
    });

    it('should highlight completed challenges', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByTestId('challenge-completed-2')).toBeInTheDocument();
    });

    it('should show XP earned from completed challenges', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByText(/\+50/)).toBeInTheDocument();
    });
  });

  describe('displays goal progress', () => {
    it('should show completed goals', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByText(/beat your average score/i)).toBeInTheDocument();
    });

    it('should not show goals section when no updates', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndNoProgress}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.queryByTestId('goals-section')).not.toBeInTheDocument();
    });
  });

  describe('continue button works', () => {
    it('should call onContinue when clicked', () => {
      const mockOnContinue = jest.fn();
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={mockOnContinue}
        />
      );
      
      const continueButton = screen.getByRole('button', { name: /continue/i });
      fireEvent.click(continueButton);
      
      expect(mockOnContinue).toHaveBeenCalled();
    });

    it('should have Continue button', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByRole('button', { name: /continue/i })).toBeInTheDocument();
    });
  });

  describe('visibility', () => {
    it('should render when isOpen is true', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByTestId('game-end-summary')).toBeInTheDocument();
    });

    it('should not render when isOpen is false', () => {
      render(
        <GameEndSummary
          isOpen={false}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.queryByTestId('game-end-summary')).not.toBeInTheDocument();
    });
  });

  describe('accessibility', () => {
    it('should have proper heading structure', () => {
      render(
        <GameEndSummary
          isOpen={true}
          gameEndData={mockGameEndData}
          previousStreak={5}
          onContinue={jest.fn()}
        />
      );
      expect(screen.getByRole('heading', { name: /game summary/i })).toBeInTheDocument();
    });
  });
});
