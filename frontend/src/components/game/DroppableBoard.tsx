'use client';

import { memo, useMemo } from 'react';
import { useDroppable } from '@dnd-kit/core';
import { motion } from 'motion/react';
import type { Grid, Piece, Position } from '@/types';
import { AnimatedCell } from './AnimatedCell';
import { canPlacePiece, getPieceCells } from '@/lib/game-logic/pieces';

export interface HighlightedCell {
  row: number;
  col: number;
  isValid: boolean;
}

export interface DroppableBoardProps {
  /** The 10x10 game grid */
  grid: Grid;
  /** Position where piece preview should be shown */
  hoverPosition: { row: number; col: number } | null;
  /** The piece being dragged (for preview) */
  draggedPiece: Piece | null;
  /** Additional highlighted cells (from click-to-place flow) */
  highlightedCells?: HighlightedCell[];
  /** Cells currently being cleared (for animation) */
  clearingCells?: Position[];
  /** Recently placed cells (for animation) */
  lastPlacedCells?: Position[];
  /** Callback when a cell is clicked */
  onCellClick?: (row: number, col: number) => void;
  /** Whether the board is disabled (non-interactive) */
  disabled?: boolean;
}

/**
 * Droppable game board that accepts piece drops
 * Shows preview of piece at hover position with validity indication
 * Includes animations for cell states
 */
function DroppableBoardComponent({
  grid,
  hoverPosition,
  draggedPiece,
  highlightedCells = [],
  clearingCells = [],
  lastPlacedCells = [],
  onCellClick,
  disabled = false,
}: DroppableBoardProps) {
  const { isOver, setNodeRef } = useDroppable({
    id: 'game-board',
  });

  // Calculate drag preview cells based on hover position and dragged piece
  const dragPreviewCells = useMemo<HighlightedCell[]>(() => {
    if (!hoverPosition || !draggedPiece) {
      return [];
    }

    const { row, col } = hoverPosition;
    const isValid = canPlacePiece(grid, draggedPiece, row, col);
    const cells = getPieceCells(draggedPiece, row, col);

    return cells
      .filter(
        (cell) =>
          cell.row >= 0 &&
          cell.row < 10 &&
          cell.col >= 0 &&
          cell.col < 10
      )
      .map((cell) => ({
        ...cell,
        isValid,
      }));
  }, [grid, hoverPosition, draggedPiece]);

  // Create maps for quick lookup
  const highlightMap = useMemo(() => {
    const map = new Map<string, { isValid: boolean; isPreview: boolean }>();
    
    highlightedCells.forEach(({ row, col, isValid }) => {
      map.set(`${row}-${col}`, { isValid, isPreview: false });
    });
    
    dragPreviewCells.forEach(({ row, col, isValid }) => {
      map.set(`${row}-${col}`, { isValid, isPreview: true });
    });
    
    return map;
  }, [highlightedCells, dragPreviewCells]);

  const clearingMap = useMemo(() => {
    const set = new Set<string>();
    clearingCells.forEach(({ row, col }) => {
      set.add(`${row}-${col}`);
    });
    return set;
  }, [clearingCells]);

  const placedMap = useMemo(() => {
    const map = new Map<string, number>();
    lastPlacedCells.forEach(({ row, col }, index) => {
      map.set(`${row}-${col}`, index);
    });
    return map;
  }, [lastPlacedCells]);

  return (
    <motion.div
      ref={setNodeRef}
      className={`
        bg-gray-900 p-2 sm:p-3 rounded-xl shadow-2xl border border-gray-700
        transition-all duration-150
        ${isOver ? 'ring-2 ring-blue-500 ring-opacity-50' : ''}
      `}
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ duration: 0.3 }}
      role="grid"
      aria-label="Game board - drop pieces here"
    >
      <div
        className="grid grid-cols-10 gap-0.5 sm:gap-1"
        style={{
          // Ensure consistent sizing across devices
          width: 'min(90vw, 400px)',
          height: 'min(90vw, 400px)',
        }}
      >
        {grid.map((row, rowIndex) =>
          row.map((cellValue, colIndex) => {
            const key = `${rowIndex}-${colIndex}`;
            const highlight = highlightMap.get(key);
            const isHighlighted = highlight !== undefined;
            const isPreview = highlight?.isPreview ?? false;
            const isClearing = clearingMap.has(key);
            const placedIndex = placedMap.get(key);
            const isRecentlyPlaced = placedIndex !== undefined;

            return (
              <div
                key={key}
                className={`
                  relative
                  ${isPreview && draggedPiece ? 'z-10' : ''}
                  ${isClearing ? 'z-20' : ''}
                `}
              >
                <AnimatedCell
                  value={cellValue}
                  row={rowIndex}
                  col={colIndex}
                  isHighlighted={isHighlighted}
                  isValidPlacement={highlight?.isValid ?? true}
                  isClearing={isClearing}
                  isRecentlyPlaced={isRecentlyPlaced}
                  placedIndex={placedIndex ?? 0}
                  onClick={!disabled ? onCellClick : undefined}
                />
                
                {/* Ghost preview for dragged piece */}
                {isPreview && draggedPiece && (
                  <motion.div
                    initial={{ opacity: 0, scale: 0.8 }}
                    animate={{ opacity: 1, scale: 1 }}
                    className={`
                      absolute inset-0 rounded-sm pointer-events-none
                      ${highlight?.isValid 
                        ? 'border-2 border-green-400' 
                        : 'border-2 border-red-400'
                      }
                    `}
                    style={{
                      backgroundColor: highlight?.isValid 
                        ? `${draggedPiece.color}66` // 40% opacity
                        : 'rgba(239, 68, 68, 0.4)',
                    }}
                  />
                )}
              </div>
            );
          })
        )}
      </div>
    </motion.div>
  );
}

export const DroppableBoard = memo(DroppableBoardComponent);
DroppableBoard.displayName = 'DroppableBoard';
