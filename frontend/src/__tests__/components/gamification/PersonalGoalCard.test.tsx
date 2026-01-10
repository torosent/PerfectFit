/**
 * PersonalGoalCard Component Tests
 *
 * Tests for personal goal cards with keyboard accessibility.
 */

import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { PersonalGoalCard } from '@/components/gamification/PersonalGoalCard';
import type { PersonalGoal } from '@/types/gamification';

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
      span: React.forwardRef(({ children, ...props }: React.PropsWithChildren<object>, ref: React.Ref<HTMLSpanElement>) => 
        React.createElement('span', { ...props, ref }, children)),
    },
    AnimatePresence: ({ children }: React.PropsWithChildren) => React.createElement(React.Fragment, null, children),
  };
});

describe('PersonalGoalCard', () => {
  const mockActiveGoal: PersonalGoal = {
    id: 1,
    type: 'BeatAverage',
    description: 'Beat your average score',
    targetValue: 5000,
    currentValue: 3500,
    progressPercentage: 70,
    isCompleted: false,
    expiresAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(), // 24 hours from now
  };

  const mockCompletedGoal: PersonalGoal = {
    id: 2,
    type: 'ImproveAccuracy',
    description: 'Improve accuracy by 10%',
    targetValue: 10,
    currentValue: 10,
    progressPercentage: 100,
    isCompleted: true,
    expiresAt: null,
  };

  const mockNewPersonalBestGoal: PersonalGoal = {
    id: 3,
    type: 'NewPersonalBest',
    description: 'Set a new personal best score',
    targetValue: 10000,
    currentValue: 8500,
    progressPercentage: 85,
    isCompleted: false,
    expiresAt: new Date(Date.now() + 2 * 60 * 60 * 1000).toISOString(), // 2 hours from now
  };

  describe('renders goal information', () => {
    it('should display goal description', () => {
      render(<PersonalGoalCard goal={mockActiveGoal} />);
      expect(screen.getByText('Beat your average score')).toBeInTheDocument();
    });

    it('should display progress values', () => {
      render(<PersonalGoalCard goal={mockActiveGoal} />);
      expect(screen.getByText('3500 / 5000')).toBeInTheDocument();
    });

    it('should display appropriate icon for BeatAverage goal', () => {
      render(<PersonalGoalCard goal={mockActiveGoal} />);
      expect(screen.getByText('📊')).toBeInTheDocument();
    });

    it('should display appropriate icon for ImproveAccuracy goal', () => {
      render(<PersonalGoalCard goal={mockCompletedGoal} />);
      expect(screen.getByText('🎯')).toBeInTheDocument();
    });

    it('should display appropriate icon for NewPersonalBest goal', () => {
      render(<PersonalGoalCard goal={mockNewPersonalBestGoal} />);
      expect(screen.getByText('🏆')).toBeInTheDocument();
    });
  });

  describe('shows completion state', () => {
    it('should show completion checkmark for completed goals', () => {
      render(<PersonalGoalCard goal={mockCompletedGoal} />);
      expect(screen.getByText('✓')).toBeInTheDocument();
    });

    it('should show celebration message for completed goals', () => {
      render(<PersonalGoalCard goal={mockCompletedGoal} />);
      expect(screen.getByText(/goal achieved/i)).toBeInTheDocument();
    });

    it('should not show progress bar for completed goals', () => {
      render(<PersonalGoalCard goal={mockCompletedGoal} />);
      expect(screen.queryByText(/\/ 10/)).not.toBeInTheDocument();
    });

    it('should show progress bar for active goals', () => {
      render(<PersonalGoalCard goal={mockActiveGoal} />);
      expect(screen.getByText('3500 / 5000')).toBeInTheDocument();
    });
  });

  describe('shows time remaining', () => {
    it('should display time remaining for goals with expiration', () => {
      render(<PersonalGoalCard goal={mockActiveGoal} />);
      // Should show days or hours remaining
      expect(screen.getByText(/\d+[hd]\s*left/i)).toBeInTheDocument();
    });

    it('should display hours for short expiration times', () => {
      render(<PersonalGoalCard goal={mockNewPersonalBestGoal} />);
      expect(screen.getByText(/\d+h\s*left/i)).toBeInTheDocument();
    });

    it('should not show time for goals without expiration', () => {
      render(<PersonalGoalCard goal={mockCompletedGoal} />);
      expect(screen.queryByText(/left/i)).not.toBeInTheDocument();
    });
  });

  describe('keyboard accessibility', () => {
    it('should have button role when onClick is provided', () => {
      const handleClick = jest.fn();
      render(<PersonalGoalCard goal={mockActiveGoal} onClick={handleClick} />);
      expect(screen.getByRole('button')).toBeInTheDocument();
    });

    it('should not have button role when onClick is not provided', () => {
      render(<PersonalGoalCard goal={mockActiveGoal} />);
      expect(screen.queryByRole('button')).not.toBeInTheDocument();
    });

    it('should be focusable when onClick is provided', () => {
      const handleClick = jest.fn();
      render(<PersonalGoalCard goal={mockActiveGoal} onClick={handleClick} />);
      const button = screen.getByRole('button');
      expect(button).toHaveAttribute('tabIndex', '0');
    });

    it('should trigger onClick when Enter key is pressed', () => {
      const handleClick = jest.fn();
      render(<PersonalGoalCard goal={mockActiveGoal} onClick={handleClick} />);
      const button = screen.getByRole('button');
      
      fireEvent.keyDown(button, { key: 'Enter' });
      
      expect(handleClick).toHaveBeenCalledTimes(1);
    });

    it('should trigger onClick when Space key is pressed', () => {
      const handleClick = jest.fn();
      render(<PersonalGoalCard goal={mockActiveGoal} onClick={handleClick} />);
      const button = screen.getByRole('button');
      
      fireEvent.keyDown(button, { key: ' ' });
      
      expect(handleClick).toHaveBeenCalledTimes(1);
    });

    it('should not trigger onClick for other keys', () => {
      const handleClick = jest.fn();
      render(<PersonalGoalCard goal={mockActiveGoal} onClick={handleClick} />);
      const button = screen.getByRole('button');
      
      fireEvent.keyDown(button, { key: 'Tab' });
      fireEvent.keyDown(button, { key: 'Escape' });
      
      expect(handleClick).not.toHaveBeenCalled();
    });

    it('should have aria-label with goal info when clickable', () => {
      const handleClick = jest.fn();
      render(<PersonalGoalCard goal={mockActiveGoal} onClick={handleClick} />);
      const button = screen.getByRole('button');
      expect(button).toHaveAttribute('aria-label', expect.stringContaining('Beat your average score'));
      expect(button).toHaveAttribute('aria-label', expect.stringContaining('70% complete'));
    });

    it('should have aria-label indicating completed for completed goals', () => {
      const handleClick = jest.fn();
      render(<PersonalGoalCard goal={mockCompletedGoal} onClick={handleClick} />);
      const button = screen.getByRole('button');
      expect(button).toHaveAttribute('aria-label', expect.stringContaining('completed'));
    });
  });

  describe('click functionality', () => {
    it('should call onClick when card is clicked', () => {
      const handleClick = jest.fn();
      render(<PersonalGoalCard goal={mockActiveGoal} onClick={handleClick} />);
      
      fireEvent.click(screen.getByRole('button'));
      
      expect(handleClick).toHaveBeenCalledTimes(1);
    });
  });

  describe('visual styling', () => {
    it('should have completed styling for completed goals', () => {
      render(<PersonalGoalCard goal={mockCompletedGoal} />);
      const card = screen.getByTestId('personal-goal-card');
      expect(card).toHaveClass('bg-green-500/10');
    });

    it('should have default styling for active goals', () => {
      render(<PersonalGoalCard goal={mockActiveGoal} />);
      const card = screen.getByTestId('personal-goal-card');
      expect(card).toHaveClass('bg-white/5');
    });
  });
});
