'use client';

import { memo } from 'react';
import type { CellValue } from '@/types';

export interface GameCellProps {
  /** The color value of the cell, or null if empty */
  value: CellValue;
  /** Row index of this cell */
  row: number;
  /** Column index of this cell */
  col: number;
  /** Whether this cell is highlighted (e.g., for piece preview) */
  isHighlighted?: boolean;
  /** Whether the highlight indicates a valid placement */
  isValidPlacement?: boolean;
  /** Click handler for the cell */
  onClick?: (row: number, col: number) => void;
}

/**
 * Single cell component for the game board
 * Displays filled/empty state with hover effects
 */
function GameCellComponent({
  value,
  row,
  col,
  isHighlighted = false,
  isValidPlacement = true,
  onClick,
}: GameCellProps) {
  const isEmpty = value === null;
  const isClickable = onClick !== undefined;

  const handleClick = () => {
    onClick?.(row, col);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      onClick?.(row, col);
    }
  };

  // Build class names
  const baseClasses = 'aspect-square rounded-sm transition-all duration-150 border';
  
  const stateClasses = isEmpty
    ? 'border-gray-700/50'
    : 'border-white/20';

  const highlightClasses = isHighlighted
    ? isValidPlacement
      ? 'ring-2 ring-green-400 ring-inset bg-green-400/30'
      : 'ring-2 ring-red-400 ring-inset bg-red-400/30'
    : '';

  const interactiveClasses = isClickable
    ? 'cursor-pointer hover:brightness-110 focus:outline-none focus:ring-2 focus:ring-teal-500'
    : '';

  const className = `${baseClasses} ${stateClasses} ${highlightClasses} ${interactiveClasses}`;

  // Style with background color if filled
  const style = !isEmpty ? { backgroundColor: value } : { backgroundColor: 'rgba(10, 25, 41, 0.5)' };

  return (
    <div
      role={isClickable ? 'button' : undefined}
      tabIndex={isClickable ? 0 : undefined}
      aria-label={`Cell ${row + 1}, ${col + 1}${isEmpty ? ' (empty)' : ' (filled)'}`}
      className={className}
      style={style}
      onClick={isClickable ? handleClick : undefined}
      onKeyDown={isClickable ? handleKeyDown : undefined}
    />
  );
}

// Memo to prevent unnecessary re-renders since we have 100 cells
export const GameCell = memo(GameCellComponent);
GameCell.displayName = 'GameCell';
