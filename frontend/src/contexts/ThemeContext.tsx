'use client';

import { createContext, useContext, useState, useEffect, useCallback, useMemo, type ReactNode } from 'react';

/**
 * Theme definitions for the game
 */
export type ThemeName = 'ocean' | 'sunset' | 'forest';

export interface ThemeColors {
  // Background colors
  bgStart: string;
  bgMid: string;
  bgEnd: string;
  
  // Cell colors
  cellEmpty: string;
  cellBorder: string;
  
  // Glow colors
  glowPrimary: string;
  glowSecondary: string;
  glowTertiary: string;
  glowAccent: string;
  
  // Accent colors
  accentPrimary: string;
  accentSecondary: string;
  accentGold: string;
  
  // Board colors
  boardBg: string;
  boardBorder: string;
  
  // Danger mode colors
  dangerBorder: string;
  dangerGlow: string;
}

export const themes: Record<ThemeName, ThemeColors> = {
  ocean: {
    bgStart: '#0c1929',
    bgMid: '#0a2540',
    bgEnd: '#0c1929',
    cellEmpty: 'rgba(30, 58, 95, 0.6)',
    cellBorder: 'rgba(56, 97, 140, 0.4)',
    glowPrimary: 'rgba(20, 184, 166, 0.5)',
    glowSecondary: 'rgba(14, 165, 233, 0.5)',
    glowTertiary: 'rgba(34, 211, 238, 0.5)',
    glowAccent: 'rgba(251, 191, 36, 0.5)',
    accentPrimary: '#14b8a6',
    accentSecondary: '#22d3ee',
    accentGold: '#fbbf24',
    boardBg: '#0a1929',
    boardBorder: 'rgba(20, 184, 166, 0.3)',
    dangerBorder: 'rgba(239, 68, 68, 0.6)',
    dangerGlow: 'rgba(239, 68, 68, 0.3)',
  },
  sunset: {
    bgStart: '#1a0a1f',
    bgMid: '#2d1235',
    bgEnd: '#1a0a1f',
    cellEmpty: 'rgba(95, 45, 60, 0.6)',
    cellBorder: 'rgba(140, 70, 90, 0.4)',
    glowPrimary: 'rgba(244, 114, 182, 0.5)',
    glowSecondary: 'rgba(251, 146, 60, 0.5)',
    glowTertiary: 'rgba(192, 132, 252, 0.5)',
    glowAccent: 'rgba(251, 191, 36, 0.5)',
    accentPrimary: '#f472b6',
    accentSecondary: '#fb923c',
    accentGold: '#fbbf24',
    boardBg: '#1a0a1f',
    boardBorder: 'rgba(244, 114, 182, 0.3)',
    dangerBorder: 'rgba(239, 68, 68, 0.6)',
    dangerGlow: 'rgba(239, 68, 68, 0.3)',
  },
  forest: {
    bgStart: '#0a1a0f',
    bgMid: '#0f2418',
    bgEnd: '#0a1a0f',
    cellEmpty: 'rgba(34, 80, 50, 0.6)',
    cellBorder: 'rgba(56, 120, 70, 0.4)',
    glowPrimary: 'rgba(74, 222, 128, 0.5)',
    glowSecondary: 'rgba(163, 230, 53, 0.5)',
    glowTertiary: 'rgba(34, 197, 94, 0.5)',
    glowAccent: 'rgba(251, 191, 36, 0.5)',
    accentPrimary: '#4ade80',
    accentSecondary: '#a3e635',
    accentGold: '#fbbf24',
    boardBg: '#0a1a0f',
    boardBorder: 'rgba(74, 222, 128, 0.3)',
    dangerBorder: 'rgba(239, 68, 68, 0.6)',
    dangerGlow: 'rgba(239, 68, 68, 0.3)',
  },
};

interface ThemeContextValue {
  theme: ThemeName;
  colors: ThemeColors;
  setTheme: (theme: ThemeName) => void;
  cycleTheme: () => void;
}

const ThemeContext = createContext<ThemeContextValue | null>(null);

const THEME_STORAGE_KEY = 'perfectfit-theme';

/**
 * Theme provider component
 * Stores theme preference in localStorage and applies CSS variables
 */
export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setThemeState] = useState<ThemeName>('ocean');
  const [isInitialized, setIsInitialized] = useState(false);

  // Load theme from localStorage on mount
  useEffect(() => {
    const stored = localStorage.getItem(THEME_STORAGE_KEY);
    if (stored && (stored === 'ocean' || stored === 'sunset' || stored === 'forest')) {
      setThemeState(stored);
    }
    setIsInitialized(true);
  }, []);

  // Apply CSS variables when theme changes
  useEffect(() => {
    if (!isInitialized) return;
    
    const colors = themes[theme];
    const root = document.documentElement;
    
    root.style.setProperty('--game-bg-start', colors.bgStart);
    root.style.setProperty('--game-bg-mid', colors.bgMid);
    root.style.setProperty('--game-bg-end', colors.bgEnd);
    root.style.setProperty('--cell-empty', colors.cellEmpty);
    root.style.setProperty('--cell-border', colors.cellBorder);
    root.style.setProperty('--glow-teal', colors.glowPrimary);
    root.style.setProperty('--glow-blue', colors.glowSecondary);
    root.style.setProperty('--glow-cyan', colors.glowTertiary);
    root.style.setProperty('--glow-gold', colors.glowAccent);
    root.style.setProperty('--accent-teal', colors.accentPrimary);
    root.style.setProperty('--accent-cyan', colors.accentSecondary);
    root.style.setProperty('--accent-gold', colors.accentGold);
    root.style.setProperty('--board-bg', colors.boardBg);
    root.style.setProperty('--board-border', colors.boardBorder);
    root.style.setProperty('--danger-border', colors.dangerBorder);
    root.style.setProperty('--danger-glow', colors.dangerGlow);
  }, [theme, isInitialized]);

  const setTheme = useCallback((newTheme: ThemeName) => {
    setThemeState(newTheme);
    localStorage.setItem(THEME_STORAGE_KEY, newTheme);
  }, []);

  const cycleTheme = useCallback(() => {
    const themeOrder: ThemeName[] = ['ocean', 'sunset', 'forest'];
    const currentIndex = themeOrder.indexOf(theme);
    const nextIndex = (currentIndex + 1) % themeOrder.length;
    setTheme(themeOrder[nextIndex]);
  }, [theme, setTheme]);

  const colors = useMemo(() => themes[theme], [theme]);

  const value = useMemo(() => ({
    theme,
    colors,
    setTheme,
    cycleTheme,
  }), [theme, colors, setTheme, cycleTheme]);

  return (
    <ThemeContext.Provider value={value}>
      {children}
    </ThemeContext.Provider>
  );
}

/**
 * Hook to access theme context
 */
export function useTheme(): ThemeContextValue {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
}

/**
 * Hook to get theme colors without the setter functions
 */
export function useThemeColors(): ThemeColors {
  const { colors } = useTheme();
  return colors;
}
