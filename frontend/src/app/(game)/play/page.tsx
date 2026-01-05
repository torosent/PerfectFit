'use client';

import { useEffect, useCallback, useState, useMemo, useRef } from 'react';
import { useGameStore, useLastSubmitResult, useIsSubmittingScore, useClearingCells, useLastPlacedCells } from '@/lib/stores/game-store';
import { useIsAuthenticated } from '@/lib/stores/auth-store';
import { canPlacePiece, getPieceCells } from '@/lib/game-logic/pieces';
import { DroppableBoard, type HighlightedCell } from '@/components/game/DroppableBoard';
import { PieceSelector } from '@/components/game/PieceSelector';
import { ScoreDisplay } from '@/components/game/ScoreDisplay';
import { GameOverModal } from '@/components/game/GameOverModal';
import { DndProvider } from '@/components/providers/DndProvider';
import { GuestBanner } from '@/components/auth/GuestBanner';
import { useHaptics } from '@/hooks/useHaptics';
import type { Grid, ClearingCell, Position } from '@/types';

/**
 * Find cells that were cleared by comparing two grids
 * Returns positions with their original colors for animation
 * Also includes placed cells that were cleared (part of completed lines)
 */
function findClearedCells(
  oldGrid: Grid | null, 
  newGrid: Grid, 
  placedCells: Position[] = [],
  placedColor?: string
): ClearingCell[] {
  if (!oldGrid) return [];
  
  const clearedCells: ClearingCell[] = [];
  const clearedPositions = new Set<string>();
  
  for (let row = 0; row < 8; row++) {
    for (let col = 0; col < 8; col++) {
      const oldValue = oldGrid[row][col];
      // Cell was filled before but is now empty = cleared
      if (oldValue !== null && newGrid[row][col] === null) {
        clearedCells.push({ row, col, color: oldValue });
        clearedPositions.add(`${row}-${col}`);
      }
    }
  }
  
  // Check if any placed cells are now empty (they were part of a cleared line)
  // These cells weren't in the old grid, so we need to add them manually
  if (placedColor) {
    for (const { row, col } of placedCells) {
      const key = `${row}-${col}`;
      // Only add if this cell is now empty and not already in cleared cells
      if (newGrid[row][col] === null && !clearedPositions.has(key)) {
        clearedCells.push({ row, col, color: placedColor });
      }
    }
  }
  
  return clearedCells;
}

/**
 * Main game play page
 * Assembles all game components and manages game flow
 */
