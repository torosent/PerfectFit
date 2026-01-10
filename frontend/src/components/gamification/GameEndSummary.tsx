'use client';

import { memo, useMemo, useCallback, useEffect, useRef, useId, type KeyboardEvent } from 'react';
import { motion } from 'motion/react';
import Image from 'next/image';
import type {
  GameEndGamification,
} from '@/types/gamification';

export interface GameEndSummaryProps {
  /** Whether the summary is visible */
  isOpen: boolean;
  /** Gamification data from game end */
  gameEndData: GameEndGamification;
  /** Previous streak before this game */
  previousStreak: number;
  /** Callback when continue is clicked */
  onContinue: () => void;
}

/**
 * Determine streak status based on previous and current
 */
function getStreakStatus(
  currentStreak: number,
  previousStreak: number
): 'increased' | 'maintained' | 'broken' {
  if (currentStreak === 0 && previousStreak > 0) return 'broken';
  if (currentStreak > previousStreak) return 'increased';
  return 'maintained';
}

/**
 * Post-game gamification recap component
 */
function GameEndSummaryComponent({
  isOpen,
  gameEndData,
  previousStreak,
  onContinue,
}: GameEndSummaryProps) {
  const titleId = useId();
  const continueButtonRef = useRef<HTMLButtonElement>(null);
  const { streak, challengeUpdates, newAchievements, seasonProgress, goalUpdates } = gameEndData;
  
  const streakStatus = useMemo(
    () => getStreakStatus(streak.currentStreak, previousStreak),
    [streak.currentStreak, previousStreak]
  );
  
  const hasAchievements = newAchievements.length > 0;
  const hasChallengeProgress = challengeUpdates.length > 0;
  const hasGoalProgress = goalUpdates.some(g => g.justCompleted);
  
  // Focus the continue button when modal opens
  useEffect(() => {
    if (isOpen) {
      // Slight delay to let animation start
      const timer = setTimeout(() => {
        continueButtonRef.current?.focus();
      }, 100);
      return () => clearTimeout(timer);
    }
  }, [isOpen]);
  
  // Handle Escape key to close modal
  const handleKeyDown = useCallback((e: KeyboardEvent<HTMLDivElement>) => {
    if (e.key === 'Escape') {
      e.preventDefault();
      onContinue();
    }
  }, [onContinue]);
  
  if (!isOpen) return null;
  
  return (
    <motion.div
      data-testid="game-end-summary"
      role="dialog"
      aria-modal="true"
      aria-labelledby={titleId}
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/70 backdrop-blur-sm"
      onKeyDown={handleKeyDown}
    >
      <motion.div
        initial={{ scale: 0.9, y: 20 }}
        animate={{ scale: 1, y: 0 }}
        exit={{ scale: 0.9, y: 20 }}
        className="max-w-md w-full max-h-[90vh] overflow-y-auto rounded-2xl p-6"
        style={{
          background: 'linear-gradient(135deg, rgba(13, 36, 61, 0.98), rgba(26, 54, 93, 0.98))',
          boxShadow: '0 0 30px rgba(20, 184, 166, 0.2)',
        }}
      >
        {/* Header */}
        <motion.h2
          id={titleId}
          role="heading"
          aria-level={1}
          initial={{ y: -10, opacity: 0 }}
          animate={{ y: 0, opacity: 1 }}
          className="text-2xl font-bold text-white text-center mb-6"
        >
          🎮 Game Summary
        </motion.h2>
        
        {/* Streak Section */}
        <motion.div
          initial={{ x: -20, opacity: 0 }}
          animate={{ x: 0, opacity: 1 }}
          transition={{ delay: 0.1 }}
          className="mb-6 p-4 rounded-xl bg-white/5"
        >
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <span className="text-3xl">🔥</span>
              <div>
                <p className="text-2xl font-bold text-white">{streak.currentStreak}</p>
                <p className="text-sm text-gray-400">day streak</p>
              </div>
            </div>
            
            {streakStatus === 'increased' && (
              <motion.div
                data-testid="streak-increased"
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                className="px-3 py-1 rounded-full bg-green-500/20 text-green-400 text-sm font-medium"
              >
                +1 🎉
              </motion.div>
            )}
            
            {streakStatus === 'maintained' && (
              <motion.div
                data-testid="streak-maintained"
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                className="px-3 py-1 rounded-full bg-blue-500/20 text-blue-400 text-sm font-medium"
              >
                ✓ Maintained
              </motion.div>
            )}
            
            {streakStatus === 'broken' && (
              <motion.div
                data-testid="streak-broken"
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                className="px-3 py-1 rounded-full bg-red-500/20 text-red-400 text-sm font-medium"
              >
                💔 Reset
              </motion.div>
            )}
          </div>
        </motion.div>
        
        {/* XP Section */}
        <motion.div
          initial={{ x: -20, opacity: 0 }}
          animate={{ x: 0, opacity: 1 }}
          transition={{ delay: 0.2 }}
          className="mb-6 p-4 rounded-xl bg-white/5"
        >
          <div className="flex items-center justify-between mb-2">
            <span className="text-gray-400">XP Earned</span>
            <motion.span
              initial={{ scale: 0.8 }}
              animate={{ scale: 1 }}
              className="text-xl font-bold text-yellow-400"
            >
              +{seasonProgress.xpEarned} XP
            </motion.span>
          </div>
          
          {seasonProgress.tierUp && (
            <motion.div
              data-testid="tier-up"
              initial={{ y: 10, opacity: 0 }}
              animate={{ y: 0, opacity: 1 }}
              transition={{ delay: 0.3 }}
              className="mt-2 flex items-center justify-center gap-2 p-2 rounded-lg bg-purple-500/20 border border-purple-500/30"
            >
              <span className="text-purple-400">⬆️</span>
              <span className="text-purple-400 font-medium">Reached Tier {seasonProgress.newTier}!</span>
            </motion.div>
          )}
        </motion.div>
        
        {/* Achievements Section */}
        {hasAchievements && (
          <motion.div
            data-testid="achievements-section"
            initial={{ x: -20, opacity: 0 }}
            animate={{ x: 0, opacity: 1 }}
            transition={{ delay: 0.3 }}
            className="mb-6 p-4 rounded-xl bg-white/5"
          >
            <h3 className="text-sm font-medium text-gray-400 mb-3">
              🏆 {newAchievements.length} Achievement{newAchievements.length > 1 ? 's' : ''} Unlocked
            </h3>
            
            <div className="space-y-2">
              {newAchievements.map((achievement) => (
                <motion.div
                  key={achievement.achievementId}
                  initial={{ x: -10, opacity: 0 }}
                  animate={{ x: 0, opacity: 1 }}
                  className="flex items-center gap-3 p-2 rounded-lg bg-yellow-500/10"
                >
                  <div className="w-8 h-8 rounded-full bg-yellow-500/20 flex items-center justify-center">
                    <Image
                      src={achievement.iconUrl}
                      alt=""
                      width={20}
                      height={20}
                      className="w-5 h-5 object-contain"
                    />
                  </div>
                  <div className="flex-1">
                    <p className="text-white font-medium">{achievement.name}</p>
                    <p className="text-xs text-gray-400">{achievement.description}</p>
                  </div>
                </motion.div>
              ))}
            </div>
          </motion.div>
        )}
        
        {/* Challenge Progress Section */}
        {hasChallengeProgress && (
          <motion.div
            data-testid="challenges-section"
            initial={{ x: -20, opacity: 0 }}
            animate={{ x: 0, opacity: 1 }}
            transition={{ delay: 0.4 }}
            className="mb-6 p-4 rounded-xl bg-white/5"
          >
            <h3 className="text-sm font-medium text-gray-400 mb-3">📋 Challenge Progress</h3>
            
            <div className="space-y-3">
              {challengeUpdates.map((challenge) => (
                <div key={challenge.challengeId} className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    {challenge.justCompleted ? (
                      <motion.span
                        data-testid={`challenge-completed-${challenge.challengeId}`}
                        initial={{ scale: 0 }}
                        animate={{ scale: 1 }}
                        className="text-green-400"
                      >
                        ✓
                      </motion.span>
                    ) : (
                      <span className="text-gray-400">○</span>
                    )}
                    <span className={challenge.justCompleted ? 'text-white' : 'text-gray-300'}>
                      {challenge.challengeName}
                    </span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-sm text-gray-400">{challenge.newProgress}%</span>
                    {challenge.justCompleted && challenge.xpEarned && (
                      <span className="text-sm text-yellow-400">+{challenge.xpEarned}</span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </motion.div>
        )}
        
        {/* Goals Section */}
        {hasGoalProgress && (
          <motion.div
            data-testid="goals-section"
            initial={{ x: -20, opacity: 0 }}
            animate={{ x: 0, opacity: 1 }}
            transition={{ delay: 0.5 }}
            className="mb-6 p-4 rounded-xl bg-white/5"
          >
            <h3 className="text-sm font-medium text-gray-400 mb-3">🎯 Goals Completed</h3>
            
            <div className="space-y-2">
              {goalUpdates.filter(g => g.justCompleted).map((goal) => (
                <motion.div
                  key={goal.goalId}
                  initial={{ x: -10, opacity: 0 }}
                  animate={{ x: 0, opacity: 1 }}
                  className="flex items-center gap-2 text-green-400"
                >
                  <span>✓</span>
                  <span>{goal.description}</span>
                </motion.div>
              ))}
            </div>
          </motion.div>
        )}
        
        {/* Continue Button */}
        <motion.button
          ref={continueButtonRef}
          initial={{ y: 20, opacity: 0 }}
          animate={{ y: 0, opacity: 1 }}
          transition={{ delay: 0.6 }}
          onClick={onContinue}
          className="w-full py-3 px-6 rounded-lg font-semibold text-white transition-colors"
          style={{ background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)' }}
          whileHover={{ scale: 1.02 }}
          whileTap={{ scale: 0.98 }}
        >
          Continue
        </motion.button>
      </motion.div>
    </motion.div>
  );
}

export const GameEndSummary = memo(GameEndSummaryComponent);
GameEndSummary.displayName = 'GameEndSummary';
