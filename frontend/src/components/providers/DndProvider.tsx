'use client';

import React, { useCallback, useState, useRef, useEffect } from 'react';
import {
  DndContext,
  DragOverlay,
  useSensor,
  useSensors,
  PointerSensor,
  TouchSensor,
  type DragStartEvent,
  type DragEndEvent,
} from '@dnd-kit/core';
import type { Piece, Grid, ClearingCell, Position } from '@/types';
import { useGameStore } from '@/lib/stores/game-store';
import { canPlacePiece, getPieceCells } from '@/lib/game-logic/pieces';
import { PieceDisplay } from '@/components/game/PieceDisplay';
import { useTouchDevice, useHaptics } from '@/hooks';

// Touch offset constants - how many pixels to lift the dragged piece above the finger
// This offset is applied to BOTH the visual overlay AND the placement calculation
// so what you see is exactly where the piece will be placed
const TOUCH_DRAG_OFFSET_Y = -80; // Lift piece higher so it's visible above finger
const TOUCH_DRAG_OFFSET_X = 0;   // No horizontal offset by default

interface DragData {
  piece: Piece;
  pieceIndex: number;
}

export interface DndProviderProps {
  children: React.ReactNode;
}

/**
 * Find cells that were cleared by comparing two grids
 * Returns positions with their original colors for animation
 * Also includes placed cells that were cleared (part of completed lines)
 */
function findClearedCells(
  oldGrid: Grid | null, 
  newGrid: Grid,
  placedCells: Position[] = [],
  placedColor?: string
): ClearingCell[] {
  if (!oldGrid) return [];
  
  const clearedCells: ClearingCell[] = [];
  const clearedPositions = new Set<string>();
  
  for (let row = 0; row < 8; row++) {
    for (let col = 0; col < 8; col++) {
      const oldValue = oldGrid[row][col];
      if (oldValue !== null && newGrid[row][col] === null) {
        clearedCells.push({ row, col, color: oldValue });
        clearedPositions.add(`${row}-${col}`);
      }
    }
  }
  
  // Check if any placed cells are now empty (they were part of a cleared line)
  // These cells weren't in the old grid, so we need to add them manually
  if (placedColor) {
    for (const { row, col } of placedCells) {
      const key = `${row}-${col}`;
      // Only add if this cell is now empty and not already in cleared cells
      if (newGrid[row][col] === null && !clearedPositions.has(key)) {
        clearedCells.push({ row, col, color: placedColor });
      }
    }
  }
  
  return clearedCells;
}

/**
 * DnD provider that wraps the game components
 * Handles drag start, move, and end events
 */
