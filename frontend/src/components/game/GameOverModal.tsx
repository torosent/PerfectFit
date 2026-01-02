'use client';

import { memo, useEffect, useRef, useState } from 'react';
import { motion, AnimatePresence, useMotionValue, animate } from 'motion/react';
import Link from 'next/link';
import { modalVariants, backdropVariants, buttonVariants, staggerContainerVariants, staggerItemVariants } from '@/lib/animations';

export interface LeaderboardResult {
  success: boolean;
  isNewHighScore: boolean;
  newRank?: number;
}

export interface GameOverModalProps {
  /** Whether the modal is open */
  isOpen: boolean;
  /** Final score */
  score: number;
  /** Total lines cleared */
  linesCleared?: number;
  /** Leaderboard submission result */
  leaderboardResult?: LeaderboardResult | null;
  /** Whether score is being submitted */
  isSubmitting?: boolean;
  /** Callback when "Play Again" is clicked */
  onPlayAgain: () => void;
  /** Callback when modal is closed (optional) */
  onClose?: () => void;
}

/**
 * Animated counter for score reveal
 */
function useCountUp(target: number, isActive: boolean, duration: number = 1.5) {
  const motionValue = useMotionValue(0);
  const [value, setValue] = useState(0);
  const [prevTarget, setPrevTarget] = useState(target);
  const [prevActive, setPrevActive] = useState(isActive);

  // Reset when becoming inactive
  if (!isActive && prevActive) {
    motionValue.set(0);
    setValue(0);
    setPrevActive(isActive);
  } else if (isActive !== prevActive) {
    setPrevActive(isActive);
  }

  if (target !== prevTarget) {
    setPrevTarget(target);
  }

  useEffect(() => {
    if (!isActive) {
      return;
    }

    const controls = animate(motionValue, target, {
      duration,
      ease: 'easeOut',
      onUpdate: (latest) => setValue(Math.round(latest)),
    });

    return () => controls.stop();
  }, [target, isActive, duration, motionValue]);

  return value;
}

/**
 * Modal displayed when the game ends
 * Shows final score and option to play again with animations
 */
