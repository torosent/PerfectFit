'use client';

import { memo } from 'react';
import { motion, AnimatePresence } from 'motion/react';

export interface StreakCounterProps {
  /** Current streak count */
  streak: number;
}

/**
 * Get streak tier based on count
 */
function getStreakTier(streak: number): 'none' | 'small' | 'medium' | 'large' {
  if (streak >= 10) return 'large';
  if (streak >= 5) return 'medium';
  if (streak >= 3) return 'small';
  return 'none';
}

/**
 * Get flame color based on streak tier
 */
function getFlameColor(tier: 'none' | 'small' | 'medium' | 'large'): string {
  switch (tier) {
    case 'large': return '#ef4444'; // Red
    case 'medium': return '#f97316'; // Orange
    case 'small': return '#fbbf24'; // Yellow
    default: return '#6b7280'; // Gray
  }
}

/**
 * Flame particles for high streaks
 */
function FlameParticles({ tier }: { tier: 'small' | 'medium' | 'large' }) {
  const particleCount = tier === 'large' ? 5 : tier === 'medium' ? 3 : 2;
  const color = getFlameColor(tier);
  
  return (
    <div className="absolute inset-0 pointer-events-none overflow-visible">
      {[...Array(particleCount)].map((_, i) => {
        const offset = i - Math.floor(particleCount / 2);

        return (
          <motion.div
            key={i}
            className="absolute w-1.5 h-1.5 rounded-full"
            style={{
              backgroundColor: color,
              left: `${30 + i * 10}%`,
              bottom: '60%',
            }}
            animate={{
              y: [0, -20, -30],
              x: [0, offset * 5, offset * 8],
              opacity: [0.8, 0.6, 0],
              scale: [1, 0.8, 0.4],
            }}
            transition={{
              duration: 1,
              repeat: Infinity,
              delay: i * 0.2,
              ease: 'easeOut',
            }}
          />
        );
      })}
    </div>
  );
}

/**
 * Displays the current streak with a flame badge that grows with streak count
 */
function StreakCounterComponent({ streak }: StreakCounterProps) {
  const tier = getStreakTier(streak);
  const flameColor = getFlameColor(tier);
  
  // Don't show anything if streak is below 3
  if (tier === 'none') return null;
  
  const flameSize = tier === 'large' ? 'text-2xl' : tier === 'medium' ? 'text-xl' : 'text-lg';
  
  return (
    <AnimatePresence>
      <motion.div
        initial={{ scale: 0, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        exit={{ scale: 0, opacity: 0 }}
        className="relative flex items-center gap-1"
      >
        {/* Flame icon */}
        <motion.div
          className={`relative ${flameSize}`}
          animate={{
            scale: [1, 1.1, 1],
          }}
          transition={{
            duration: 0.5,
            repeat: Infinity,
            ease: 'easeInOut',
          }}
        >
          <span role="img" aria-label="Streak">🔥</span>
          
          {/* Glow effect */}
          <motion.div
            className="absolute inset-0 rounded-full blur-md -z-10"
            style={{ backgroundColor: flameColor }}
            animate={{
              opacity: [0.3, 0.5, 0.3],
            }}
            transition={{
              duration: 1,
              repeat: Infinity,
              ease: 'easeInOut',
            }}
          />
        </motion.div>
        
        {/* Streak count */}
        <motion.span
          key={streak}
          initial={{ scale: 0.5, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          className="font-bold tabular-nums"
          style={{ color: flameColor }}
        >
          {streak}
        </motion.span>
        
        {/* Particles for medium+ streaks */}
        {tier !== 'small' && <FlameParticles tier={tier} />}
      </motion.div>
    </AnimatePresence>
  );
}

export const StreakCounter = memo(StreakCounterComponent);
StreakCounter.displayName = 'StreakCounter';
