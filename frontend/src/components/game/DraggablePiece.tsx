'use client';

import { memo, useMemo, useState, useEffect } from 'react';
import { useDraggable } from '@dnd-kit/core';
import { CSS } from '@dnd-kit/utilities';
import { motion } from 'motion/react';
import type { Piece } from '@/types';
import { PieceDisplay } from './PieceDisplay';
import { pieceVariants } from '@/lib/animations';

export interface DraggablePieceProps {
  /** The piece to display and drag */
  piece: Piece;
  /** Index of this piece in the current pieces array */
  index: number;
  /** Whether this piece is currently selected (via click) */
  isSelected?: boolean;
  /** Whether dragging is disabled */
  disabled?: boolean;
  /** Click handler for fallback selection */
  onClick?: () => void;
}

/**
 * Wrapper component that makes a piece draggable
 * Uses @dnd-kit's useDraggable hook with motion animations
 * Includes idle bobbing animation that's staggered between pieces
 */
function DraggablePieceComponent({
  piece,
  index,
  isSelected = false,
  disabled = false,
  onClick,
}: DraggablePieceProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    isDragging,
  } = useDraggable({
    id: `piece-${index}`,
    data: {
      piece,
      pieceIndex: index,
    },
    disabled,
  });

  // Track prefers-reduced-motion on client side
  const [prefersReducedMotion, setPrefersReducedMotion] = useState(false);
  
  useEffect(() => {
    const mediaQuery = window.matchMedia?.('(prefers-reduced-motion: reduce)');
    setPrefersReducedMotion(mediaQuery?.matches ?? false);
    
    const handler = (e: MediaQueryListEvent) => setPrefersReducedMotion(e.matches);
    mediaQuery?.addEventListener?.('change', handler);
    return () => mediaQuery?.removeEventListener?.('change', handler);
  }, []);

  // Apply transform during drag merged with base styles
  const baseStyles = {
    backgroundColor: isSelected ? 'rgba(20, 184, 166, 0.15)' : 'rgba(10, 25, 41, 0.5)',
    borderWidth: 1,
    borderStyle: 'solid' as const,
    borderColor: isSelected ? 'rgba(20, 184, 166, 0.5)' : 'rgba(20, 184, 166, 0.2)',
  };

  const style = transform
    ? {
        ...baseStyles,
        transform: CSS.Translate.toString(transform),
      }
    : baseStyles;

  // Determine animation state - return variant name
  const getAnimationState = (): string => {
    if (isDragging) return 'dragging';
    if (isSelected) return 'selected';
    return 'idle';
  };

  // Should we apply bobbing? Only when idle and not reduced motion
  const shouldBob = !isDragging && !isSelected && !disabled && !prefersReducedMotion;

  // Idle bobbing animation - staggered timing based on index
  const bobbingAnimation = useMemo(() => {
    if (!shouldBob) {
      return undefined;
    }
    
    // Stagger the animation start time based on piece index
    const delay = index * 0.7; // ~0.7 seconds stagger between pieces
    const duration = 2.5; // 2.5 second cycle
    
    return {
      y: [0, -4, 0, -2, 0],
      transition: {
        duration,
        repeat: Infinity,
        ease: 'easeInOut' as const,
        delay,
        times: [0, 0.3, 0.5, 0.7, 1],
      },
    };
  }, [shouldBob, index]);

  // Combine variant state with bobbing animation
  const animateValue = useMemo(() => {
    if (bobbingAnimation) {
      return bobbingAnimation;
    }
    return getAnimationState();
  }, [bobbingAnimation, isDragging, isSelected]);

  return (
    <motion.div
      ref={setNodeRef}
      style={style}
      {...listeners}
      {...attributes}
      onClick={onClick}
      variants={pieceVariants}
      initial="idle"
      animate={animateValue}
      whileHover={!disabled && !isDragging ? 'hover' : undefined}
      whileTap={!disabled ? 'tap' : undefined}
      className={`
        p-4 sm:p-3 rounded-lg min-w-[60px] min-h-[60px]
        ${!disabled ? 'cursor-grab active:cursor-grabbing' : 'cursor-not-allowed'}
        ${isSelected ? 'ring-2 ring-teal-500' : ''}
        focus:outline-none focus:ring-2 focus:ring-teal-500 focus:ring-offset-2
        touch-none select-none
      `}
      role="button"
      tabIndex={disabled ? -1 : 0}
      aria-label={`Drag ${piece.type} piece to place on board`}
      aria-pressed={isSelected}
      aria-disabled={disabled}
      data-min-touch-target="true"
      data-mobile-padding="true"
    >
      <PieceDisplay
        piece={piece}
        cellSize={20}
        isSelected={isSelected}
        isDisabled={disabled}
      />
    </motion.div>
  );
}

export const DraggablePiece = memo(DraggablePieceComponent);
DraggablePiece.displayName = 'DraggablePiece';
