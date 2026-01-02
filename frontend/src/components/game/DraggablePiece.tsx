'use client';

import { memo } from 'react';
import { useDraggable } from '@dnd-kit/core';
import { CSS } from '@dnd-kit/utilities';
import type { Piece } from '@/types';
import { PieceDisplay } from './PieceDisplay';

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
 * Uses @dnd-kit's useDraggable hook
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

  // Apply transform during drag
  const style = transform
    ? {
        transform: CSS.Translate.toString(transform),
      }
    : undefined;

  return (
    <div
      ref={setNodeRef}
      style={style}
      {...listeners}
      {...attributes}
      onClick={onClick}
      className={`
        p-3 rounded-lg transition-all duration-150
        ${!disabled ? 'hover:bg-gray-700/50 cursor-grab active:cursor-grabbing' : 'cursor-not-allowed'}
        ${isSelected ? 'bg-gray-700/70' : 'bg-gray-800/50'}
        ${isDragging ? 'opacity-50 scale-95' : ''}
        border border-gray-700
        focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 focus:ring-offset-gray-900
        touch-none select-none
      `}
      role="button"
      tabIndex={disabled ? -1 : 0}
      aria-label={`Drag ${piece.type} piece to place on board`}
      aria-pressed={isSelected}
      aria-disabled={disabled}
    >
      <PieceDisplay
        piece={piece}
        cellSize={20}
        isSelected={isSelected}
        isDisabled={disabled}
      />
    </div>
  );
}

export const DraggablePiece = memo(DraggablePieceComponent);
DraggablePiece.displayName = 'DraggablePiece';
