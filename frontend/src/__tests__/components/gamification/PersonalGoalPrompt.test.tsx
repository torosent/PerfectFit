/**
 * PersonalGoalPrompt Component Tests
 *
 * Tests for the personal goal notification prompt.
 */

import React from 'react';
import { render, screen, fireEvent, act } from '@testing-library/react';
import { PersonalGoalPrompt } from '@/components/gamification/PersonalGoalPrompt';
import type { PersonalGoal } from '@/types/gamification';

// Mock motion/react to avoid animation issues in tests
jest.mock('motion/react', () => {
  const React = require('react');
  return {
    motion: {
      div: React.forwardRef(({ children, role, className, ...props }: React.PropsWithChildren<{ role?: string; className?: string }>, ref: React.Ref<HTMLDivElement>) => 
        React.createElement('div', { role, className, ...props, ref }, children)),
    },
    AnimatePresence: ({ children }: React.PropsWithChildren) => React.createElement(React.Fragment, null, children),
  };
});

describe('PersonalGoalPrompt', () => {
  const mockGoal: PersonalGoal = {
    id: 1,
    type: 'BeatAverage',
    targetValue: 100,
    currentValue: 50,
    description: 'Beat your average score of 100',
    progressPercentage: 50,
    isCompleted: false,
    createdAt: new Date().toISOString(),
  };

  const mockOnDismiss = jest.fn();

  beforeEach(() => {
    mockOnDismiss.mockClear();
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  describe('renders goal information', () => {
    it('should render when isVisible is true', () => {
      render(
        <PersonalGoalPrompt
          goal={mockGoal}
          isVisible={true}
          onDismiss={mockOnDismiss}
        />
      );
      expect(screen.getByText("Today's Goal")).toBeInTheDocument();
    });

    it('should not render when isVisible is false', () => {
      render(
        <PersonalGoalPrompt
          goal={mockGoal}
          isVisible={false}
          onDismiss={mockOnDismiss}
        />
      );
      expect(screen.queryByText("Today's Goal")).not.toBeInTheDocument();
    });

    it('should not render when goal is null', () => {
      render(
        <PersonalGoalPrompt
          goal={null}
          isVisible={true}
          onDismiss={mockOnDismiss}
        />
      );
      expect(screen.queryByText("Today's Goal")).not.toBeInTheDocument();
    });

    it('should display goal description', () => {
      render(
        <PersonalGoalPrompt
          goal={mockGoal}
          isVisible={true}
          onDismiss={mockOnDismiss}
        />
      );
      expect(screen.getByText('Beat your average score of 100')).toBeInTheDocument();
    });

    it('should display progress percentage', () => {
      render(
        <PersonalGoalPrompt
          goal={mockGoal}
          isVisible={true}
          onDismiss={mockOnDismiss}
        />
      );
      expect(screen.getByText('50%')).toBeInTheDocument();
    });
  });

  describe('dismiss functionality', () => {
    it('should call onDismiss when dismiss button is clicked', () => {
      render(
        <PersonalGoalPrompt
          goal={mockGoal}
          isVisible={true}
          onDismiss={mockOnDismiss}
        />
      );
      
      const dismissButton = screen.getByRole('button', { name: /dismiss/i });
      fireEvent.click(dismissButton);
      
      expect(mockOnDismiss).toHaveBeenCalled();
    });

    it('should auto-dismiss after timeout', () => {
      render(
        <PersonalGoalPrompt
          goal={mockGoal}
          isVisible={true}
          onDismiss={mockOnDismiss}
          autoDismissDelay={5000}
        />
      );
      
      expect(mockOnDismiss).not.toHaveBeenCalled();
      
      act(() => {
        jest.advanceTimersByTime(5000);
      });
      
      expect(mockOnDismiss).toHaveBeenCalled();
    });

    it('should not auto-dismiss when autoDismissDelay is 0', () => {
      render(
        <PersonalGoalPrompt
          goal={mockGoal}
          isVisible={true}
          onDismiss={mockOnDismiss}
          autoDismissDelay={0}
        />
      );
      
      act(() => {
        jest.advanceTimersByTime(10000);
      });
      
      expect(mockOnDismiss).not.toHaveBeenCalled();
    });
  });

  describe('keyboard accessibility', () => {
    it('should close when Escape key is pressed on document', () => {
      render(
        <PersonalGoalPrompt
          goal={mockGoal}
          isVisible={true}
          onDismiss={mockOnDismiss}
        />
      );
      
      // Simulate Escape key on document (global listener)
      fireEvent.keyDown(document, { key: 'Escape' });
      
      expect(mockOnDismiss).toHaveBeenCalledTimes(1);
    });

    it('should not close on other key presses on document', () => {
      render(
        <PersonalGoalPrompt
          goal={mockGoal}
          isVisible={true}
          onDismiss={mockOnDismiss}
        />
      );
      
      fireEvent.keyDown(document, { key: 'Enter' });
      fireEvent.keyDown(document, { key: 'Tab' });
      fireEvent.keyDown(document, { key: 'Space' });
      
      expect(mockOnDismiss).not.toHaveBeenCalled();
    });

    it('should not respond to Escape when not visible', () => {
      render(
        <PersonalGoalPrompt
          goal={mockGoal}
          isVisible={false}
          onDismiss={mockOnDismiss}
        />
      );
      
      fireEvent.keyDown(document, { key: 'Escape' });
      
      expect(mockOnDismiss).not.toHaveBeenCalled();
    });

    it('should have dialog role for accessibility', () => {
      render(
        <PersonalGoalPrompt
          goal={mockGoal}
          isVisible={true}
          onDismiss={mockOnDismiss}
        />
      );
      
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    it('should have aria-label for screen readers', () => {
      render(
        <PersonalGoalPrompt
          goal={mockGoal}
          isVisible={true}
          onDismiss={mockOnDismiss}
        />
      );
      
      const dialog = screen.getByRole('dialog');
      expect(dialog).toHaveAttribute('aria-label', 'Personal goal notification');
    });

    it('should cleanup event listener on unmount', () => {
      const { unmount } = render(
        <PersonalGoalPrompt
          goal={mockGoal}
          isVisible={true}
          onDismiss={mockOnDismiss}
        />
      );
      
      unmount();
      
      // Should not call onDismiss after unmount
      fireEvent.keyDown(document, { key: 'Escape' });
      expect(mockOnDismiss).not.toHaveBeenCalled();
    });
  });

  describe('different goal types', () => {
    it('should render BeatAverage goal', () => {
      render(
        <PersonalGoalPrompt
          goal={mockGoal}
          isVisible={true}
          onDismiss={mockOnDismiss}
        />
      );
      expect(screen.getByText("Today's Goal")).toBeInTheDocument();
    });

    it('should render ImproveAccuracy goal', () => {
      const accuracyGoal: PersonalGoal = {
        ...mockGoal,
        type: 'ImproveAccuracy',
        description: 'Achieve 95% accuracy',
      };
      
      render(
        <PersonalGoalPrompt
          goal={accuracyGoal}
          isVisible={true}
          onDismiss={mockOnDismiss}
        />
      );
      expect(screen.getByText('Achieve 95% accuracy')).toBeInTheDocument();
    });

    it('should render NewPersonalBest goal', () => {
      const personalBestGoal: PersonalGoal = {
        ...mockGoal,
        type: 'NewPersonalBest',
        description: 'Set a new personal best!',
      };
      
      render(
        <PersonalGoalPrompt
          goal={personalBestGoal}
          isVisible={true}
          onDismiss={mockOnDismiss}
        />
      );
      expect(screen.getByText('Set a new personal best!')).toBeInTheDocument();
    });
  });
});
