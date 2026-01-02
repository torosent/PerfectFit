import { create } from 'zustand';
import type { GameState, Position } from '@/types';
import * as gameClient from '@/lib/api/game-client';

/**
 * Animation state for visual feedback
 */
export interface AnimationState {
  /** Cells currently being cleared (for clearing animation) */
  clearingCells: Position[];
  /** Recently placed cells (for placement animation) */
  lastPlacedCells: Position[];
  /** Points earned from last action (for popup) */
  lastPointsEarned: number;
  /** Combo from last action (for popup) */
  lastCombo: number;
  /** Key to trigger animation re-renders */
  animationKey: number;
}

/**
 * Game store state interface
 */
export interface GameStore {
  // State
  gameState: GameState | null;
  isLoading: boolean;
  error: string | null;
  selectedPieceIndex: number | null;
  
  // Drag-and-drop state
  hoverPosition: Position | null;
  draggedPieceIndex: number | null;
  
  // Animation state
  animationState: AnimationState;
  
  // Computed helpers (not reactive, but convenient)
  isGameOver: () => boolean;
  canPlay: () => boolean;
  
  // Actions
  startNewGame: () => Promise<void>;
  loadGame: (id: string) => Promise<void>;
  placePiece: (pieceIndex: number, row: number, col: number) => Promise<boolean>;
  selectPiece: (index: number | null) => void;
  clearError: () => void;
  endCurrentGame: () => Promise<void>;
  resetStore: () => void;
  
  // Drag-and-drop actions
  setHoverPosition: (pos: Position | null) => void;
  setDraggedPieceIndex: (index: number | null) => void;
  
  // Animation actions
  setClearingCells: (cells: Position[]) => void;
  setLastPlacedCells: (cells: Position[]) => void;
  setLastPointsEarned: (points: number) => void;
  setLastCombo: (combo: number) => void;
  clearAnimationState: () => void;
}

/**
 * Initial animation state
 */
const initialAnimationState: AnimationState = {
  clearingCells: [],
  lastPlacedCells: [],
  lastPointsEarned: 0,
  lastCombo: 0,
  animationKey: 0,
};

/**
 * Initial state for the game store
 */
const initialState = {
  gameState: null,
  isLoading: false,
  error: null,
  selectedPieceIndex: null,
  hoverPosition: null,
  draggedPieceIndex: null,
  animationState: initialAnimationState,
};

/**
 * Zustand store for game state management
 */
