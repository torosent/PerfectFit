'use client';

import { memo } from 'react';
import { motion } from 'motion/react';
import type { CosmeticInfo } from '@/types/gamification';

export interface BoardThemePreviewProps {
  /** Board theme data */
  theme: CosmeticInfo | null;
  /** Size variant */
  size?: 'sm' | 'md' | 'lg';
  /** Whether to show enlarged on hover */
  expandOnHover?: boolean;
}

/**
 * Default theme colors
 */
const DEFAULT_COLORS = {
  primary: '#0ea5e9',
  secondary: '#14b8a6',
  background: '#0d243d',
  cell: '#1a365d',
};

/**
 * Mini board visualization with theme colors
 */
function BoardThemePreviewComponent({
  theme,
  size = 'md',
  expandOnHover = true,
}: BoardThemePreviewProps) {
  const sizeClasses = {
    sm: 'w-16 h-16',
    md: 'w-24 h-24',
    lg: 'w-32 h-32',
  };
  
  const cellSizes = {
    sm: 'w-2 h-2',
    md: 'w-3 h-3',
    lg: 'w-4 h-4',
  };
  
  // For now, use default colors. In production, parse theme.assetUrl for colors
  const colors = DEFAULT_COLORS;
  
  // Create a mini 4x4 grid
  const grid = [
    [1, 0, 0, 1],
    [1, 1, 0, 0],
    [0, 1, 1, 0],
    [0, 0, 1, 1],
  ];
  
  return (
    <motion.div
      className={`relative ${sizeClasses[size]} rounded-lg overflow-hidden`}
      style={{ backgroundColor: colors.background }}
      whileHover={expandOnHover ? { scale: 1.5, zIndex: 10 } : {}}
      transition={{ type: 'spring', stiffness: 300, damping: 20 }}
    >
      {/* Grid */}
      <div className="absolute inset-1 grid grid-cols-4 grid-rows-4 gap-0.5">
        {grid.flat().map((filled, index) => (
          <motion.div
            key={index}
            className={`${cellSizes[size]} rounded-sm`}
            style={{
              backgroundColor: filled ? colors.primary : colors.cell,
            }}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ delay: index * 0.02 }}
          />
        ))}
      </div>
      
      {/* Theme name overlay (on hover) */}
      {theme && (
        <div className="absolute inset-0 flex items-end justify-center opacity-0 hover:opacity-100 transition-opacity bg-gradient-to-t from-black/70 to-transparent">
          <p className="text-xs text-white font-medium pb-1 truncate px-1">
            {theme.name}
          </p>
        </div>
      )}
    </motion.div>
  );
}

export const BoardThemePreview = memo(BoardThemePreviewComponent);
BoardThemePreview.displayName = 'BoardThemePreview';
