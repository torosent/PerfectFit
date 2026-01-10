'use client';

import { memo, useState, useCallback, useEffect, useRef, useId, type KeyboardEvent } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { useStreaks } from '@/hooks/useStreaks';

export interface StreakFreezeButtonProps {
  /** Compact mode */
  compact?: boolean;
  /** Callback after freeze is used */
  onFreezeUsed?: () => void;
}

/**
 * Button to use a streak freeze token
 * Shows confirmation modal before use
 */
function StreakFreezeButtonComponent({
  compact = false,
  onFreezeUsed,
}: StreakFreezeButtonProps) {
  const { freezeTokens, isAtRisk, useFreeze: consumeFreeze, isLoading } = useStreaks();
  const [showConfirm, setShowConfirm] = useState(false);
  const [isUsing, setIsUsing] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);
  
  const canUseFreeze = freezeTokens > 0 && isAtRisk;
  
  const handleUseFreeze = async () => {
    if (!canUseFreeze || isUsing) return;
    
    setIsUsing(true);
    try {
      await consumeFreeze();
      setShowConfirm(false);
      setShowSuccess(true);
      setTimeout(() => setShowSuccess(false), 2000);
      onFreezeUsed?.();
    } catch (error) {
      console.error('Failed to use streak freeze:', error);
    } finally {
      setIsUsing(false);
    }
  };
  
  if (compact) {
    return (
      <>
        <motion.button
          onClick={() => setShowConfirm(true)}
          disabled={!canUseFreeze || isLoading}
          className={`flex items-center gap-1 px-2 py-1 rounded-lg text-sm transition-colors ${
            canUseFreeze
              ? 'bg-cyan-500/20 text-cyan-400 hover:bg-cyan-500/30'
              : 'bg-white/5 text-gray-500 cursor-not-allowed'
          }`}
          whileHover={canUseFreeze ? { scale: 1.05 } : {}}
          whileTap={canUseFreeze ? { scale: 0.95 } : {}}
          title={
            !isAtRisk
              ? "Streak not at risk"
              : freezeTokens === 0
              ? "No freeze tokens available"
              : "Use freeze token"
          }
        >
          <span>❄️</span>
          <span>{freezeTokens}</span>
        </motion.button>
        
        {/* Confirmation modal */}
        <AnimatePresence>
          {showConfirm && (
            <ConfirmModal
              freezeTokens={freezeTokens}
              onConfirm={handleUseFreeze}
              onCancel={() => setShowConfirm(false)}
              isUsing={isUsing}
            />
          )}
        </AnimatePresence>
        
        {/* Success toast */}
        <AnimatePresence>
          {showSuccess && <SuccessToast />}
        </AnimatePresence>
      </>
    );
  }
  
  return (
    <>
      <motion.button
        onClick={() => setShowConfirm(true)}
        disabled={!canUseFreeze || isLoading}
        className={`flex items-center gap-2 px-4 py-2 rounded-xl transition-colors ${
          canUseFreeze
            ? 'bg-cyan-500/20 border border-cyan-500/30 text-cyan-400 hover:bg-cyan-500/30'
            : 'bg-white/5 border border-white/10 text-gray-500 cursor-not-allowed'
        }`}
        whileHover={canUseFreeze ? { scale: 1.02 } : {}}
        whileTap={canUseFreeze ? { scale: 0.98 } : {}}
      >
        <span className="text-xl">❄️</span>
        <div className="text-left">
          <p className="font-medium">Use Freeze Token</p>
          <p className="text-xs opacity-70">{freezeTokens} remaining</p>
        </div>
      </motion.button>
      
      {/* Confirmation modal */}
      <AnimatePresence>
        {showConfirm && (
          <ConfirmModal
            freezeTokens={freezeTokens}
            onConfirm={handleUseFreeze}
            onCancel={() => setShowConfirm(false)}
            isUsing={isUsing}
          />
        )}
      </AnimatePresence>
      
      {/* Success toast */}
      <AnimatePresence>
        {showSuccess && <SuccessToast />}
      </AnimatePresence>
    </>
  );
}

/**
 * Confirmation modal for using freeze token
 */
function ConfirmModal({
  freezeTokens,
  onConfirm,
  onCancel,
  isUsing,
}: {
  freezeTokens: number;
  onConfirm: () => void;
  onCancel: () => void;
  isUsing: boolean;
}) {
  const titleId = useId();
  const confirmButtonRef = useRef<HTMLButtonElement>(null);
  
  // Focus the confirm button when modal opens
  useEffect(() => {
    confirmButtonRef.current?.focus();
  }, []);
  
  // Handle Escape key to close modal
  const handleKeyDown = useCallback((e: KeyboardEvent<HTMLDivElement>) => {
    if (e.key === 'Escape') {
      e.preventDefault();
      onCancel();
    }
  }, [onCancel]);
  
  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      role="dialog"
      aria-modal="true"
      aria-labelledby={titleId}
      className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/70 backdrop-blur-sm"
      onClick={onCancel}
      onKeyDown={handleKeyDown}
    >
      <motion.div
        initial={{ scale: 0.9, y: 20 }}
        animate={{ scale: 1, y: 0 }}
        exit={{ scale: 0.9, y: 20 }}
        className="max-w-sm w-full p-6 rounded-2xl text-center"
        style={{ background: 'linear-gradient(135deg, rgba(13, 36, 61, 0.98), rgba(26, 54, 93, 0.98))' }}
        onClick={(e) => e.stopPropagation()}
      >
        <span className="text-5xl mb-4 block">❄️</span>
        <h3 id={titleId} className="text-xl font-bold text-white mb-2">Use Streak Freeze?</h3>
        <p className="text-gray-400 mb-4">
          This will protect your streak for today. You have {freezeTokens} freeze token{freezeTokens !== 1 ? 's' : ''} remaining.
        </p>
        
        <div className="flex gap-3">
          <motion.button
            onClick={onCancel}
            className="flex-1 py-2 px-4 rounded-lg font-medium text-gray-400 bg-white/5 hover:bg-white/10"
            whileHover={{ scale: 1.02 }}
            whileTap={{ scale: 0.98 }}
          >
            Cancel
          </motion.button>
          <motion.button
            ref={confirmButtonRef}
            onClick={onConfirm}
            disabled={isUsing}
            className="flex-1 py-2 px-4 rounded-lg font-medium text-white bg-gradient-to-r from-cyan-500 to-blue-500 disabled:opacity-50"
            whileHover={{ scale: 1.02 }}
            whileTap={{ scale: 0.98 }}
          >
            {isUsing ? 'Using...' : 'Use Freeze'}
          </motion.button>
        </div>
      </motion.div>
    </motion.div>
  );
}

/**
 * Success toast after using freeze
 */
function SuccessToast() {
  return (
    <motion.div
      initial={{ opacity: 0, y: 50 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      className="fixed bottom-20 left-1/2 -translate-x-1/2 z-50 px-4 py-3 rounded-xl bg-green-500/20 border border-green-500/30 text-green-400 flex items-center gap-2"
    >
      <span>✓</span>
      <span>Streak protected!</span>
    </motion.div>
  );
}

export const StreakFreezeButton = memo(StreakFreezeButtonComponent);
StreakFreezeButton.displayName = 'StreakFreezeButton';
