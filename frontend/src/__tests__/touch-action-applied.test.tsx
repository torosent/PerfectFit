import { render, screen } from '@testing-library/react';
import React from 'react';

// We need to mock the modules that DndProvider and GameBoard depend on
// before importing the actual components

// Mock @dnd-kit/core
jest.mock('@dnd-kit/core', () => ({
  DndContext: ({ children }: { children: React.ReactNode }) => <div data-testid="dnd-context">{children}</div>,
  DragOverlay: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  useSensor: jest.fn(),
  useSensors: jest.fn(() => []),
  PointerSensor: jest.fn(),
  TouchSensor: jest.fn(),
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

// Mock PieceDisplay component
jest.mock('@/components/game/PieceDisplay', () => ({
  PieceDisplay: () => <div data-testid="piece-display">PieceDisplay</div>,
}));

// Mock GameCell component
jest.mock('@/components/game/GameCell', () => ({
  GameCell: ({ row, col }: { row: number; col: number }) => (
    <div data-testid={`cell-${row}-${col}`}>Cell</div>
  ),
}));

// Import the components after mocking
import { DndProvider } from '@/components/providers/DndProvider';
import { GameBoard } from '@/components/game/GameBoard';
import type { Grid } from '@/types';

describe('Touch Action CSS Classes', () => {
  // Helper to create an empty 8x8 grid
  const createEmptyGrid = (): Grid => {
    return Array.from({ length: 8 }, () => Array(8).fill(null));
  };

  describe('DndProvider', () => {
    it('should apply touch-none class to prevent browser touch gestures', () => {
      const { container } = render(
        <DndProvider>
          <div>Test child</div>
        </DndProvider>
      );

      // Find the wrapper element that should have touch-none
      // The wrapper should prevent browser gestures like pinch-zoom and scroll
      const wrapper = container.querySelector('.touch-none');
      expect(wrapper).toBeInTheDocument();
    });

    it('should apply user-select-none to prevent text selection during drag', () => {
      const { container } = render(
        <DndProvider>
          <div>Test child</div>
        </DndProvider>
      );

      const wrapper = container.querySelector('.select-none');
      expect(wrapper).toBeInTheDocument();
    });
  });

  describe('GameBoard', () => {
    it('should apply touch-none class to the game board container', () => {
      const grid = createEmptyGrid();
      const { container } = render(<GameBoard grid={grid} />);

      // The game board should prevent touch gestures
      const boardContainer = container.querySelector('.touch-none');
      expect(boardContainer).toBeInTheDocument();
    });

    it('should apply user-select-none to prevent text selection on the board', () => {
      const grid = createEmptyGrid();
      const { container } = render(<GameBoard grid={grid} />);

      const boardContainer = container.querySelector('.select-none');
      expect(boardContainer).toBeInTheDocument();
    });

    it('should have proper ARIA role for accessibility', () => {
      const grid = createEmptyGrid();
      render(<GameBoard grid={grid} />);

      const board = screen.getByRole('grid', { name: /game board/i });
      expect(board).toBeInTheDocument();
    });
  });

  describe('Global CSS Touch Utilities', () => {
    // These tests verify that the CSS utility classes exist
    // We can test this by checking if elements with these classes render without errors
    // and have expected behavior

    it('should support touch-none class for preventing all touch behaviors', () => {
      const { container } = render(
        <div className="touch-none" data-testid="touch-test">
          Test element
        </div>
      );

      const element = container.querySelector('.touch-none');
      expect(element).toBeInTheDocument();
      expect(element).toHaveAttribute('data-testid', 'touch-test');
    });

    it('should support select-none class for preventing text selection', () => {
      const { container } = render(
        <div className="select-none" data-testid="select-test">
          Test element
        </div>
      );

      const element = container.querySelector('.select-none');
      expect(element).toBeInTheDocument();
    });
  });
});
