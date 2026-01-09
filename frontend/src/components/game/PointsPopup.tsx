'use client';

import { memo, useEffect, useState } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { pointsPopupVariants } from '@/lib/animations';

export interface PointsPopupProps {
  /** Points to display */
  points: number;
  /** Key to trigger re-animation when points change */
  triggerKey?: number;
  /** Position for the popup (relative to parent) */
  position?: { x: number; y: number };
}

/**
 * Get popup tier based on points earned
 */
function getPointsTier(points: number): {
  label: string | null;
  color: string;
  bgColor: string;
  scale: number;
} {
  if (points >= 3000) {
    return { label: 'LEGENDARY!', color: '#a855f7', bgColor: 'rgba(168, 85, 247, 0.95)', scale: 1.5 };
  }
  if (points >= 2000) {
    return { label: 'INCREDIBLE!', color: '#ec4899', bgColor: 'rgba(236, 72, 153, 0.95)', scale: 1.4 };
  }
  if (points >= 1000) {
    return { label: 'AMAZING!', color: '#f97316', bgColor: 'rgba(249, 115, 22, 0.95)', scale: 1.3 };
  }
  if (points >= 500) {
    return { label: 'GREAT!', color: '#fbbf24', bgColor: 'rgba(251, 191, 36, 0.95)', scale: 1.2 };
  }
  if (points >= 300) {
    return { label: 'NICE!', color: '#22c55e', bgColor: 'rgba(34, 197, 94, 0.95)', scale: 1.1 };
  }
  return { label: null, color: '#22c55e', bgColor: 'rgba(34, 197, 94, 0.9)', scale: 1 };
}

/**
 * Floating points indicator that appears when scoring
 * Shows "+X points" floating up and fading out with tier-based styling
 */
function PointsPopupComponent({ 
  points, 
  triggerKey = 0,
  position = { x: 0, y: 0 },
}: PointsPopupProps) {
  const [displayPoints, setDisplayPoints] = useState(points);
  const [prevPoints, setPrevPoints] = useState(points);
  const [showPopup, setShowPopup] = useState(false);
  const [prevTriggerKey, setPrevTriggerKey] = useState(triggerKey);
  // Version counter to trigger effect re-runs
  const [version, setVersion] = useState(0);

  // Use derived state pattern for all state updates
  const pointsChanged = points !== prevPoints;
  const triggerChanged = triggerKey !== prevTriggerKey;

  if (pointsChanged || triggerChanged) {
    if (points > 0) {
      setDisplayPoints(points);
      setShowPopup(true);
      setVersion(v => v + 1);
    }
    setPrevPoints(points);
    setPrevTriggerKey(triggerKey);
  }

  // Timer effect to auto-hide after duration
  useEffect(() => {
    if (!showPopup) return;

    // Longer duration for bigger scores
    const duration = displayPoints >= 1000 ? 2000 : 1300;
    const timer = setTimeout(() => {
      setShowPopup(false);
    }, duration);

    return () => clearTimeout(timer);
  }, [version, showPopup, displayPoints]);

  if (!showPopup || displayPoints <= 0) return null;

  const tier = getPointsTier(displayPoints);

  return (
    <AnimatePresence>
      {showPopup && (
        <motion.div
          key={`points-${triggerKey}-${displayPoints}`}
          variants={pointsPopupVariants}
          initial="initial"
          animate="animate"
          className="absolute pointer-events-none z-30"
          style={{
            left: position.x,
            top: position.y,
          }}
        >
          <motion.div 
            className="flex flex-col items-center"
            initial={{ scale: tier.scale * 0.5 }}
            animate={{ scale: tier.scale }}
            transition={{ type: 'spring', stiffness: 400, damping: 15 }}
          >
            {/* Tier label for big scores */}
            {tier.label && (
              <motion.div
                className="px-3 py-0.5 rounded-full mb-1 font-black text-sm tracking-wider"
                style={{ 
                  backgroundColor: tier.bgColor,
                  color: 'white',
                  textShadow: '0 1px 2px rgba(0,0,0,0.3)',
                }}
                initial={{ y: 10, opacity: 0, scale: 0.8 }}
                animate={{ y: 0, opacity: 1, scale: 1 }}
                transition={{ delay: 0.1, type: 'spring', stiffness: 500 }}
              >
                {tier.label}
              </motion.div>
            )}
            
            {/* Points display */}
            <motion.div 
              className="px-4 py-2 rounded-lg shadow-lg backdrop-blur-sm"
              style={{ 
                backgroundColor: tier.bgColor,
                boxShadow: `0 0 20px ${tier.color}80, 0 4px 12px rgba(0,0,0,0.3)`,
              }}
              animate={displayPoints >= 500 ? {
                boxShadow: [
                  `0 0 20px ${tier.color}80, 0 4px 12px rgba(0,0,0,0.3)`,
                  `0 0 35px ${tier.color}aa, 0 4px 12px rgba(0,0,0,0.3)`,
                  `0 0 20px ${tier.color}80, 0 4px 12px rgba(0,0,0,0.3)`,
                ],
              } : undefined}
              transition={{ duration: 0.6, repeat: displayPoints >= 500 ? 2 : 0 }}
            >
              <span 
                className="text-lg font-bold text-white whitespace-nowrap"
                style={{ textShadow: '0 1px 3px rgba(0,0,0,0.4)' }}
              >
                +{displayPoints.toLocaleString()}
              </span>
            </motion.div>
            
            {/* Sparkle particles for big scores */}
            {displayPoints >= 500 && (
              <div className="absolute inset-0 pointer-events-none">
                {[...Array(displayPoints >= 1000 ? 8 : 4)].map((_, i) => (
                  <motion.div
                    key={i}
                    className="absolute w-2 h-2 rounded-full"
                    style={{
                      backgroundColor: tier.color,
                      left: '50%',
                      top: '50%',
                    }}
                    initial={{ scale: 0, x: 0, y: 0 }}
                    animate={{
                      scale: [0, 1, 0],
                      x: [0, (Math.random() - 0.5) * 80],
                      y: [0, (Math.random() - 0.5) * 80],
                      opacity: [1, 1, 0],
                    }}
                    transition={{
                      duration: 0.8,
                      delay: i * 0.05,
                      ease: 'easeOut',
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

export const PointsPopup = memo(PointsPopupComponent);
PointsPopup.displayName = 'PointsPopup';
