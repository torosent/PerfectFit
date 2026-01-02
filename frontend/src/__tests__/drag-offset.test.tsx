import { render, screen, act } from '@testing-library/react';
import React from 'react';

// Variable to control touch device state
let isTouchDeviceValue = false;

// Mock useTouchDevice hook - needs to be before the import
jest.mock('@/hooks/useTouchDevice', () => ({
  useTouchDevice: jest.fn(() => isTouchDeviceValue),
}));

// Store the DragOverlay children renderer so we can trigger drag overlay rendering
let dragOverlayChildrenRenderer: ((draggedPiece: unknown) => React.ReactNode) | null = null;

// Mock @dnd-kit/core - pass through children to allow testing the wrapper div
jest.mock('@dnd-kit/core', () => ({
  DndContext: ({ children, onDragStart }: { children: React.ReactNode; onDragStart?: (event: unknown) => void }) => {
    // Store onDragStart so tests can trigger it
    (global as Record<string, unknown>).__mockOnDragStart = onDragStart;
    return <div data-testid="dnd-context">{children}</div>;
  },
  DragOverlay: ({ children }: { children: React.ReactNode }) => {
    // Store the children renderer
    dragOverlayChildrenRenderer = () => children;
    return <div data-testid="drag-overlay">{children}</div>;
  },
  useSensor: jest.fn(),
  useSensors: jest.fn(() => []),
  PointerSensor: jest.fn(),
  TouchSensor: jest.fn(),
}));

// Mock the game store
const mockGameStore = {
  gameState: null,
  placePiece: jest.fn(),
  setHoverPosition: jest.fn(),
  setDraggedPieceIndex: jest.fn(),
  setClearingCells: jest.fn(),
  setLastPlacedCells: jest.fn(),
  clearAnimationState: jest.fn(),
};

jest.mock('@/lib/stores/game-store', () => ({
  useGameStore: jest.fn(() => mockGameStore),
}));

// Mock PieceDisplay component
jest.mock('@/components/game/PieceDisplay', () => ({
  PieceDisplay: () => <div data-testid="piece-display">PieceDisplay</div>,
}));

// Import after mocks
import { DndProvider } from '@/components/providers/DndProvider';
import { useTouchDevice } from '@/hooks/useTouchDevice';

// Helper to trigger drag start
function triggerDragStart() {
  const onDragStart = (global as Record<string, unknown>).__mockOnDragStart as ((event: unknown) => void) | undefined;
  if (onDragStart) {
    const mockPiece = {
      shape: [[true]],
      color: '#ff0000',
    };
    onDragStart({
      active: {
        id: 'test-piece',
        data: {
          current: {
            piece: mockPiece,
            pieceIndex: 0,
          },
        },
      },
    });
  }
}

describe('Drag Offset for Touch Devices', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    isTouchDeviceValue = false;
    dragOverlayChildrenRenderer = null;
  });

  describe('Touch device offset', () => {
    it('should use useTouchDevice hook', () => {
      isTouchDeviceValue = true;

      render(
        <DndProvider>
          <div>Test child</div>
        </DndProvider>
      );

      // Verify the hook was called
      expect(useTouchDevice).toHaveBeenCalled();
    });

    it('should render children normally', () => {
      isTouchDeviceValue = true;

      render(
        <DndProvider>
          <div data-testid="test-child">Test child content</div>
        </DndProvider>
      );

      expect(screen.getByTestId('test-child')).toBeInTheDocument();
    });

    it('should apply translateY(-60px) transform on touch devices when dragging', () => {
      isTouchDeviceValue = true;

      render(
        <DndProvider>
          <div data-testid="test-child">Test child</div>
        </DndProvider>
      );

      // Trigger drag start to make the drag overlay visible
      act(() => {
        triggerDragStart();
      });

      // Find the touch offset wrapper by its data-testid
      const offsetWrapper = screen.getByTestId('touch-offset-wrapper');
      expect(offsetWrapper).toBeInTheDocument();
      expect(offsetWrapper).toHaveStyle({ transform: 'translateY(-60px)' });
    });
  });

  describe('Desktop/Mouse device (no offset)', () => {
    it('should use useTouchDevice hook on non-touch devices', () => {
      isTouchDeviceValue = false;

      render(
        <DndProvider>
          <div>Test child</div>
        </DndProvider>
      );

      expect(useTouchDevice).toHaveBeenCalled();
    });

    it('should render children normally without offset on desktop', () => {
      isTouchDeviceValue = false;

      render(
        <DndProvider>
          <div data-testid="test-child">Test child content</div>
        </DndProvider>
      );

      expect(screen.getByTestId('test-child')).toBeInTheDocument();
    });

    it('should not apply transform on desktop devices when dragging', () => {
      isTouchDeviceValue = false;

      render(
        <DndProvider>
          <div data-testid="test-child">Test child</div>
        </DndProvider>
      );

      // Trigger drag start to make the drag overlay visible
      act(() => {
        triggerDragStart();
      });

      // Find the touch offset wrapper by its data-testid
      const offsetWrapper = screen.getByTestId('touch-offset-wrapper');
      expect(offsetWrapper).toBeInTheDocument();
      // On desktop, no transform style should be applied
      expect(offsetWrapper).not.toHaveStyle({ transform: 'translateY(-60px)' });
    });
  });

  describe('DndProvider renders correctly', () => {
    it('should render DndContext', () => {
      render(
        <DndProvider>
          <div>Test child</div>
        </DndProvider>
      );

      expect(screen.getByTestId('dnd-context')).toBeInTheDocument();
    });

    it('should render DragOverlay', () => {
      render(
        <DndProvider>
          <div>Test child</div>
        </DndProvider>
      );

      expect(screen.getByTestId('drag-overlay')).toBeInTheDocument();
    });

    it('should render touch-offset-wrapper with PieceDisplay when dragging', () => {
      isTouchDeviceValue = true;

      render(
        <DndProvider>
          <div data-testid="test-child">Test child</div>
        </DndProvider>
      );

      // Before dragging, wrapper should not exist
      expect(screen.queryByTestId('touch-offset-wrapper')).not.toBeInTheDocument();

      // Trigger drag start
      act(() => {
        triggerDragStart();
      });

      // After dragging, wrapper should exist with PieceDisplay inside
      const offsetWrapper = screen.getByTestId('touch-offset-wrapper');
      expect(offsetWrapper).toBeInTheDocument();
      expect(screen.getByTestId('piece-display')).toBeInTheDocument();
    });
  });
});
