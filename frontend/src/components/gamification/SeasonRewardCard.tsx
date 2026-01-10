'use client';

import { memo } from 'react';
import { motion } from 'motion/react';
import type { SeasonReward, RewardType } from '@/types/gamification';

export interface SeasonRewardCardProps {
  /** Reward data */
  reward: SeasonReward;
  /** Whether the reward can be claimed */
  canClaim?: boolean;
  /** Callback when claim button is clicked */
  onClaim?: () => void;
  /** Whether claiming is in progress */
  isClaiming?: boolean;
}

/**
 * Get icon for reward type
 */
function getRewardIcon(rewardType: RewardType): string {
  switch (rewardType) {
    case 'XPBoost': return '⚡';
    case 'StreakFreeze': return '❄️';
    case 'Cosmetic': return '✨';
    default: return '🎁';
  }
}

/**
 * Get reward name
 */
function getRewardName(rewardType: RewardType, value: number): string {
  switch (rewardType) {
    case 'XPBoost': return `${value}% XP Boost`;
    case 'StreakFreeze': return `${value}x Streak Freeze`;
    case 'Cosmetic': return 'Cosmetic';
    default: return 'Reward';
  }
}

/**
 * Individual season pass tier reward card
 */
function SeasonRewardCardComponent({
  reward,
  canClaim,
  onClaim,
  isClaiming = false,
}: SeasonRewardCardProps) {
  const { tier, rewardType, rewardValue, xpRequired, isClaimed } = reward;
  const isLocked = !isClaimed && !canClaim;
  
  return (
    <motion.div
      className={`relative p-4 rounded-xl border-2 transition-colors ${
        isClaimed
          ? 'bg-green-500/10 border-green-500/30'
          : canClaim
          ? 'bg-blue-500/10 border-blue-500/50 animate-pulse'
          : 'bg-white/5 border-white/10'
      }`}
      whileHover={canClaim ? { scale: 1.02 } : {}}
    >
      {/* Tier number */}
      <div className="flex items-center justify-between mb-3">
        <span className={`text-sm font-medium ${isClaimed ? 'text-green-400' : 'text-gray-400'}`}>
          Tier {tier}
        </span>
        <span className="text-xs text-gray-500">{xpRequired} XP</span>
      </div>
      
      {/* Reward icon */}
      <div
        className={`w-16 h-16 mx-auto mb-3 rounded-xl flex items-center justify-center ${
          isClaimed
            ? 'bg-green-500/20'
            : canClaim
            ? 'bg-blue-500/20'
            : 'bg-white/5'
        } ${isLocked ? 'opacity-50' : ''}`}
      >
        <span className={`text-3xl ${isLocked ? 'grayscale' : ''}`}>
          {getRewardIcon(rewardType)}
        </span>
        
        {/* Lock overlay */}
        {isLocked && (
          <div className="absolute inset-0 flex items-center justify-center">
            <span className="text-lg">🔒</span>
          </div>
        )}
      </div>
      
      {/* Reward name */}
      <p className={`text-center text-sm font-medium mb-3 ${
        isClaimed ? 'text-green-400' : isLocked ? 'text-gray-500' : 'text-white'
      }`}>
        {getRewardName(rewardType, rewardValue)}
      </p>
      
      {/* Status/Action */}
      {isClaimed && (
        <div className="flex items-center justify-center gap-1 text-green-400 text-sm">
          <span>✓</span>
          <span>Claimed</span>
        </div>
      )}
      
      {canClaim && !isClaimed && onClaim && (
        <motion.button
          onClick={onClaim}
          disabled={isClaiming}
          className="w-full py-2 px-4 rounded-lg font-medium text-white bg-gradient-to-r from-blue-500 to-cyan-400 hover:from-blue-600 hover:to-cyan-500 disabled:opacity-50 disabled:cursor-not-allowed"
          whileHover={{ scale: 1.02 }}
          whileTap={{ scale: 0.98 }}
        >
          {isClaiming ? 'Claiming...' : 'Claim'}
        </motion.button>
      )}
      
      {isLocked && (
        <div className="text-center text-xs text-gray-500">
          Reach {xpRequired} XP to unlock
        </div>
      )}
    </motion.div>
  );
}

export const SeasonRewardCard = memo(SeasonRewardCardComponent);
SeasonRewardCard.displayName = 'SeasonRewardCard';
