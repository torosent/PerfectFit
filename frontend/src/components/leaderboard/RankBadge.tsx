'use client';

import { memo } from 'react';
import { motion } from 'motion/react';

export interface RankBadgeProps {
  /** The rank to display */
  rank: number;
  /** Optional size variant */
  size?: 'sm' | 'md' | 'lg';
}

/**
 * Medal emoji mapping for top 3 ranks
 */
const MEDALS: Record<number, { emoji: string; label: string; color: string }> = {
  1: { emoji: '🥇', label: 'Gold Medal', color: 'from-yellow-400 to-yellow-600' },
  2: { emoji: '🥈', label: 'Silver Medal', color: 'from-gray-300 to-gray-500' },
  3: { emoji: '🥉', label: 'Bronze Medal', color: 'from-orange-400 to-orange-600' },
};

/**
 * Size classes for the badge
 */
const SIZE_CLASSES = {
  sm: 'text-lg w-8 h-8',
  md: 'text-xl w-10 h-10',
  lg: 'text-2xl w-12 h-12',
};

/**
 * Text size classes for numeric ranks
 */
const TEXT_SIZE_CLASSES = {
  sm: 'text-xs',
  md: 'text-sm',
  lg: 'text-base',
};

/**
 * Displays a rank badge with medal icons for top 3
 * Shows number with styling for other ranks
 */
function RankBadgeComponent({ rank, size = 'md' }: RankBadgeProps) {
  const medal = MEDALS[rank];
  const sizeClass = SIZE_CLASSES[size];
  const textSizeClass = TEXT_SIZE_CLASSES[size];

  if (medal) {
    return (
      <motion.div
        className={`${sizeClass} flex items-center justify-center`}
        initial={{ scale: 0, rotate: -180 }}
        animate={{ scale: 1, rotate: 0 }}
        transition={{
          type: 'spring',
          stiffness: 300,
          damping: 20,
          delay: rank * 0.1,
        }}
        whileHover={{ scale: 1.1 }}
        title={`Rank #${rank} - ${medal.label}`}
        aria-label={`Rank ${rank}, ${medal.label}`}
      >
        <motion.span
          className="select-none"
          animate={{
            filter: [
              'drop-shadow(0 0 2px rgba(255,255,255,0.3))',
              'drop-shadow(0 0 8px rgba(255,255,255,0.6))',
              'drop-shadow(0 0 2px rgba(255,255,255,0.3))',
            ],
          }}
          transition={{
            duration: 2,
            repeat: Infinity,
            ease: 'easeInOut',
          }}
        >
          {medal.emoji}
        </motion.span>
      </motion.div>
    );
  }

  // Numeric rank for positions 4+
  return (
    <motion.div
      className={`
        ${sizeClass} ${textSizeClass}
        flex items-center justify-center
        font-bold text-gray-400
        rounded-full
      `}
      style={{ backgroundColor: 'rgba(13, 36, 61, 0.8)', borderWidth: 1, borderStyle: 'solid', borderColor: 'rgba(20, 184, 166, 0.3)' }}
      initial={{ scale: 0 }}
      animate={{ scale: 1 }}
      transition={{
        type: 'spring',
        stiffness: 300,
        damping: 20,
        delay: Math.min(rank * 0.02, 0.5),
      }}
      title={`Rank #${rank}`}
      aria-label={`Rank ${rank}`}
    >
      {rank}
    </motion.div>
  );
}

export const RankBadge = memo(RankBadgeComponent);
RankBadge.displayName = 'RankBadge';
