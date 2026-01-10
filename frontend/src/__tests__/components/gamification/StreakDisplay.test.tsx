/**
 * StreakDisplay Component Tests
 *
 * Tests for the animated streak counter component.
 */

import React from 'react';
import { render, screen } from '@testing-library/react';
import { StreakDisplay } from '@/components/gamification/StreakDisplay';

// Mock motion/react to avoid animation issues in tests
jest.mock('motion/react', () => {
  const React = require('react');
  return {
    motion: {
      div: React.forwardRef(({ children, ...props }: React.PropsWithChildren<object>, ref: React.Ref<HTMLDivElement>) => 
        React.createElement('div', { ...props, ref }, children)),
      span: React.forwardRef(({ children, ...props }: React.PropsWithChildren<object>, ref: React.Ref<HTMLSpanElement>) => 
        React.createElement('span', { ...props, ref }, children)),
      button: React.forwardRef(({ children, ...props }: React.PropsWithChildren<object>, ref: React.Ref<HTMLButtonElement>) => 
        React.createElement('button', { ...props, ref }, children)),
      p: React.forwardRef(({ children, ...props }: React.PropsWithChildren<object>, ref: React.Ref<HTMLParagraphElement>) => 
        React.createElement('p', { ...props, ref }, children)),
    },
    AnimatePresence: ({ children }: React.PropsWithChildren) => React.createElement(React.Fragment, null, children),
  };
});

describe('StreakDisplay', () => {
  describe('renders current streak', () => {
    it('should display the current streak number', () => {
      render(<StreakDisplay currentStreak={5} longestStreak={10} freezeTokens={2} isAtRisk={false} />);
      expect(screen.getByText('5')).toBeInTheDocument();
    });

    it('should display zero streak', () => {
      render(<StreakDisplay currentStreak={0} longestStreak={10} freezeTokens={2} isAtRisk={false} />);
      expect(screen.getByText('0')).toBeInTheDocument();
    });

    it('should display large streak numbers', () => {
      render(<StreakDisplay currentStreak={365} longestStreak={365} freezeTokens={0} isAtRisk={false} />);
      // Use getAllByText since 365 appears for both current and longest streak
      const elements = screen.getAllByText('365');
      expect(elements.length).toBeGreaterThanOrEqual(1);
    });
  });

  describe('shows fire emoji for active streak', () => {
    it('should show fire emoji when streak is active', () => {
      render(<StreakDisplay currentStreak={5} longestStreak={10} freezeTokens={2} isAtRisk={false} />);
      expect(screen.getByText('🔥')).toBeInTheDocument();
    });

    it('should show fire emoji even when streak is 1', () => {
      render(<StreakDisplay currentStreak={1} longestStreak={10} freezeTokens={2} isAtRisk={false} />);
      expect(screen.getByText('🔥')).toBeInTheDocument();
    });
  });

  describe('shows warning for at-risk streak', () => {
    it('should indicate when streak is at risk', () => {
      render(<StreakDisplay currentStreak={5} longestStreak={10} freezeTokens={2} isAtRisk={true} />);
      expect(screen.getByTestId('streak-at-risk')).toBeInTheDocument();
    });

    it('should show warning text when at risk', () => {
      render(<StreakDisplay currentStreak={5} longestStreak={10} freezeTokens={2} isAtRisk={true} />);
      expect(screen.getByText(/at risk/i)).toBeInTheDocument();
    });

    it('should not show warning when not at risk', () => {
      render(<StreakDisplay currentStreak={5} longestStreak={10} freezeTokens={2} isAtRisk={false} />);
      expect(screen.queryByTestId('streak-at-risk')).not.toBeInTheDocument();
    });
  });

  describe('displays freeze tokens', () => {
    it('should display freeze token count with snowflake emoji', () => {
      render(<StreakDisplay currentStreak={5} longestStreak={10} freezeTokens={3} isAtRisk={false} />);
      expect(screen.getByText('❄️')).toBeInTheDocument();
      expect(screen.getByText('3')).toBeInTheDocument();
    });

    it('should show zero freeze tokens', () => {
      render(<StreakDisplay currentStreak={5} longestStreak={10} freezeTokens={0} isAtRisk={false} />);
      expect(screen.getByText('❄️')).toBeInTheDocument();
      // Look for the 0 near freeze tokens
      expect(screen.getByTestId('freeze-token-count')).toHaveTextContent('0');
    });
  });

  describe('compact mode', () => {
    it('should render in compact mode when specified', () => {
      render(
        <StreakDisplay
          currentStreak={5}
          longestStreak={10}
          freezeTokens={2}
          isAtRisk={false}
          compact
        />
      );
      expect(screen.getByTestId('streak-display-compact')).toBeInTheDocument();
    });

    it('should render in full mode by default', () => {
      render(<StreakDisplay currentStreak={5} longestStreak={10} freezeTokens={2} isAtRisk={false} />);
      expect(screen.getByTestId('streak-display-full')).toBeInTheDocument();
    });
  });

  describe('longest streak display', () => {
    it('should display longest streak in full mode', () => {
      render(<StreakDisplay currentStreak={5} longestStreak={42} freezeTokens={2} isAtRisk={false} />);
      expect(screen.getByText(/best/i)).toBeInTheDocument();
      expect(screen.getByText('42')).toBeInTheDocument();
    });
  });

  describe('accessibility', () => {
    it('should have proper aria-label for streak', () => {
      render(<StreakDisplay currentStreak={5} longestStreak={10} freezeTokens={2} isAtRisk={false} />);
      expect(screen.getByLabelText(/current streak.*5/i)).toBeInTheDocument();
    });

    it('should indicate at-risk status for screen readers', () => {
      render(<StreakDisplay currentStreak={5} longestStreak={10} freezeTokens={2} isAtRisk={true} />);
      expect(screen.getByRole('alert')).toBeInTheDocument();
    });
  });
});
