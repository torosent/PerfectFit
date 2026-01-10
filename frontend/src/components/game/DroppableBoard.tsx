'use client';

import { memo, useMemo, useEffect, useState } from 'react';
import { useDroppable } from '@dnd-kit/core';
import { motion, AnimatePresence } from 'motion/react';
import type { Grid, Piece, Position, ClearingCell } from '@/types';
import { AnimatedCell } from './AnimatedCell';
import { canPlacePiece, getPieceCells, getPotentialLineClear } from '@/lib/game-logic/pieces';

export interface HighlightedCell {
  row: number;
  col: number;
  isValid: boolean;
}

export interface DroppableBoardProps {
  /** The 8x8 game grid */
  grid: Grid;
  /** Position where piece preview should be shown */
  hoverPosition: { row: number; col: number } | null;
  /** The piece being dragged (for preview) */
  draggedPiece: Piece | null;
  /** Additional highlighted cells (from click-to-place flow) */
  highlightedCells?: HighlightedCell[];
  /** Cells currently being cleared (for animation) - includes original colors */
  clearingCells?: ClearingCell[];
  /** Recently placed cells (for animation) */
  lastPlacedCells?: Position[];
  /** Callback when a cell is clicked */
  onCellClick?: (row: number, col: number) => void;
  /** Whether the board is disabled (non-interactive) */
  disabled?: boolean;
  /** Screen shake intensity (0 = none, 1 = small, 2 = medium, 3 = large) */
  shakeIntensity?: number;
  /** Whether danger mode is active (few valid placements left) */
  isDangerMode?: boolean;
}

/**
 * Shockwave effect component for cleared lines
 */
function LineShockwave({ 
  type, 
  index, 
  delay 
}: { 
  type: 'row' | 'column'; 
  index: number; 
  delay: number;
}) {
  const isRow = type === 'row';
  
  return (
    <motion.div
      className="absolute pointer-events-none z-30"
      style={{
        ...(isRow ? {
          left: 0,
          right: 0,
          top: `${index * 12.5}%`,
          height: '12.5%',
        } : {
          top: 0,
          bottom: 0,
          left: `${index * 12.5}%`,
          width: '12.5%',
        }),
      }}
      initial={{ opacity: 0 }}
      animate={{ opacity: [0, 1, 0] }}
      transition={{ duration: 0.5, delay }}
    >
      {/* Sweeping light effect */}
      <motion.div
        className="absolute inset-0"
        style={{
          background: isRow 
            ? 'linear-gradient(90deg, transparent 0%, rgba(255,255,255,0.9) 45%, white 50%, rgba(255,255,255,0.9) 55%, transparent 100%)'
            : 'linear-gradient(180deg, transparent 0%, rgba(255,255,255,0.9) 45%, white 50%, rgba(255,255,255,0.9) 55%, transparent 100%)',
        }}
        initial={{ 
          [isRow ? 'x' : 'y']: '-100%',
          opacity: 0,
        }}
        animate={{ 
          [isRow ? 'x' : 'y']: '200%',
          opacity: [0, 1, 1, 0],
        }}
        transition={{ 
          duration: 0.4, 
          delay,
          ease: 'easeOut',
        }}
      />
      
      {/* Glow trail */}
      <motion.div
        className="absolute inset-0"
        style={{
          background: isRow
            ? 'linear-gradient(90deg, transparent, rgba(59, 130, 246, 0.5), rgba(139, 92, 246, 0.5), rgba(236, 72, 153, 0.5), transparent)'
            : 'linear-gradient(180deg, transparent, rgba(59, 130, 246, 0.5), rgba(139, 92, 246, 0.5), rgba(236, 72, 153, 0.5), transparent)',
          filter: 'blur(4px)',
        }}
        initial={{ 
          [isRow ? 'x' : 'y']: '-100%',
          opacity: 0,
        }}
        animate={{ 
          [isRow ? 'x' : 'y']: '200%',
          opacity: [0, 0.8, 0.8, 0],
        }}
        transition={{ 
          duration: 0.5, 
          delay,
          ease: 'easeOut',
        }}
      />
    </motion.div>
  );
}

/**
 * Screen shake keyframes based on intensity
 */
