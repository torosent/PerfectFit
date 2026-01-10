'use client';

import { memo, useState, useCallback } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { useAuthStore, useIsAuthenticated } from '@/lib/stores/auth-store';
import { useStreaks } from '@/hooks/useStreaks';
import { useChallenges } from '@/hooks/useChallenges';
import { useAchievements } from '@/hooks/useAchievements';
import { useSeasonPass } from '@/hooks/useSeasonPass';
import { useCosmetics } from '@/hooks/useCosmetics';
import { usePersonalGoals } from '@/hooks/usePersonalGoals';
import { StreakDisplay } from '@/components/gamification/StreakDisplay';
import { StreakFreezeButton } from '@/components/gamification/StreakFreezeButton';
import { ChallengesList } from '@/components/gamification/ChallengesList';
import { SeasonPassTrack } from '@/components/gamification/SeasonPassTrack';
import { AchievementsPanel } from '@/components/gamification/AchievementsPanel';
import { PersonalGoalCard } from '@/components/gamification/PersonalGoalCard';
import type { Cosmetic, CosmeticType } from '@/types/gamification';
import Link from 'next/link';

type Tab = 'overview' | 'achievements' | 'cosmetics' | 'challenges';

const TABS: { key: Tab; label: string; icon: string }[] = [
  { key: 'overview', label: 'Overview', icon: '📊' },
  { key: 'achievements', label: 'Achievements', icon: '🏆' },
  { key: 'cosmetics', label: 'Cosmetics', icon: '🎨' },
  { key: 'challenges', label: 'Challenges', icon: '📋' },
];

const COSMETIC_TYPES: { key: CosmeticType; label: string; icon: string }[] = [
  { key: 'BoardTheme', label: 'Board Themes', icon: '🎮' },
  { key: 'AvatarFrame', label: 'Avatar Frames', icon: '🖼️' },
  { key: 'Badge', label: 'Badges', icon: '🏅' },
];

const RARITY_COLORS: Record<string, string> = {
  Common: 'border-gray-500 bg-gray-500/10',
  Rare: 'border-blue-500 bg-blue-500/10',
  Epic: 'border-purple-500 bg-purple-500/10',
  Legendary: 'border-yellow-500 bg-yellow-500/10',
};

/**
 * Cosmetic card component for displaying and equipping cosmetics
 */
