'use client';

import { memo, useEffect } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { firePerfectClearConfetti, fireStarburst } from '@/lib/confetti';

export interface PerfectClearOverlayProps {
  /** Whether to show the perfect clear celebration */
  isVisible: boolean;
  /** Callback when animation completes */
  onComplete?: () => void;
}

/**
 * Golden starburst effect component
 */
function Starburst() {
  return (
    <motion.div
      className="absolute inset-0 pointer-events-none"
      initial={{ opacity: 0 }}
      animate={{ opacity: [0, 1, 0] }}
      transition={{ duration: 1.5 }}
    >
      {/* Radiating lines */}
      {[...Array(12)].map((_, i) => (
        <motion.div
          key={i}
          className="absolute left-1/2 top-1/2 w-1 origin-bottom"
          style={{
            height: '150%',
            background: 'linear-gradient(to top, rgba(251, 191, 36, 0.8), transparent)',
            transform: `translateX(-50%) rotate(${i * 30}deg)`,
          }}
          initial={{ scaleY: 0, opacity: 0 }}
          animate={{ 
            scaleY: [0, 1, 0.5],
            opacity: [0, 1, 0],
          }}
          transition={{
            duration: 1,
            delay: i * 0.03,
            ease: 'easeOut',
          }}
        />
      ))}
      
      {/* Central glow */}
      <motion.div
        className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 w-32 h-32 rounded-full"
        style={{
          background: 'radial-gradient(circle, rgba(251, 191, 36, 0.8) 0%, rgba(251, 191, 36, 0) 70%)',
        }}
        initial={{ scale: 0, opacity: 0 }}
        animate={{ 
          scale: [0, 2, 3],
          opacity: [0, 1, 0],
        }}
        transition={{ duration: 1.2, ease: 'easeOut' }}
      />
    </motion.div>
  );
}

/**
 * Perfect clear celebration overlay
 * Shows a golden explosion/starburst effect with "PERFECT CLEAR!" text
 */
function PerfectClearOverlayComponent({ isVisible, onComplete }: PerfectClearOverlayProps) {
  // Trigger confetti when visible
  useEffect(() => {
    if (isVisible) {
      // Fire confetti effects
      firePerfectClearConfetti();
      fireStarburst();
      
      // Call onComplete after animation
      const timer = setTimeout(() => {
        onComplete?.();
      }, 3000);
      
      return () => clearTimeout(timer);
    }
  }, [isVisible, onComplete]);

  return (
    <AnimatePresence>
      {isVisible && (
        <motion.div
          className="fixed inset-0 z-50 flex items-center justify-center pointer-events-none"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.3 }}
        >
          {/* Background overlay */}
          <motion.div
            className="absolute inset-0 bg-black/30"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
          />
          
          {/* Starburst effect */}
          <Starburst />
          
          {/* Text */}
          <motion.div
            className="relative z-10 text-center"
            initial={{ scale: 0, opacity: 0, rotate: -10 }}
            animate={{ 
              scale: [0, 1.2, 1],
              opacity: [0, 1, 1],
              rotate: [0, 5, 0],
            }}
            exit={{ scale: 0, opacity: 0 }}
            transition={{
              duration: 0.6,
              ease: [0.34, 1.56, 0.64, 1],
            }}
          >
            {/* Glow background */}
            <motion.div
              className="absolute inset-0 blur-xl -z-10"
              style={{
                background: 'radial-gradient(ellipse, rgba(251, 191, 36, 0.5) 0%, transparent 70%)',
              }}
              animate={{
                scale: [1, 1.2, 1],
                opacity: [0.5, 0.8, 0.5],
              }}
              transition={{
                duration: 1,
                repeat: Infinity,
                ease: 'easeInOut',
              }}
            />
            
            {/* Main text */}
            <motion.h1
              className="text-4xl sm:text-6xl font-black tracking-wider"
              style={{
                background: 'linear-gradient(135deg, #fbbf24, #f59e0b, #fcd34d, #ffffff, #fbbf24)',
                backgroundSize: '200% 200%',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                backgroundClip: 'text',
                textShadow: '0 0 30px rgba(251, 191, 36, 0.5)',
                filter: 'drop-shadow(0 0 10px rgba(251, 191, 36, 0.8))',
              }}
              animate={{
                backgroundPosition: ['0% 50%', '100% 50%', '0% 50%'],
              }}
              transition={{
                duration: 2,
                repeat: Infinity,
                ease: 'linear',
              }}
            >
              PERFECT CLEAR!
            </motion.h1>
            
            {/* Subtitle */}
            <motion.p
              className="mt-2 text-xl sm:text-2xl font-semibold text-yellow-300"
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.3 }}
            >
              ⭐ Board cleared! ⭐
            </motion.p>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}

export const PerfectClearOverlay = memo(PerfectClearOverlayComponent);
PerfectClearOverlay.displayName = 'PerfectClearOverlay';
