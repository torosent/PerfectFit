'use client';

import { memo, useEffect, useState } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { comboVariants } from '@/lib/animations';

function pseudoRandom01(seed: number): number {
  // Deterministic pseudo-random in [0, 1). Keeps render pure (no Math.random()).
  let t = seed + 0x6d2b79f5;
  t = Math.imul(t ^ (t >>> 15), t | 1);
  t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
  return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
}

export interface ComboPopupProps {
  /** Current combo value */
  combo: number;
  /** Key to trigger re-animation when combo changes */
  triggerKey?: number;
}

/**
 * Get combo tier styling based on combo count
 */
function getComboTier(combo: number): {
  label: string;
  gradient: string;
  glowColor: string;
  scale: number;
} {
  if (combo >= 5) {
    return {
      label: 'UNSTOPPABLE!',
      gradient: 'from-purple-500 via-pink-500 to-red-500',
      glowColor: 'rgba(168, 85, 247, 0.8)',
      scale: 1.3,
    };
  }
  if (combo >= 4) {
    return {
      label: 'ON FIRE!',
      gradient: 'from-pink-500 to-orange-500',
      glowColor: 'rgba(236, 72, 153, 0.8)',
      scale: 1.2,
    };
  }
  if (combo >= 3) {
    return {
      label: 'AWESOME!',
      gradient: 'from-orange-500 to-yellow-500',
      glowColor: 'rgba(249, 115, 22, 0.8)',
      scale: 1.15,
    };
  }
  if (combo >= 2) {
    return {
      label: 'GREAT!',
      gradient: 'from-yellow-500 to-amber-500',
      glowColor: 'rgba(251, 191, 36, 0.7)',
      scale: 1.1,
    };
  }
  return {
    label: 'COMBO!',
    gradient: 'from-yellow-500 to-orange-500',
    glowColor: 'rgba(251, 191, 36, 0.5)',
    scale: 1,
  };
}

/**
 * Get combo multiplier display
 */
function getMultiplierText(combo: number): string {
  const multipliers: Record<number, number> = {
    1: 1.5,
    2: 2.0,
    3: 3.0,
    4: 4.0,
    5: 5.0,
  };
  const mult = multipliers[combo] ?? 5.0 + ((combo - 5) * 0.5);
  return `${mult}× Points!`;
}

/**
 * Floating combo indicator that appears when combo increases
 * Shows "×X combo!" with spring animation and tier-based styling
 */
function ComboPopupComponent({ combo, triggerKey }: ComboPopupProps) {
  const [displayCombo, setDisplayCombo] = useState(combo);
  const [prevCombo, setPrevCombo] = useState(combo);
  const [showPopup, setShowPopup] = useState(false);
  const [prevTriggerKey, setPrevTriggerKey] = useState(triggerKey);
  // Version counter to trigger effect re-runs
  const [version, setVersion] = useState(0);

  // Use derived state pattern for all state updates
  const comboChanged = combo !== prevCombo;
  const triggerChanged = triggerKey !== prevTriggerKey;

  if (comboChanged || triggerChanged) {
    if (combo > 0) {
      setDisplayCombo(combo);
      setShowPopup(true);
      setVersion(v => v + 1);
    } else {
      setShowPopup(false);
    }
    setPrevCombo(combo);
    setPrevTriggerKey(triggerKey);
  }

  // Timer effect to auto-hide after duration
  useEffect(() => {
    if (!showPopup) return;

    // Longer duration for higher combos
    const duration = displayCombo >= 3 ? 2000 : 1500;
    const timer = setTimeout(() => {
      setShowPopup(false);
    }, duration);

    return () => clearTimeout(timer);
  }, [version, showPopup, displayCombo]);

  const tier = getComboTier(displayCombo);

  return (
    <AnimatePresence mode="wait">
      {showPopup && displayCombo > 0 && (
        <motion.div
          key={`combo-${triggerKey}-${displayCombo}`}
          variants={comboVariants}
          initial="initial"
          animate="show"
          exit="exit"
          className="absolute top-0 left-1/2 -translate-x-1/2 -translate-y-full pointer-events-none z-20"
        >
          <motion.div
            className="flex flex-col items-center"
            initial={{ scale: 0.5 }}
            animate={{ scale: tier.scale }}
            transition={{ type: 'spring', stiffness: 400, damping: 15 }}
          >
            {/* Combo tier label */}
            <motion.div
              className={`px-4 py-1 rounded-full bg-gradient-to-r ${tier.gradient} shadow-lg mb-1`}
              animate={{
                boxShadow: [
                  `0 0 10px ${tier.glowColor}`,
                  `0 0 25px ${tier.glowColor}`,
                  `0 0 10px ${tier.glowColor}`,
                ],
              }}
              transition={{
                duration: 0.6,
                repeat: Infinity,
                ease: 'easeInOut',
              }}
            >
              <span className="text-lg font-black text-white whitespace-nowrap drop-shadow-md tracking-wide">
                ×{displayCombo} {tier.label}
              </span>
            </motion.div>
            
            {/* Multiplier bonus indicator */}
            <motion.div
              initial={{ opacity: 0, y: -5 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.1 }}
              className={`px-3 py-0.5 rounded-full bg-gradient-to-r ${tier.gradient} shadow-md`}
              style={{ opacity: 0.9 }}
            >
              <span className="text-sm font-bold text-white whitespace-nowrap">
                {getMultiplierText(displayCombo)}
              </span>
            </motion.div>
            
            {/* Fire/sparkle particles for high combos */}
            {displayCombo >= 3 && (
              <div className="absolute inset-0 pointer-events-none overflow-visible">
                {[...Array(displayCombo >= 5 ? 10 : 6)].map((_, i) => (
                  <motion.div
                    key={i}
                    className="absolute w-2 h-2 rounded-full"
                    style={{
                      background: `linear-gradient(135deg, ${displayCombo >= 4 ? '#ec4899' : '#fbbf24'}, ${displayCombo >= 4 ? '#a855f7' : '#f97316'})`,
                      left: '50%',
                      top: '50%',
                    }}
                    animate={{
                      x: [0, (pseudoRandom01(displayCombo * 100 + i * 2 + 1) - 0.5) * 100],
                      y: [0, -50 - pseudoRandom01(displayCombo * 100 + i * 2 + 2) * 50],
                      opacity: [1, 0],
                      scale: [1, 0.3],
                    }}
                    transition={{
                      duration: 0.8,
                      delay: i * 0.05,
                      ease: 'easeOut',
                      repeat: displayCombo >= 4 ? 2 : 1,
                      repeatDelay: 0.3,
                    }}
                  />
                ))}
              </div>
            )}
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}

export const ComboPopup = memo(ComboPopupComponent);
ComboPopup.displayName = 'ComboPopup';
