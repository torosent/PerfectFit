/**
 * AchievementBadge Component Tests
 *
 * Tests for individual achievement badge display with keyboard accessibility.
 */

import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { AchievementBadge } from '@/components/gamification/AchievementBadge';
import type { Achievement } from '@/types/gamification';

// Mock motion/react to avoid animation issues in tests
jest.mock('motion/react', () => {
  const React = require('react');
  return {
    motion: {
      div: React.forwardRef(({ children, onClick, onKeyDown, role, tabIndex, ...props }: React.PropsWithChildren<{
        onClick?: () => void;
        onKeyDown?: (e: React.KeyboardEvent) => void;
        role?: string;
        tabIndex?: number;
      }>, ref: React.Ref<HTMLDivElement>) => 
        React.createElement('div', { onClick, onKeyDown, role, tabIndex, ...props, ref }, children)),
      circle: ({ children, ...props }: React.PropsWithChildren<object>) => 
        React.createElement('circle', props, children),
    },
    AnimatePresence: ({ children }: React.PropsWithChildren) => React.createElement(React.Fragment, null, children),
  };
});

describe('AchievementBadge', () => {
  const mockUnlockedAchievement: Achievement = {
    id: 1,
    name: 'First Victory',
    description: 'Win your first game',
    category: 'Games',
    iconUrl: '/icons/first-victory.png',
    isUnlocked: true,
    unlockedAt: new Date().toISOString(),
    progress: 100,
    isSecret: false,
  };

  const mockLockedAchievement: Achievement = {
    id: 2,
    name: 'Score Master',
    description: 'Score 100,000 points total',
    category: 'Score',
    iconUrl: '/icons/score-master.png',
    isUnlocked: false,
    unlockedAt: null,
    progress: 45,
    isSecret: false,
  };

  const mockSecretAchievement: Achievement = {
    id: 3,
    name: 'Hidden Gem',
    description: 'A secret achievement',
    category: 'Special',
    iconUrl: '/icons/secret.png',
    isUnlocked: false,
    unlockedAt: null,
    progress: 0,
    isSecret: true,
  };

  describe('renders achievement badge', () => {
    it('should display unlocked achievement icon', () => {
      render(<AchievementBadge achievement={mockUnlockedAchievement} />);
      const icon = screen.getByRole('img');
      expect(icon).toHaveAttribute('src', '/icons/first-victory.png');
      expect(icon).toHaveAttribute('alt', 'First Victory');
    });

    it('should display locked achievement with grayscale', () => {
      render(<AchievementBadge achievement={mockLockedAchievement} />);
      const icon = screen.getByRole('img');
      expect(icon).toHaveClass('opacity-50');
    });

    it('should show secret achievement as hidden', () => {
      render(<AchievementBadge achievement={mockSecretAchievement} />);
      // Secret achievements show ❓ emoji instead of icon
      expect(screen.getByText('❓')).toBeInTheDocument();
    });
  });

  describe('tooltip functionality', () => {
    it('should render tooltip with achievement name', () => {
      render(<AchievementBadge achievement={mockUnlockedAchievement} showTooltip={true} />);
      expect(screen.getByText('First Victory')).toBeInTheDocument();
    });

    it('should render tooltip with description', () => {
      render(<AchievementBadge achievement={mockUnlockedAchievement} showTooltip={true} />);
      expect(screen.getByText('Win your first game')).toBeInTheDocument();
    });

    it('should show progress for locked achievements', () => {
      render(<AchievementBadge achievement={mockLockedAchievement} showTooltip={true} />);
      expect(screen.getByText('45% complete')).toBeInTheDocument();
    });

    it('should hide tooltip when showTooltip is false', () => {
      render(<AchievementBadge achievement={mockUnlockedAchievement} showTooltip={false} />);
      // The tooltip container should not be present
      expect(screen.queryByText('Win your first game')).not.toBeInTheDocument();
    });
  });

  describe('keyboard accessibility', () => {
    it('should have button role when onClick is provided', () => {
      const handleClick = jest.fn();
      render(<AchievementBadge achievement={mockUnlockedAchievement} onClick={handleClick} />);
      expect(screen.getByRole('button')).toBeInTheDocument();
    });

    it('should not have button role when onClick is not provided', () => {
      render(<AchievementBadge achievement={mockUnlockedAchievement} />);
      expect(screen.queryByRole('button')).not.toBeInTheDocument();
    });

    it('should be focusable when onClick is provided', () => {
      const handleClick = jest.fn();
      render(<AchievementBadge achievement={mockUnlockedAchievement} onClick={handleClick} />);
      const button = screen.getByRole('button');
      expect(button).toHaveAttribute('tabIndex', '0');
    });

    it('should trigger onClick when Enter key is pressed', () => {
      const handleClick = jest.fn();
      render(<AchievementBadge achievement={mockUnlockedAchievement} onClick={handleClick} />);
      const button = screen.getByRole('button');
      
      fireEvent.keyDown(button, { key: 'Enter' });
      
      expect(handleClick).toHaveBeenCalledTimes(1);
    });

    it('should trigger onClick when Space key is pressed', () => {
      const handleClick = jest.fn();
      render(<AchievementBadge achievement={mockUnlockedAchievement} onClick={handleClick} />);
      const button = screen.getByRole('button');
      
      fireEvent.keyDown(button, { key: ' ' });
      
      expect(handleClick).toHaveBeenCalledTimes(1);
    });

    it('should not trigger onClick for other keys', () => {
      const handleClick = jest.fn();
      render(<AchievementBadge achievement={mockUnlockedAchievement} onClick={handleClick} />);
      const button = screen.getByRole('button');
      
      fireEvent.keyDown(button, { key: 'Tab' });
      fireEvent.keyDown(button, { key: 'Escape' });
      fireEvent.keyDown(button, { key: 'a' });
      
      expect(handleClick).not.toHaveBeenCalled();
    });

    it('should have aria-label with achievement info when clickable', () => {
      const handleClick = jest.fn();
      render(<AchievementBadge achievement={mockUnlockedAchievement} onClick={handleClick} />);
      const button = screen.getByRole('button');
      expect(button).toHaveAttribute('aria-label', 'First Victory: Win your first game');
    });

    it('should include progress in aria-label for locked achievements', () => {
      const handleClick = jest.fn();
      render(<AchievementBadge achievement={mockLockedAchievement} onClick={handleClick} />);
      const button = screen.getByRole('button');
      expect(button).toHaveAttribute('aria-label', expect.stringContaining('45% complete'));
    });
  });

  describe('click functionality', () => {
    it('should call onClick when badge is clicked', () => {
      const handleClick = jest.fn();
      render(<AchievementBadge achievement={mockUnlockedAchievement} onClick={handleClick} />);
      
      fireEvent.click(screen.getByRole('button'));
      
      expect(handleClick).toHaveBeenCalledTimes(1);
    });
  });

  describe('size variants', () => {
    it('should render with small size', () => {
      const { container } = render(
        <AchievementBadge achievement={mockUnlockedAchievement} size="sm" />
      );
      expect(container.querySelector('.w-12')).toBeInTheDocument();
    });

    it('should render with medium size (default)', () => {
      const { container } = render(
        <AchievementBadge achievement={mockUnlockedAchievement} />
      );
      expect(container.querySelector('.w-16')).toBeInTheDocument();
    });

    it('should render with large size', () => {
      const { container } = render(
        <AchievementBadge achievement={mockUnlockedAchievement} size="lg" />
      );
      expect(container.querySelector('.w-20')).toBeInTheDocument();
    });
  });
});
