'use client';

import { memo, useMemo } from 'react';
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
  /** Click handler for the cell */
  onClick?: (row: number, col: number) => void;
}

/**
 * Generate sparkle particles for clearing animation
 */
function ClearingSparkles({ delay, color }: { delay: number; color: string }) {
  const sparkles = useMemo(() => {
    return Array.from({ length: 5 }, (_, i) => ({
      id: i,
      x: (Math.random() - 0.5) * 30,
      y: -Math.random() * 40 - 10,
      scale: 0.5 + Math.random() * 0.5,
      delay: delay + i * 0.05,
    }));
  }, [delay]);

  return (
    <>
      {sparkles.map((sparkle) => (
        <motion.div
          key={sparkle.id}
          className="absolute w-1 h-1 rounded-full pointer-events-none"
          style={{
            backgroundColor: color || 'white',
            left: '50%',
            top: '50%',
            boxShadow: `0 0 4px ${color || 'white'}, 0 0 8px ${color || 'white'}`,
          }}
          initial={{ scale: 0, x: 0, y: 0, opacity: 0 }}
          animate={{
            scale: [0, sparkle.scale * 2, 0],
            x: sparkle.x,
            y: sparkle.y,
            opacity: [0, 1, 0],
          }}
          transition={{
            duration: 0.6,
            delay: sparkle.delay,
            ease: 'easeOut',
          }}
        />
      ))}
    </>
  );
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
      aria-label={`Cell ${row + 1}, ${col + 1}${isEmpty && !isClearing ? ' (empty)' : ' (filled)'}`}
      className={className}
      style={{
        backgroundColor: !isEmpty ? displayColor : undefined,
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
      {/* Clearing animation effects */}
      <AnimatePresence>
        {isClearing && (
          <>
            {/* Glow ring effect */}
            <motion.div
              className="absolute inset-0 rounded-sm pointer-events-none"
              style={{
                backgroundColor: displayColor || 'white',
              }}
              initial={{ scale: 1, opacity: 1 }}
              animate={{
                scale: [1, 1.5, 2],
                opacity: [0.8, 0.4, 0],
              }}
              transition={{
                duration: 0.5,
                delay: clearingDelay,
                ease: 'easeOut',
              }}
            />
            
            {/* Inner flash */}
            <motion.div
              className="absolute inset-0 rounded-sm pointer-events-none"
              initial={{ opacity: 0 }}
              animate={{
                opacity: [0, 1, 0],
                background: [
                  `radial-gradient(circle, white 0%, ${displayColor || 'white'} 100%)`,
                  `radial-gradient(circle, white 0%, transparent 100%)`,
                  'transparent',
                ],
              }}
              transition={{
                duration: 0.4,
                delay: clearingDelay + 0.1,
                ease: 'easeOut',
              }}
            />
            
            {/* Sparkle particles */}
            <ClearingSparkles delay={clearingDelay} color={displayColor || 'white'} />
          </>
        )}
      </AnimatePresence>
    </motion.div>
  );
}

export const AnimatedCell = memo(AnimatedCellComponent);
AnimatedCell.displayName = 'AnimatedCell';
