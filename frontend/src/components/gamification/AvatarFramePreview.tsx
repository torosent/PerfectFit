'use client';

import { memo } from 'react';
import { motion } from 'motion/react';
import type { CosmeticInfo, CosmeticRarity } from '@/types/gamification';

export interface AvatarFramePreviewProps {
  /** User's avatar emoji */
  avatar: string;
  /** Frame cosmetic data */
  frame: CosmeticInfo | null;
  /** Size variant */
  size?: 'sm' | 'md' | 'lg';
}

/**
 * Get frame style based on rarity
 */
function getFrameStyle(rarity?: CosmeticRarity): {
  border: string;
  glow: string;
  animation: boolean;
} {
  switch (rarity) {
    case 'Legendary':
      return {
        border: 'border-4 border-yellow-500',
        glow: '0 0 20px rgba(234, 179, 8, 0.5)',
        animation: true,
      };
    case 'Epic':
      return {
        border: 'border-4 border-purple-500',
        glow: '0 0 15px rgba(168, 85, 247, 0.4)',
        animation: true,
      };
    case 'Rare':
      return {
        border: 'border-3 border-blue-500',
        glow: '0 0 10px rgba(59, 130, 246, 0.3)',
        animation: false,
      };
    case 'Common':
    default:
      return {
        border: 'border-2 border-gray-500',
        glow: 'none',
        animation: false,
      };
  }
}

/**
 * Avatar display with cosmetic frame applied
 */
function AvatarFramePreviewComponent({
  avatar,
  frame,
  size = 'md',
}: AvatarFramePreviewProps) {
  const sizeClasses = {
    sm: 'w-10 h-10 text-xl',
    md: 'w-14 h-14 text-2xl',
    lg: 'w-20 h-20 text-4xl',
  };
  
  const frameStyle = frame ? getFrameStyle(frame.rarity) : getFrameStyle('Common');
  
  return (
    <motion.div
      className={`relative ${sizeClasses[size]} rounded-full ${frameStyle.border} bg-slate-800 flex items-center justify-center`}
      style={{ boxShadow: frameStyle.glow }}
      animate={frameStyle.animation ? {
        boxShadow: [
          frameStyle.glow,
          frameStyle.glow.replace(/[\d.]+\)$/, '0.7)'),
          frameStyle.glow,
        ],
      } : {}}
      transition={frameStyle.animation ? { duration: 2, repeat: Infinity } : {}}
    >
      {/* Avatar emoji */}
      <span className="select-none">{avatar}</span>
      
      {/* Frame overlay for special frames */}
      {frame?.assetUrl && (
        <img
          src={frame.assetUrl}
          alt={frame.name}
          className="absolute inset-0 w-full h-full object-cover pointer-events-none"
        />
      )}
      
      {/* Decorative elements for legendary frames */}
      {frame?.rarity === 'Legendary' && (
        <>
          <motion.div
            className="absolute -top-1 -right-1 w-3 h-3 rounded-full bg-yellow-400"
            animate={{
              scale: [1, 1.2, 1],
              opacity: [0.7, 1, 0.7],
            }}
            transition={{ duration: 1.5, repeat: Infinity }}
          />
          <motion.div
            className="absolute -bottom-1 -left-1 w-2 h-2 rounded-full bg-yellow-400"
            animate={{
              scale: [1, 1.2, 1],
              opacity: [0.7, 1, 0.7],
            }}
            transition={{ duration: 1.5, repeat: Infinity, delay: 0.5 }}
          />
        </>
      )}
    </motion.div>
  );
}

export const AvatarFramePreview = memo(AvatarFramePreviewComponent);
AvatarFramePreview.displayName = 'AvatarFramePreview';