export const useGameStore = create<GameStore>((set, get) => ({
  // Initial state
  ...initialState,

  // Computed helpers
  isGameOver: () => {
    const state = get().gameState;
    return state?.status === 'ended';
  },

  canPlay: () => {
    const { gameState, isLoading } = get();
    return gameState !== null && gameState.status === 'playing' && !isLoading;
  },

  // Actions
  startNewGame: async () => {
    set({ isLoading: true, error: null, selectedPieceIndex: null });
    
    try {
      const gameState = await gameClient.createGame();
      set({ gameState, isLoading: false });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to start new game';
      set({ error: message, isLoading: false });
    }
  },

  loadGame: async (id: string) => {
    set({ isLoading: true, error: null, selectedPieceIndex: null });
    
    try {
      const gameState = await gameClient.getGame(id);
      set({ gameState, isLoading: false });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to load game';
      set({ error: message, isLoading: false });
    }
  },

  placePiece: async (pieceIndex: number, row: number, col: number) => {
    const { gameState } = get();
    
    if (!gameState) {
      set({ error: 'No active game' });
      return false;
    }

    set({ isLoading: true, error: null });

    try {
      const response = await gameClient.placePiece(gameState.id, {
        pieceIndex,
        position: { row, col },
      });

      if (response.success) {
        set({ 
          gameState: response.gameState, 
          isLoading: false,
          selectedPieceIndex: null, // Deselect after successful placement
        });
        return true;
      } else {
        set({ 
          error: 'Invalid piece placement',
          isLoading: false,
        });
        return false;
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to place piece';
      set({ error: message, isLoading: false });
      return false;
    }
  },

  selectPiece: (index: number | null) => {
    const { gameState } = get();
    
    // Validate piece index if selecting
    if (index !== null && gameState) {
      const piece = gameState.currentPieces[index];
      // Can only select pieces that exist (not already used)
      if (!piece) {
        return;
      }
    }
    
    set({ selectedPieceIndex: index });
  },

  clearError: () => {
    set({ error: null });
  },

  endCurrentGame: async () => {
    const { gameState } = get();
    
    if (!gameState) {
      return;
    }

    set({ isLoading: true, error: null });

    try {
      const finalState = await gameClient.endGame(gameState.id);
      set({ gameState: finalState, isLoading: false, selectedPieceIndex: null });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to end game';
      set({ error: message, isLoading: false });
    }
  },

  resetStore: () => {
    set(initialState);
  },

  // Drag-and-drop actions
  setHoverPosition: (pos: Position | null) => {
    set({ hoverPosition: pos });
  },

  setDraggedPieceIndex: (index: number | null) => {
    set({ draggedPieceIndex: index });
  },

  // Animation actions
  setClearingCells: (cells: Position[]) => {
    set((state) => ({
      animationState: {
        ...state.animationState,
        clearingCells: cells,
        animationKey: state.animationState.animationKey + 1,
      },
    }));
  },

  setLastPlacedCells: (cells: Position[]) => {
    set((state) => ({
      animationState: {
        ...state.animationState,
        lastPlacedCells: cells,
        animationKey: state.animationState.animationKey + 1,
      },
    }));
  },

  setLastPointsEarned: (points: number) => {
    set((state) => ({
      animationState: {
        ...state.animationState,
        lastPointsEarned: points,
        animationKey: state.animationState.animationKey + 1,
      },
    }));
  },

  setLastCombo: (combo: number) => {
    set((state) => ({
      animationState: {
        ...state.animationState,
        lastCombo: combo,
        animationKey: state.animationState.animationKey + 1,
      },
    }));
  },

  clearAnimationState: () => {
    set((state) => ({
      animationState: {
        ...initialAnimationState,
        animationKey: state.animationState.animationKey,
      },
    }));
  },
}));

/**
 * Selector hooks for specific parts of state
 * These help with performance by only subscribing to what's needed
 */
export const useGameState = () => useGameStore((state) => state.gameState);
export const useGameGrid = () => useGameStore((state) => state.gameState?.grid ?? null);
export const useCurrentPieces = () => useGameStore((state) => state.gameState?.currentPieces ?? []);
export const useGameScore = () => useGameStore((state) => state.gameState?.score ?? 0);
export const useGameCombo = () => useGameStore((state) => state.gameState?.combo ?? 0);
export const useGameStatus = () => useGameStore((state) => state.gameState?.status ?? null);
export const useSelectedPieceIndex = () => useGameStore((state) => state.selectedPieceIndex);
export const useIsLoading = () => useGameStore((state) => state.isLoading);
export const useGameError = () => useGameStore((state) => state.error);
export const useHoverPosition = () => useGameStore((state) => state.hoverPosition);
export const useDraggedPieceIndex = () => useGameStore((state) => state.draggedPieceIndex);

// Animation state selectors
export const useAnimationState = () => useGameStore((state) => state.animationState);
export const useClearingCells = () => useGameStore((state) => state.animationState.clearingCells);
export const useLastPlacedCells = () => useGameStore((state) => state.animationState.lastPlacedCells);
export const useLastPointsEarned = () => useGameStore((state) => state.animationState.lastPointsEarned);
export const useLastCombo = () => useGameStore((state) => state.animationState.lastCombo);
export const useAnimationKey = () => useGameStore((state) => state.animationState.animationKey);