export function DndProvider({ children }: DndProviderProps) {
  const [draggedPiece, setDraggedPiece] = useState<Piece | null>(null);
  const [draggedPieceIndex, setDraggedPieceIndexState] = useState<number | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [isTouchDragging, setIsTouchDragging] = useState(false);
  
  // Detect if we're on a touch device to apply drag offset
  const isTouchDevice = useTouchDevice();
  
  // Haptic feedback for touch interactions
  const haptics = useHaptics();
  
  const { 
    gameState, 
    placePiece, 
    setHoverPosition, 
    setDraggedPieceIndex,
    setClearingCells,
    setLastPlacedCells,
    clearAnimationState,
  } = useGameStore();

  // Track pointer position for grid calculations
  const pointerPositionRef = useRef<{ x: number; y: number } | null>(null);
  // Track grid before placement for animation detection
  const prevGridRef = useRef<Grid | null>(null);

  // Configure sensors for both mouse and touch
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 5, // Start drag after 5px movement
      },
    }),
    useSensor(TouchSensor, {
      activationConstraint: {
        delay: 150, // Small delay for touch to distinguish from scroll
        tolerance: 5,
      },
    })
  );

  // Track pointer position during drag via native events
  useEffect(() => {
    if (!isDragging) return;

    const handlePointerMove = (e: PointerEvent) => {
      pointerPositionRef.current = { x: e.clientX, y: e.clientY };
      
      // Find the game board element
      const boardElement = document.querySelector('[aria-label="Game board - drop pieces here"]');
      if (!boardElement) return;

      const boardRect = boardElement.getBoundingClientRect();
      const cellWidth = boardRect.width / 8;
      const cellHeight = boardRect.height / 8;

      // Calculate grid position - no offset for mouse/pointer
      const col = Math.floor((e.clientX - boardRect.left) / cellWidth);
      const row = Math.floor((e.clientY - boardRect.top) / cellHeight);

      // Only update if within bounds
      if (row >= 0 && row < 8 && col >= 0 && col < 8) {
        setHoverPosition({ row, col });
      } else {
        setHoverPosition(null);
      }
    };

    const handleTouchMove = (e: TouchEvent) => {
      if (e.touches.length > 0) {
        const touch = e.touches[0];
        pointerPositionRef.current = { x: touch.clientX, y: touch.clientY };
        
        // Find the game board element
        const boardElement = document.querySelector('[aria-label="Game board - drop pieces here"]');
        if (!boardElement) return;

        const boardRect = boardElement.getBoundingClientRect();
        const cellWidth = boardRect.width / 8;
        const cellHeight = boardRect.height / 8;

        // Apply the SAME offset to placement calculation that we apply to the visual
        // This ensures "what you see is where it places"
        const adjustedX = touch.clientX + TOUCH_DRAG_OFFSET_X;
        const adjustedY = touch.clientY + TOUCH_DRAG_OFFSET_Y;

        // Calculate grid position using the offset position
        const col = Math.floor((adjustedX - boardRect.left) / cellWidth);
        const row = Math.floor((adjustedY - boardRect.top) / cellHeight);

        // Only update if within bounds
        if (row >= 0 && row < 8 && col >= 0 && col < 8) {
          setHoverPosition({ row, col });
        } else {
          setHoverPosition(null);
        }
      }
    };

    window.addEventListener('pointermove', handlePointerMove);
    window.addEventListener('touchmove', handleTouchMove);

    return () => {
      window.removeEventListener('pointermove', handlePointerMove);
      window.removeEventListener('touchmove', handleTouchMove);
    };
  }, [isDragging, setHoverPosition]);

  const handleDragStart = useCallback((event: DragStartEvent) => {
    const data = event.active.data.current as DragData | undefined;
    
    if (data?.piece) {
      setDraggedPiece(data.piece);
      setDraggedPieceIndexState(data.pieceIndex);
      setDraggedPieceIndex(data.pieceIndex);
      setIsDragging(true);
      
      // Detect if this is a touch-initiated drag
      // @dnd-kit uses 'touch' activator for touch events
      const activatorEvent = event.activatorEvent;
      const isTouch = (typeof TouchEvent !== 'undefined' && activatorEvent instanceof TouchEvent) || 
                      (activatorEvent as PointerEvent)?.pointerType === 'touch';
      setIsTouchDragging(isTouch);
      
      // Haptic feedback on piece pickup
      haptics.lightTap();
    }
  }, [setDraggedPieceIndex, haptics]);

  const handleDragMove = useCallback(() => {
    // Position tracking is handled by the native event listeners
    // This callback is kept for potential future use
  }, []);

  const handleDragEnd = useCallback(async (event: DragEndEvent) => {
    const { over } = event;
    
    // Get the final hover position from store
    const { hoverPosition, gameState: currentGameState } = useGameStore.getState();
    
    // Attempt to place piece if dropped over board with valid position
    if (
      over?.id === 'game-board' && 
      draggedPieceIndex !== null && 
      hoverPosition &&
      currentGameState &&
      draggedPiece
    ) {
      // Check if placement is valid
      if (canPlacePiece(currentGameState.grid, draggedPiece, hoverPosition.row, hoverPosition.col)) {
        // Store grid before placement for animation comparison
        prevGridRef.current = currentGameState.grid.map(r => [...r]);
        
        // Get the cells that will be placed (for animation)
        const placedCells = getPieceCells(draggedPiece, hoverPosition.row, hoverPosition.col);
        const placedColor = draggedPiece.color;
        
        // Place the piece
        const success = await placePiece(draggedPieceIndex, hoverPosition.row, hoverPosition.col);
        
        if (success) {
          // Haptic feedback on successful placement
          haptics.mediumTap();
          
          // Trigger placed animation
          setLastPlacedCells(placedCells);
          
          // Check for cleared cells after a small delay to let state update
          setTimeout(() => {
            const { gameState: newGameState } = useGameStore.getState();
            if (newGameState && prevGridRef.current) {
              const clearedCells = findClearedCells(
                prevGridRef.current, 
                newGameState.grid,
                placedCells,
                placedColor
              );
              
              if (clearedCells.length > 0) {
                setClearingCells(clearedCells);
                
                // Haptic feedback for line clear (celebratory)
                haptics.lineClear();
                
                // Clear animation state after animation completes
                setTimeout(() => {
                  clearAnimationState();
                }, 700);
              } else {
                // Clear placed animation after it completes
                setTimeout(() => {
                  clearAnimationState();
                }, 400);
              }
              
              prevGridRef.current = null;
            }
          }, 50);
        }
      }
    }
    
    // Reset drag state
    setDraggedPiece(null);
    setDraggedPieceIndexState(null);
    setDraggedPieceIndex(null);
    setHoverPosition(null);
    setIsDragging(false);
    setIsTouchDragging(false);
    pointerPositionRef.current = null;
  }, [draggedPieceIndex, draggedPiece, placePiece, setDraggedPieceIndex, setHoverPosition, setLastPlacedCells, setClearingCells, clearAnimationState, haptics]);

  const handleDragCancel = useCallback(() => {
    setDraggedPiece(null);
    setDraggedPieceIndexState(null);
    setDraggedPieceIndex(null);
    setHoverPosition(null);
    setIsDragging(false);
    setIsTouchDragging(false);
    pointerPositionRef.current = null;
  }, [setDraggedPieceIndex, setHoverPosition]);

  return (
    <div className="touch-none select-none">
      <DndContext
        sensors={sensors}
        onDragStart={handleDragStart}
        onDragMove={handleDragMove}
        onDragEnd={handleDragEnd}
        onDragCancel={handleDragCancel}
      >
        {children}
      
        {/* Drag overlay shows the piece being dragged */}
        {/* On touch devices, we offset the visual piece AND the placement calculation */}
        {/* so "what you see is where it places" - this makes dragging much easier on mobile */}
        <DragOverlay dropAnimation={null}>
          {draggedPiece && (
            <div 
              className="opacity-80 scale-110 pointer-events-none"
              data-testid="touch-offset-wrapper"
              style={isTouchDragging ? { 
                transform: `translate(${TOUCH_DRAG_OFFSET_X}px, ${TOUCH_DRAG_OFFSET_Y}px)` 
              } : undefined}
            >
              <PieceDisplay
                piece={draggedPiece}
                cellSize={20}
                isSelected={false}
                isDisabled={false}
              />
            </div>
          )}
        </DragOverlay>
      </DndContext>
    </div>
  );
}

export default DndProvider;
