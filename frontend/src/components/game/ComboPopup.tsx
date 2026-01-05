'use client';

import { memo, useEffect, useState } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { comboVariants } from '@/lib/animations';

export interface ComboPopupProps {
  /** Current combo value */
  combo: number;
  /** Key to trigger re-animation when combo changes */
  triggerKey?: number;
}

/**
 * Floating combo indicator that appears when combo increases
 * Shows "×X combo!" with spring animation
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

    const timer = setTimeout(() => {
      setShowPopup(false);
    }, 1500);

    return () => clearTimeout(timer);
  }, [version, showPopup]);

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
            className="px-4 py-2 rounded-full bg-gradient-to-r from-yellow-500 to-orange-500 shadow-lg"
            animate={{
              boxShadow: [
                '0 0 10px rgba(251, 191, 36, 0.5)',
                '0 0 20px rgba(251, 191, 36, 0.8)',
                '0 0 10px rgba(251, 191, 36, 0.5)',
              ],
            }}
            transition={{
              duration: 0.8,
              repeat: Infinity,
              ease: 'easeInOut',
            }}
          >
            <span className="text-lg font-bold text-white whitespace-nowrap drop-shadow-md">
              ×{displayCombo} Combo!
            </span>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}

export const ComboPopup = memo(ComboPopupComponent);
ComboPopup.displayName = 'ComboPopup';
