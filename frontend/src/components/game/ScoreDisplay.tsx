'use client';

import { memo } from 'react';

export interface ScoreDisplayProps {
  /** Current score */
  score: number;
  /** Current combo multiplier */
  combo: number;
  /** Total lines cleared */
  linesCleared?: number;
}

/**
 * Displays the current game score and combo
 */
function ScoreDisplayComponent({ 
  score, 
  combo, 
  linesCleared = 0 
}: ScoreDisplayProps) {
  const hasCombo = combo > 0;

  return (
    <div className="flex flex-col sm:flex-row gap-4 sm:gap-8 items-center justify-center">
      {/* Score */}
      <div className="text-center">
        <p className="text-xs sm:text-sm font-medium text-gray-400 uppercase tracking-wider">
          Score
        </p>
        <p 
          className="text-3xl sm:text-4xl font-bold text-white tabular-nums"
          aria-live="polite"
        >
          {score.toLocaleString()}
        </p>
      </div>

      {/* Combo */}
      <div className="text-center">
        <p className="text-xs sm:text-sm font-medium text-gray-400 uppercase tracking-wider">
          Combo
        </p>
        <p 
          className={`text-2xl sm:text-3xl font-bold tabular-nums transition-colors ${
            hasCombo ? 'text-yellow-400' : 'text-gray-500'
          }`}
          aria-live="polite"
        >
          {hasCombo ? `×${combo}` : '—'}
        </p>
      </div>

      {/* Lines Cleared */}
      <div className="text-center">
        <p className="text-xs sm:text-sm font-medium text-gray-400 uppercase tracking-wider">
          Lines
        </p>
        <p 
          className="text-2xl sm:text-3xl font-bold text-gray-300 tabular-nums"
          aria-live="polite"
        >
          {linesCleared}
        </p>
      </div>
    </div>
  );
}

export const ScoreDisplay = memo(ScoreDisplayComponent);
ScoreDisplay.displayName = 'ScoreDisplay';
