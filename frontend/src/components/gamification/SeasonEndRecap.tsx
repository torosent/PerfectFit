'use client';

import { memo, useId } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import type { SeasonPassInfo } from '@/types/gamification';

export interface SeasonEndRecapProps {
  /** Whether the modal is open */
  isOpen: boolean;
  /** Season pass data */
  seasonPass: SeasonPassInfo;
  /** Total rewards claimed */
  rewardsClaimed: number;
  /** Total XP earned in the season */
  xpEarned: number;
  /** Callback when modal is closed */
  onClose: () => void;
  /** Next season teaser info */
  nextSeasonTeaser?: {
    name: string;
    startsAt: string;
  };
}

/**
 * End-of-season summary modal
 */
function SeasonEndRecapComponent({
  isOpen,
  seasonPass,
  rewardsClaimed,
  xpEarned,
  onClose,
  nextSeasonTeaser,
}: SeasonEndRecapProps) {
  const titleId = useId();
  
  return (
    <AnimatePresence>
      {isOpen && (
        <div
          role="dialog"
          aria-modal="true"
          aria-labelledby={titleId}
          className="fixed inset-0 z-50 flex items-center justify-center p-4"
        >
          {/* Backdrop */}
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="absolute inset-0 bg-black/80 backdrop-blur-sm"
            onClick={onClose}
          />
          
          {/* Modal content */}
          <motion.div
            initial={{ scale: 0.9, opacity: 0, y: 20 }}
            animate={{ scale: 1, opacity: 1, y: 0 }}
            exit={{ scale: 0.9, opacity: 0, y: -20 }}
            className="relative max-w-md w-full rounded-2xl overflow-hidden"
            style={{
              background: 'linear-gradient(135deg, rgba(13, 36, 61, 0.98), rgba(26, 54, 93, 0.98))',
            }}
          >
            {/* Header with gradient */}
            <div
              className="p-6 text-center"
              style={{
                background: 'linear-gradient(135deg, rgba(168, 85, 247, 0.3), rgba(59, 130, 246, 0.3))',
              }}
            >
              <motion.span
                className="text-5xl block mb-3"
                animate={{ rotate: [0, 10, -10, 0] }}
                transition={{ duration: 0.5, delay: 0.3 }}
              >
                🏆
              </motion.span>
              <h2 id={titleId} className="text-2xl font-bold text-white">
                Season Complete!
              </h2>
              <p className="text-purple-300">{seasonPass.seasonName}</p>
            </div>
            
            {/* Stats */}
            <div className="p-6 space-y-4">
              {/* Season stats */}
              <div className="grid grid-cols-3 gap-4 text-center">
                <div className="p-3 rounded-xl bg-white/5">
                  <p className="text-2xl font-bold text-yellow-400">{xpEarned}</p>
                  <p className="text-xs text-gray-400">XP Earned</p>
                </div>
                <div className="p-3 rounded-xl bg-white/5">
                  <p className="text-2xl font-bold text-cyan-400">{seasonPass.currentTier}</p>
                  <p className="text-xs text-gray-400">Tier Reached</p>
                </div>
                <div className="p-3 rounded-xl bg-white/5">
                  <p className="text-2xl font-bold text-green-400">{rewardsClaimed}</p>
                  <p className="text-xs text-gray-400">Rewards</p>
                </div>
              </div>
              
              {/* Rewards collected summary */}
              <div className="p-4 rounded-xl bg-white/5">
                <h3 className="text-sm font-medium text-gray-400 mb-2">Rewards Collected</h3>
                <div className="flex flex-wrap gap-2">
                  {seasonPass.rewards
                    .filter(r => r.isClaimed)
                    .slice(0, 5)
                    .map(reward => (
                      <div
                        key={reward.id}
                        className="w-10 h-10 rounded-lg bg-white/10 flex items-center justify-center"
                      >
                        <span>
                          {reward.rewardType === 'XPBoost' && '⚡'}
                          {reward.rewardType === 'StreakFreeze' && '❄️'}
                          {reward.rewardType === 'Cosmetic' && '✨'}
                        </span>
                      </div>
                    ))}
                  {rewardsClaimed > 5 && (
                    <div className="w-10 h-10 rounded-lg bg-white/10 flex items-center justify-center text-gray-400 text-sm">
                      +{rewardsClaimed - 5}
                    </div>
                  )}
                </div>
              </div>
              
              {/* Next season teaser */}
              {nextSeasonTeaser && (
                <motion.div
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: 0.5 }}
                  className="p-4 rounded-xl bg-gradient-to-r from-purple-500/20 to-blue-500/20 border border-purple-500/30"
                >
                  <p className="text-sm text-gray-400">Coming Soon</p>
                  <p className="text-lg font-bold text-white">{nextSeasonTeaser.name}</p>
                  <p className="text-sm text-purple-400">
                    Starts {new Date(nextSeasonTeaser.startsAt).toLocaleDateString()}
                  </p>
                </motion.div>
              )}
              
              {/* Actions */}
              <div className="flex gap-3">
                <motion.button
                  onClick={onClose}
                  className="flex-1 py-3 px-6 rounded-lg font-semibold text-white"
                  style={{ background: 'linear-gradient(135deg, #a855f7, #7c3aed)' }}
                  whileHover={{ scale: 1.02 }}
                  whileTap={{ scale: 0.98 }}
                >
                  Continue
                </motion.button>
              </div>
            </div>
          </motion.div>
        </div>
      )}
    </AnimatePresence>
  );
}

export const SeasonEndRecap = memo(SeasonEndRecapComponent);
SeasonEndRecap.displayName = 'SeasonEndRecap';
