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
 * Get combo multiplier display (matching backend logic)
 */
function getComboMultiplier(comboCount: number): string {
  if (comboCount <= 0) return '1×';
  const multipliers: Record<number, number> = {
    1: 1.5,
    2: 2.0,
    3: 3.0,
    4: 4.0,
    5: 5.0,
  };
  const mult = multipliers[comboCount] ?? 5.0 + ((comboCount - 5) * 0.5);
  return `${mult}×`;
}

/**
 * Get combo tier color based on combo count
 */
function getComboColor(combo: number): string {
  if (combo <= 0) return '#6b7280';
  if (combo === 1) return '#fbbf24'; // Yellow
  if (combo === 2) return '#f97316'; // Orange  
  if (combo === 3) return '#ef4444'; // Red
  if (combo === 4) return '#ec4899'; // Pink
  return '#a855f7'; // Purple for 5+
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
  const comboColor = getComboColor(combo);

  return (
    <div 
      className="flex items-start justify-center gap-6 sm:gap-8 w-full px-2"
      style={{ flexDirection: 'row' }}
    >
      {/* Score */}
      <div className="text-center">
        <p className="text-[10px] sm:text-sm font-medium text-gray-400 uppercase tracking-wider">
          Score
        </p>
        <motion.p
          key={scoreKey}
          variants={scoreVariants}
          initial="initial"
          animate="update"
          className="text-2xl sm:text-4xl font-bold text-white tabular-nums"
          aria-live="polite"
        >
          {animatedScore.toLocaleString()}
        </motion.p>
      </div>

      {/* Combo with Multiplier */}
      <div className="text-center relative min-w-[80px]">
        <p className="text-[10px] sm:text-sm font-medium text-gray-400 uppercase tracking-wider">
          Combo
        </p>
        <AnimatePresence mode="wait">
          <motion.div
            key={comboKey}
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            exit={{ scale: 0.8, opacity: 0 }}
            transition={{ type: 'spring', stiffness: 400, damping: 15 }}
            className="flex flex-col items-center"
          >
            <motion.p
              style={{ color: comboColor }}
              className="text-xl sm:text-3xl font-bold tabular-nums"
              aria-live="polite"
            >
              {hasCombo ? `×${combo}` : '—'}
            </motion.p>
            {hasCombo && (
              <motion.span
                initial={{ opacity: 0, y: -5 }}
                animate={{ opacity: 1, y: 0 }}
                className="text-[10px] sm:text-xs font-semibold whitespace-nowrap"
                style={{ color: comboColor }}
              >
                {getComboMultiplier(combo)} bonus
              </motion.span>
            )}
          </motion.div>
        </AnimatePresence>
        
        {/* Enhanced glow effect for active combo */}
        {hasCombo && (
          <motion.div
            className="absolute inset-0 -z-10 rounded-lg"
            animate={{
              boxShadow: [
                `0 0 10px ${comboColor}40`,
                `0 0 25px ${comboColor}60`,
                `0 0 10px ${comboColor}40`,
              ],
            }}
            transition={{
              duration: 1.2,
              repeat: Infinity,
              ease: 'easeInOut',
            }}
          />
        )}
        
        {/* Fire particles for high combos */}
        {combo >= 3 && (
          <motion.div
            className="absolute -top-2 left-1/2 -translate-x-1/2 pointer-events-none"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
          >
            {[...Array(combo >= 5 ? 5 : 3)].map((_, i) => (
              <motion.div
                key={i}
                className="absolute w-1.5 h-1.5 rounded-full"
                style={{ 
                  backgroundColor: comboColor,
                  left: `${(i - 1) * 8}px`,
                }}
                animate={{
                  y: [0, -15, 0],
                  opacity: [0.8, 0, 0.8],
                  scale: [1, 0.5, 1],
                }}
                transition={{
                  duration: 0.8,
                  repeat: Infinity,
                  delay: i * 0.15,
                  ease: 'easeOut',
                }}
              />
            ))}
          </motion.div>
        )}
      </div>

      {/* Lines Cleared */}
      <div className="text-center">
        <p className="text-[10px] sm:text-sm font-medium text-gray-400 uppercase tracking-wider">
          Lines
        </p>
        <motion.p
          className="text-xl sm:text-3xl font-bold text-gray-300 tabular-nums"
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
