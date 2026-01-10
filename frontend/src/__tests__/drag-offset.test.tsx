import { render, screen, act } from '@testing-library/react';
import React from 'react';

// Variable to control touch device state
let isTouchDeviceValue = false;

// Mock useTouchDevice hook - needs to be before the import
jest.mock('@/hooks/useTouchDevice', () => ({
  useTouchDevice: jest.fn(() => isTouchDeviceValue),
}));

// Mock useHaptics hook
jest.mock('@/hooks', () => ({
  useTouchDevice: jest.fn(() => isTouchDeviceValue),
  useHaptics: jest.fn(() => ({
    lightTap: jest.fn(),
    mediumTap: jest.fn(),
    lineClear: jest.fn(),
  })),
}));

// Mock @dnd-kit/core - pass through children to allow testing the wrapper div
jest.mock('@dnd-kit/core', () => ({
  DndContext: ({ children, onDragStart }: { children: React.ReactNode; onDragStart?: (event: unknown) => void }) => {
    // Store onDragStart so tests can trigger it
    (global as Record<string, unknown>).__mockOnDragStart = onDragStart;
    return <div data-testid="dnd-context">{children}</div>;
  },
  DragOverlay: ({ children }: { children: React.ReactNode }) => {
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

// Helper to trigger drag start with optional touch simulation
function triggerDragStart(isTouch = false) {
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
      activatorEvent: isTouch 
        ? { pointerType: 'touch' } as PointerEvent
        : { pointerType: 'mouse' } as PointerEvent,
    });
  }
}

describe('Drag Offset for Touch Devices', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    isTouchDeviceValue = false;
  });

  describe('Touch device offset', () => {
    it('should render on touch devices', () => {
      isTouchDeviceValue = true;

      render(
        <DndProvider>
          <div>Test child</div>
        </DndProvider>
      );

      // Verify the provider renders correctly
      expect(screen.getByTestId('dnd-context')).toBeInTheDocument();
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

    it('should apply translate transform on touch-initiated drags', () => {
      isTouchDeviceValue = true;

      render(
        <DndProvider>
          <div data-testid="test-child">Test child</div>
        </DndProvider>
      );

      // Trigger drag start with touch event simulation
      act(() => {
        triggerDragStart(true); // Simulate touch drag
      });

      // Find the touch offset wrapper by its data-testid
      const offsetWrapper = screen.getByTestId('touch-offset-wrapper');
      expect(offsetWrapper).toBeInTheDocument();
      // The offset is applied as translate(0px, -80px) for touch drags
      expect(offsetWrapper).toHaveStyle({ transform: 'translate(0px, -80px)' });
    });
  });

  describe('Desktop/Mouse device (no offset)', () => {
    it('should render on non-touch devices', () => {
      isTouchDeviceValue = false;

      render(
        <DndProvider>
          <div>Test child</div>
        </DndProvider>
      );

      // Verify the provider renders correctly
      expect(screen.getByTestId('dnd-context')).toBeInTheDocument();
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

    it('should not apply transform on mouse-initiated drags', () => {
      isTouchDeviceValue = false;

      render(
        <DndProvider>
          <div data-testid="test-child">Test child</div>
        </DndProvider>
      );

      // Trigger drag start with mouse event simulation
      act(() => {
        triggerDragStart(false); // Simulate mouse drag
      });

      // Find the touch offset wrapper by its data-testid
      const offsetWrapper = screen.getByTestId('touch-offset-wrapper');
      expect(offsetWrapper).toBeInTheDocument();
      // On mouse drags, no transform style should be applied
      expect(offsetWrapper).not.toHaveStyle({ transform: 'translate(0px, -80px)' });
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
        triggerDragStart(true);
      });

      // After dragging, wrapper should exist with PieceDisplay inside
      const offsetWrapper = screen.getByTestId('touch-offset-wrapper');
      expect(offsetWrapper).toBeInTheDocument();
      expect(screen.getByTestId('piece-display')).toBeInTheDocument();
    });
  });
});
