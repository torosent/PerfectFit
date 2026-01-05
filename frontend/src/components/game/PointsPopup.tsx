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
 * Floating points indicator that appears when scoring
 * Shows "+X points" floating up and fading out
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

    const timer = setTimeout(() => {
      setShowPopup(false);
    }, 1300);

    return () => clearTimeout(timer);
  }, [version, showPopup]);

  if (!showPopup || displayPoints <= 0) return null;

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
          <div className="px-3 py-1.5 rounded-lg bg-green-500/90 shadow-lg backdrop-blur-sm">
            <span className="text-lg font-bold text-white whitespace-nowrap">
              +{displayPoints.toLocaleString()}
            </span>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}

export const PointsPopup = memo(PointsPopupComponent);
PointsPopup.displayName = 'PointsPopup';
