'use client';

import { memo, useMemo, useCallback } from 'react';
import { motion } from 'motion/react';
import type { PersonalGoal, GoalType } from '@/types/gamification';

export interface PersonalGoalCardProps {
  /** Goal data */
  goal: PersonalGoal;
  /** Callback when goal is clicked */
  onClick?: () => void;
}

/**
 * Get icon for goal type
 */
function getGoalIcon(goalType: GoalType): string {
  switch (goalType) {
    case 'BeatAverage': return '📊';
    case 'ImproveAccuracy': return '🎯';
    case 'NewPersonalBest': return '🏆';
    default: return '🎯';
  }
}

/**
 * Format time remaining until expiry
 */
function formatTimeRemaining(expiresAt: string | null): string | null {
  if (!expiresAt) return null;
  
  const now = new Date();
  const expires = new Date(expiresAt);
  const diffMs = expires.getTime() - now.getTime();
  
  if (diffMs <= 0) return 'Expired';
  
  const hours = Math.floor(diffMs / (1000 * 60 * 60));
  const days = Math.floor(hours / 24);
  
  if (days > 0) return `${days}d left`;
  if (hours > 0) return `${hours}h left`;
  
  const minutes = Math.floor(diffMs / (1000 * 60));
  return `${minutes}m left`;
}

/**
 * Personal goal card with progress display
 */
function PersonalGoalCardComponent({ goal, onClick }: PersonalGoalCardProps) {
  const {
    type,
    description,
    targetValue,
    currentValue,
    progressPercentage,
    isCompleted,
    expiresAt,
  } = goal;
  
  const timeRemaining = useMemo(
    () => formatTimeRemaining(expiresAt),
    [expiresAt]
  );
  
  const icon = getGoalIcon(type);
  
  // Handle keyboard interaction for clickable cards
  const handleKeyDown = useCallback((e: React.KeyboardEvent) => {
    if (onClick && (e.key === 'Enter' || e.key === ' ')) {
      e.preventDefault();
      onClick();
    }
  }, [onClick]);
  
  return (
    <motion.div
      data-testid="personal-goal-card"
      className={`relative p-4 rounded-xl border transition-colors cursor-pointer ${
        isCompleted
          ? 'bg-green-500/10 border-green-500/30'
          : 'bg-white/5 border-white/10 hover:border-white/20'
      }`}
      onClick={onClick}
      onKeyDown={onClick ? handleKeyDown : undefined}
      role={onClick ? 'button' : undefined}
      tabIndex={onClick ? 0 : undefined}
      aria-label={`${description}, ${isCompleted ? 'completed' : `${progressPercentage}% complete`}`}
      whileHover={{ scale: 1.01 }}
      whileTap={{ scale: 0.99 }}
    >
      <div className="flex items-start gap-3">
        {/* Icon */}
        <div
          className={`flex-shrink-0 w-10 h-10 rounded-xl flex items-center justify-center ${
            isCompleted ? 'bg-green-500/20' : 'bg-white/10'
          }`}
        >
          <span className="text-xl">{icon}</span>
        </div>
        
        {/* Content */}
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2 mb-1">
            <p className={`font-medium ${isCompleted ? 'text-green-400' : 'text-white'}`}>
              {description}
            </p>
            
            {isCompleted && (
              <motion.span
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                className="flex-shrink-0 w-5 h-5 rounded-full bg-green-500 flex items-center justify-center"
              >
                <span className="text-xs text-white">✓</span>
              </motion.span>
            )}
          </div>
          
          {/* Progress bar */}
          {!isCompleted && (
            <div className="mt-2">
              <div className="flex items-center justify-between text-xs text-gray-400 mb-1">
                <span>{currentValue} / {targetValue}</span>
                {timeRemaining && (
                  <span className="text-yellow-400">{timeRemaining}</span>
                )}
              </div>
              <div className="h-2 rounded-full bg-white/10 overflow-hidden">
                <motion.div
                  className="h-full rounded-full bg-gradient-to-r from-blue-500 to-cyan-400"
                  initial={{ width: 0 }}
                  animate={{ width: `${progressPercentage}%` }}
                  transition={{ duration: 0.5, ease: 'easeOut' }}
                />
              </div>
            </div>
          )}
          
          {/* Completion celebration */}
          {isCompleted && (
            <motion.div
              initial={{ opacity: 0, y: 5 }}
              animate={{ opacity: 1, y: 0 }}
              className="mt-2 text-sm text-green-400"
            >
              🎉 Goal achieved!
            </motion.div>
          )}
        </div>
      </div>
    </motion.div>
  );
}

export const PersonalGoalCard = memo(PersonalGoalCardComponent);
PersonalGoalCard.displayName = 'PersonalGoalCard';