function GameOverModalComponent({
  isOpen,
  score,
  linesCleared = 0,
  leaderboardResult,
  isSubmitting = false,
  onPlayAgain,
  onClose,
}: GameOverModalProps) {
  const modalRef = useRef<HTMLDivElement>(null);
  const playAgainRef = useRef<HTMLButtonElement>(null);
  
  const animatedScore = useCountUp(score, isOpen, 1.5);
  const animatedLines = useCountUp(linesCleared, isOpen, 1);

  // Focus trap and escape key handling
  useEffect(() => {
    if (!isOpen) return;

    // Focus the play again button when modal opens
    const timer = setTimeout(() => {
      playAgainRef.current?.focus();
    }, 300);

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
    return () => {
      clearTimeout(timer);
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen, onClose]);

  return (
    <AnimatePresence>
      {isOpen && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center p-4"
          role="dialog"
          aria-modal="true"
          aria-labelledby="game-over-title"
        >
          {/* Backdrop */}
          <motion.div
            variants={backdropVariants}
            initial="hidden"
            animate="visible"
            exit="exit"
            className="absolute inset-0 bg-black/70 backdrop-blur-sm"
            onClick={onClose}
            aria-hidden="true"
          />

          {/* Modal Content */}
          <motion.div
            ref={modalRef}
            variants={modalVariants}
            initial="hidden"
            animate="visible"
            exit="exit"
            className="relative bg-gray-800 rounded-2xl shadow-2xl border border-gray-700 p-8 max-w-sm w-full overflow-hidden"
          >
            {/* Animated background gradient */}
            <motion.div
              className="absolute inset-0 opacity-20"
              animate={{
                background: [
                  'radial-gradient(circle at 20% 20%, #3b82f6 0%, transparent 50%)',
                  'radial-gradient(circle at 80% 80%, #8b5cf6 0%, transparent 50%)',
                  'radial-gradient(circle at 20% 80%, #3b82f6 0%, transparent 50%)',
                  'radial-gradient(circle at 80% 20%, #8b5cf6 0%, transparent 50%)',
                  'radial-gradient(circle at 20% 20%, #3b82f6 0%, transparent 50%)',
                ],
              }}
              transition={{
                duration: 8,
                repeat: Infinity,
                ease: 'linear',
              }}
            />

            <motion.div
              variants={staggerContainerVariants}
              initial="hidden"
              animate="visible"
              className="relative z-10"
            >
              {/* Game Over Title */}
              <motion.h2
                variants={staggerItemVariants}
                id="game-over-title"
                className="text-3xl font-bold text-center text-white mb-6"
              >
                Game Over!
              </motion.h2>

              {/* Stats */}
              <div className="space-y-4 mb-8">
                {/* Final Score */}
                <motion.div 
                  variants={staggerItemVariants}
                  className="text-center"
                >
                  <p className="text-sm font-medium text-gray-400 uppercase tracking-wider">
                    Final Score
                  </p>
                  <motion.p
                    className="text-5xl font-bold text-yellow-400 tabular-nums"
                    animate={{
                      textShadow: [
                        '0 0 10px rgba(251, 191, 36, 0.3)',
                        '0 0 20px rgba(251, 191, 36, 0.6)',
                        '0 0 10px rgba(251, 191, 36, 0.3)',
                      ],
                    }}
                    transition={{
                      duration: 2,
                      repeat: Infinity,
                      ease: 'easeInOut',
                    }}
                  >
                    {animatedScore.toLocaleString()}
                  </motion.p>
                </motion.div>

                {/* Lines Cleared */}
                <motion.div 
                  variants={staggerItemVariants}
                  className="text-center"
                >
                  <p className="text-sm font-medium text-gray-400 uppercase tracking-wider">
                    Lines Cleared
                  </p>
                  <p className="text-2xl font-semibold text-gray-300 tabular-nums">
                    {animatedLines}
                  </p>
                </motion.div>

                {/* Leaderboard Result */}
                {isSubmitting && (
                  <motion.div
                    variants={staggerItemVariants}
                    className="text-center"
                  >
                    <div className="flex items-center justify-center gap-2 text-blue-400">
                      <div className="w-4 h-4 border-2 border-blue-400 border-t-transparent rounded-full animate-spin" />
                      <span className="text-sm">Submitting score...</span>
                    </div>
                  </motion.div>
                )}

                {leaderboardResult && !isSubmitting && (
                  <motion.div
                    variants={staggerItemVariants}
                    initial={{ opacity: 0, scale: 0.8 }}
                    animate={{ opacity: 1, scale: 1 }}
                    className="text-center space-y-2"
                  >
                    {leaderboardResult.isNewHighScore && (
                      <motion.div
                        className="flex items-center justify-center gap-2"
                        animate={{
                          scale: [1, 1.05, 1],
                        }}
                        transition={{
                          duration: 1,
                          repeat: Infinity,
                          ease: 'easeInOut',
                        }}
                      >
                        <span className="text-2xl">🎉</span>
                        <span className="text-lg font-bold text-green-400">
                          New High Score!
                        </span>
                        <span className="text-2xl">🎉</span>
                      </motion.div>
                    )}
                    
                    {leaderboardResult.newRank && (
                      <motion.p
                        className="text-purple-400 font-semibold"
                        initial={{ y: 10, opacity: 0 }}
                        animate={{ y: 0, opacity: 1 }}
                        transition={{ delay: 0.2 }}
                      >
                        {leaderboardResult.newRank <= 3 ? (
                          <>
                            {leaderboardResult.newRank === 1 && '🥇'}
                            {leaderboardResult.newRank === 2 && '🥈'}
                            {leaderboardResult.newRank === 3 && '🥉'}
                            {' '}You ranked #{leaderboardResult.newRank}!
                          </>
                        ) : (
                          <>You ranked #{leaderboardResult.newRank}!</>
                        )}
                      </motion.p>
                    )}
                  </motion.div>
                )}
              </div>

              {/* Actions */}
              <motion.div 
                variants={staggerItemVariants}
                className="flex flex-col gap-3"
              >
                <motion.button
                  ref={playAgainRef}
                  variants={buttonVariants}
                  initial="idle"
                  whileHover="hover"
                  whileTap="tap"
                  onClick={onPlayAgain}
                  className="w-full py-3 px-6 bg-blue-600 hover:bg-blue-500 text-white font-semibold rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 focus:ring-offset-gray-800"
                >
                  Play Again
                </motion.button>

                <Link
                  href="/leaderboard"
                  className="w-full"
                >
                  <motion.span
                    variants={buttonVariants}
                    initial="idle"
                    whileHover="hover"
                    whileTap="tap"
                    className="flex items-center justify-center gap-2 w-full py-3 px-6 bg-purple-600 hover:bg-purple-500 text-white font-medium rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-purple-500 focus:ring-offset-2 focus:ring-offset-gray-800"
                  >
                    <span>🏆</span>
                    View Leaderboard
                  </motion.span>
                </Link>

                {onClose && (
                  <motion.button
                    variants={buttonVariants}
                    initial="idle"
                    whileHover="hover"
                    whileTap="tap"
                    onClick={onClose}
                    className="w-full py-3 px-6 bg-gray-700 hover:bg-gray-600 text-gray-300 font-medium rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 focus:ring-offset-gray-800"
                  >
                    Close
                  </motion.button>
                )}
              </motion.div>
            </motion.div>
          </motion.div>
        </div>
      )}
    </AnimatePresence>
  );
}

export const GameOverModal = memo(GameOverModalComponent);
GameOverModal.displayName = 'GameOverModal';
