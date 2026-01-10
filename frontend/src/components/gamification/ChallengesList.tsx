'use client';

import { memo, useState, useCallback, useId } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import type { Challenge, ChallengeType } from '@/types/gamification';
import { ChallengeCard } from './ChallengeCard';

export interface ChallengesListProps {
  /** All challenges */
  challenges: Challenge[];
  /** Currently selected tab type */
  defaultTab?: ChallengeType;
  /** Whether data is loading */
  isLoading?: boolean;
  /** Optional callback when challenge is clicked */
  onChallengeClick?: (challenge: Challenge) => void;
}

/**
 * Daily/weekly challenges panel with tabs
 */
function ChallengesListComponent({
  challenges,
  defaultTab = 'Daily',
  isLoading = false,
  onChallengeClick,
}: ChallengesListProps) {
  const [activeTab, setActiveTab] = useState<ChallengeType>(defaultTab);
  const uniqueId = useId();
  
  const dailyChallenges = challenges.filter(c => c.type === 'Daily');
  const weeklyChallenges = challenges.filter(c => c.type === 'Weekly');
  
  const activeChallenges = activeTab === 'Daily' ? dailyChallenges : weeklyChallenges;
  const completedCount = activeChallenges.filter(c => c.isCompleted).length;
  const totalCount = activeChallenges.length;
  
  const handleTabClick = useCallback((tab: ChallengeType) => {
    setActiveTab(tab);
  }, []);
  
  // Tab IDs for accessibility
  const dailyTabId = `${uniqueId}-tab-daily`;
  const weeklyTabId = `${uniqueId}-tab-weekly`;
  const dailyPanelId = `${uniqueId}-panel-daily`;
  const weeklyPanelId = `${uniqueId}-panel-weekly`;
  
  return (
    <div className="rounded-xl bg-white/5 backdrop-blur-md overflow-hidden">
      {/* Tab header */}
      <div role="tablist" aria-label="Challenge types" className="flex border-b border-white/10">
        <button
          id={dailyTabId}
          role="tab"
          aria-selected={activeTab === 'Daily'}
          aria-controls={dailyPanelId}
          onClick={() => handleTabClick('Daily')}
          className={`flex-1 py-3 px-4 text-sm font-medium transition-colors ${
            activeTab === 'Daily'
              ? 'text-blue-400 bg-blue-500/10 border-b-2 border-blue-400'
              : 'text-gray-400 hover:text-white hover:bg-white/5'
          }`}
        >
          Daily ({dailyChallenges.filter(c => c.isCompleted).length}/{dailyChallenges.length})
        </button>
        <button
          id={weeklyTabId}
          role="tab"
          aria-selected={activeTab === 'Weekly'}
          aria-controls={weeklyPanelId}
          onClick={() => handleTabClick('Weekly')}
          className={`flex-1 py-3 px-4 text-sm font-medium transition-colors ${
            activeTab === 'Weekly'
              ? 'text-purple-400 bg-purple-500/10 border-b-2 border-purple-400'
              : 'text-gray-400 hover:text-white hover:bg-white/5'
          }`}
        >
          Weekly ({weeklyChallenges.filter(c => c.isCompleted).length}/{weeklyChallenges.length})
        </button>
      </div>
      
      {/* Challenges list */}
      <div
        id={activeTab === 'Daily' ? dailyPanelId : weeklyPanelId}
        role="tabpanel"
        aria-labelledby={activeTab === 'Daily' ? dailyTabId : weeklyTabId}
        className="p-4"
      >
        {/* Completion summary */}
        {totalCount > 0 && (
          <div className="flex items-center justify-between mb-4">
            <span className="text-sm text-gray-400">
              {completedCount} of {totalCount} completed
            </span>
            <div className="h-2 flex-1 mx-4 rounded-full bg-white/10 overflow-hidden">
              <motion.div
                className={`h-full rounded-full ${
                  activeTab === 'Daily'
                    ? 'bg-gradient-to-r from-blue-500 to-cyan-400'
                    : 'bg-gradient-to-r from-purple-500 to-pink-400'
                }`}
                initial={{ width: 0 }}
                animate={{ width: `${(completedCount / totalCount) * 100}%` }}
                transition={{ duration: 0.5 }}
              />
            </div>
          </div>
        )}
        
        {/* Loading state */}
        {isLoading && (
          <div className="space-y-3">
            {[1, 2, 3].map(i => (
              <div key={i} className="h-24 rounded-xl bg-white/5 animate-pulse" />
            ))}
          </div>
        )}
        
        {/* Empty state */}
        {!isLoading && activeChallenges.length === 0 && (
          <div className="py-8 text-center">
            <span className="text-4xl mb-3 block">📋</span>
            <p className="text-gray-400">No {activeTab.toLowerCase()} challenges available</p>
            <p className="text-sm text-gray-500 mt-1">Check back later!</p>
          </div>
        )}
        
        {/* Challenge cards */}
        {!isLoading && activeChallenges.length > 0 && (
          <AnimatePresence mode="wait">
            <motion.div
              key={activeTab}
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
              transition={{ duration: 0.2 }}
              className="space-y-3"
            >
              {activeChallenges.map((challenge, index) => (
                <motion.div
                  key={challenge.id}
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: index * 0.05 }}
                >
                  <ChallengeCard
                    challenge={challenge}
                    onClick={onChallengeClick ? () => onChallengeClick(challenge) : undefined}
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

export const ChallengesList = memo(ChallengesListComponent);
ChallengesList.displayName = 'ChallengesList';
