'use client';

import { memo, useCallback, useMemo } from 'react';
import { motion } from 'motion/react';
import type { SeasonPassInfo, SeasonReward, RewardType } from '@/types/gamification';

export interface SeasonPassTrackProps {
  /** Season pass data */
  seasonPass: SeasonPassInfo;
  /** Callback when claiming a reward */
  onClaimReward: (rewardId: string) => void;
  /** Whether a claim is in progress */
  isClaimingReward?: boolean;
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
 * Get reward description
 */
function getRewardDescription(reward: SeasonReward): string {
  switch (reward.rewardType) {
    case 'XPBoost': return `+${reward.rewardValue}% XP Boost`;
    case 'StreakFreeze': return `${reward.rewardValue}x Streak Freeze`;
    case 'Cosmetic': return 'Cosmetic Reward';
    default: return 'Reward';
  }
}

/**
 * Season Pass horizontal progress track component
 */
function SeasonPassTrackComponent({
  seasonPass,
  onClaimReward,
  isClaimingReward = false,
}: SeasonPassTrackProps) {
  const { seasonName, seasonNumber, currentXP, currentTier, rewards } = seasonPass;
  
  // Calculate progress to next tier
  const nextReward = useMemo(() => {
    return rewards.find(r => !r.isClaimed && !r.canClaim);
  }, [rewards]);
  
  const progressToNextTier = useMemo(() => {
    if (!nextReward) return 100;
    const prevXP = rewards.find(r => r.tier === currentTier)?.xpRequired ?? 0;
    const range = nextReward.xpRequired - prevXP;
    const progress = currentXP - prevXP;
    return Math.min(Math.max((progress / range) * 100, 0), 100);
  }, [currentXP, currentTier, nextReward, rewards]);
  
  const handleClaimReward = useCallback((rewardId: number) => {
    if (!isClaimingReward) {
      onClaimReward(String(rewardId));
    }
  }, [onClaimReward, isClaimingReward]);
  
  return (
    <div
      className="p-4 rounded-xl bg-white/5 backdrop-blur-md"
      aria-label={`Season pass progress, currently at tier ${currentTier}`}
    >
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <div>
          <h3 className="text-lg font-bold text-white">{seasonName}</h3>
          <p className="text-sm text-gray-400">Season {seasonNumber}</p>
        </div>
        <div className="text-right">
          <p className="text-sm text-gray-400">Current Tier</p>
          <p className="text-2xl font-bold text-white" data-testid="current-tier">
            {currentTier}
          </p>
        </div>
      </div>
      
      {/* XP Progress */}
      <div className="mb-4">
        <div className="flex items-center justify-between text-sm mb-1">
          <span className="text-gray-400">XP: <span className="text-white font-medium">{currentXP}</span></span>
          {nextReward && (
            <span className="text-gray-400">Next: {nextReward.xpRequired} XP</span>
          )}
        </div>
        <div
          data-testid="tier-progress-bar"
          className="h-2 rounded-full bg-white/10 overflow-hidden"
        >
          <motion.div
            className="h-full rounded-full bg-gradient-to-r from-blue-500 to-purple-500"
            initial={{ width: 0 }}
            animate={{ width: `${progressToNextTier}%` }}
            transition={{ duration: 0.5, ease: 'easeOut' }}
          />
        </div>
      </div>
      
      {/* Tier Track */}
      <div className="relative">
        {/* Track line */}
        <div className="absolute top-8 left-0 right-0 h-1 bg-white/10 rounded-full" />
        
        {/* Current position indicator */}
        <motion.div
          data-testid="current-tier-indicator"
          className="absolute top-7 w-3 h-3 rounded-full bg-blue-500 shadow-lg"
          style={{
            left: `${(currentTier / rewards.length) * 100}%`,
            transform: 'translateX(-50%)',
            boxShadow: '0 0 10px rgba(59, 130, 246, 0.5)',
          }}
          animate={{
            scale: [1, 1.2, 1],
          }}
          transition={{ duration: 2, repeat: Infinity }}
        />
        
        {/* Reward nodes */}
        <div className="flex justify-between">
          {rewards.map((reward) => {
            const isClaimed = reward.isClaimed;
            const canClaim = reward.canClaim && !reward.isClaimed;
            const isLocked = !isClaimed && !canClaim;
            
            return (
              <motion.div
                key={reward.id}
                data-testid={`reward-tier-${reward.tier}`}
                className={`relative flex flex-col items-center w-16 ${
                  isClaimed ? 'claimed' : canClaim ? 'claimable' : 'locked'
                }`}
                whileHover={canClaim ? { scale: 1.05 } : {}}
              >
                {/* Tier number */}
                <span className="text-xs text-gray-400 mb-1">{reward.tier}</span>
                
                {/* Reward icon node */}
                <div
                  className={`relative w-12 h-12 rounded-xl flex items-center justify-center border-2 transition-colors ${
                    isClaimed
                      ? 'bg-green-500/20 border-green-500/50'
                      : canClaim
                      ? 'bg-blue-500/20 border-blue-500/50 animate-pulse'
                      : 'bg-white/5 border-white/20'
                  }`}
                >
                  {/* Reward icon */}
                  <span className={`text-xl ${isLocked ? 'opacity-50 grayscale' : ''}`}>
                    {getRewardIcon(reward.rewardType)}
                  </span>
                  
                  {/* Lock overlay */}
                  {isLocked && (
                    <div className="absolute inset-0 flex items-center justify-center bg-black/30 rounded-xl">
                      <span className="text-xs">🔒</span>
                    </div>
                  )}
                  
                  {/* Claimed checkmark */}
                  {isClaimed && (
                    <div
                      data-testid={`claimed-checkmark-${reward.tier}`}
                      className="absolute -top-1 -right-1 w-5 h-5 rounded-full bg-green-500 flex items-center justify-center"
                    >
                      <span className="text-xs text-white">✓</span>
                    </div>
                  )}
                </div>
                
                {/* XP requirement */}
                <span className="text-xs text-gray-400 mt-1">{reward.xpRequired} XP</span>
                
                {/* Claim button */}
                {canClaim && (
                  <motion.button
                    data-testid={`claim-button-${reward.tier}`}
                    onClick={() => handleClaimReward(reward.id)}
                    disabled={isClaimingReward}
                    className="mt-2 px-3 py-1 text-xs font-medium rounded-full bg-blue-500 text-white hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
                    whileHover={{ scale: 1.05 }}
                    whileTap={{ scale: 0.95 }}
                    aria-label={`Claim tier ${reward.tier} reward`}
                  >
                    Claim
                  </motion.button>
                )}
              </motion.div>
            );
          })}
        </div>
      </div>
    </div>
  );
}

export const SeasonPassTrack = memo(SeasonPassTrackComponent);
SeasonPassTrack.displayName = 'SeasonPassTrack';
