'use client';

import { memo, useMemo } from 'react';
import { useDroppable } from '@dnd-kit/core';
import type { Grid, Piece } from '@/types';
import { GameCell } from './GameCell';
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
  /** Callback when a cell is clicked */
  onCellClick?: (row: number, col: number) => void;
  /** Whether the board is disabled (non-interactive) */
  disabled?: boolean;
}

/**
 * Droppable game board that accepts piece drops
 * Shows preview of piece at hover position with validity indication
 */
function DroppableBoardComponent({
  grid,
  hoverPosition,
  draggedPiece,
  highlightedCells = [],
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



  // Create a map for quick highlight lookup
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

  return (
    <div
      ref={setNodeRef}
      className={`
        bg-gray-900 p-2 sm:p-3 rounded-xl shadow-2xl border border-gray-700
        transition-all duration-150
        ${isOver ? 'ring-2 ring-blue-500 ring-opacity-50' : ''}
      `}
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

            return (
              <div
                key={key}
                className={`
                  relative
                  ${isPreview && draggedPiece ? 'z-10' : ''}
                `}
              >
                <GameCell
                  value={cellValue}
                  row={rowIndex}
                  col={colIndex}
                  isHighlighted={isHighlighted}
                  isValidPlacement={highlight?.isValid ?? true}
                  onClick={!disabled ? onCellClick : undefined}
                />
                
                {/* Ghost preview for dragged piece */}
                {isPreview && draggedPiece && (
                  <div
                    className={`
                      absolute inset-0 rounded-sm pointer-events-none
                      ${highlight?.isValid 
                        ? 'bg-opacity-40 border-2 border-green-400' 
                        : 'bg-opacity-40 border-2 border-red-400'
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
    </div>
  );
}

export const DroppableBoard = memo(DroppableBoardComponent);
DroppableBoard.displayName = 'DroppableBoard';
