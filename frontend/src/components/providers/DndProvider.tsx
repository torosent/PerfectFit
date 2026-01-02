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
import type { Piece, Grid, ClearingCell } from '@/types';
import { useGameStore } from '@/lib/stores/game-store';
import { canPlacePiece, getPieceCells } from '@/lib/game-logic/pieces';
import { PieceDisplay } from '@/components/game/PieceDisplay';

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
 */
function findClearedCells(oldGrid: Grid | null, newGrid: Grid): ClearingCell[] {
  if (!oldGrid) return [];
  
  const clearedCells: ClearingCell[] = [];
  for (let row = 0; row < 10; row++) {
    for (let col = 0; col < 10; col++) {
      const oldValue = oldGrid[row][col];
      if (oldValue !== null && newGrid[row][col] === null) {
        clearedCells.push({ row, col, color: oldValue });
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
      const cellWidth = boardRect.width / 10;
      const cellHeight = boardRect.height / 10;

      // Calculate grid position
      const col = Math.floor((e.clientX - boardRect.left) / cellWidth);
      const row = Math.floor((e.clientY - boardRect.top) / cellHeight);

      // Only update if within bounds
      if (row >= 0 && row < 10 && col >= 0 && col < 10) {
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
        const cellWidth = boardRect.width / 10;
        const cellHeight = boardRect.height / 10;

        // Calculate grid position
        const col = Math.floor((touch.clientX - boardRect.left) / cellWidth);
        const row = Math.floor((touch.clientY - boardRect.top) / cellHeight);

        // Only update if within bounds
        if (row >= 0 && row < 10 && col >= 0 && col < 10) {
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
    }
  }, [setDraggedPieceIndex]);

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
        
        // Place the piece
        const success = await placePiece(draggedPieceIndex, hoverPosition.row, hoverPosition.col);
        
        if (success) {
          // Trigger placed animation
          setLastPlacedCells(placedCells);
          
          // Check for cleared cells after a small delay to let state update
          setTimeout(() => {
            const { gameState: newGameState } = useGameStore.getState();
            if (newGameState && prevGridRef.current) {
              const clearedCells = findClearedCells(prevGridRef.current, newGameState.grid);
              
              if (clearedCells.length > 0) {
                setClearingCells(clearedCells);
                
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
    pointerPositionRef.current = null;
  }, [draggedPieceIndex, draggedPiece, placePiece, setDraggedPieceIndex, setHoverPosition, setLastPlacedCells, setClearingCells, clearAnimationState]);

  const handleDragCancel = useCallback(() => {
    setDraggedPiece(null);
    setDraggedPieceIndexState(null);
    setDraggedPieceIndex(null);
    setHoverPosition(null);
    setIsDragging(false);
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
        <DragOverlay dropAnimation={null}>
          {draggedPiece && (
            <div className="opacity-80 scale-110 pointer-events-none">
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
