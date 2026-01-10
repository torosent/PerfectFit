'use client';

import { memo, useEffect, useId, useCallback, useRef } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import type { AchievementUnlock, RewardType } from '@/types/gamification';
import { fireConfetti } from '@/lib/confetti';

export interface AchievementUnlockModalProps {
  /** Whether the modal is open */
  isOpen: boolean;
  /** Achievement that was unlocked */
  achievement: AchievementUnlock;
  /** Callback when modal is closed */
  onClose: () => void;
  /** Auto-close delay in ms (0 to disable) */
  autoCloseDelay?: number;
}

/**
 * Get reward display text
 */
function getRewardText(rewardType: RewardType, rewardValue: number): string {
  switch (rewardType) {
    case 'XPBoost': return `+${rewardValue} XP`;
    case 'StreakFreeze': return `${rewardValue}x Streak Freeze`;
    case 'Cosmetic': return 'Cosmetic Unlocked';
    default: return 'Reward Unlocked';
  }
}

/**
 * Achievement unlock celebration modal
 * Full-screen overlay with confetti and animations
 */
function AchievementUnlockModalComponent({
  isOpen,
  achievement,
  onClose,
  autoCloseDelay = 5000,
}: AchievementUnlockModalProps) {
  const titleId = useId();
  const closeButtonRef = useRef<HTMLButtonElement>(null);
  
  // Fire confetti when modal opens
  useEffect(() => {
    if (isOpen) {
      fireConfetti({
        particleCount: 100,
        spread: 70,
        colors: ['#fbbf24', '#a855f7', '#22d3ee', '#14b8a6'],
      });
    }
  }, [isOpen]);
  
  // Focus the close button when modal opens
  useEffect(() => {
    if (isOpen) {
      const timer = setTimeout(() => {
        closeButtonRef.current?.focus();
      }, 100);
      return () => clearTimeout(timer);
    }
  }, [isOpen]);
  
  // Auto-close timer
  useEffect(() => {
    if (!isOpen || autoCloseDelay === 0) return;
    
    const timer = setTimeout(() => {
      onClose();
    }, autoCloseDelay);
    
    return () => clearTimeout(timer);
  }, [isOpen, autoCloseDelay, onClose]);
  
  // Handle Escape key to close modal
  const handleKeyDown = useCallback((e: React.KeyboardEvent) => {
    if (e.key === 'Escape') {
      e.preventDefault();
      onClose();
    }
  }, [onClose]);
  
  return (
    <AnimatePresence>
      {isOpen && (
        <div
          role="dialog"
          aria-modal="true"
          aria-labelledby={titleId}
          className="fixed inset-0 z-50 flex items-center justify-center p-4"
          onKeyDown={handleKeyDown}
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
            initial={{ scale: 0.5, opacity: 0, y: 20 }}
            animate={{ scale: 1, opacity: 1, y: 0 }}
            exit={{ scale: 0.9, opacity: 0, y: -20 }}
            transition={{ type: 'spring', stiffness: 300, damping: 25 }}
            className="relative max-w-sm w-full rounded-2xl p-6 text-center overflow-hidden"
            style={{
              background: 'linear-gradient(135deg, rgba(13, 36, 61, 0.95), rgba(26, 54, 93, 0.95))',
              boxShadow: '0 0 40px rgba(168, 85, 247, 0.3)',
            }}
          >
            {/* Animated background glow */}
            <motion.div
              className="absolute inset-0 opacity-30 pointer-events-none"
              animate={{
                background: [
                  'radial-gradient(circle at 30% 30%, rgba(168, 85, 247, 0.4) 0%, transparent 50%)',
                  'radial-gradient(circle at 70% 70%, rgba(251, 191, 36, 0.4) 0%, transparent 50%)',
                  'radial-gradient(circle at 30% 70%, rgba(34, 211, 238, 0.4) 0%, transparent 50%)',
                  'radial-gradient(circle at 70% 30%, rgba(168, 85, 247, 0.4) 0%, transparent 50%)',
                ],
              }}
              transition={{ duration: 4, repeat: Infinity, ease: 'linear' }}
            />
            
            {/* Content */}
            <div className="relative z-10">
              {/* Title */}
              <motion.h2
                id={titleId}
                initial={{ y: -20, opacity: 0 }}
                animate={{ y: 0, opacity: 1 }}
                transition={{ delay: 0.1 }}
                className="text-lg font-semibold text-yellow-400 uppercase tracking-wider mb-4"
              >
                🏆 Achievement Unlocked!
              </motion.h2>
              
              {/* Achievement badge */}
              <motion.div
                initial={{ scale: 0, rotate: -180 }}
                animate={{ scale: 1, rotate: 0 }}
                transition={{ type: 'spring', stiffness: 200, damping: 15, delay: 0.2 }}
                className="relative mx-auto w-24 h-24 mb-4"
              >
                {/* Glow ring */}
                <motion.div
                  className="absolute inset-0 rounded-full"
                  animate={{
                    boxShadow: [
                      '0 0 20px rgba(251, 191, 36, 0.4)',
                      '0 0 40px rgba(251, 191, 36, 0.6)',
                      '0 0 20px rgba(251, 191, 36, 0.4)',
                    ],
                  }}
                  transition={{ duration: 2, repeat: Infinity }}
                />
                
                {/* Badge background */}
                <div className="absolute inset-0 rounded-full bg-gradient-to-br from-yellow-400 to-amber-600 flex items-center justify-center">
                  <motion.img
                    src={achievement.iconUrl}
                    alt={achievement.name}
                    className="w-16 h-16 object-contain"
                    initial={{ scale: 0 }}
                    animate={{ scale: 1 }}
                    transition={{ delay: 0.4, type: 'spring' }}
                  />
                </div>
              </motion.div>
              
              {/* Achievement name */}
              <motion.h3
                initial={{ y: 20, opacity: 0 }}
                animate={{ y: 0, opacity: 1 }}
                transition={{ delay: 0.3 }}
                className="text-2xl font-bold text-white mb-2"
              >
                {achievement.name}
              </motion.h3>
              
              {/* Description */}
              <motion.p
                initial={{ y: 20, opacity: 0 }}
                animate={{ y: 0, opacity: 1 }}
                transition={{ delay: 0.4 }}
                className="text-gray-300 mb-4"
              >
                {achievement.description}
              </motion.p>
              
              {/* Reward */}
              <motion.div
                initial={{ scale: 0.8, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                transition={{ delay: 0.5 }}
                className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-yellow-500/20 border border-yellow-500/30"
              >
                <span className="text-yellow-400">
                  {achievement.rewardType === 'XPBoost' && '⚡'}
                  {achievement.rewardType === 'StreakFreeze' && '❄️'}
                  {achievement.rewardType === 'Cosmetic' && '✨'}
                </span>
                <span className="text-yellow-400 font-medium">
                  {getRewardText(achievement.rewardType, achievement.rewardValue)}
                </span>
              </motion.div>
              
              {/* Dismiss button */}
              <motion.button
                ref={closeButtonRef}
                initial={{ y: 20, opacity: 0 }}
                animate={{ y: 0, opacity: 1 }}
                transition={{ delay: 0.6 }}
                onClick={onClose}
                className="mt-6 w-full py-3 px-6 rounded-lg font-semibold text-white transition-colors"
                style={{ background: 'linear-gradient(135deg, #a855f7, #7c3aed)' }}
                whileHover={{ scale: 1.02 }}
                whileTap={{ scale: 0.98 }}
              >
                Awesome!
              </motion.button>
            </div>
          </motion.div>
        </div>
      )}
    </AnimatePresence>
  );
}

export const AchievementUnlockModal = memo(AchievementUnlockModalComponent);
AchievementUnlockModal.displayName = 'AchievementUnlockModal';
