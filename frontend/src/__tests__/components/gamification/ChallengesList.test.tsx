/**
 * ChallengesList Component Tests
 *
 * Tests for challenges list with tabs and accessibility.
 */

import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { ChallengesList } from '@/components/gamification/ChallengesList';
import type { Challenge } from '@/types/gamification';

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

describe('ChallengesList', () => {
  const mockDailyChallenges: Challenge[] = [
    {
      id: 1,
      name: 'Score 5000',
      description: 'Score 5000 points in a single game',
      type: 'Daily',
      targetValue: 5000,
      currentProgress: 2500,
      xpReward: 50,
      isCompleted: false,
      endsAt: new Date(Date.now() + 12 * 60 * 60 * 1000).toISOString(),
    },
    {
      id: 2,
      name: 'Play 3 games',
      description: 'Complete 3 games today',
      type: 'Daily',
      targetValue: 3,
      currentProgress: 3,
      xpReward: 30,
      isCompleted: true,
      endsAt: new Date(Date.now() + 12 * 60 * 60 * 1000).toISOString(),
    },
  ];

  const mockWeeklyChallenges: Challenge[] = [
    {
      id: 3,
      name: 'Perfect week',
      description: 'Play every day this week',
      type: 'Weekly',
      targetValue: 7,
      currentProgress: 4,
      xpReward: 200,
      isCompleted: false,
      endsAt: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString(),
    },
  ];

  const allChallenges = [...mockDailyChallenges, ...mockWeeklyChallenges];

  describe('renders tab navigation', () => {
    it('should render daily and weekly tabs', () => {
      render(<ChallengesList challenges={allChallenges} />);
      expect(screen.getByRole('tab', { name: /daily/i })).toBeInTheDocument();
      expect(screen.getByRole('tab', { name: /weekly/i })).toBeInTheDocument();
    });

    it('should display completion count in tab labels', () => {
      render(<ChallengesList challenges={allChallenges} />);
      expect(screen.getByRole('tab', { name: /daily.*1\/2/i })).toBeInTheDocument();
      expect(screen.getByRole('tab', { name: /weekly.*0\/1/i })).toBeInTheDocument();
    });
  });

  describe('ARIA tabs pattern', () => {
    it('should have tablist role on tab container', () => {
      render(<ChallengesList challenges={allChallenges} />);
      expect(screen.getByRole('tablist')).toBeInTheDocument();
    });

    it('should have aria-label on tablist', () => {
      render(<ChallengesList challenges={allChallenges} />);
      expect(screen.getByRole('tablist')).toHaveAttribute('aria-label', 'Challenge types');
    });

    it('should have proper aria-selected on active tab', () => {
      render(<ChallengesList challenges={allChallenges} defaultTab="Daily" />);
      const dailyTab = screen.getByRole('tab', { name: /daily/i });
      const weeklyTab = screen.getByRole('tab', { name: /weekly/i });
      
      expect(dailyTab).toHaveAttribute('aria-selected', 'true');
      expect(weeklyTab).toHaveAttribute('aria-selected', 'false');
    });

    it('should have tab IDs', () => {
      render(<ChallengesList challenges={allChallenges} />);
      const dailyTab = screen.getByRole('tab', { name: /daily/i });
      const weeklyTab = screen.getByRole('tab', { name: /weekly/i });
      
      expect(dailyTab).toHaveAttribute('id');
      expect(weeklyTab).toHaveAttribute('id');
    });

    it('should have aria-controls on tabs', () => {
      render(<ChallengesList challenges={allChallenges} />);
      const dailyTab = screen.getByRole('tab', { name: /daily/i });
      
      expect(dailyTab).toHaveAttribute('aria-controls');
    });

    it('should have tabpanel role on content area', () => {
      render(<ChallengesList challenges={allChallenges} />);
      expect(screen.getByRole('tabpanel')).toBeInTheDocument();
    });

    it('should have aria-labelledby on tabpanel referencing active tab', () => {
      render(<ChallengesList challenges={allChallenges} defaultTab="Daily" />);
      const dailyTab = screen.getByRole('tab', { name: /daily/i });
      const tabpanel = screen.getByRole('tabpanel');
      
      const tabId = dailyTab.getAttribute('id');
      expect(tabpanel).toHaveAttribute('aria-labelledby', tabId);
    });
  });

  describe('tab switching', () => {
    it('should switch to weekly tab when clicked', () => {
      render(<ChallengesList challenges={allChallenges} defaultTab="Daily" />);
      const weeklyTab = screen.getByRole('tab', { name: /weekly/i });
      
      fireEvent.click(weeklyTab);
      
      expect(weeklyTab).toHaveAttribute('aria-selected', 'true');
      expect(screen.getByText('Perfect week')).toBeInTheDocument();
    });

    it('should switch back to daily tab when clicked', () => {
      render(<ChallengesList challenges={allChallenges} defaultTab="Weekly" />);
      const dailyTab = screen.getByRole('tab', { name: /daily/i });
      
      fireEvent.click(dailyTab);
      
      expect(dailyTab).toHaveAttribute('aria-selected', 'true');
      expect(screen.getByText('Score 5000')).toBeInTheDocument();
    });
  });

  describe('challenge list display', () => {
    it('should display daily challenges when daily tab is active', () => {
      render(<ChallengesList challenges={allChallenges} defaultTab="Daily" />);
      expect(screen.getByText('Score 5000')).toBeInTheDocument();
      expect(screen.getByText('Play 3 games')).toBeInTheDocument();
      expect(screen.queryByText('Perfect week')).not.toBeInTheDocument();
    });

    it('should display weekly challenges when weekly tab is active', () => {
      render(<ChallengesList challenges={allChallenges} defaultTab="Weekly" />);
      expect(screen.getByText('Perfect week')).toBeInTheDocument();
      expect(screen.queryByText('Score 5000')).not.toBeInTheDocument();
    });
  });

  describe('empty state', () => {
    it('should show empty message when no challenges', () => {
      render(<ChallengesList challenges={[]} />);
      expect(screen.getByText(/no daily challenges available/i)).toBeInTheDocument();
    });
  });

  describe('loading state', () => {
    it('should show loading skeleton when isLoading', () => {
      const { container } = render(<ChallengesList challenges={[]} isLoading={true} />);
      expect(container.querySelector('.animate-pulse')).toBeInTheDocument();
    });
  });
});
