'use client';

import { memo, useEffect, useRef, useState } from 'react';
import { motion, AnimatePresence, useMotionValue, animate } from 'motion/react';
import Link from 'next/link';
import Image from 'next/image';
import { modalVariants, backdropVariants, buttonVariants, staggerContainerVariants, staggerItemVariants } from '@/lib/animations';
import type { GameEndGamification } from '@/types/gamification';

export interface LeaderboardResult {
  success: boolean;
  isNewHighScore: boolean;
  newRank?: number;
  errorMessage?: string;
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
  /** Gamification data from game end */
  gamification?: GameEndGamification | null;
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
  gamification,
  onPlayAgain,
  onClose,
}: GameOverModalProps) {
  const modalRef = useRef<HTMLDivElement>(null);
  const playAgainRef = useRef<HTMLButtonElement>(null);
  
  const animatedScore = useCountUp(score, isOpen, 1.5);
  const animatedLines = useCountUp(linesCleared, isOpen, 1);

  // Gamification helpers
  const hasAchievements = gamification?.newAchievements && gamification.newAchievements.length > 0;
  const hasChallengeProgress = gamification?.challengeUpdates && gamification.challengeUpdates.length > 0;
  const hasGoalProgress = gamification?.goalUpdates && gamification.goalUpdates.some(g => g.justCompleted);
  const streak = gamification?.streak;
  const seasonProgress = gamification?.seasonProgress;

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
            className="relative rounded-2xl shadow-2xl p-6 md:p-8 max-w-md w-full overflow-hidden max-h-[90vh] overflow-y-auto"
            style={{ backgroundColor: '#0d243d', borderWidth: 1, borderStyle: 'solid', borderColor: 'rgba(20, 184, 166, 0.3)' }}
          >
            {/* Animated background gradient */}
            <motion.div
              className="absolute inset-0 opacity-20"
              animate={{
                background: [
                  'radial-gradient(circle at 20% 20%, #14b8a6 0%, transparent 50%)',
                  'radial-gradient(circle at 80% 80%, #0ea5e9 0%, transparent 50%)',
                  'radial-gradient(circle at 20% 80%, #22d3ee 0%, transparent 50%)',
                  'radial-gradient(circle at 80% 20%, #14b8a6 0%, transparent 50%)',
                  'radial-gradient(circle at 20% 20%, #14b8a6 0%, transparent 50%)',
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
                    <div className="flex items-center justify-center gap-2" style={{ color: '#2dd4bf' }}>
                      <div className="w-4 h-4 border-2 rounded-full animate-spin" style={{ borderColor: '#2dd4bf', borderTopColor: 'transparent' }} />
                      <span className="text-sm">Submitting score...</span>
                    </div>
                  </motion.div>
                )}


                {/* Basic Stats: Games Played & High Score */}
                {gamification && (
                  <motion.div
                    variants={staggerItemVariants}
                    className="grid grid-cols-2 gap-4 pt-2 border-t border-gray-700/50"
                  >
                    <div className="text-center">
                      <p className="text-xs font-medium text-gray-400 uppercase tracking-wider">High Score</p>
                      <p className="text-lg font-semibold text-white">{gamification.highScore.toLocaleString()}</p>
                    </div>
                    <div className="text-center">
                      <p className="text-xs font-medium text-gray-400 uppercase tracking-wider">Games Played</p>
                      <p className="text-lg font-semibold text-white">{gamification.gamesPlayed.toLocaleString()}</p>
                    </div>
                  </motion.div>
                )}

                {/* Gamification Updates */}
                {gamification && (
                  <motion.div variants={staggerItemVariants} className="space-y-4 pt-4 border-t border-gray-700/50">
                    
                    {/* XP Gained */}
                    {seasonProgress && seasonProgress.xpEarned > 0 && (
                      <div className="bg-white/5 rounded-lg p-3 flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <span className="text-yellow-400 font-bold">XP</span>
                          <span className="text-sm text-gray-300">Season Progress</span>
                        </div>
                        <div className="flex items-center gap-2">
                          <span className="text-yellow-400 font-bold">+{seasonProgress.xpEarned} XP</span>
                          {seasonProgress.tierUp && (
                            <span className="px-2 py-0.5 rounded bg-purple-500/20 text-purple-400 text-xs font-bold border border-purple-500/30">
                              TIER UP!
                            </span>
                          )}
                        </div>
                      </div>
                    )}

                    {/* Streak */}
                    {streak && (
                      <div className="bg-white/5 rounded-lg p-3 flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <span className="text-2xl">🔥</span>
                          <span className="text-sm text-gray-300">Daily Streak</span>
                        </div>
                        <div className="text-right">
                          <span className="text-white font-bold text-lg">{streak.currentStreak}</span>
                          <span className="text-gray-400 text-xs ml-1">days</span>
                        </div>
                      </div>
                    )}

                    {/* New Achievements */}
                    {hasAchievements && (
                      <div className="bg-yellow-500/10 rounded-lg p-3 border border-yellow-500/20">
                        <h4 className="text-xs font-bold text-yellow-400 uppercase mb-2">Achievement Unlocked!</h4>
                        <div className="space-y-2">
                          {gamification.newAchievements.map(ach => (
                            <div key={ach.achievementId} className="flex items-center gap-3">
                              {ach.iconUrl ? (
                                <Image src={ach.iconUrl} alt="icon" width={24} height={24} className="w-6 h-6" />
                              ) : (
                                <div className="w-6 h-6 rounded-full bg-yellow-400/20 flex items-center justify-center text-yellow-400">🏆</div>
                              )}
                              <div className="text-left">
                                <p className="text-sm font-bold text-white leading-tight">{ach.name}</p>
                                <p className="text-xs text-gray-400 leading-tight">{ach.description}</p>
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}

                    {/* Challenge Progress */}
                    {hasChallengeProgress && (
                      <div className="bg-blue-500/10 rounded-lg p-3 border border-blue-500/20">
                        <h4 className="text-xs font-bold text-blue-400 uppercase mb-2">Challenge Progress</h4>
                        <div className="space-y-2">
                          {gamification.challengeUpdates.map(ch => (
                            <div key={ch.challengeId} className="flex items-center justify-between">
                              <span className="text-sm text-gray-200 truncate pr-2 max-w-[150px]">{ch.challengeName}</span>
                              <div className="flex items-center gap-2 text-xs">
                                {ch.justCompleted ? (
                                  <span className="text-green-400 font-bold">COMPLETED!</span>
                                ) : (
                                  <span className="text-blue-300">+{ch.newProgress - (ch.newProgress ?? 0)} progress</span>
                                )}
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}
                  </motion.div>
                )}

                {leaderboardResult && !isSubmitting && (
                  <motion.div
                    variants={staggerItemVariants}
                    initial={{ opacity: 0, scale: 0.8 }}
                    animate={{ opacity: 1, scale: 1 }}
                    className="text-center space-y-2"
                  >
                    {leaderboardResult.success === false && leaderboardResult.errorMessage && (
                      <motion.div
                        className="p-3 rounded-lg bg-red-500/10 border border-red-500/30"
                        initial={{ y: 10, opacity: 0 }}
                        animate={{ y: 0, opacity: 1 }}
                      >
                        <p className="text-sm text-red-400">
                          {leaderboardResult.errorMessage}
                        </p>
                      </motion.div>
                    )}

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
                        className="font-semibold"
                        style={{ color: '#2dd4bf' }}
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
                  className="w-full py-3 px-6 text-white font-semibold rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2"
                  style={{ background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)' }}
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
                    className="flex items-center justify-center gap-2 w-full py-3 px-6 text-white font-medium rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2"
                    style={{ background: 'linear-gradient(135deg, #0ea5e9, #22d3ee)' }}
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
                    className="w-full py-3 px-6 font-medium rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2"
                    style={{ backgroundColor: 'rgba(13, 36, 61, 0.8)', color: '#94a3b8' }}
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
