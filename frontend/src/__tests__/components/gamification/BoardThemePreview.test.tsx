/**
 * BoardThemePreview Component Tests
 *
 * Tests for board theme visualization.
 */

import React from 'react';
import { render, screen } from '@testing-library/react';
import { BoardThemePreview } from '@/components/gamification/BoardThemePreview';
import type { CosmeticInfo } from '@/types/gamification';

// Mock motion/react to avoid animation issues in tests
jest.mock('motion/react', () => {
  const React = require('react');
  return {
    motion: {
      div: React.forwardRef(({ children, ...props }: React.PropsWithChildren<object>, ref: React.Ref<HTMLDivElement>) => 
        React.createElement('div', { ...props, ref }, children)),
    },
    AnimatePresence: ({ children }: React.PropsWithChildren) => React.createElement(React.Fragment, null, children),
  };
});

describe('BoardThemePreview', () => {
  const mockTheme: CosmeticInfo = {
    id: 1,
    name: 'Ocean Blue',
    assetUrl: '/themes/ocean-blue.json',
  };

  describe('renders board preview', () => {
    it('should render without theme', () => {
      const { container } = render(<BoardThemePreview theme={null} />);
      expect(container.firstChild).toBeInTheDocument();
    });

    it('should render with theme data', () => {
      const { container } = render(<BoardThemePreview theme={mockTheme} />);
      expect(container.firstChild).toBeInTheDocument();
    });

    it('should render a 4x4 grid of cells', () => {
      const { container } = render(<BoardThemePreview theme={mockTheme} />);
      // 4x4 grid = 16 cells
      const cells = container.querySelectorAll('.rounded-sm');
      expect(cells.length).toBe(16);
    });
  });

  describe('size variants', () => {
    it('should render with small size', () => {
      const { container } = render(<BoardThemePreview theme={mockTheme} size="sm" />);
      expect(container.querySelector('.w-16')).toBeInTheDocument();
    });

    it('should render with medium size (default)', () => {
      const { container } = render(<BoardThemePreview theme={mockTheme} />);
      expect(container.querySelector('.w-24')).toBeInTheDocument();
    });

    it('should render with large size', () => {
      const { container } = render(<BoardThemePreview theme={mockTheme} size="lg" />);
      expect(container.querySelector('.w-32')).toBeInTheDocument();
    });
  });

  describe('theme name overlay', () => {
    it('should display theme name when theme is provided', () => {
      render(<BoardThemePreview theme={mockTheme} />);
      expect(screen.getByText('Ocean Blue')).toBeInTheDocument();
    });

    it('should not display theme name when theme is null', () => {
      render(<BoardThemePreview theme={null} />);
      expect(screen.queryByText('Ocean Blue')).not.toBeInTheDocument();
    });
  });

  describe('expand on hover', () => {
    it('should have hover scale enabled by default', () => {
      const { container } = render(<BoardThemePreview theme={mockTheme} />);
      const wrapper = container.firstChild;
      expect(wrapper).toBeInTheDocument();
    });

    it('should respect expandOnHover prop', () => {
      const { container } = render(<BoardThemePreview theme={mockTheme} expandOnHover={false} />);
      const wrapper = container.firstChild;
      expect(wrapper).toBeInTheDocument();
    });
  });

  describe('visual styling', () => {
    it('should have rounded corners', () => {
      const { container } = render(<BoardThemePreview theme={mockTheme} />);
      expect(container.querySelector('.rounded-lg')).toBeInTheDocument();
    });

    it('should have overflow hidden', () => {
      const { container } = render(<BoardThemePreview theme={mockTheme} />);
      expect(container.querySelector('.overflow-hidden')).toBeInTheDocument();
    });
  });
});
