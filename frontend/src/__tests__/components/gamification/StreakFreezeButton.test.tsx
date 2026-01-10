/**
 * StreakFreezeButton Component Tests
 *
 * Tests for streak freeze button with modal accessibility.
 */

import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { StreakFreezeButton } from '@/components/gamification/StreakFreezeButton';

// Mock motion/react to avoid animation issues in tests
jest.mock('motion/react', () => {
  const React = require('react');
  return {
    motion: {
      div: React.forwardRef(({ children, onClick, onKeyDown, role, ...props }: React.PropsWithChildren<{
        onClick?: (e?: React.MouseEvent) => void;
        onKeyDown?: (e: React.KeyboardEvent) => void;
        role?: string;
      }>, ref: React.Ref<HTMLDivElement>) => 
        React.createElement('div', { onClick, onKeyDown, role, ...props, ref }, children)),
      button: React.forwardRef(({ children, onClick, disabled, ...props }: React.PropsWithChildren<{
        onClick?: () => void;
        disabled?: boolean;
      }>, ref: React.Ref<HTMLButtonElement>) => 
        React.createElement('button', { onClick, disabled, ...props, ref }, children)),
    },
    AnimatePresence: ({ children }: React.PropsWithChildren) => React.createElement(React.Fragment, null, children),
  };
});

// Mock useStreaks hook
const mockUseFreeze = jest.fn();
jest.mock('@/hooks/useStreaks', () => ({
  useStreaks: () => ({
    freezeTokens: 2,
    isAtRisk: true,
    useFreeze: mockUseFreeze,
    isLoading: false,
  }),
}));

describe('StreakFreezeButton', () => {
  beforeEach(() => {
    mockUseFreeze.mockReset();
    mockUseFreeze.mockResolvedValue(undefined);
  });

  describe('renders button', () => {
    it('should render freeze button', () => {
      render(<StreakFreezeButton />);
      expect(screen.getByRole('button')).toBeInTheDocument();
    });

    it('should display freeze token count', () => {
      render(<StreakFreezeButton />);
      expect(screen.getByText('2 remaining')).toBeInTheDocument();
    });

    it('should show freeze emoji', () => {
      render(<StreakFreezeButton />);
      expect(screen.getByText('❄️')).toBeInTheDocument();
    });
  });

  describe('confirmation modal', () => {
    it('should open modal when button is clicked', () => {
      render(<StreakFreezeButton />);
      
      fireEvent.click(screen.getByRole('button'));
      
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    it('should have aria-modal="true" on dialog', () => {
      render(<StreakFreezeButton />);
      
      fireEvent.click(screen.getByRole('button'));
      
      expect(screen.getByRole('dialog')).toHaveAttribute('aria-modal', 'true');
    });

    it('should have aria-labelledby pointing to title', () => {
      render(<StreakFreezeButton />);
      
      fireEvent.click(screen.getByRole('button'));
      
      const dialog = screen.getByRole('dialog');
      expect(dialog).toHaveAttribute('aria-labelledby');
      
      const labelledById = dialog.getAttribute('aria-labelledby');
      const title = document.getElementById(labelledById!);
      expect(title).toHaveTextContent('Use Streak Freeze?');
    });

    it('should display confirmation message', () => {
      render(<StreakFreezeButton />);
      
      fireEvent.click(screen.getByRole('button'));
      
      expect(screen.getByText(/protect your streak/i)).toBeInTheDocument();
    });

    it('should show freeze tokens remaining', () => {
      render(<StreakFreezeButton />);
      
      fireEvent.click(screen.getByRole('button'));
      
      expect(screen.getByText(/2 freeze tokens remaining/i)).toBeInTheDocument();
    });

    it('should have Cancel and Use Freeze buttons', () => {
      render(<StreakFreezeButton />);
      
      fireEvent.click(screen.getByRole('button'));
      
      expect(screen.getByRole('button', { name: /^cancel$/i })).toBeInTheDocument();
      // Modal has "Use Freeze" button, the main button says "Use Freeze Token"
      expect(screen.getByRole('button', { name: /^use freeze$/i })).toBeInTheDocument();
    });
  });

  describe('modal interactions', () => {
    it('should close modal when Cancel is clicked', () => {
      render(<StreakFreezeButton />);
      
      fireEvent.click(screen.getByRole('button'));
      fireEvent.click(screen.getByRole('button', { name: /cancel/i }));
      
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });

    it('should close modal when clicking backdrop', () => {
      render(<StreakFreezeButton />);
      
      fireEvent.click(screen.getByRole('button'));
      
      // Click the backdrop (dialog element itself)
      const dialog = screen.getByRole('dialog');
      fireEvent.click(dialog);
      
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });

    it('should close modal when Escape key is pressed', () => {
      render(<StreakFreezeButton />);
      
      fireEvent.click(screen.getByRole('button'));
      
      const dialog = screen.getByRole('dialog');
      fireEvent.keyDown(dialog, { key: 'Escape' });
      
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });

    it('should call useFreeze when Use Freeze is clicked', async () => {
      render(<StreakFreezeButton />);
      
      // Click main button to open modal
      fireEvent.click(screen.getByRole('button'));
      // Click the modal's "Use Freeze" button (exact match to avoid matching "Use Freeze Token")
      fireEvent.click(screen.getByRole('button', { name: /^use freeze$/i }));
      
      await waitFor(() => {
        expect(mockUseFreeze).toHaveBeenCalled();
      });
    });
  });

  describe('compact mode', () => {
    it('should render in compact mode', () => {
      render(<StreakFreezeButton compact />);
      expect(screen.getByRole('button')).toBeInTheDocument();
    });

    it('should show token count in compact mode', () => {
      render(<StreakFreezeButton compact />);
      expect(screen.getByText('2')).toBeInTheDocument();
    });
  });
});
