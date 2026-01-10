'use client';

import { memo, useState, useCallback, useMemo, useId } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import type { Achievement, AchievementCategory } from '@/types/gamification';
import { AchievementBadge } from './AchievementBadge';

export interface AchievementsPanelProps {
  /** All achievements */
  achievements: Achievement[];
  /** Whether data is loading */
  isLoading?: boolean;
  /** Callback when achievement is clicked */
  onAchievementClick?: (achievement: Achievement) => void;
}

const CATEGORIES: { key: AchievementCategory | 'all'; label: string; icon: string }[] = [
  { key: 'all', label: 'All', icon: '🏆' },
  { key: 'Score', label: 'Score', icon: '💯' },
  { key: 'Streak', label: 'Streak', icon: '🔥' },
  { key: 'Games', label: 'Games', icon: '🎮' },
  { key: 'Challenge', label: 'Challenge', icon: '📋' },
  { key: 'Special', label: 'Special', icon: '⭐' },
];

/**
 * Categorized achievements grid with tabs
 */
function AchievementsPanelComponent({
  achievements,
  isLoading = false,
  onAchievementClick,
}: AchievementsPanelProps) {
  const [activeCategory, setActiveCategory] = useState<AchievementCategory | 'all'>('all');
  const uniqueId = useId();
  
  const filteredAchievements = useMemo(() => {
    if (activeCategory === 'all') return achievements;
    return achievements.filter(a => a.category === activeCategory);
  }, [achievements, activeCategory]);
  
  // Sort: unlocked first, then by progress
  const sortedAchievements = useMemo(() => {
    return [...filteredAchievements].sort((a, b) => {
      if (a.isUnlocked && !b.isUnlocked) return -1;
      if (!a.isUnlocked && b.isUnlocked) return 1;
      return b.progress - a.progress;
    });
  }, [filteredAchievements]);
  
  // Stats
  const totalUnlocked = achievements.filter(a => a.isUnlocked).length;
  const categoryStats = useMemo(() => {
    const stats: Record<string, { unlocked: number; total: number }> = { all: { unlocked: 0, total: 0 } };
    achievements.forEach(a => {
      if (!stats[a.category]) {
        stats[a.category] = { unlocked: 0, total: 0 };
      }
      stats[a.category].total++;
      stats.all.total++;
      if (a.isUnlocked) {
        stats[a.category].unlocked++;
        stats.all.unlocked++;
      }
    });
    return stats;
  }, [achievements]);
  
  const handleCategoryClick = useCallback((category: AchievementCategory | 'all') => {
    setActiveCategory(category);
  }, []);
  
  // Generate tab and panel IDs for accessibility
  const getTabId = (key: string) => `${uniqueId}-tab-${key}`;
  const getPanelId = (key: string) => `${uniqueId}-panel-${key}`;
  
  return (
    <div className="rounded-xl bg-white/5 backdrop-blur-md overflow-hidden">
      {/* Header */}
      <div className="p-4 border-b border-white/10">
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-bold text-white">Achievements</h3>
          <span className="text-sm text-gray-400">
            {totalUnlocked} / {achievements.length} unlocked
          </span>
        </div>
      </div>
      
      {/* Category tabs */}
      <div role="tablist" aria-label="Achievement categories" className="flex overflow-x-auto scrollbar-hide border-b border-white/10">
        {CATEGORIES.map(cat => {
          const stats = categoryStats[cat.key] || { unlocked: 0, total: 0 };
          const isActive = activeCategory === cat.key;
          
          return (
            <button
              key={cat.key}
              id={getTabId(cat.key)}
              role="tab"
              aria-selected={isActive}
              aria-controls={getPanelId(cat.key)}
              onClick={() => handleCategoryClick(cat.key)}
              className={`flex-shrink-0 px-4 py-3 text-sm font-medium transition-colors ${
                isActive
                  ? 'text-yellow-400 bg-yellow-500/10 border-b-2 border-yellow-400'
                  : 'text-gray-400 hover:text-white hover:bg-white/5'
              }`}
            >
              <span className="mr-1">{cat.icon}</span>
              <span>{cat.label}</span>
              <span className="ml-1 text-xs opacity-70">
                ({stats.unlocked}/{stats.total})
              </span>
            </button>
          );
        })}
      </div>
      
      {/* Achievement grid */}
      <div
        id={getPanelId(activeCategory)}
        role="tabpanel"
        aria-labelledby={getTabId(activeCategory)}
        className="p-4"
      >
        {/* Loading state */}
        {isLoading && (
          <div className="grid grid-cols-4 sm:grid-cols-6 md:grid-cols-8 gap-3">
            {[...Array(16)].map((_, i) => (
              <div key={i} className="aspect-square rounded-full bg-white/5 animate-pulse" />
            ))}
          </div>
        )}
        
        {/* Empty state */}
        {!isLoading && sortedAchievements.length === 0 && (
          <div className="py-8 text-center">
            <span className="text-4xl mb-3 block">🏆</span>
            <p className="text-gray-400">No achievements in this category</p>
          </div>
        )}
        
        {/* Achievements grid */}
        {!isLoading && sortedAchievements.length > 0 && (
          <AnimatePresence mode="wait">
            <motion.div
              key={activeCategory}
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              transition={{ duration: 0.2 }}
              className="grid grid-cols-4 sm:grid-cols-6 md:grid-cols-8 gap-3"
            >
              {sortedAchievements.map((achievement, index) => (
                <motion.div
                  key={achievement.id}
                  initial={{ opacity: 0, scale: 0.8 }}
                  animate={{ opacity: 1, scale: 1 }}
                  transition={{ delay: index * 0.02 }}
                  className="flex justify-center"
                >
                  <AchievementBadge
                    achievement={achievement}
                    size="md"
                    onClick={onAchievementClick ? () => onAchievementClick(achievement) : undefined}
                  />
                </motion.div>
              ))}
            </motion.div>
          </AnimatePresence>
        )}
      </div>
    </div>
  );
}

export const AchievementsPanel = memo(AchievementsPanelComponent);
AchievementsPanel.displayName = 'AchievementsPanel';
