'use client';

import { memo, useCallback, type KeyboardEvent } from 'react';
import { motion } from 'motion/react';
import Image from 'next/image';
import type { Achievement, AchievementCategory } from '@/types/gamification';

export interface AchievementBadgeProps {
  /** Achievement data */
  achievement: Achievement;
  /** Size variant */
  size?: 'sm' | 'md' | 'lg';
  /** Show tooltip on hover */
  showTooltip?: boolean;
  /** Callback when badge is clicked */
  onClick?: () => void;
}

/**
 * Get rarity border color based on progress thresholds
 * Higher progress = higher rarity
 */
function getRarityColor(achievement: Achievement): string {
  if (achievement.isSecret && !achievement.isUnlocked) return 'border-gray-600';
  
  // Rarity based on category and implied difficulty
  const rarityMap: Record<AchievementCategory, string> = {
    Score: 'border-yellow-500', // Gold
    Streak: 'border-orange-500', // Orange
    Games: 'border-blue-500', // Blue
    Challenge: 'border-purple-500', // Purple
    Special: 'border-pink-500', // Pink/legendary
  };
  
  return rarityMap[achievement.category] || 'border-gray-500';
}

/**
 * Get background gradient based on unlock status
 */
function getBackgroundStyle(achievement: Achievement): string {
  if (!achievement.isUnlocked) {
    return 'bg-gray-800/50 grayscale';
  }
  return 'bg-gradient-to-br from-yellow-500/20 to-amber-600/20';
}

/**
 * Individual achievement badge component
 * Shows locked/unlocked states with progress ring
 */
function AchievementBadgeComponent({
  achievement,
  size = 'md',
  showTooltip = true,
  onClick,
}: AchievementBadgeProps) {
  const {
    name,
    description,
    iconUrl,
    isUnlocked,
    progress,
    isSecret,
  } = achievement;
  
  const rarityColor = getRarityColor(achievement);
  const bgStyle = getBackgroundStyle(achievement);
  
  const sizeClasses = {
    sm: 'w-12 h-12',
    md: 'w-16 h-16',
    lg: 'w-20 h-20',
  };
  
  const iconSizes = {
    sm: 'w-6 h-6',
    md: 'w-8 h-8',
    lg: 'w-10 h-10',
  };

  const iconPixelSizes: Record<typeof size, number> = {
    sm: 24,
    md: 32,
    lg: 40,
  };
  
  // Calculate stroke dasharray for progress ring
  const radius = size === 'lg' ? 36 : size === 'md' ? 28 : 20;
  const circumference = 2 * Math.PI * radius;
  const progressOffset = circumference - (progress / 100) * circumference;
  
  // Display for secret achievements
  const displayName = isSecret && !isUnlocked ? '???' : name;
  const displayDescription = isSecret && !isUnlocked ? 'Secret Achievement' : description;
  const displayIcon = isSecret && !isUnlocked ? null : iconUrl;
  
  // Handle keyboard interaction for clickable badges
  const handleKeyDown = useCallback((e: KeyboardEvent) => {
    if (onClick && (e.key === 'Enter' || e.key === ' ')) {
      e.preventDefault();
      onClick();
    }
  }, [onClick]);
  
  return (
    <motion.div
      className="relative group"
      whileHover={{ scale: 1.05 }}
      whileTap={{ scale: 0.95 }}
      onClick={onClick}
      onKeyDown={onClick ? handleKeyDown : undefined}
      role={onClick ? 'button' : undefined}
      tabIndex={onClick ? 0 : undefined}
      aria-label={onClick ? `${displayName}: ${displayDescription}${!isUnlocked && progress > 0 ? `, ${progress}% complete` : ''}` : undefined}
    >
      {/* Badge container */}
      <div
        className={`relative ${sizeClasses[size]} rounded-full border-2 ${rarityColor} ${bgStyle} 
          flex items-center justify-center overflow-hidden cursor-pointer transition-all`}
      >
        {/* Progress ring (for locked achievements with progress) */}
        {!isUnlocked && progress > 0 && (
          <svg
            className="absolute inset-0 -rotate-90"
            viewBox={`0 0 ${(radius + 4) * 2} ${(radius + 4) * 2}`}
          >
            <circle
              cx={radius + 4}
              cy={radius + 4}
              r={radius}
              fill="none"
              stroke="rgba(255,255,255,0.1)"
              strokeWidth="3"
            />
            <motion.circle
              cx={radius + 4}
              cy={radius + 4}
              r={radius}
              fill="none"
              stroke="rgba(34, 211, 238, 0.8)"
              strokeWidth="3"
              strokeDasharray={circumference}
              initial={{ strokeDashoffset: circumference }}
              animate={{ strokeDashoffset: progressOffset }}
              transition={{ duration: 1, ease: 'easeOut' }}
              strokeLinecap="round"
            />
          </svg>
        )}
        
        {/* Icon */}
        {displayIcon ? (
          <Image
            src={displayIcon}
            alt={displayName}
            width={iconPixelSizes[size]}
            height={iconPixelSizes[size]}
            className={`${iconSizes[size]} object-contain ${!isUnlocked ? 'opacity-50' : ''}`}
          />
        ) : (
          <span className="text-2xl">{isSecret ? '❓' : '🏆'}</span>
        )}
        
        {/* Lock overlay for locked achievements */}
        {!isUnlocked && (
          <div className="absolute inset-0 flex items-center justify-center bg-black/40 rounded-full">
            <span className="text-sm">🔒</span>
          </div>
        )}
        
        {/* Glow effect for unlocked */}
        {isUnlocked && (
          <motion.div
            className="absolute inset-0 rounded-full pointer-events-none"
            animate={{
              boxShadow: [
                '0 0 10px rgba(251, 191, 36, 0.3)',
                '0 0 20px rgba(251, 191, 36, 0.5)',
                '0 0 10px rgba(251, 191, 36, 0.3)',
              ],
            }}
            transition={{ duration: 2, repeat: Infinity }}
          />
        )}
      </div>
      
      {/* Tooltip */}
      {showTooltip && (
        <div
          className="absolute z-20 bottom-full left-1/2 -translate-x-1/2 mb-2 px-3 py-2 
            rounded-lg bg-gray-900 text-white text-xs whitespace-nowrap
            opacity-0 invisible group-hover:opacity-100 group-hover:visible
            transition-all pointer-events-none"
          style={{ boxShadow: '0 4px 12px rgba(0,0,0,0.3)' }}
        >
          <p className="font-semibold">{displayName}</p>
          <p className="text-gray-400">{displayDescription}</p>
          {!isUnlocked && progress > 0 && (
            <p className="text-cyan-400 mt-1">{progress}% complete</p>
          )}
          <div
            className="absolute top-full left-1/2 -translate-x-1/2 w-0 h-0 
              border-l-4 border-r-4 border-t-4 border-transparent border-t-gray-900"
          />
        </div>
      )}
    </motion.div>
  );
}

export const AchievementBadge = memo(AchievementBadgeComponent);
AchievementBadge.displayName = 'AchievementBadge';