const shakeKeyframes = {
  0: { x: 0, y: 0 },
  1: { 
    x: [0, -2, 2, -1, 1, 0],
    y: [0, 1, -1, 1, -1, 0],
  },
  2: {
    x: [0, -4, 4, -3, 3, -1, 1, 0],
    y: [0, 2, -2, 2, -2, 1, -1, 0],
  },
  3: {
    x: [0, -6, 6, -5, 5, -3, 3, -1, 1, 0],
    y: [0, 3, -3, 3, -3, 2, -2, 1, -1, 0],
  },
};

/**
 * Droppable game board that accepts piece drops
 * Shows preview of piece at hover position with validity indication
 * Includes animations for cell states, screen shake, and danger mode
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
  shakeIntensity = 0,
  isDangerMode = false,
}: DroppableBoardProps) {
  const { isOver, setNodeRef } = useDroppable({
    id: 'game-board',
  });

  // Track shake animation state
  const [isShaking, setIsShaking] = useState(false);

  // Track reduced motion preference
  const [prefersReducedMotion, setPrefersReducedMotion] = useState(() => {
    if (typeof window === 'undefined' || !window.matchMedia) return false;
    return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  });

  useEffect(() => {
    if (typeof window === 'undefined' || !window.matchMedia) {
      return;
    }

    const mediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)');

    const handleChange = (event: MediaQueryListEvent) => {
      setPrefersReducedMotion(event.matches);
    };

    mediaQuery.addEventListener('change', handleChange);

    return () => {
      mediaQuery.removeEventListener('change', handleChange);
    };
  }, []);
  // Trigger shake when intensity changes and is > 0
  useEffect(() => {
    if (shakeIntensity > 0 && !prefersReducedMotion) {
      // Avoid setState synchronously within effect body.
      const startTimer = setTimeout(() => setIsShaking(true), 0);
      const endTimer = setTimeout(() => setIsShaking(false), 300);
      return () => {
        clearTimeout(startTimer);
        clearTimeout(endTimer);
      };
    }
  }, [shakeIntensity, prefersReducedMotion]);

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
          cell.row < 8 &&
          cell.col >= 0 &&
          cell.col < 8
      )
      .map((cell) => ({
        ...cell,
        isValid,
      }));
  }, [grid, hoverPosition, draggedPiece]);

  // Calculate potential line clears for drag preview
  const potentialLineClear = useMemo(() => {
    if (!hoverPosition || !draggedPiece) {
      return { rows: [], cols: [] };
    }
    return getPotentialLineClear(grid, draggedPiece, hoverPosition.row, hoverPosition.col);
  }, [grid, hoverPosition, draggedPiece]);

  // Calculate which rows and columns are being cleared for shockwave effect
  const clearingLines = useMemo(() => {
    if (clearingCells.length === 0) return { rows: [], columns: [] };
    
    const rowCounts = new Map<number, number>();
    const colCounts = new Map<number, number>();
    
    clearingCells.forEach(({ row, col }) => {
      rowCounts.set(row, (rowCounts.get(row) || 0) + 1);
      colCounts.set(col, (colCounts.get(col) || 0) + 1);
    });
    
    // A complete row/column has 8 cells
    const rows = Array.from(rowCounts.entries())
      .filter(([, count]) => count >= 8)
      .map(([row]) => row);
    const columns = Array.from(colCounts.entries())
      .filter(([, count]) => count >= 8)
      .map(([col]) => col);
    
    return { rows, columns };
  }, [clearingCells]);

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

  // Map for clearing cells - includes the original color for animation
  const clearingMap = useMemo(() => {
    const map = new Map<string, string>();
    clearingCells.forEach(({ row, col, color }) => {
      map.set(`${row}-${col}`, color);
    });
    return map;
  }, [clearingCells]);

  const placedMap = useMemo(() => {
    const map = new Map<string, number>();
    lastPlacedCells.forEach(({ row, col }, index) => {
      map.set(`${row}-${col}`, index);
    });
    return map;
  }, [lastPlacedCells]);

  // Create a set of cells that would be cleared (for fading preview)
  const pendingClearSet = useMemo(() => {
    const set = new Set<string>();
    // Add all cells in rows that would be cleared
    for (const row of potentialLineClear.rows) {
      for (let col = 0; col < 8; col++) {
        set.add(`${row}-${col}`);
      }
    }
    // Add all cells in columns that would be cleared
    for (const col of potentialLineClear.cols) {
      for (let row = 0; row < 8; row++) {
        set.add(`${row}-${col}`);
      }
    }
    return set;
  }, [potentialLineClear]);

  // Get shake animation values
  const shakeAnimation = isShaking && shakeIntensity > 0 
    ? shakeKeyframes[shakeIntensity as keyof typeof shakeKeyframes] 
    : shakeKeyframes[0];

  return (
    <div
      className="overflow-x-auto overflow-y-hidden"
      data-scroll-container="true"
    >
      <motion.div
        ref={setNodeRef}
        className={`
          relative p-2 sm:p-3 rounded-xl shadow-2xl
          transition-all duration-150
          ${isOver ? 'ring-2 ring-teal-500 ring-opacity-50' : ''}
        `}
        style={{ 
          backgroundColor: '#0a1929', 
          borderWidth: isDangerMode ? 2 : 1, 
          borderStyle: 'solid', 
          borderColor: isDangerMode ? 'var(--danger-border, rgba(239, 68, 68, 0.6))' : 'rgba(20, 184, 166, 0.3)',
          boxShadow: isDangerMode ? '0 0 20px var(--danger-glow, rgba(239, 68, 68, 0.3)), 0 0 40px var(--danger-glow, rgba(239, 68, 68, 0.1))' : undefined,
        }}
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ 
          opacity: 1, 
          scale: 1,
          ...shakeAnimation,
        }}
        transition={isShaking ? { duration: 0.3 } : { duration: 0.3 }}
        role="grid"
        aria-label="Game board - drop pieces here"
        data-mobile-optimized="true"
        data-min-cell-size="44"
      >
        {/* Danger mode pulsing border overlay */}
        {isDangerMode && (
          <motion.div
            className="absolute inset-0 rounded-xl pointer-events-none z-40"
            style={{
              borderWidth: 2,
              borderStyle: 'solid',
              borderColor: 'var(--danger-border, rgba(239, 68, 68, 0.6))',
            }}
            animate={{
              opacity: [0.5, 1, 0.5],
              borderColor: [
                'rgba(239, 68, 68, 0.4)',
                'rgba(239, 68, 68, 0.8)',
                'rgba(239, 68, 68, 0.4)',
              ],
            }}
            transition={{
              duration: 1.5,
              repeat: Infinity,
              ease: 'easeInOut',
            }}
          />
        )}

        {/* Line clearing shockwave effects */}
        <AnimatePresence>
          {clearingLines.rows.map((row, i) => (
            <LineShockwave 
              key={`row-${row}`} 
              type="row" 
              index={row} 
              delay={i * 0.1}
            />
          ))}
          {clearingLines.columns.map((col, i) => (
            <LineShockwave 
              key={`col-${col}`} 
              type="column" 
              index={col} 
              delay={clearingLines.rows.length * 0.1 + i * 0.1}
            />
          ))}
        </AnimatePresence>

        <div
          className="grid grid-cols-8 gap-0.5 sm:gap-1 relative z-10"
          style={{
            // Mobile-first: use 44px cells for touch targets, max out at 400px on larger screens
            // 8 cells × 44px = 352px minimum for touch-friendly mobile
            width: 'max(352px, min(90vw, 400px))',
            height: 'max(352px, min(90vw, 400px))',
          }}
        >
          {grid.map((row, rowIndex) =>
            row.map((cellValue, colIndex) => {
              const key = `${rowIndex}-${colIndex}`;
              const highlight = highlightMap.get(key);
              const isHighlighted = highlight !== undefined;
              const isPreview = highlight?.isPreview ?? false;
              const clearingColor = clearingMap.get(key);
              const isClearing = clearingColor !== undefined;
              const placedIndex = placedMap.get(key);
              const isRecentlyPlaced = placedIndex !== undefined;
              const isPendingClear = pendingClearSet.has(key);

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
                    clearingColor={clearingColor}
                    isRecentlyPlaced={isRecentlyPlaced}
                    placedIndex={placedIndex ?? 0}
                    isPendingClear={isPendingClear}
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
    </div>
  );
}

export const DroppableBoard = memo(DroppableBoardComponent);
DroppableBoard.displayName = 'DroppableBoard';
