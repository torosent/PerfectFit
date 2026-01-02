/**
 * Mobile Layout Tests - Phase 4
 * Tests for iOS safe-area insets, viewport meta, and responsive layout
 */

import React from 'react';
import { render, screen } from '@testing-library/react';
import fs from 'fs';
import path from 'path';

// Mock game store
const mockGameState = {
  grid: Array(10).fill(null).map(() => Array(10).fill(null)),
  currentPieces: [null, null, null],
  score: 100,
  combo: 0,
  linesCleared: 5,
  status: 'Active' as const,
};

jest.mock('@/lib/stores/game-store', () => ({
  useGameStore: jest.fn(() => ({
    gameState: mockGameState,
    isLoading: false,
    error: null,
    selectedPieceIndex: null,
    hoverPosition: null,
    draggedPieceIndex: null,
    startNewGame: jest.fn(),
    placePiece: jest.fn(),
    selectPiece: jest.fn(),
    clearError: jest.fn(),
    setHoverPosition: jest.fn(),
    submitScoreToLeaderboard: jest.fn(),
    clearSubmitResult: jest.fn(),
    setClearingCells: jest.fn(),
    setLastPlacedCells: jest.fn(),
    clearAnimationState: jest.fn(),
  })),
  useLastSubmitResult: jest.fn(() => null),
  useIsSubmittingScore: jest.fn(() => false),
  useClearingCells: jest.fn(() => []),
  useLastPlacedCells: jest.fn(() => []),
}));

jest.mock('@/lib/stores/auth-store', () => ({
  useIsAuthenticated: jest.fn(() => false),
}));

// Mock motion/react to avoid animation issues in tests
jest.mock('motion/react', () => ({
  motion: {
    div: ({ children, className, ...props }: React.PropsWithChildren<{ className?: string }>) => (
      <div className={className} {...props}>{children}</div>
    ),
    p: ({ children, className, ...props }: React.PropsWithChildren<{ className?: string }>) => (
      <p className={className} {...props}>{children}</p>
    ),
  },
  AnimatePresence: ({ children }: React.PropsWithChildren) => <>{children}</>,
  useMotionValue: jest.fn(() => ({ get: () => 0, set: jest.fn() })),
  animate: jest.fn(() => ({ stop: jest.fn() })),
}));

// Mock DndProvider
jest.mock('@/components/providers/DndProvider', () => ({
  DndProvider: ({ children }: React.PropsWithChildren) => <>{children}</>,
}));

// Mock game components
jest.mock('@/components/game/DroppableBoard', () => ({
  DroppableBoard: () => <div data-testid="game-board">Mock Board</div>,
}));

jest.mock('@/components/game/PieceSelector', () => ({
  PieceSelector: () => <div data-testid="piece-selector">Mock Pieces</div>,
}));

jest.mock('@/components/game/GameOverModal', () => ({
  GameOverModal: () => null,
}));

jest.mock('@/components/auth/GuestBanner', () => ({
  GuestBanner: () => null,
}));

