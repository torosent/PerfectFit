'use client';

import { useEffect, useCallback, useState, useMemo } from 'react';
import { useGameStore, useLastSubmitResult, useIsSubmittingScore } from '@/lib/stores/game-store';
import { useIsAuthenticated } from '@/lib/stores/auth-store';
import { canPlacePiece, getPieceCells } from '@/lib/game-logic/pieces';
import { DroppableBoard, type HighlightedCell } from '@/components/game/DroppableBoard';
import { PieceSelector } from '@/components/game/PieceSelector';
import { ScoreDisplay } from '@/components/game/ScoreDisplay';
import { GameOverModal } from '@/components/game/GameOverModal';
import { DndProvider } from '@/components/providers/DndProvider';
import { GuestBanner } from '@/components/auth/GuestBanner';

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
  } = useGameStore();

  const isAuthenticated = useIsAuthenticated();
  const lastSubmitResult = useLastSubmitResult();
  const isSubmittingScore = useIsSubmittingScore();

  const [hoveredCell, setHoveredCell] = useState<{ row: number; col: number } | null>(null);

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
          cell.row < 10 &&
          cell.col >= 0 &&
          cell.col < 10
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
        await placePiece(selectedPieceIndex, row, col);
        setHoveredCell(null);
      }
    },
    [gameState, selectedPieceIndex, placePiece]
  );

  // Handle play again
  const handlePlayAgain = useCallback(() => {
    clearSubmitResult();
    startNewGame();
  }, [clearSubmitResult, startNewGame]);

  // Clear error after display
  useEffect(() => {
    if (error) {
      const timer = setTimeout(clearError, 5000);
      return () => clearTimeout(timer);
    }
  }, [error, clearError]);

  const isGameOver = gameState?.status === 'ended';

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
      <main className="min-h-screen text-white p-4 sm:p-8">
        <div className="max-w-4xl mx-auto">
          {/* Guest Banner */}
          <GuestBanner className="mb-6" />

          {/* Loading State */}
          {isLoading && !gameState && (
            <div className="flex items-center justify-center py-20">
              <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent" />
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
                  className="mt-4 py-2 px-6 bg-gray-700 hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed text-gray-300 font-medium rounded-lg transition-colors"
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
