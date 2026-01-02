# State Management

## Overview

PerfectFit uses [Zustand](https://github.com/pmndrs/zustand) for state management. Zustand provides a simple, unopinionated approach with excellent TypeScript support.

## Stores

### Game Store

Location: `src/lib/stores/game-store.ts`

Manages all game-related state including board, pieces, score, and animations.

#### State

```typescript
interface GameStore {
  // Core game state
  gameState: GameState | null;
  isLoading: boolean;
  error: string | null;
  selectedPieceIndex: number | null;
  
  // Drag-and-drop state
  hoverPosition: Position | null;
  draggedPieceIndex: number | null;
  
  // Animation state
  animationState: AnimationState;
  
  // Leaderboard state
  lastSubmitResult: SubmitScoreResult | null;
  isSubmittingScore: boolean;
}
```

#### Actions

| Action | Description |
|--------|-------------|
| `startNewGame()` | Create a new game session |
| `loadGame(id)` | Load existing game by ID |
| `placePiece(index, row, col)` | Place a piece on the board |
| `selectPiece(index)` | Select a piece for placement |
| `endCurrentGame()` | End the current game |
| `resetStore()` | Reset all state |
| `setHoverPosition(pos)` | Update hover preview position |
| `setDraggedPieceIndex(index)` | Track dragged piece |
| `submitScoreToLeaderboard()` | Submit final score |

#### Usage

```tsx
'use client';

import { useGameStore } from '@/lib/stores';

function GameComponent() {
  // Access full store
  const { gameState, startNewGame, placePiece } = useGameStore();
  
  // Or use selector hooks for performance
  const score = useGameScore();
  const grid = useGameGrid();
  const pieces = useCurrentPieces();
  
  return (
    <div>
      <p>Score: {score}</p>
      <button onClick={startNewGame}>New Game</button>
    </div>
  );
}
```

#### Selector Hooks

For optimal performance, use specific selectors:

```typescript
// Core state selectors
useGameState()          // Full GameState object
useGameGrid()           // 10x10 grid
useCurrentPieces()      // Array of available pieces
useGameScore()          // Current score
useGameCombo()          // Current combo
useGameStatus()         // 'Playing' | 'Ended'
useSelectedPieceIndex() // Selected piece index

// Drag-and-drop selectors
useHoverPosition()      // Current hover position
useDraggedPieceIndex()  // Currently dragged piece

// Animation selectors
useAnimationState()     // Full animation state
useClearingCells()      // Cells being cleared
useLastPlacedCells()    // Recently placed cells
useLastPointsEarned()   // Points from last action
useLastCombo()          // Combo from last action

// Loading/error selectors
useIsLoading()          // Loading state
useGameError()          // Error message
```

---

### Auth Store

Location: `src/lib/stores/auth-store.ts`

Manages authentication state with persistence to localStorage.

#### State

```typescript
interface AuthState {
  user: UserProfile | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isInitialized: boolean;
  error: string | null;
}
```

#### Actions

| Action | Description |
|--------|-------------|
| `login(token, user)` | Set auth state after login |
| `logout()` | Clear auth state |
| `loginAsGuest()` | Create guest session |
| `initializeAuth()` | Validate stored token on load |
| `setError(error)` | Set error message |
| `clearError()` | Clear error message |

#### Usage

```tsx
'use client';

import { useAuthStore, useIsAuthenticated, useUser } from '@/lib/stores';

function AuthComponent() {
  // Full store access
  const { login, logout, loginAsGuest } = useAuthStore();
  
  // Selectors
  const isAuthenticated = useIsAuthenticated();
  const user = useUser();
  
  if (!isAuthenticated) {
    return <button onClick={loginAsGuest}>Play as Guest</button>;
  }
  
  return (
    <div>
      <p>Welcome, {user?.displayName}</p>
      <button onClick={logout}>Logout</button>
    </div>
  );
}
```

#### Selector Hooks

```typescript
useUser()               // UserProfile or null
useToken()              // JWT token
useIsAuthenticated()    // Boolean auth status
useIsAuthLoading()      // Loading state
useIsAuthInitialized()  // Whether auth has been checked
useAuthError()          // Error message
useIsGuest()            // Whether user is guest
```

#### Persistence

The auth store uses `zustand/persist` middleware:

```typescript
persist(
  (set, get) => ({ ... }),
  {
    name: 'perfectfit-auth',
    storage: createJSONStorage(() => localStorage),
    partialize: (state) => ({
      token: state.token,
      user: state.user,
      isAuthenticated: state.isAuthenticated,
    }),
  }
)
```

Only `token`, `user`, and `isAuthenticated` are persisted.

---

## API Integration

Stores integrate with API clients in `src/lib/api/`:

### Game Store → Game Client

```typescript
// In game-store.ts
import * as gameClient from '@/lib/api/game-client';

startNewGame: async () => {
  set({ isLoading: true });
  try {
    const token = useAuthStore.getState().token;
    const gameState = await gameClient.createGame(token);
    set({ gameState, isLoading: false });
  } catch (err) {
    set({ error: err.message, isLoading: false });
  }
}
```

### Auth Store → Auth Client

```typescript
// In auth-store.ts
import * as authClient from '@/lib/api/auth-client';

loginAsGuest: async () => {
  set({ isLoading: true });
  try {
    const { token, user } = await authClient.createGuestSession();
    set({ token, user, isAuthenticated: true, isLoading: false });
  } catch (err) {
    set({ error: err.message, isLoading: false });
  }
}
```

---

## Best Practices

### 1. Use Selectors for Performance

```tsx
// ❌ Bad - subscribes to entire store
const { gameState } = useGameStore();
const score = gameState?.score;

// ✅ Good - subscribes only to score
const score = useGameScore();
```

### 2. Handle Loading States

```tsx
function GameBoard() {
  const isLoading = useIsLoading();
  const grid = useGameGrid();
  
  if (isLoading) return <Loading />;
  if (!grid) return <div>No game loaded</div>;
  
  return <Board grid={grid} />;
}
```

### 3. Initialize Auth on App Load

```tsx
// In providers or layout
'use client';

import { useEffect } from 'react';
import { useAuthStore } from '@/lib/stores';

export function AuthInitializer({ children }) {
  const initializeAuth = useAuthStore((s) => s.initializeAuth);
  const isInitialized = useAuthStore((s) => s.isInitialized);
  
  useEffect(() => {
    initializeAuth();
  }, [initializeAuth]);
  
  if (!isInitialized) return <Loading />;
  
  return children;
}
```

### 4. Cross-Store Communication

Stores can read from each other:

```typescript
// Game store accessing auth store
const token = useAuthStore.getState().token;
```

### 5. TypeScript Types

All stores are fully typed. Import types as needed:

```typescript
import type { GameStore, GameState } from '@/lib/stores/game-store';
import type { AuthStore, AuthState } from '@/lib/stores/auth-store';
```

---

## Testing Stores

```typescript
import { useGameStore } from '@/lib/stores/game-store';

describe('Game Store', () => {
  beforeEach(() => {
    // Reset store before each test
    useGameStore.getState().resetStore();
  });

  it('starts a new game', async () => {
    const { startNewGame } = useGameStore.getState();
    
    await startNewGame();
    
    const state = useGameStore.getState();
    expect(state.gameState).toBeDefined();
    expect(state.gameState?.status).toBe('Playing');
  });
});
```