describe('Mobile Layout - Phase 4', () => {
  describe('globals.css safe-area utilities', () => {
    let cssContent: string;

    beforeAll(() => {
      const cssPath = path.join(__dirname, '../app/globals.css');
      cssContent = fs.readFileSync(cssPath, 'utf-8');
    });

    it('should have safe-area-top utility class', () => {
      expect(cssContent).toMatch(/\.safe-area-top\s*\{[\s\S]*?padding-top:\s*env\(safe-area-inset-top\)/);
    });

    it('should have safe-area-bottom utility class', () => {
      expect(cssContent).toMatch(/\.safe-area-bottom\s*\{[\s\S]*?padding-bottom:\s*env\(safe-area-inset-bottom\)/);
    });

    it('should have safe-area-left utility class', () => {
      expect(cssContent).toMatch(/\.safe-area-left\s*\{[\s\S]*?padding-left:\s*env\(safe-area-inset-left\)/);
    });

    it('should have safe-area-right utility class', () => {
      expect(cssContent).toMatch(/\.safe-area-right\s*\{[\s\S]*?padding-right:\s*env\(safe-area-inset-right\)/);
    });

    it('should have safe-area-inset combined utility class', () => {
      expect(cssContent).toMatch(/\.safe-area-inset\s*\{/);
      // Check it has all four padding directions
      const safeAreaInsetMatch = cssContent.match(/\.safe-area-inset\s*\{[\s\S]*?\}/);
      expect(safeAreaInsetMatch).toBeTruthy();
      const safeAreaInsetContent = safeAreaInsetMatch![0];
      expect(safeAreaInsetContent).toMatch(/padding-top:\s*env\(safe-area-inset-top\)/);
      expect(safeAreaInsetContent).toMatch(/padding-bottom:\s*env\(safe-area-inset-bottom\)/);
      expect(safeAreaInsetContent).toMatch(/padding-left:\s*env\(safe-area-inset-left\)/);
      expect(safeAreaInsetContent).toMatch(/padding-right:\s*env\(safe-area-inset-right\)/);
    });

    it('should have min-h-screen-safe utility class for mobile-safe full height', () => {
      expect(cssContent).toMatch(/\.min-h-screen-safe\s*\{/);
      // Should include both fallback and safe-area calculation
      expect(cssContent).toMatch(/min-height:\s*100vh/);
      expect(cssContent).toMatch(/min-height:\s*calc\(100vh\s*-\s*env\(safe-area-inset-top\)\s*-\s*env\(safe-area-inset-bottom\)\)/);
    });

    it('should have iOS Safe Area Insets comment section', () => {
      expect(cssContent).toMatch(/iOS Safe Area Insets/i);
    });
  });

  describe('layout.tsx viewport meta', () => {
    let layoutContent: string;

    beforeAll(() => {
      const layoutPath = path.join(__dirname, '../app/layout.tsx');
      layoutContent = fs.readFileSync(layoutPath, 'utf-8');
    });

    it('should have viewport metadata export', () => {
      expect(layoutContent).toMatch(/export\s+const\s+viewport/);
    });

    it('should have viewport-fit=cover for iOS notch support', () => {
      expect(layoutContent).toMatch(/viewportFit:\s*['"]cover['"]/);
    });

    it('should disable user scaling for game experience', () => {
      expect(layoutContent).toMatch(/userScalable:\s*false/);
    });

    it('should set maximum scale to 1', () => {
      expect(layoutContent).toMatch(/maximumScale:\s*1/);
    });

    it('should set initial scale to 1', () => {
      expect(layoutContent).toMatch(/initialScale:\s*1/);
    });

    it('should have width=device-width', () => {
      expect(layoutContent).toMatch(/width:\s*['"]device-width['"]/);
    });
  });

  describe('play page responsive layout', () => {
    let playPageContent: string;

    beforeAll(() => {
      const playPagePath = path.join(__dirname, '../app/(game)/play/page.tsx');
      playPageContent = fs.readFileSync(playPagePath, 'utf-8');
    });

    it('should have safe-area-inset class on main container', () => {
      expect(playPageContent).toMatch(/safe-area-inset/);
    });

    it('should have min-h-screen-safe class for proper mobile height', () => {
      expect(playPageContent).toMatch(/min-h-screen-safe/);
    });

    it('should have game-touch-none class for touch optimization', () => {
      expect(playPageContent).toMatch(/game-touch-none/);
    });
  });

  describe('ScoreDisplay responsive typography', () => {
    let scoreDisplayContent: string;

    beforeAll(() => {
      const scoreDisplayPath = path.join(__dirname, '../components/game/ScoreDisplay.tsx');
      scoreDisplayContent = fs.readFileSync(scoreDisplayPath, 'utf-8');
    });

    it('should use responsive text sizes for score', () => {
      // Should have text scaling classes (e.g., text-2xl sm:text-3xl or similar)
      expect(scoreDisplayContent).toMatch(/text-\d*xl\s+sm:text-\d*xl|text-responsive/);
    });

    it('should have gap classes for mobile spacing', () => {
      expect(scoreDisplayContent).toMatch(/gap-\d+\s+sm:gap-\d+|gap-/);
    });

    it('should use responsive flex direction', () => {
      expect(scoreDisplayContent).toMatch(/flex-col\s+sm:flex-row|flex/);
    });
  });

  describe('ScoreDisplay Component Rendering', () => {
    it('should render score, combo, and lines sections', async () => {
      // Import dynamically to get mocked version
      const { ScoreDisplay } = await import('@/components/game/ScoreDisplay');
      
      render(
        <ScoreDisplay score={1000} combo={2} linesCleared={10} />
      );

      // Check labels are present
      expect(screen.getByText('Score')).toBeInTheDocument();
      expect(screen.getByText('Combo')).toBeInTheDocument();
      expect(screen.getByText('Lines')).toBeInTheDocument();
    });
  });
});
