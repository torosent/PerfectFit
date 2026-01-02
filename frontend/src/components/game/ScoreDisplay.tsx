'use client';

import { memo, useEffect, useState } from 'react';
import { motion, useMotionValue, AnimatePresence, animate } from 'motion/react';
import { scoreVariants } from '@/lib/animations';

export interface ScoreDisplayProps {
  /** Current score */
  score: number;
  /** Current combo multiplier */
  combo: number;
  /** Total lines cleared */
  linesCleared?: number;
}

/**
 * Animated counter hook for smooth number transitions
 */
function useAnimatedCounter(value: number, duration: number = 0.5) {
  const motionValue = useMotionValue(0);
  const [displayValue, setDisplayValue] = useState(value);
  
  useEffect(() => {
    const controls = animate(motionValue, value, {
      duration,
      ease: 'easeOut',
      onUpdate: (latest) => {
        setDisplayValue(Math.round(latest));
      },
    });

    return controls.stop;
  }, [value, duration, motionValue]);

  return displayValue;
}

/**
 * Displays the current game score and combo with animations
 */
function ScoreDisplayComponent({ 
  score, 
  combo, 
  linesCleared = 0 
}: ScoreDisplayProps) {
  const hasCombo = combo > 0;
  
  // Track key changes using state
  const [scoreKey, setScoreKey] = useState(0);
  const [comboKey, setComboKey] = useState(0);
  const [prevScore, setPrevScore] = useState(score);
  const [prevCombo, setPrevCombo] = useState(combo);

  // Update keys when values change (using derived state pattern)
  if (score !== prevScore) {
    setScoreKey(k => k + 1);
    setPrevScore(score);
  }
  
  if (combo > prevCombo) {
    setComboKey(k => k + 1);
    setPrevCombo(combo);
  } else if (combo !== prevCombo) {
    setPrevCombo(combo);
  }

  const animatedScore = useAnimatedCounter(score);
  const animatedLines = useAnimatedCounter(linesCleared, 0.3);

  return (
    <div className="flex flex-col sm:flex-row gap-4 sm:gap-8 items-center justify-center">
      {/* Score */}
      <div className="text-center">
        <p className="text-xs sm:text-sm font-medium text-gray-400 uppercase tracking-wider">
          Score
        </p>
        <motion.p
          key={scoreKey}
          variants={scoreVariants}
          initial="initial"
          animate="update"
          className="text-3xl sm:text-4xl font-bold text-white tabular-nums"
          aria-live="polite"
        >
          {animatedScore.toLocaleString()}
        </motion.p>
      </div>

      {/* Combo */}
      <div className="text-center relative">
        <p className="text-xs sm:text-sm font-medium text-gray-400 uppercase tracking-wider">
          Combo
        </p>
        <AnimatePresence mode="wait">
          <motion.p
            key={comboKey}
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ 
              scale: 1, 
              opacity: 1,
              color: hasCombo ? '#fbbf24' : '#6b7280',
            }}
            exit={{ scale: 0.8, opacity: 0 }}
            transition={{ type: 'spring', stiffness: 400, damping: 15 }}
            className="text-2xl sm:text-3xl font-bold tabular-nums"
            aria-live="polite"
          >
            {hasCombo ? `×${combo}` : '—'}
          </motion.p>
        </AnimatePresence>
        
        {/* Glow effect for active combo */}
        {hasCombo && (
          <motion.div
            className="absolute inset-0 -z-10 rounded-lg"
            animate={{
              boxShadow: [
                '0 0 10px rgba(251, 191, 36, 0.2)',
                '0 0 20px rgba(251, 191, 36, 0.4)',
                '0 0 10px rgba(251, 191, 36, 0.2)',
              ],
            }}
            transition={{
              duration: 1.5,
              repeat: Infinity,
              ease: 'easeInOut',
            }}
          />
        )}
      </div>

      {/* Lines Cleared */}
      <div className="text-center">
        <p className="text-xs sm:text-sm font-medium text-gray-400 uppercase tracking-wider">
          Lines
        </p>
        <motion.p
          className="text-2xl sm:text-3xl font-bold text-gray-300 tabular-nums"
          aria-live="polite"
          animate={{ scale: linesCleared > 0 ? [1, 1.1, 1] : 1 }}
          transition={{ duration: 0.2 }}
        >
          {animatedLines}
        </motion.p>
      </div>
    </div>
  );
}

export const ScoreDisplay = memo(ScoreDisplayComponent);
ScoreDisplay.displayName = 'ScoreDisplay';
