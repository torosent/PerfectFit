'use client';

import { memo, useCallback } from 'react';
import type { Piece } from '@/types';
import { DraggablePiece } from './DraggablePiece';

export interface PieceSelectorProps {
  /** Array of available pieces */
  pieces: Piece[];
  /** Index of the currently selected piece */
  selectedIndex: number | null;
  /** Callback when a piece is selected */
  onSelect: (index: number | null) => void;
  /** Whether selection is disabled */
  disabled?: boolean;
}

/**
 * Shows the available pieces for the player to select or drag.
 * Players must place all 3 pieces before new ones are given.
 * Supports both click-to-select and drag-and-drop interactions.
 */
function PieceSelectorComponent({
  pieces,
  selectedIndex,
  onSelect,
  disabled = false,
}: PieceSelectorProps) {
  const handlePieceClick = useCallback(
    (index: number) => {
      if (disabled) return;
      
      const piece = pieces[index];
      if (!piece) return; // Can't select non-existent pieces
      
      // Toggle selection
      if (selectedIndex === index) {
        onSelect(null);
      } else {
        onSelect(index);
      }
    },
    [disabled, pieces, selectedIndex, onSelect]
  );

  // Calculate how many pieces have been used this turn (always show 3 slots)
  const totalSlots = 3;
  const usedCount = totalSlots - pieces.length;
  const piecesPlaced = usedCount;
  const piecesRemaining = pieces.length;

  return (
    <div className="flex flex-col gap-3" data-touch-optimized="true">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-semibold text-gray-400 uppercase tracking-wider">
          Available Pieces
        </h2>
        <span className="text-xs text-gray-500">
          {piecesPlaced}/3 placed this turn
        </span>
      </div>
      <div 
        className="flex gap-4 sm:gap-6 justify-center items-center flex-wrap"
        data-touch-spacing="true"
        data-min-touch-target="true"
      >
        {/* Show used slots first */}
        {Array.from({ length: usedCount }).map((_, index) => (
          <div
            key={`used-${index}`}
            className="p-3 rounded-lg opacity-30"
            style={{ backgroundColor: 'rgba(10, 25, 41, 0.5)', borderWidth: 1, borderStyle: 'solid', borderColor: 'rgba(20, 184, 166, 0.2)' }}
            aria-label={`Piece ${index + 1} (used)`}
          >
            <div
              className="flex items-center justify-center text-gray-600"
              style={{ width: 60, height: 60 }}
            >
              <span className="text-2xl">✓</span>
            </div>
          </div>
        ))}
        
        {/* Show remaining pieces */}
        {pieces.map((piece, index) => {
          const isSelected = selectedIndex === index;

          return (
            <DraggablePiece
              key={`piece-${index}`}
              piece={piece}
              index={index}
              isSelected={isSelected}
              disabled={disabled}
              onClick={() => handlePieceClick(index)}
            />
          );
        })}
      </div>
      {selectedIndex !== null && pieces[selectedIndex] && (
        <p className="text-sm text-center" style={{ color: '#2dd4bf' }}>
          Click on the board or drag the {pieces[selectedIndex]?.type} piece to place it
        </p>
      )}
      {selectedIndex === null && !disabled && piecesRemaining > 0 && (
        <p className="text-sm text-center text-gray-500">
          {piecesRemaining === 3 
            ? 'Place all 3 pieces to complete your turn'
            : `${piecesRemaining} piece${piecesRemaining === 1 ? '' : 's'} left to place`}
        </p>
      )}
    </div>
  );
}

export const PieceSelector = memo(PieceSelectorComponent);
PieceSelector.displayName = 'PieceSelector';
