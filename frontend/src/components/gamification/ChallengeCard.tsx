'use client';

import { memo, useMemo, useCallback } from 'react';
import { motion } from 'motion/react';
import type { Challenge } from '@/types/gamification';

export interface ChallengeCardProps {
  /** Challenge data */
  challenge: Challenge;
  /** Optional click handler */
  onClick?: () => void;
}

/**
 * Format time remaining in a human-readable way
 */
function formatTimeRemaining(endsAt: string): string {
  const now = new Date();
  const end = new Date(endsAt);
  const diffMs = end.getTime() - now.getTime();
  
  if (diffMs <= 0) return 'Expired';
  
  const hours = Math.floor(diffMs / (1000 * 60 * 60));
  const days = Math.floor(hours / 24);
  
  if (days > 0) return `${days}d left`;
  if (hours > 0) return `${hours}h left`;
  
  const minutes = Math.floor(diffMs / (1000 * 60));
  return `${minutes}m left`;
}

/**
 * Format progress numbers with commas
 */
function formatNumber(num: number): string {
  return num.toLocaleString();
}

/**
 * Individual challenge card with progress bar
 */
function ChallengeCardComponent({ challenge, onClick }: ChallengeCardProps) {
  const {
    name,
    description,
    type,
    targetValue,
    currentProgress,
    xpReward,
    isCompleted,
    endsAt,
  } = challenge;
  
  const progressPercentage = useMemo(() => {
    return Math.min(Math.round((currentProgress / targetValue) * 100), 100);
  }, [currentProgress, targetValue]);
  
  const timeRemaining = useMemo(() => formatTimeRemaining(endsAt), [endsAt]);
  
  // Handle keyboard interaction for clickable cards
  const handleKeyDown = useCallback((e: React.KeyboardEvent) => {
    if (onClick && (e.key === 'Enter' || e.key === ' ')) {
      e.preventDefault();
      onClick();
    }
  }, [onClick]);
  
  return (
    <motion.div
      data-testid="challenge-card"
      className={`relative p-4 rounded-xl bg-white/5 backdrop-blur-md border transition-colors
        ${isCompleted ? 'completed border-green-500/30 bg-green-500/5' : 'border-white/10 hover:border-white/20'}
      `}
      onClick={onClick}
      onKeyDown={onClick ? handleKeyDown : undefined}
      role={onClick ? 'button' : undefined}
      tabIndex={onClick ? 0 : undefined}
      whileHover={{ scale: 1.01 }}
      whileTap={{ scale: 0.99 }}
      aria-label={`${name}, ${progressPercentage}% complete`}
    >
      {/* Type badge and time remaining */}
      <div className="flex items-center justify-between mb-2">
        <span
          data-testid="challenge-type-badge"
          className={`px-2 py-0.5 text-xs font-medium rounded-full
            ${type === 'Daily' 
              ? 'daily bg-blue-500/20 text-blue-400' 
              : 'weekly bg-purple-500/20 text-purple-400'
            }
          `}
        >
          {type}
        </span>
        
        <span className="text-xs text-gray-400">
          {timeRemaining}
        </span>
      </div>
      
      {/* Challenge name and completion indicator */}
      <div className="flex items-start justify-between gap-2 mb-1">
        <h3 className="font-semibold text-white">{name}</h3>
        
        {isCompleted && (
          <motion.div
            data-testid="challenge-completed"
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            transition={{ type: 'spring', stiffness: 400, damping: 15 }}
            className="flex-shrink-0 w-5 h-5 flex items-center justify-center rounded-full bg-green-500"
          >
            <span className="text-xs text-white">✓</span>
          </motion.div>
        )}
      </div>
      
      {/* Description */}
      <p className="text-sm text-gray-400 mb-3">{description}</p>
      
      {/* Progress bar */}
      <div className="mb-2">
        <div className="flex items-center justify-between text-xs text-gray-400 mb-1">
          <span>{formatNumber(currentProgress)} / {formatNumber(targetValue)}</span>
          <span>{progressPercentage}%</span>
        </div>
        <div
          role="progressbar"
          aria-valuenow={progressPercentage}
          aria-valuemin={0}
          aria-valuemax={100}
          className="h-2 rounded-full bg-white/10 overflow-hidden"
        >
          <motion.div
            className={`h-full rounded-full ${
              isCompleted 
                ? 'bg-gradient-to-r from-green-500 to-emerald-400' 
                : 'bg-gradient-to-r from-blue-500 to-cyan-400'
            }`}
            initial={{ width: 0 }}
            animate={{ width: `${progressPercentage}%` }}
            transition={{ duration: 0.5, ease: 'easeOut' }}
          />
        </div>
      </div>
      
      {/* XP Reward */}
      <div className="flex items-center justify-end">
        <motion.span
          className={`px-2 py-1 text-xs font-medium rounded-md ${
            isCompleted 
              ? 'bg-green-500/20 text-green-400' 
              : 'bg-yellow-500/20 text-yellow-400'
          }`}
          whileHover={{ scale: 1.05 }}
        >
          {isCompleted ? '✓ ' : '+'}{xpReward} XP
        </motion.span>
      </div>
    </motion.div>
  );
}

export const ChallengeCard = memo(ChallengeCardComponent);
ChallengeCard.displayName = 'ChallengeCard';
