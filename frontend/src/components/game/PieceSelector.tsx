'use client';

import { memo, useCallback } from 'react';
import type { Piece } from '@/types';
import { DraggablePiece } from './DraggablePiece';

export interface PieceSelectorProps {
  /** Array of available pieces (null entries mean piece was used) */
  pieces: (Piece | null)[];
  /** Index of the currently selected piece */
  selectedIndex: number | null;
  /** Callback when a piece is selected */
  onSelect: (index: number | null) => void;
  /** Whether selection is disabled */
  disabled?: boolean;
}

/**
 * Shows the 3 available pieces for the player to select or drag
 * Supports both click-to-select and drag-and-drop interactions
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
      if (!piece) return; // Can't select used pieces
      
      // Toggle selection
      if (selectedIndex === index) {
        onSelect(null);
      } else {
        onSelect(index);
      }
    },
    [disabled, pieces, selectedIndex, onSelect]
  );

  return (
    <div className="flex flex-col gap-3">
      <h2 className="text-sm font-semibold text-gray-400 uppercase tracking-wider">
        Available Pieces
      </h2>
      <div className="flex gap-4 justify-center items-center flex-wrap">
        {pieces.map((piece, index) => {
          const isUsed = piece === null;
          const isSelected = selectedIndex === index;

          if (isUsed) {
            // Show placeholder for used piece
            return (
              <div
                key={index}
                className="p-3 rounded-lg bg-gray-800/50 border border-gray-700 opacity-30"
                aria-label={`Piece ${index + 1} (used)`}
              >
                <div
                  className="flex items-center justify-center text-gray-600"
                  style={{ width: 60, height: 60 }}
                >
                  <span className="text-2xl">✓</span>
                </div>
              </div>
            );
          }

          return (
            <DraggablePiece
              key={index}
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
        <p className="text-sm text-center text-blue-400">
          Click on the board or drag the {pieces[selectedIndex]?.type} piece to place it
        </p>
      )}
      {selectedIndex === null && !disabled && (
        <p className="text-sm text-center text-gray-500">
          Click to select or drag pieces directly onto the board
        </p>
      )}
    </div>
  );
}

export const PieceSelector = memo(PieceSelectorComponent);
PieceSelector.displayName = 'PieceSelector';