export default function PlayPage() {
  const {
    gameState,
    isLoading,
    error,
    selectedPieceIndex,
    hoverPosition,
    draggedPieceIndex,
    startNewGame,
    placePiece,
    selectPiece,
    clearError,
    setHoverPosition,
    submitScoreToLeaderboard,
    clearSubmitResult,
    setClearingCells,
    setLastPlacedCells,
    clearAnimationState,
  } = useGameStore();

  const isAuthenticated = useIsAuthenticated();
  const lastSubmitResult = useLastSubmitResult();
  const isSubmittingScore = useIsSubmittingScore();
  const clearingCells = useClearingCells();
  const lastPlacedCells = useLastPlacedCells();

  const [hoveredCell, setHoveredCell] = useState<{ row: number; col: number } | null>(null);
  
  // Track previous grid for detecting cleared cells
  const prevGridRef = useRef<Grid | null>(null);
  
  // Track placed cells and their color for clearing animation
  const placedCellsRef = useRef<{ cells: Position[]; color: string } | null>(null);
  
  // Track previous game over state for haptic feedback
  const prevGameOverRef = useRef(false);
  
  // Haptic feedback
  const haptics = useHaptics();

  // Start a new game on mount
  useEffect(() => {
    startNewGame();
  }, [startNewGame]);

  // Compute highlighted cells based on current state (derived state, not effect)
  const highlightedCells = useMemo<HighlightedCell[]>(() => {
    if (!gameState || selectedPieceIndex === null || !hoveredCell) {
      return [];
    }

    const piece = gameState.currentPieces[selectedPieceIndex];
    if (!piece) {
      return [];
    }

    const { row, col } = hoveredCell;
    const isValid = canPlacePiece(gameState.grid, piece, row, col);
    const cells = getPieceCells(piece, row, col);

    return cells
      .filter(
        (cell) =>
          cell.row >= 0 &&
          cell.row < 8 &&
          cell.col >= 0 &&
          cell.col < 8
      )
      .map((cell) => ({
        ...cell,
        isValid,
      }));
  }, [gameState, selectedPieceIndex, hoveredCell]);

  // Handle cell click - place piece or just track hover
  const handleCellClick = useCallback(
    async (row: number, col: number) => {
      if (!gameState || selectedPieceIndex === null) {
        setHoveredCell({ row, col });
        return;
      }

      const piece = gameState.currentPieces[selectedPieceIndex];
      if (!piece) return;

      // Check if placement is valid before attempting
      if (canPlacePiece(gameState.grid, piece, row, col)) {
        // Store grid before placement for animation comparison
        prevGridRef.current = gameState.grid.map(r => [...r]);
        
        // Get the cells that will be placed (for animation)
        const placedCells = getPieceCells(piece, row, col);
        
        // Store placed cells and color for clearing animation detection
        placedCellsRef.current = { cells: placedCells, color: piece.color };
        
        // Place the piece
        const success = await placePiece(selectedPieceIndex, row, col);
        
        if (success) {
          // Trigger placed animation
          setLastPlacedCells(placedCells);
          setHoveredCell(null);
        }
      }
    },
    [gameState, selectedPieceIndex, placePiece, setLastPlacedCells]
  );

  // Detect cleared cells when grid changes
  useEffect(() => {
    if (gameState?.grid && prevGridRef.current) {
      const placedInfo = placedCellsRef.current;
      const clearedCells = findClearedCells(
        prevGridRef.current, 
        gameState.grid,
        placedInfo?.cells ?? [],
        placedInfo?.color
      );
      
      if (clearedCells.length > 0) {
        // Trigger clearing animation
        setClearingCells(clearedCells);
        
        // Clear animation state after animation completes
        const animationDuration = 700; // Match animation duration
        setTimeout(() => {
          clearAnimationState();
        }, animationDuration);
      } else {
        // Clear placed animation after it completes
        setTimeout(() => {
          clearAnimationState();
        }, 400);
      }
      
      // Reset refs after processing
      prevGridRef.current = null;
      placedCellsRef.current = null;
    }
  }, [gameState?.grid, setClearingCells, clearAnimationState]);

  // Handle play again
  const handlePlayAgain = useCallback(() => {
    clearSubmitResult();
    clearAnimationState();
    startNewGame();
  }, [clearSubmitResult, clearAnimationState, startNewGame]);

  // Clear error after display
  useEffect(() => {
    if (error) {
      const timer = setTimeout(clearError, 5000);
      return () => clearTimeout(timer);
    }
  }, [error, clearError]);

  const isGameOver = gameState?.status === 'Ended';

  // Trigger haptic feedback when game ends
  useEffect(() => {
    if (isGameOver && !prevGameOverRef.current) {
      haptics.gameOver();
    }
    prevGameOverRef.current = isGameOver;
  }, [isGameOver, haptics]);

  // Submit score when game ends and user is authenticated
  useEffect(() => {
    if (isGameOver && isAuthenticated && !lastSubmitResult && !isSubmittingScore) {
      submitScoreToLeaderboard();
    }
  }, [isGameOver, isAuthenticated, lastSubmitResult, isSubmittingScore, submitScoreToLeaderboard]);

  // Get the dragged piece for preview
  const draggedPiece = useMemo(() => {
    if (draggedPieceIndex === null || !gameState) return null;
    return gameState.currentPieces[draggedPieceIndex] ?? null;
  }, [draggedPieceIndex, gameState]);

  return (
    <DndProvider>
      <main className="min-h-screen-safe safe-area-inset game-touch-none text-white p-4 sm:p-8">
        <div className="max-w-4xl mx-auto">
          {/* Guest Banner */}
          <GuestBanner className="mb-6 mt-6" />

          {/* Loading State */}
          {isLoading && !gameState && (
            <div className="flex items-center justify-center py-20">
              <div className="animate-spin rounded-full h-12 w-12 border-4" style={{ borderColor: '#14b8a6', borderTopColor: 'transparent' }} />
              <span className="ml-4 text-lg">Starting game...</span>
            </div>
          )}

          {/* Error Display */}
          {error && (
            <div
              className="bg-red-900/50 border border-red-500 text-red-200 px-4 py-3 rounded-lg mb-6 text-center"
              role="alert"
            >
              <p>{error}</p>
              <button
                onClick={clearError}
                className="mt-2 text-sm underline hover:no-underline"
              >
                Dismiss
              </button>
            </div>
          )}

          {/* Game Content */}
          {gameState && (
            <div className="flex flex-col items-center gap-8">
              {/* Score Display */}
              <ScoreDisplay
                score={gameState.score}
                combo={gameState.combo}
                linesCleared={gameState.linesCleared}
              />

              {/* Game Board - Droppable */}
              <div
                onMouseLeave={() => {
                  setHoveredCell(null);
                  if (draggedPieceIndex === null) {
                    setHoverPosition(null);
                  }
                }}
              >
                <DroppableBoard
                  grid={gameState.grid}
                  hoverPosition={hoverPosition}
                  draggedPiece={draggedPiece}
                  highlightedCells={highlightedCells}
                  clearingCells={clearingCells}
                  lastPlacedCells={lastPlacedCells}
                  onCellClick={handleCellClick}
                  disabled={isGameOver || isLoading}
                />
              </div>

              {/* Piece Selector */}
              <PieceSelector
                pieces={gameState.currentPieces}
                selectedIndex={selectedPieceIndex}
                onSelect={selectPiece}
                disabled={isGameOver || isLoading}
              />

              {/* New Game Button */}
              {!isGameOver && (
                <button
                  onClick={handlePlayAgain}
                  disabled={isLoading}
                  className="mt-4 py-2 px-6 disabled:opacity-50 disabled:cursor-not-allowed text-gray-300 font-medium rounded-lg transition-colors"
                  style={{ backgroundColor: 'rgba(13, 36, 61, 0.8)', borderWidth: 1, borderStyle: 'solid', borderColor: 'rgba(20, 184, 166, 0.3)' }}
                >
                  Restart Game
                </button>
              )}
            </div>
          )}

          {/* Game Over Modal */}
          <GameOverModal
            isOpen={isGameOver}
            score={gameState?.score ?? 0}
            linesCleared={gameState?.linesCleared ?? 0}
            leaderboardResult={lastSubmitResult}
            isSubmitting={isSubmittingScore}
            onPlayAgain={handlePlayAgain}
          />
        </div>
      </main>
    </DndProvider>
  );
}