const CosmeticCard = memo(function CosmeticCard({
  cosmetic,
  onEquip,
  isEquipping,
}: {
  cosmetic: Cosmetic;
  onEquip: (id: number) => void;
  isEquipping: boolean;
}) {
  const rarityClass = RARITY_COLORS[cosmetic.rarity] || RARITY_COLORS.Common;

  return (
    <motion.div
      whileHover={{ scale: 1.02 }}
      className={`relative p-4 rounded-xl border-2 ${rarityClass} ${
        !cosmetic.isOwned ? 'opacity-50 grayscale' : ''
      }`}
    >
      {/* Equipped badge */}
      {cosmetic.isEquipped && (
        <div className="absolute -top-2 -right-2 px-2 py-0.5 text-xs font-bold bg-green-500 text-white rounded-full">
          Equipped
        </div>
      )}

      {/* Preview */}
      <div className="aspect-square rounded-lg bg-white/5 mb-3 flex items-center justify-center overflow-hidden">
        {cosmetic.previewUrl ? (
          <img
            src={cosmetic.previewUrl}
            alt={cosmetic.name}
            className="w-full h-full object-cover"
          />
        ) : (
          <span className="text-4xl">
            {cosmetic.type === 'BoardTheme' ? '🎮' : cosmetic.type === 'AvatarFrame' ? '🖼️' : '🏅'}
          </span>
        )}
      </div>

      {/* Info */}
      <h4 className="font-semibold text-white text-sm truncate">{cosmetic.name}</h4>
      <p className="text-xs text-gray-400 truncate mb-2">{cosmetic.description}</p>

      {/* Rarity */}
      <span
        className={`inline-block px-2 py-0.5 text-xs rounded-full ${
          cosmetic.rarity === 'Common'
            ? 'bg-gray-600 text-gray-200'
            : cosmetic.rarity === 'Rare'
            ? 'bg-blue-600 text-blue-100'
            : cosmetic.rarity === 'Epic'
            ? 'bg-purple-600 text-purple-100'
            : 'bg-yellow-600 text-yellow-100'
        }`}
      >
        {cosmetic.rarity}
      </span>

      {/* Equip button */}
      {cosmetic.isOwned && !cosmetic.isEquipped && (
        <button
          onClick={() => onEquip(cosmetic.id)}
          disabled={isEquipping}
          className="mt-3 w-full py-2 text-sm font-medium text-white rounded-lg transition-all disabled:opacity-50"
          style={{ background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)' }}
        >
          {isEquipping ? 'Equipping...' : 'Equip'}
        </button>
      )}

      {/* Locked overlay */}
      {!cosmetic.isOwned && (
        <div className="absolute inset-0 flex items-center justify-center bg-black/40 rounded-xl">
          <span className="text-2xl">🔒</span>
        </div>
      )}
    </motion.div>
  );
});

/**
 * Profile/Gamification Page
 * Shows user's achievements, cosmetics, challenges, and stats
 */
function ProfilePageContent() {
  const [activeTab, setActiveTab] = useState<Tab>('overview');
  const [activeCosmeticType, setActiveCosmeticType] = useState<CosmeticType>('BoardTheme');
  const [isEquipping, setIsEquipping] = useState(false);

  const user = useAuthStore((s) => s.user);
  const isAuthenticated = useIsAuthenticated();

  const {
    currentStreak,
    longestStreak,
    freezeTokens,
    isAtRisk,
    resetTime,
    isLoading: streakLoading,
  } = useStreaks();

  const { challenges, isLoading: challengesLoading } = useChallenges();
  const { achievements, isLoading: achievementsLoading } = useAchievements();
  const { seasonPass, claimReward, isLoading: seasonLoading } = useSeasonPass();
  const { cosmetics, byType, equipCosmetic, isLoading: cosmeticsLoading } = useCosmetics();
  const { activeGoals } = usePersonalGoals();

  const handleEquip = useCallback(
    async (cosmeticId: number) => {
      setIsEquipping(true);
      try {
        await equipCosmetic(String(cosmeticId));
      } finally {
        setIsEquipping(false);
      }
    },
    [equipCosmetic]
  );

  // Stats calculations
  const totalAchievements = achievements.length;
  const unlockedAchievements = achievements.filter((a) => a.isUnlocked).length;
  const completedChallenges = challenges.filter((c) => c.isCompleted).length;
  const ownedCosmetics = cosmetics.filter((c) => c.isOwned).length;

  // Not authenticated - show sign in prompt
  if (!isAuthenticated) {
    return (
      <div className="min-h-[60vh] flex items-center justify-center p-4">
        <div className="text-center">
          <span className="text-6xl mb-4 block">🔒</span>
          <h1 className="text-2xl font-bold text-white mb-2">Sign In Required</h1>
          <p className="text-gray-400 mb-6">
            Sign in to view your profile, achievements, and cosmetics.
          </p>
          <Link
            href="/login"
            className="inline-block py-3 px-6 text-white font-medium rounded-lg"
            style={{ background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)' }}
          >
            Sign In
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto p-4 pb-20">
      {/* Profile Header */}
      <motion.div
        initial={{ opacity: 0, y: -20 }}
        animate={{ opacity: 1, y: 0 }}
        className="text-center mb-8"
      >
        {/* Avatar */}
        <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-gradient-to-br from-teal-500 to-cyan-500 flex items-center justify-center text-4xl border-4 border-white/20">
          {user?.avatar || '😎'}
        </div>
        <h1 className="text-2xl font-bold text-white mb-1">
          {user?.displayName || 'Player'}
        </h1>
        <p className="text-gray-400">Level {Math.floor((seasonPass?.currentXP || 0) / 100) + 1}</p>

        {/* Quick Stats */}
        <div className="flex justify-center gap-6 mt-4">
          <div className="text-center">
            <p className="text-2xl font-bold text-white">{currentStreak}</p>
            <p className="text-xs text-gray-400">Day Streak</p>
          </div>
          <div className="text-center">
            <p className="text-2xl font-bold text-white">{unlockedAchievements}</p>
            <p className="text-xs text-gray-400">Achievements</p>
          </div>
          <div className="text-center">
            <p className="text-2xl font-bold text-white">{user?.highScore || 0}</p>
            <p className="text-xs text-gray-400">High Score</p>
          </div>
        </div>
      </motion.div>

      {/* Tab Navigation */}
      <div className="flex overflow-x-auto scrollbar-hide border-b border-white/10 mb-6">
        {TABS.map((tab) => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            className={`flex-shrink-0 px-4 py-3 text-sm font-medium transition-colors ${
              activeTab === tab.key
                ? 'text-teal-400 border-b-2 border-teal-400 bg-teal-500/10'
                : 'text-gray-400 hover:text-white hover:bg-white/5'
            }`}
          >
            <span className="mr-1">{tab.icon}</span>
            {tab.label}
          </button>
        ))}
      </div>

      {/* Tab Content */}
      <AnimatePresence mode="wait">
        <motion.div
          key={activeTab}
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: -10 }}
          transition={{ duration: 0.2 }}
        >
          {/* Overview Tab */}
          {activeTab === 'overview' && (
            <div className="space-y-6">
              {/* Streak Section */}
              <section>
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
              </section>

              {/* Personal Goals */}
              {activeGoals.length > 0 && (
                <section>
                  <h2 className="text-lg font-semibold text-white mb-3 flex items-center gap-2">
                    <span>🎯</span> Personal Goals
                  </h2>
                  <div className="space-y-3">
                    {activeGoals.map((goal) => (
                      <PersonalGoalCard key={goal.id} goal={goal} />
                    ))}
                  </div>
                </section>
              )}

              {/* Season Pass */}
              {seasonPass && (
                <section>
                  <h2 className="text-lg font-semibold text-white mb-3 flex items-center gap-2">
                    <span>⭐</span> Season Pass
                  </h2>
                  <SeasonPassTrack seasonPass={seasonPass} onClaimReward={claimReward} />
                </section>
              )}

              {/* Stats Grid */}
              <section>
                <h2 className="text-lg font-semibold text-white mb-3 flex items-center gap-2">
                  <span>📈</span> Stats
                </h2>
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
                  <div className="p-4 rounded-xl bg-white/5 text-center">
                    <p className="text-2xl font-bold text-white">{user?.gamesPlayed || 0}</p>
                    <p className="text-xs text-gray-400">Games Played</p>
                  </div>
                  <div className="p-4 rounded-xl bg-white/5 text-center">
                    <p className="text-2xl font-bold text-white">{longestStreak}</p>
                    <p className="text-xs text-gray-400">Best Streak</p>
                  </div>
                  <div className="p-4 rounded-xl bg-white/5 text-center">
                    <p className="text-2xl font-bold text-white">{completedChallenges}</p>
                    <p className="text-xs text-gray-400">Challenges Done</p>
                  </div>
                  <div className="p-4 rounded-xl bg-white/5 text-center">
                    <p className="text-2xl font-bold text-white">{ownedCosmetics}</p>
                    <p className="text-xs text-gray-400">Cosmetics</p>
                  </div>
                </div>
              </section>
            </div>
          )}

          {/* Achievements Tab */}
          {activeTab === 'achievements' && (
            <AchievementsPanel
              achievements={achievements}
              isLoading={achievementsLoading}
            />
          )}

          {/* Cosmetics Tab */}
          {activeTab === 'cosmetics' && (
            <div className="space-y-6">
              {/* Cosmetic Type Tabs */}
              <div className="flex gap-2 overflow-x-auto scrollbar-hide">
                {COSMETIC_TYPES.map((type) => (
                  <button
                    key={type.key}
                    onClick={() => setActiveCosmeticType(type.key)}
                    className={`flex-shrink-0 px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                      activeCosmeticType === type.key
                        ? 'text-white'
                        : 'text-gray-400 hover:text-white bg-white/5'
                    }`}
                    style={
                      activeCosmeticType === type.key
                        ? { background: 'linear-gradient(135deg, #14b8a6, #0ea5e9)' }
                        : undefined
                    }
                  >
                    <span className="mr-1">{type.icon}</span>
                    {type.label}
                  </button>
                ))}
              </div>

              {/* Cosmetics Grid */}
              {cosmeticsLoading ? (
                <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
                  {[...Array(8)].map((_, i) => (
                    <div key={i} className="aspect-square rounded-xl bg-white/5 animate-pulse" />
                  ))}
                </div>
              ) : (
                <AnimatePresence mode="wait">
                  <motion.div
                    key={activeCosmeticType}
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    exit={{ opacity: 0 }}
                    className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4"
                  >
                    {(activeCosmeticType === 'BoardTheme'
                      ? byType.boardThemes
                      : activeCosmeticType === 'AvatarFrame'
                      ? byType.avatarFrames
                      : byType.badges
                    ).map((cosmetic) => (
                      <CosmeticCard
                        key={cosmetic.id}
                        cosmetic={cosmetic}
                        onEquip={handleEquip}
                        isEquipping={isEquipping}
                      />
                    ))}
                  </motion.div>
                </AnimatePresence>
              )}

              {/* Empty state */}
              {!cosmeticsLoading &&
                (activeCosmeticType === 'BoardTheme'
                  ? byType.boardThemes
                  : activeCosmeticType === 'AvatarFrame'
                  ? byType.avatarFrames
                  : byType.badges
                ).length === 0 && (
                  <div className="text-center py-12">
                    <span className="text-4xl mb-3 block">🎨</span>
                    <p className="text-gray-400">No cosmetics available yet</p>
                  </div>
                )}
            </div>
          )}

          {/* Challenges Tab */}
          {activeTab === 'challenges' && (
            <ChallengesList challenges={challenges} isLoading={challengesLoading} />
          )}
        </motion.div>
      </AnimatePresence>
    </div>
  );
}

export default function ProfilePage() {
  return <ProfilePageContent />;
}
