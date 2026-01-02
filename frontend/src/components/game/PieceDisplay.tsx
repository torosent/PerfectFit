'use client';

import { memo } from 'react';
import type { Piece } from '@/types';

export interface PieceDisplayProps {
  /** The piece to display */
  piece: Piece;
  /** Size of each cell in pixels */
  cellSize?: number;
  /** Whether this piece is currently selected */
  isSelected?: boolean;
  /** Whether this piece is disabled (already used) */
  isDisabled?: boolean;
  /** Optional additional CSS classes */
  className?: string;
}

/**
 * Displays a single piece using its shape matrix
 * Used in PieceSelector and for piece previews
 */
function PieceDisplayComponent({
  piece,
  cellSize = 24,
  isSelected = false,
  isDisabled = false,
  className = '',
}: PieceDisplayProps) {
  const { shape, color, type } = piece;
  const rows = shape.length;
  const cols = shape[0]?.length ?? 0;

  // Calculate grid dimensions
  const gridWidth = cols * cellSize + (cols - 1) * 2; // cells + gaps
  const gridHeight = rows * cellSize + (rows - 1) * 2;

  const baseClasses = 'relative flex items-center justify-center';
  
  const stateClasses = isDisabled
    ? 'opacity-30 cursor-not-allowed'
    : isSelected
    ? 'ring-2 ring-blue-400 ring-offset-2 ring-offset-gray-900'
    : '';

  return (
    <div
      className={`${baseClasses} ${stateClasses} ${className}`}
      style={{ minWidth: gridWidth, minHeight: gridHeight }}
      role="img"
      aria-label={`${type} piece`}
    >
      <div
        className="grid gap-0.5"
        style={{
          gridTemplateColumns: `repeat(${cols}, ${cellSize}px)`,
          gridTemplateRows: `repeat(${rows}, ${cellSize}px)`,
        }}
      >
        {shape.flatMap((row, rowIndex) =>
          row.map((cell, colIndex) => (
            <div
              key={`${rowIndex}-${colIndex}`}
              className={`rounded-sm transition-colors ${
                cell === 1 ? 'border border-white/20' : ''
              }`}
              style={{
                width: cellSize,
                height: cellSize,
                backgroundColor: cell === 1 ? color : 'transparent',
              }}
            />
          ))
        )}
      </div>
    </div>
  );
}

export const PieceDisplay = memo(PieceDisplayComponent);
PieceDisplay.displayName = 'PieceDisplay';
