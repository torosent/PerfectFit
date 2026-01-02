'use client';

import { memo, useEffect, useRef } from 'react';

export interface GameOverModalProps {
  /** Whether the modal is open */
  isOpen: boolean;
  /** Final score */
  score: number;
  /** Total lines cleared */
  linesCleared?: number;
  /** Callback when "Play Again" is clicked */
  onPlayAgain: () => void;
  /** Callback when modal is closed (optional) */
  onClose?: () => void;
}

/**
 * Modal displayed when the game ends
 * Shows final score and option to play again
 */
function GameOverModalComponent({
  isOpen,
  score,
  linesCleared = 0,
  onPlayAgain,
  onClose,
}: GameOverModalProps) {
  const modalRef = useRef<HTMLDivElement>(null);
  const playAgainRef = useRef<HTMLButtonElement>(null);

  // Focus trap and escape key handling
  useEffect(() => {
    if (!isOpen) return;

    // Focus the play again button when modal opens
    playAgainRef.current?.focus();

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && onClose) {
        onClose();
      }
      
      // Trap focus within modal
      if (e.key === 'Tab') {
        const focusableElements = modalRef.current?.querySelectorAll(
          'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        
        if (!focusableElements?.length) return;
        
        const firstElement = focusableElements[0] as HTMLElement;
        const lastElement = focusableElements[focusableElements.length - 1] as HTMLElement;

        if (e.shiftKey && document.activeElement === firstElement) {
          e.preventDefault();
          lastElement.focus();
        } else if (!e.shiftKey && document.activeElement === lastElement) {
          e.preventDefault();
          firstElement.focus();
        }
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
      aria-labelledby="game-over-title"
    >
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-black/70 backdrop-blur-sm"
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Modal Content */}
      <div
        ref={modalRef}
        className="relative bg-gray-800 rounded-2xl shadow-2xl border border-gray-700 p-8 max-w-sm w-full animate-in fade-in zoom-in-95 duration-200"
      >
        {/* Game Over Title */}
        <h2
          id="game-over-title"
          className="text-3xl font-bold text-center text-white mb-6"
        >
          Game Over!
        </h2>

        {/* Stats */}
        <div className="space-y-4 mb-8">
          {/* Final Score */}
          <div className="text-center">
            <p className="text-sm font-medium text-gray-400 uppercase tracking-wider">
              Final Score
            </p>
            <p className="text-5xl font-bold text-yellow-400 tabular-nums">
              {score.toLocaleString()}
            </p>
          </div>

          {/* Lines Cleared */}
          <div className="text-center">
            <p className="text-sm font-medium text-gray-400 uppercase tracking-wider">
              Lines Cleared
            </p>
            <p className="text-2xl font-semibold text-gray-300 tabular-nums">
              {linesCleared}
            </p>
          </div>
        </div>

        {/* Actions */}
        <div className="flex flex-col gap-3">
          <button
            ref={playAgainRef}
            onClick={onPlayAgain}
            className="w-full py-3 px-6 bg-blue-600 hover:bg-blue-500 text-white font-semibold rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 focus:ring-offset-gray-800"
          >
            Play Again
          </button>

          {onClose && (
            <button
              onClick={onClose}
              className="w-full py-3 px-6 bg-gray-700 hover:bg-gray-600 text-gray-300 font-medium rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 focus:ring-offset-gray-800"
            >
              Close
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

export const GameOverModal = memo(GameOverModalComponent);
GameOverModal.displayName = 'GameOverModal';
