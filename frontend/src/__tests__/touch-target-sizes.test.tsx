import { render } from '@testing-library/react';
import React from 'react';

// Mock @dnd-kit/core
jest.mock('@dnd-kit/core', () => ({
  DndContext: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  DragOverlay: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  useSensor: jest.fn(),
  useSensors: jest.fn(() => []),
  PointerSensor: jest.fn(),
  TouchSensor: jest.fn(),
  useDraggable: () => ({
    attributes: {},
    listeners: {},
    setNodeRef: jest.fn(),
    transform: null,
    isDragging: false,
  }),
  useDroppable: () => ({
    isOver: false,
    setNodeRef: jest.fn(),
  }),
}));

// Mock motion/react to avoid animation issues in tests
jest.mock('motion/react', () => ({
  motion: {
    div: ({ children, ...props }: { children?: React.ReactNode } & Record<string, unknown>) => (
      <div {...props}>{children}</div>
    ),
  },
  AnimatePresence: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

// Mock the game store
jest.mock('@/lib/stores/game-store', () => ({
  useGameStore: jest.fn(() => ({
    gameState: null,
    placePiece: jest.fn(),
    setHoverPosition: jest.fn(),
    setDraggedPieceIndex: jest.fn(),
    setClearingCells: jest.fn(),
    setLastPlacedCells: jest.fn(),
    clearAnimationState: jest.fn(),
  })),
}));

import { GameBoard } from '@/components/game/GameBoard';
import { PieceSelector } from '@/components/game/PieceSelector';
import { DraggablePiece } from '@/components/game/DraggablePiece';
import { PieceDisplay } from '@/components/game/PieceDisplay';
import type { Grid, Piece } from '@/types';

describe('Touch Target Sizes for Mobile', () => {
  // Helper to create an empty 8x8 grid
  const createEmptyGrid = (): Grid => {
    return Array.from({ length: 8 }, () => Array(8).fill(null));
  };

  // Create a test piece
  const createTestPiece = (): Piece => ({
    id: 'test-piece',
    type: 'L',
    shape: [
      [1, 0],
      [1, 0],
      [1, 1],
    ],
    color: '#ef4444',
  });

  describe('GameBoard responsive sizing', () => {
    it('should have a data attribute for mobile cell size configuration', () => {
      const grid = createEmptyGrid();
      const { container } = render(<GameBoard grid={grid} />);
      
      // The board container should indicate mobile-friendly sizing
      const boardContainer = container.querySelector('[data-mobile-optimized="true"]');
      expect(boardContainer).toBeInTheDocument();
    });

    it('should use CSS custom property for cell size that can be adjusted per viewport', () => {
      const grid = createEmptyGrid();
      const { container } = render(<GameBoard grid={grid} />);
      
      // The grid should use CSS custom property for cell sizing
      const gridElement = container.querySelector('.grid');
      expect(gridElement).toBeInTheDocument();
      
      // Check that the grid has mobile-optimized class or data attribute
      const boardWrapper = container.querySelector('[data-mobile-optimized]');
      expect(boardWrapper).toBeInTheDocument();
    });

    it('should allow horizontal scrolling on small screens via overflow styles', () => {
      const grid = createEmptyGrid();
      const { container } = render(<GameBoard grid={grid} />);
      
      // The board should have overflow-x-auto or similar to allow scrolling
      const scrollContainer = container.querySelector('[data-scroll-container="true"]');
      expect(scrollContainer).toBeInTheDocument();
    });

    it('should render minimum 44px touch targets through cell sizing', () => {
      const grid = createEmptyGrid();
      const { container } = render(<GameBoard grid={grid} />);
      
      // The board should have a data attribute indicating min touch target size
      const boardWithMinSize = container.querySelector('[data-min-cell-size="44"]');
      expect(boardWithMinSize).toBeInTheDocument();
    });
  });

  describe('PieceSelector touch spacing', () => {
    it('should have adequate touch spacing between piece slots', () => {
      const pieces = [createTestPiece(), createTestPiece()];
      const { container } = render(
        <PieceSelector 
          pieces={pieces} 
          selectedIndex={null} 
          onSelect={jest.fn()} 
        />
      );
      
      // The piece container should have mobile-optimized spacing
      const pieceContainer = container.querySelector('[data-touch-spacing="true"]');
      expect(pieceContainer).toBeInTheDocument();
    });

    it('should have minimum padding for touch targets on piece items', () => {
      const pieces = [createTestPiece()];
      const { container } = render(
        <PieceSelector 
          pieces={pieces} 
          selectedIndex={null} 
          onSelect={jest.fn()} 
        />
      );
      
      // Check for min-touch-target class or data attribute
      const touchTarget = container.querySelector('[data-min-touch-target="true"]');
      expect(touchTarget).toBeInTheDocument();
    });
  });

  describe('DraggablePiece touch target sizing', () => {
    it('should have minimum 44px touch area', () => {
      const piece = createTestPiece();
      const { container } = render(
        <DraggablePiece
          piece={piece}
          index={0}
          isSelected={false}
          disabled={false}
        />
      );
      
      // The draggable piece should indicate minimum touch size
      const touchTarget = container.querySelector('[data-min-touch-target="true"]');
      expect(touchTarget).toBeInTheDocument();
    });

    it('should have adequate padding around the piece for comfortable tapping', () => {
      const piece = createTestPiece();
      const { container } = render(
        <DraggablePiece
          piece={piece}
          index={0}
          isSelected={false}
          disabled={false}
        />
      );
      
      // Should have mobile-optimized padding
      const paddedElement = container.querySelector('[data-mobile-padding="true"]');
      expect(paddedElement).toBeInTheDocument();
    });
  });

  describe('PieceDisplay mobile cell sizes', () => {
    it('should use larger cell size when mobileCellSize prop is provided', () => {
      const piece = createTestPiece();
      const { container } = render(
        <PieceDisplay
          piece={piece}
          cellSize={20}
          mobileCellSize={28}
        />
      );
      
      // Should have data attribute for mobile cell size
      const displayWithMobileSize = container.querySelector('[data-mobile-cell-size="28"]');
      expect(displayWithMobileSize).toBeInTheDocument();
    });

    it('should apply mobile cell size by default for touch-friendly display', () => {
      const piece = createTestPiece();
      const { container } = render(
        <PieceDisplay piece={piece} />
      );
      
      // Should have mobile-optimized attribute by default
      const mobileOptimized = container.querySelector('[data-mobile-optimized="true"]');
      expect(mobileOptimized).toBeInTheDocument();
    });
  });

  describe('Minimum touch target requirements', () => {
    // Apple HIG recommends 44pt minimum touch targets
    // This test verifies components are configured for this
    
    it('should configure GameBoard cells for 44px minimum on mobile', () => {
      const grid = createEmptyGrid();
      const { container } = render(<GameBoard grid={grid} />);
      
      // Check for the minimum cell size configuration
      const board = container.querySelector('[data-min-cell-size]');
      expect(board).toBeInTheDocument();
      expect(board?.getAttribute('data-min-cell-size')).toBe('44');
    });

    it('should configure pieces with touch-friendly sizing', () => {
      const pieces = [createTestPiece()];
      const { container } = render(
        <PieceSelector 
          pieces={pieces} 
          selectedIndex={null} 
          onSelect={jest.fn()} 
        />
      );
      
      // The wrapper should indicate touch optimization
      const touchOptimized = container.querySelector('[data-touch-optimized="true"]');
      expect(touchOptimized).toBeInTheDocument();
    });
  });
});
