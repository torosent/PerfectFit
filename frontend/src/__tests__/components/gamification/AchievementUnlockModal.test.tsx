/**
 * AchievementUnlockModal Component Tests
 *
 * Tests for the achievement unlock celebration modal.
 */

import React from 'react';
import { render, screen, fireEvent, act } from '@testing-library/react';
import { AchievementUnlockModal } from '@/components/gamification/AchievementUnlockModal';
import type { AchievementUnlock } from '@/types/gamification';

// Mock motion/react to avoid animation issues in tests
jest.mock('motion/react', () => {
  const React = require('react');
  return {
    motion: {
      div: React.forwardRef(({ children, onClick, ...props }: React.PropsWithChildren<{ onClick?: () => void }>, ref: React.Ref<HTMLDivElement>) => 
        React.createElement('div', { onClick, ...props, ref }, children)),
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
      img: React.forwardRef((props: object, ref: React.Ref<HTMLImageElement>) => 
        React.createElement('img', { ...props, ref })),
    },
    AnimatePresence: ({ children }: React.PropsWithChildren) => React.createElement(React.Fragment, null, children),
  };
});

// Mock confetti
jest.mock('@/lib/confetti', () => ({
  fireConfetti: jest.fn(),
  fireHighScoreConfetti: jest.fn(),
}));

describe('AchievementUnlockModal', () => {
  const mockAchievement: AchievementUnlock = {
    achievementId: 1,
    name: 'First Steps',
    description: 'Complete your first game',
    iconUrl: '/icons/achievements/first-steps.png',
    rewardType: 'XPBoost',
    rewardValue: 50,
  };

  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  describe('opens with animation', () => {
    it('should render modal when isOpen is true', () => {
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={mockAchievement}
          onClose={jest.fn()}
        />
      );
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    it('should not render modal when isOpen is false', () => {
      render(
        <AchievementUnlockModal
          isOpen={false}
          achievement={mockAchievement}
          onClose={jest.fn()}
        />
      );
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });

  describe('displays achievement info', () => {
    it('should display achievement name', () => {
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={mockAchievement}
          onClose={jest.fn()}
        />
      );
      expect(screen.getByText('First Steps')).toBeInTheDocument();
    });

    it('should display achievement description', () => {
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={mockAchievement}
          onClose={jest.fn()}
        />
      );
      expect(screen.getByText('Complete your first game')).toBeInTheDocument();
    });

    it('should display achievement icon', () => {
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={mockAchievement}
          onClose={jest.fn()}
        />
      );
      const icon = screen.getByRole('img', { name: /first steps/i });
      expect(icon).toBeInTheDocument();
      expect(icon).toHaveAttribute('src', '/icons/achievements/first-steps.png');
    });

    it('should display reward info', () => {
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={mockAchievement}
          onClose={jest.fn()}
        />
      );
      expect(screen.getByText(/\+50\s*XP/i)).toBeInTheDocument();
    });

    it('should display Achievement Unlocked title', () => {
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={mockAchievement}
          onClose={jest.fn()}
        />
      );
      expect(screen.getByText(/achievement unlocked/i)).toBeInTheDocument();
    });
  });

  describe('closes on button click', () => {
    it('should call onClose when Awesome! button is clicked', () => {
      const mockOnClose = jest.fn();
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={mockAchievement}
          onClose={mockOnClose}
        />
      );
      
      const button = screen.getByRole('button', { name: /awesome/i });
      fireEvent.click(button);
      
      expect(mockOnClose).toHaveBeenCalled();
    });

    it('should have dismiss button with "Awesome!" text', () => {
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={mockAchievement}
          onClose={jest.fn()}
        />
      );
      expect(screen.getByRole('button', { name: /awesome/i })).toBeInTheDocument();
    });
  });

  describe('auto-closes after timeout', () => {
    it('should auto-close after 5 seconds', () => {
      const mockOnClose = jest.fn();
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={mockAchievement}
          onClose={mockOnClose}
          autoCloseDelay={5000}
        />
      );
      
      expect(mockOnClose).not.toHaveBeenCalled();
      
      act(() => {
        jest.advanceTimersByTime(5000);
      });
      
      expect(mockOnClose).toHaveBeenCalled();
    });

    it('should not auto-close if disabled', () => {
      const mockOnClose = jest.fn();
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={mockAchievement}
          onClose={mockOnClose}
          autoCloseDelay={0}
        />
      );
      
      act(() => {
        jest.advanceTimersByTime(10000);
      });
      
      expect(mockOnClose).not.toHaveBeenCalled();
    });
  });

  describe('reward display variations', () => {
    it('should display XP reward correctly', () => {
      const xpAchievement: AchievementUnlock = {
        ...mockAchievement,
        rewardType: 'XPBoost',
        rewardValue: 100,
      };
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={xpAchievement}
          onClose={jest.fn()}
        />
      );
      expect(screen.getByText(/\+100\s*XP/i)).toBeInTheDocument();
    });

    it('should display streak freeze reward correctly', () => {
      const freezeAchievement: AchievementUnlock = {
        ...mockAchievement,
        rewardType: 'StreakFreeze',
        rewardValue: 1,
      };
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={freezeAchievement}
          onClose={jest.fn()}
        />
      );
      expect(screen.getByText(/streak freeze/i)).toBeInTheDocument();
    });

    it('should display cosmetic reward correctly', () => {
      const cosmeticAchievement: AchievementUnlock = {
        ...mockAchievement,
        rewardType: 'Cosmetic',
        rewardValue: 1,
      };
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={cosmeticAchievement}
          onClose={jest.fn()}
        />
      );
      expect(screen.getByText(/cosmetic unlocked/i)).toBeInTheDocument();
    });
  });

  describe('accessibility', () => {
    it('should have proper dialog role', () => {
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={mockAchievement}
          onClose={jest.fn()}
        />
      );
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    it('should have aria-modal attribute', () => {
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={mockAchievement}
          onClose={jest.fn()}
        />
      );
      expect(screen.getByRole('dialog')).toHaveAttribute('aria-modal', 'true');
    });

    it('should have aria-labelledby pointing to title', () => {
      render(
        <AchievementUnlockModal
          isOpen={true}
          achievement={mockAchievement}
          onClose={jest.fn()}
        />
      );
      const dialog = screen.getByRole('dialog');
      expect(dialog).toHaveAttribute('aria-labelledby');
    });
  });
});
