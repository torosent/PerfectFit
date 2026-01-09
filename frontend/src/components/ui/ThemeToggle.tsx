'use client';

import { memo } from 'react';
import { motion } from 'motion/react';
import { useTheme, type ThemeName } from '@/contexts/ThemeContext';

/**
 * Theme icon component
 */
function ThemeIcon({ theme }: { theme: ThemeName }) {
  switch (theme) {
    case 'ocean':
      return <span>🌊</span>;
    case 'sunset':
      return <span>🌅</span>;
    case 'forest':
      return <span>🌲</span>;
    default:
      return <span>🎨</span>;
  }
}

/**
 * Theme name display
 */
const themeNames: Record<ThemeName, string> = {
  ocean: 'Ocean',
  sunset: 'Sunset',
  forest: 'Forest',
};

export interface ThemeToggleProps {
  /** Additional CSS classes */
  className?: string;
  /** Show theme name label */
  showLabel?: boolean;
}

/**
 * Theme toggle button that cycles through available themes
 */
function ThemeToggleComponent({ className = '', showLabel = false }: ThemeToggleProps) {
  const { theme, cycleTheme, colors } = useTheme();

  return (
    <motion.button
      onClick={cycleTheme}
      whileHover={{ scale: 1.05 }}
      whileTap={{ scale: 0.95 }}
      className={`
        flex items-center gap-2 px-3 py-2 rounded-lg
        transition-colors duration-200
        ${className}
      `}
      style={{
        backgroundColor: 'rgba(255, 255, 255, 0.1)',
        borderWidth: 1,
        borderStyle: 'solid',
        borderColor: colors.boardBorder,
      }}
      aria-label={`Current theme: ${themeNames[theme]}. Click to change theme.`}
      title={`Theme: ${themeNames[theme]}`}
    >
      <motion.span
        key={theme}
        initial={{ rotate: -180, opacity: 0 }}
        animate={{ rotate: 0, opacity: 1 }}
        exit={{ rotate: 180, opacity: 0 }}
        transition={{ duration: 0.3 }}
        className="text-lg"
      >
        <ThemeIcon theme={theme} />
      </motion.span>
      
      {showLabel && (
        <motion.span
          key={`label-${theme}`}
          initial={{ opacity: 0, x: -10 }}
          animate={{ opacity: 1, x: 0 }}
          className="text-sm font-medium text-white"
        >
          {themeNames[theme]}
        </motion.span>
      )}
    </motion.button>
  );
}

export const ThemeToggle = memo(ThemeToggleComponent);
ThemeToggle.displayName = 'ThemeToggle';
