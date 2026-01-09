'use client';

import { memo } from 'react';
import { motion, AnimatePresence } from 'motion/react';
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
  /** The original color of the cell being cleared (for animation) */
  clearingColor?: string;
  /** Whether this cell was recently placed */
  isRecentlyPlaced?: boolean;
  /** Index within the placed piece (for stagger animation) */
  placedIndex?: number;
  /** Whether this cell is in a row/column that would be cleared (preview) */
  isPendingClear?: boolean;
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
  clearingColor,
  isRecentlyPlaced = false,
  placedIndex = 0,
  isPendingClear = false,
  onClick,
}: AnimatedCellProps) {
  // Use the clearing color during animation, otherwise use the current value
  const displayColor = isClearing ? clearingColor : value;
  const isEmpty = displayColor === null || displayColor === undefined;
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
  const baseClasses = 'aspect-square rounded-sm border relative overflow-visible';
  
  const stateClasses = isEmpty && !isClearing
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

  const clearingClasses = isClearing
    ? 'z-10'
    : '';

  const className = `${baseClasses} ${stateClasses} ${highlightClasses} ${interactiveClasses} ${clearingClasses}`;

  // Animation delay for staggered effects - wave sweeps across the line
  const clearingDelay = isClearing ? getClearingDelay(row, col) : 0;
  const placedDelay = isRecentlyPlaced ? getPlacedDelay(placedIndex) : 0;

  // Generate 3D effect box-shadow for filled cells
  const get3DBoxShadow = (color: string | null | undefined) => {
    if (!color) return undefined;
    return `
      inset 3px 3px 6px rgba(255, 255, 255, 0.35),
      inset -2px -2px 5px rgba(0, 0, 0, 0.5),
      3px 3px 6px rgba(0, 0, 0, 0.5)
    `;
  };

  return (
    <motion.div
      variants={cellVariants}
      initial={isEmpty ? 'empty' : 'filled'}
      animate={{
        ...(getAnimationState() === 'clearing' ? { scale: [1, 0.8, 0] } : {}),
        ...(getAnimationState() === 'placed' ? { scale: [0.8, 1.1, 1] } : {}),
        opacity: isPendingClear ? 0.35 : 1,
      }}
      transition={{
        delay: clearingDelay || placedDelay,
        opacity: { duration: 0.15 },
      }}
      role={isClickable ? 'button' : undefined}
      tabIndex={isClickable ? 0 : undefined}
      aria-label={`Cell ${row + 1}, ${col + 1}${isEmpty && !isClearing ? ' (empty)' : ' (filled)'}`}
      className={className}
      style={{
        backgroundColor: !isEmpty ? displayColor : undefined,
        boxShadow: !isEmpty && !isClearing && !isPendingClear ? get3DBoxShadow(displayColor) : undefined,
        // CSS custom property for clearing animation
        '--cell-color': displayColor || 'transparent',
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
    >
      {/* Clearing animation effects - gradient flash with glow and 3D */}
      <AnimatePresence>
        {isClearing && (
          <motion.div
            className="absolute inset-0 rounded-sm pointer-events-none"
            style={{
              background: `linear-gradient(135deg, 
                rgba(255,255,255,0.5) 0%, 
                ${displayColor || 'white'} 20%, 
                ${displayColor || 'white'} 80%, 
                rgba(0,0,0,0.35) 100%)`,
              boxShadow: `
                0 0 16px ${displayColor || 'white'}, 
                0 0 32px ${displayColor || 'white'},
                inset 3px 3px 6px rgba(255, 255, 255, 0.5),
                inset -3px -3px 6px rgba(0, 0, 0, 0.4),
                4px 4px 8px rgba(0, 0, 0, 0.6)
              `,
            }}
            initial={{ scale: 1, opacity: 1 }}
            animate={{
              scale: [1, 1.1, 1],
              opacity: [1, 1, 0],
            }}
            transition={{
              duration: 0.5,
              delay: clearingDelay,
              ease: 'easeOut',
              times: [0, 0.4, 1],
            }}
          />
        )}
      </AnimatePresence>
    </motion.div>
  );
}

export const AnimatedCell = memo(AnimatedCellComponent);
AnimatedCell.displayName = 'AnimatedCell';
