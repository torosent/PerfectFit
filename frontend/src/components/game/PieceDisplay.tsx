'use client';

import { memo } from 'react';
import type { Piece } from '@/types';

export interface PieceDisplayProps {
  /** The piece to display */
  piece: Piece;
  /** Size of each cell in pixels */
  cellSize?: number;
  /** Size of each cell on mobile devices (for touch-friendly targets) */
  mobileCellSize?: number;
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
  mobileCellSize,
  isSelected = false,
  isDisabled = false,
  className = '',
}: PieceDisplayProps) {
  const { shape, color, type } = piece;
  const rows = shape.length;
  const cols = shape[0]?.length ?? 0;

  // Use mobileCellSize if provided, otherwise fall back to cellSize
  // This allows components to specify larger touch-friendly sizes for mobile
  const effectiveCellSize = mobileCellSize ?? cellSize;
  
  // Calculate grid dimensions using effective cell size
  const gridWidth = cols * effectiveCellSize + (cols - 1) * 2; // cells + gaps
  const gridHeight = rows * effectiveCellSize + (rows - 1) * 2;

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
      data-mobile-optimized="true"
      {...(mobileCellSize !== undefined && { 'data-mobile-cell-size': String(mobileCellSize) })}
    >
      <div
        className="grid gap-0.5"
        style={{
          gridTemplateColumns: `repeat(${cols}, ${effectiveCellSize}px)`,
          gridTemplateRows: `repeat(${rows}, ${effectiveCellSize}px)`,
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
                width: effectiveCellSize,
                height: effectiveCellSize,
                backgroundColor: cell === 1 ? color : 'transparent',
                boxShadow: cell === 1 
                  ? `
                    inset 3px 3px 6px rgba(255, 255, 255, 0.35),
                    inset -2px -2px 5px rgba(0, 0, 0, 0.5),
                    3px 3px 6px rgba(0, 0, 0, 0.5)
                  `
                  : undefined,
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
