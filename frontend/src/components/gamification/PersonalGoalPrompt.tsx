'use client';

import { memo, useEffect, useMemo, useRef, useCallback } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import type { PersonalGoal } from '@/types/gamification';

export interface PersonalGoalPromptProps {
  /** Goal to display */
  goal: PersonalGoal | null;
  /** Whether the prompt should be visible */
  isVisible: boolean;
  /** Callback when dismissed */
  onDismiss: () => void;
  /** Auto-dismiss delay in ms (0 to disable) */
  autoDismissDelay?: number;
}

/**
 * Get motivational message for goal type
 */
function getMotivationalMessage(goal: PersonalGoal): string {
  const messages = {
    BeatAverage: [
      "Let's beat your average today! 💪",
      "You've got this! Aim higher!",
      "Time to level up your game!",
    ],
    ImproveAccuracy: [
      "Focus and precision win the game!",
      "Every move counts - make them count!",
      "Quality over quantity today!",
    ],
    NewPersonalBest: [
      "Today could be your best day yet!",
      "Push your limits - a new record awaits!",
      "You're capable of greatness!",
    ],
  };
  
  const typeMessages = messages[goal.type] || messages.BeatAverage;

  // Deterministic selection (no Math.random())
  const source = `${goal.type}:${goal.description}`;
  let hash = 0;
  for (let i = 0; i < source.length; i++) {
    hash = (hash * 31 + source.charCodeAt(i)) | 0;
  }
  const index = Math.abs(hash) % typeMessages.length;
  return typeMessages[index];
}

/**
 * Goal notification toast that appears at game start
 */
function PersonalGoalPromptComponent({
  goal,
  isVisible,
  onDismiss,
  autoDismissDelay = 5000,
}: PersonalGoalPromptProps) {
  const dialogRef = useRef<HTMLDivElement>(null);

  const motivationalMessage = useMemo(() => {
    if (!goal || !isVisible) return '';
    return getMotivationalMessage(goal);
  }, [goal, isVisible]);
  
  // Memoize the escape handler
  const handleEscapeKey = useCallback((e: KeyboardEvent) => {
    if (e.key === 'Escape') {
      onDismiss();
    }
  }, [onDismiss]);
  
  // Global escape key listener when visible
  useEffect(() => {
    if (!isVisible) return;
    
    document.addEventListener('keydown', handleEscapeKey);
    return () => document.removeEventListener('keydown', handleEscapeKey);
  }, [isVisible, handleEscapeKey]);
  
  // Auto-dismiss timer
  useEffect(() => {
    if (!isVisible || autoDismissDelay === 0) return;
    
    const timer = setTimeout(() => {
      onDismiss();
    }, autoDismissDelay);
    
    return () => clearTimeout(timer);
  }, [isVisible, autoDismissDelay, onDismiss]);
  
  if (!goal) return null;
  
  return (
    <AnimatePresence>
      {isVisible && (
        <motion.div
          ref={dialogRef}
          initial={{ opacity: 0, y: -50, x: '-50%' }}
          animate={{ opacity: 1, y: 0, x: '-50%' }}
          exit={{ opacity: 0, y: -30, x: '-50%' }}
          className="fixed top-4 left-1/2 z-50 w-full max-w-sm px-4"
          role="dialog"
          aria-label="Personal goal notification"
        >
          <div
            className="p-4 rounded-xl backdrop-blur-md border border-white/20"
            style={{
              background: 'linear-gradient(135deg, rgba(13, 36, 61, 0.95), rgba(26, 54, 93, 0.95))',
              boxShadow: '0 10px 40px rgba(0,0,0,0.3)',
            }}
          >
            <div className="flex items-start gap-3">
              {/* Icon */}
              <div className="flex-shrink-0 w-10 h-10 rounded-xl bg-yellow-500/20 flex items-center justify-center">
                <span className="text-xl">🎯</span>
              </div>
              
              {/* Content */}
              <div className="flex-1">
                <p className="text-sm text-yellow-400 font-medium mb-1">
                  Today&apos;s Goal
                </p>
                <p className="text-white font-medium">
                  {goal.description}
                </p>
                <p className="text-sm text-gray-400 mt-1">
                  {motivationalMessage}
                </p>
                
                {/* Progress */}
                <div className="mt-2 flex items-center gap-2">
                  <div className="flex-1 h-1.5 rounded-full bg-white/10 overflow-hidden">
                    <motion.div
                      className="h-full rounded-full bg-yellow-400"
                      initial={{ width: 0 }}
                      animate={{ width: `${goal.progressPercentage}%` }}
                      transition={{ duration: 0.5 }}
                    />
                  </div>
                  <span className="text-xs text-gray-400">
                    {goal.progressPercentage}%
                  </span>
                </div>
              </div>
              
              {/* Dismiss button */}
              <button
                onClick={onDismiss}
                className="flex-shrink-0 w-6 h-6 rounded-full bg-white/10 flex items-center justify-center text-gray-400 hover:text-white hover:bg-white/20 transition-colors"
                aria-label="Dismiss"
              >
                ✕
              </button>
            </div>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}

export const PersonalGoalPrompt = memo(PersonalGoalPromptComponent);
PersonalGoalPrompt.displayName = 'PersonalGoalPrompt';
