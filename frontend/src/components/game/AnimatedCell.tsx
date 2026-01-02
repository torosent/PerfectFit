'use client';

import { memo } from 'react';
import { motion } from 'motion/react';
import type { CellValue } from '@/types';
import { cellVariants, getClearingDelay, getPlacedDelay } from '@/lib/animations';

export interface AnimatedCellProps {
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
  /** Whether this cell is currently being cleared */
  isClearing?: boolean;
  /** Whether this cell was recently placed */
  isRecentlyPlaced?: boolean;
  /** Index within the placed piece (for stagger animation) */
  placedIndex?: number;
  /** Click handler for the cell */
  onClick?: (row: number, col: number) => void;
}

/**
 * Animated cell component for the game board
 * Uses motion for smooth state transitions
 */
function AnimatedCellComponent({
  value,
  row,
  col,
  isHighlighted = false,
  isValidPlacement = true,
  isClearing = false,
  isRecentlyPlaced = false,
  placedIndex = 0,
  onClick,
}: AnimatedCellProps) {
  const isEmpty = value === null;
  const isClickable = onClick !== undefined;

  const handleClick = () => {
    onClick?.(row, col);
  };

  // Determine the animation state
  const getAnimationState = () => {
    if (isClearing) return 'clearing';
    if (isRecentlyPlaced) return 'placed';
    if (!isEmpty) return 'filled';
    return 'empty';
  };

  // Build class names
  const baseClasses = 'aspect-square rounded-sm border';
  
  const stateClasses = isEmpty
    ? 'bg-gray-800/50 border-gray-700/50'
    : 'border-white/20';

  const highlightClasses = isHighlighted
    ? isValidPlacement
      ? 'ring-2 ring-green-400 ring-inset bg-green-400/30'
      : 'ring-2 ring-red-400 ring-inset bg-red-400/30'
    : '';

  const interactiveClasses = isClickable
    ? 'cursor-pointer hover:brightness-110 focus:outline-none focus:ring-2 focus:ring-blue-500'
    : '';

  const clearingClasses = isClearing
    ? 'z-10'
    : '';

  const className = `${baseClasses} ${stateClasses} ${highlightClasses} ${interactiveClasses} ${clearingClasses}`;

  // Animation delay for staggered effects
  const clearingDelay = isClearing ? getClearingDelay(row, col) : 0;
  const placedDelay = isRecentlyPlaced ? getPlacedDelay(placedIndex) : 0;

  return (
    <motion.div
      variants={cellVariants}
      initial={isEmpty ? 'empty' : 'filled'}
      animate={getAnimationState()}
      transition={{
        delay: clearingDelay || placedDelay,
      }}
      role={isClickable ? 'button' : undefined}
      tabIndex={isClickable ? 0 : undefined}
      aria-label={`Cell ${row + 1}, ${col + 1}${isEmpty ? ' (empty)' : ' (filled)'}`}
      className={className}
      style={{
        backgroundColor: !isEmpty && !isClearing ? value : undefined,
        // CSS custom property for clearing animation
        '--cell-color': value || 'transparent',
      } as React.CSSProperties}
      onClick={isClickable ? handleClick : undefined}
      onKeyDown={isClickable ? (e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          onClick?.(row, col);
        }
      } : undefined}
      whileHover={isClickable ? { scale: 1.05 } : undefined}
      whileTap={isClickable ? { scale: 0.95 } : undefined}
    />
  );
}

export const AnimatedCell = memo(AnimatedCellComponent);
AnimatedCell.displayName = 'AnimatedCell';
