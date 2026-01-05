'use client';

import { memo, useCallback } from 'react';
import type { Grid } from '@/types';
import { GameCell } from './GameCell';

export interface HighlightedCell {
  row: number;
  col: number;
  isValid: boolean;
}

export interface GameBoardProps {
  /** The 8x8 game grid */
  grid: Grid;
  /** Cells to highlight (e.g., piece preview) */
  highlightedCells?: HighlightedCell[];
  /** Callback when a cell is clicked */
  onCellClick?: (row: number, col: number) => void;
  /** Whether the board is disabled (non-interactive) */
  disabled?: boolean;
}

/**
 * Main game board component
 * Renders a 8x8 CSS grid of game cells
 */
function GameBoardComponent({
  grid,
  highlightedCells = [],
  onCellClick,
  disabled = false,
}: GameBoardProps) {
  // Create a map for quick highlight lookup
  const highlightMap = new Map<string, boolean>();
  highlightedCells.forEach(({ row, col, isValid }) => {
    highlightMap.set(`${row}-${col}`, isValid);
  });

  const handleCellClick = useCallback(
    (row: number, col: number) => {
      if (!disabled && onCellClick) {
        onCellClick(row, col);
      }
    },
    [disabled, onCellClick]
  );

  return (
    <div
      className="overflow-x-auto overflow-y-hidden"
      data-scroll-container="true"
    >
      <div
        className="p-2 sm:p-3 rounded-xl shadow-2xl touch-none select-none"
        style={{ backgroundColor: '#0a1929', borderWidth: 1, borderStyle: 'solid', borderColor: 'rgba(20, 184, 166, 0.3)' }}
        role="grid"
        aria-label="Game board"
        data-mobile-optimized="true"
        data-min-cell-size="44"
      >
        <div
          className="grid grid-cols-8 gap-0.5 sm:gap-1"
          style={{
            // Mobile-first: use 44px cells for touch targets (8 × 44px = 352px minimum)
            // On larger screens: scale up to fit viewport (max 600px for comfortable play)
            width: 'max(352px, min(90vw, 600px))',
            height: 'max(352px, min(90vw, 600px))',
          }}
        >
        {grid.map((row, rowIndex) =>
          row.map((cellValue, colIndex) => {
            const key = `${rowIndex}-${colIndex}`;
            const highlightValid = highlightMap.get(key);
            const isHighlighted = highlightValid !== undefined;

            return (
              <GameCell
                key={key}
                value={cellValue}
                row={rowIndex}
                col={colIndex}
                isHighlighted={isHighlighted}
                isValidPlacement={highlightValid ?? true}
                onClick={!disabled ? handleCellClick : undefined}
              />
            );
          })
        )}
        </div>
      </div>
    </div>
  );
}

export const GameBoard = memo(GameBoardComponent);
GameBoard.displayName = 'GameBoard';
