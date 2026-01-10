'use client';

import { memo } from 'react';
import { motion } from 'motion/react';
import { useStreaks } from '@/hooks/useStreaks';
import { useChallenges } from '@/hooks/useChallenges';
import { useAchievements } from '@/hooks/useAchievements';
import { useSeasonPass } from '@/hooks/useSeasonPass';
import { usePersonalGoals } from '@/hooks/usePersonalGoals';
import { StreakDisplay } from './StreakDisplay';
import { StreakFreezeButton } from './StreakFreezeButton';
import { ChallengesList } from './ChallengesList';
import { SeasonPassTrack } from './SeasonPassTrack';
import { AchievementsPanel } from './AchievementsPanel';
import { PersonalGoalCard } from './PersonalGoalCard';

export interface GamificationDashboardProps {
  /** Whether to show in compact mode */
  compact?: boolean;
}

/**
 * Main gamification dashboard page
 * Shows all gamification features in one view
 */
function GamificationDashboardComponent({ compact = false }: GamificationDashboardProps) {
  const {
    currentStreak,
    longestStreak,
    freezeTokens,
    isAtRisk,
    resetTime,
    isLoading: streakLoading,
  } = useStreaks();
  
  const {
    challenges,
    isLoading: challengesLoading,
  } = useChallenges();
  
  const {
    achievements,
    isLoading: achievementsLoading,
  } = useAchievements();
  
  const {
    seasonPass,
    claimReward,
    isLoading: seasonLoading,
  } = useSeasonPass();
  
  const {
    activeGoals,
    isLoading: goalsLoading,
  } = usePersonalGoals();
  
  const isLoading = streakLoading || challengesLoading || achievementsLoading || seasonLoading || goalsLoading;
  
  if (compact) {
    return (
      <div className="space-y-4">
        {/* Streak overview */}
        <StreakDisplay
          currentStreak={currentStreak}
          longestStreak={longestStreak}
          freezeTokens={freezeTokens}
          isAtRisk={isAtRisk}
          compact
        />
        
        {/* Quick stats */}
        <div className="grid grid-cols-3 gap-2 text-center">
          <div className="p-2 rounded-lg bg-white/5">
            <p className="text-lg font-bold text-white">{challenges.filter(c => c.isCompleted).length}/{challenges.length}</p>
            <p className="text-xs text-gray-400">Challenges</p>
          </div>
          <div className="p-2 rounded-lg bg-white/5">
            <p className="text-lg font-bold text-white">{achievements.filter(a => a.isUnlocked).length}</p>
            <p className="text-xs text-gray-400">Achievements</p>
          </div>
          <div className="p-2 rounded-lg bg-white/5">
            <p className="text-lg font-bold text-white">Tier {seasonPass?.currentTier || 0}</p>
            <p className="text-xs text-gray-400">Season</p>
          </div>
        </div>
      </div>
    );
  }
  
  return (
    <div className="space-y-6 p-4 max-w-4xl mx-auto">
      {/* Header */}
      <motion.div
        initial={{ opacity: 0, y: -20 }}
        animate={{ opacity: 1, y: 0 }}
        className="text-center"
      >
        <h1 className="text-3xl font-bold text-white mb-2">🎮 Gamification</h1>
        <p className="text-gray-400">Track your progress and earn rewards</p>
      </motion.div>
      
      {/* Streak Section */}
      <motion.section
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.1 }}
      >
        <h2 className="text-lg font-semibold text-white mb-3 flex items-center gap-2">
          <span>🔥</span> Daily Streak
        </h2>
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="flex-1">
            <StreakDisplay
              currentStreak={currentStreak}
              longestStreak={longestStreak}
              freezeTokens={freezeTokens}
              isAtRisk={isAtRisk}
              resetTime={resetTime}
            />
          </div>
          {isAtRisk && freezeTokens > 0 && (
            <div className="flex items-center justify-center">
              <StreakFreezeButton />
            </div>
          )}
        </div>
      </motion.section>
      
      {/* Personal Goals */}
      {activeGoals.length > 0 && (
        <motion.section
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.15 }}
        >
          <h2 className="text-lg font-semibold text-white mb-3 flex items-center gap-2">
            <span>🎯</span> Personal Goals
          </h2>
          <div className="space-y-3">
            {activeGoals.map(goal => (
              <PersonalGoalCard key={goal.id} goal={goal} />
            ))}
          </div>
        </motion.section>
      )}
      
      {/* Season Pass */}
      {seasonPass && (
        <motion.section
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
        >
          <h2 className="text-lg font-semibold text-white mb-3 flex items-center gap-2">
            <span>⭐</span> Season Pass
          </h2>
          <SeasonPassTrack
            seasonPass={seasonPass}
            onClaimReward={claimReward}
          />
        </motion.section>
      )}
      
      {/* Challenges */}
      <motion.section
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.25 }}
      >
        <h2 className="text-lg font-semibold text-white mb-3 flex items-center gap-2">
          <span>📋</span> Challenges
        </h2>
        <ChallengesList
          challenges={challenges}
          isLoading={challengesLoading}
        />
      </motion.section>
      
      {/* Achievements */}
      <motion.section
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.3 }}
      >
        <h2 className="text-lg font-semibold text-white mb-3 flex items-center gap-2">
          <span>🏆</span> Achievements
        </h2>
        <AchievementsPanel
          achievements={achievements}
          isLoading={achievementsLoading}
        />
      </motion.section>
      
      {/* Loading overlay */}
      {isLoading && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="animate-spin w-8 h-8 border-2 border-white border-t-transparent rounded-full" />
        </div>
      )}
    </div>
  );
}

export const GamificationDashboard = memo(GamificationDashboardComponent);
GamificationDashboard.displayName = 'GamificationDashboard';
