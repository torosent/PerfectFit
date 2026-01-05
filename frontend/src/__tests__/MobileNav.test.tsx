import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MobileNav } from '@/components/ui/MobileNav';

// Mock next/navigation
const mockPathname = '/';
jest.mock('next/navigation', () => ({
  usePathname: () => mockPathname,
}));

// Mock next/link
jest.mock('next/link', () => {
  return function MockLink({ children, href, onClick, ...props }: { children: React.ReactNode; href: string; onClick?: () => void; [key: string]: unknown }) {
    return (
      <a href={href} onClick={onClick} {...props}>
        {children}
      </a>
    );
  };
});

describe('MobileNav', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Hamburger Button', () => {
    it('renders hamburger button', () => {
      render(<MobileNav />);
      
      const button = screen.getByRole('button', { name: /open menu/i });
      expect(button).toBeInTheDocument();
    });

    it('has correct aria-label when menu is closed', () => {
      render(<MobileNav />);
      
      const button = screen.getByRole('button', { name: /open menu/i });
      expect(button).toHaveAttribute('aria-label', 'Open menu');
      expect(button).toHaveAttribute('aria-expanded', 'false');
    });

    it('has correct aria-label when menu is open', () => {
      render(<MobileNav />);
      
      const button = screen.getByRole('button', { name: /open menu/i });
      fireEvent.click(button);
      
      expect(button).toHaveAttribute('aria-label', 'Close menu');
      expect(button).toHaveAttribute('aria-expanded', 'true');
    });
  });

  describe('Menu Open/Close', () => {
    it('opens menu when hamburger button is clicked', () => {
      render(<MobileNav />);
      
      const button = screen.getByRole('button', { name: /open menu/i });
      
      // Menu should not be visible initially
      expect(screen.queryByRole('navigation')).not.toBeInTheDocument();
      
      // Click to open
      fireEvent.click(button);
      
      // Menu should now be visible
      expect(screen.getByRole('navigation')).toBeInTheDocument();
    });

    it('closes menu when hamburger button is clicked again', () => {
      render(<MobileNav />);
      
      const button = screen.getByRole('button', { name: /open menu/i });
      
      // Open menu
      fireEvent.click(button);
      expect(screen.getByRole('navigation')).toBeInTheDocument();
      
      // Close menu
      fireEvent.click(button);
      expect(screen.queryByRole('navigation')).not.toBeInTheDocument();
    });

    it('displays all navigation links when menu is open', () => {
      render(<MobileNav />);
      
      const button = screen.getByRole('button', { name: /open menu/i });
      fireEvent.click(button);
      
      expect(screen.getByRole('link', { name: /play/i })).toBeInTheDocument();
      expect(screen.getByRole('link', { name: /leaderboard/i })).toBeInTheDocument();
    });

    it('closes menu when a navigation link is clicked', () => {
      render(<MobileNav />);
      
      const button = screen.getByRole('button', { name: /open menu/i });
      fireEvent.click(button);
      
      // Menu is open
      expect(screen.getByRole('navigation')).toBeInTheDocument();
      
      // Click a link
      const playLink = screen.getByRole('link', { name: /play/i });
      fireEvent.click(playLink);
      
      // Menu should close
      expect(screen.queryByRole('navigation')).not.toBeInTheDocument();
    });
  });

  describe('Click Outside', () => {
    it('closes menu when clicking outside', async () => {
      const { container } = render(
        <div>
          <MobileNav />
          <div data-testid="outside">Outside element</div>
        </div>
      );
      
      const button = screen.getByRole('button', { name: /open menu/i });
      fireEvent.click(button);
      
      // Menu is open
      expect(screen.getByRole('navigation')).toBeInTheDocument();
      
      // Click outside
      const outsideElement = screen.getByTestId('outside');
      fireEvent.mouseDown(outsideElement);
      
      // Menu should close
      await waitFor(() => {
        expect(screen.queryByRole('navigation')).not.toBeInTheDocument();
      });
    });

    it('does not close menu when clicking inside', () => {
      render(<MobileNav />);
      
      const button = screen.getByRole('button', { name: /open menu/i });
      fireEvent.click(button);
      
      // Menu is open
      const nav = screen.getByRole('navigation');
      expect(nav).toBeInTheDocument();
      
      // Click inside menu
      fireEvent.mouseDown(nav);
      
      // Menu should still be open
      expect(screen.getByRole('navigation')).toBeInTheDocument();
    });
  });

  describe('Accessibility', () => {
    it('has proper aria-expanded attribute', () => {
      render(<MobileNav />);
      
      const button = screen.getByRole('button', { name: /open menu/i });
      
      expect(button).toHaveAttribute('aria-expanded', 'false');
      
      fireEvent.click(button);
      expect(button).toHaveAttribute('aria-expanded', 'true');
      
      fireEvent.click(button);
      expect(button).toHaveAttribute('aria-expanded', 'false');
    });

    it('has correct href attributes on navigation links', () => {
      render(<MobileNav />);
      
      const button = screen.getByRole('button', { name: /open menu/i });
      fireEvent.click(button);
      
      expect(screen.getByRole('link', { name: /play/i })).toHaveAttribute('href', '/play');
      expect(screen.getByRole('link', { name: /leaderboard/i })).toHaveAttribute('href', '/leaderboard');
    });
  });

  describe('CSS Classes', () => {
    it('has sm:hidden class to only show on mobile', () => {
      const { container } = render(<MobileNav />);
      
      const mobileNavWrapper = container.firstChild;
      expect(mobileNavWrapper).toHaveClass('sm:hidden');
    });

    it('hamburger button has touch-feedback class', () => {
      render(<MobileNav />);
      
      const button = screen.getByRole('button', { name: /open menu/i });
      expect(button).toHaveClass('touch-feedback');
    });
  });
});
