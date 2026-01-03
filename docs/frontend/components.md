# Frontend Components

## Component Architecture

Components are organized by feature and follow these patterns:
- **Server Components** (default) for static UI and data fetching
- **Client Components** (`'use client'`) for interactivity

## Game Components

Located in `src/components/game/`:

### GameBoard

The main game board component displaying the 10x10 grid.

```tsx
import { GameBoard } from '@/components/game';

<GameBoard />
```

**Features**:
- Renders 10x10 grid of cells
- Shows current piece positions
- Highlights valid drop targets
- Displays hover preview during drag

### DroppableBoard

Wrapper that makes the board a drop target for pieces.

```tsx
import { DroppableBoard } from '@/components/game';

<DroppableBoard>
  <GameBoard />
</DroppableBoard>
```

**Props**: None (uses @dnd-kit context internally)

### DraggablePiece

A draggable piece component for the piece selector.

```tsx
import { DraggablePiece } from '@/components/game';

<DraggablePiece 
  piece={piece} 
  index={0} 
  isUsed={false}
/>
```

**Props**:
| Prop | Type | Description |
|------|------|-------------|
| `piece` | `Piece` | Piece data (type, shape, color) |
| `index` | `number` | Index in piece array (0-2) |
| `isUsed` | `boolean` | Whether piece has been placed |

### PieceSelector

Displays the 3 available pieces for the current turn.

```tsx
import { PieceSelector } from '@/components/game';

<PieceSelector />
```

**Features**:
- Shows 3 pieces per turn
- Click to select, drag to place
- Visual feedback for used pieces
- Integrates with game store

### PieceDisplay

Renders a single piece shape.

```tsx
import { PieceDisplay } from '@/components/game';

<PieceDisplay 
  piece={piece} 
  cellSize={24} 
/>
```

**Props**:
| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `piece` | `Piece` | Required | Piece to display |
| `cellSize` | `number` | `24` | Size of each cell in pixels |

### GameCell

Individual cell in the game grid.

```tsx
import { GameCell } from '@/components/game';

<GameCell 
  value="#a855f7"  // color or null
  row={0}
  col={0}
/>
```

**Props**:
| Prop | Type | Description |
|------|------|-------------|
| `value` | `string \| null` | Cell color or null if empty |
| `row` | `number` | Row index (0-9) |
| `col` | `number` | Column index (0-9) |

### AnimatedCell

Cell with animation support for clearing and placement.

```tsx
import { AnimatedCell } from '@/components/game';

<AnimatedCell 
  value="#a855f7"
  row={0}
  col={0}
  isClearing={false}
  isNewlyPlaced={true}
/>
```

**Props**:
| Prop | Type | Description |
|------|------|-------------|
| `value` | `string \| null` | Cell color |
| `row` | `number` | Row index |
| `col` | `number` | Column index |
| `isClearing` | `boolean` | Whether cell is being cleared |
| `isNewlyPlaced` | `boolean` | Whether cell was just placed |

### ScoreDisplay

Shows current score and combo information.

```tsx
import { ScoreDisplay } from '@/components/game';

<ScoreDisplay />
```

**Displays**:
- Current score
- Combo multiplier
- Lines cleared
- Turn progress (pieces remaining)

### ComboPopup

Animated popup showing combo achievements.

```tsx
import { ComboPopup } from '@/components/game';

<ComboPopup combo={3} />
```

**Props**:
| Prop | Type | Description |
|------|------|-------------|
| `combo` | `number` | Current combo count |

### PointsPopup

Animated popup showing points earned.

```tsx
import { PointsPopup } from '@/components/game';

<PointsPopup points={150} />
```

**Props**:
| Prop | Type | Description |
|------|------|-------------|
| `points` | `number` | Points earned |

### GameOverModal

Modal displayed when game ends.

```tsx
import { GameOverModal } from '@/components/game';

<GameOverModal 
  score={5000}
  onNewGame={() => startNewGame()}
  onSubmitScore={() => submitScore()}
/>
```

**Props**:
| Prop | Type | Description |
|------|------|-------------|
| `score` | `number` | Final score |
| `onNewGame` | `() => void` | Start new game callback |
| `onSubmitScore` | `() => void` | Submit score callback |

---

## Auth Components

Located in `src/components/auth/`:

### LoginButton

OAuth login button with provider icons.

```tsx
import { LoginButton } from '@/components/auth';

<LoginButton provider="google" />
<LoginButton provider="microsoft" />
<LoginButton provider="guest" />
```

**Props**:
| Prop | Type | Description |
|------|------|-------------|
| `provider` | `'google' \| 'microsoft' \| 'facebook' \| 'guest'` | OAuth provider |

### UserProfile

Displays current user information.

```tsx
import { UserProfile } from '@/components/auth';

<UserProfile />
```

**Displays**:
- Display name
- High score
- Games played
- Logout button

---

## Leaderboard Components

Located in `src/components/leaderboard/`:

### LeaderboardTable

Displays top scores table.

```tsx
import { LeaderboardTable } from '@/components/leaderboard';

<LeaderboardTable entries={entries} />
```

**Props**:
| Prop | Type | Description |
|------|------|-------------|
| `entries` | `LeaderboardEntry[]` | Leaderboard entries |

### UserRank

Shows current user's rank and stats.

```tsx
import { UserRank } from '@/components/leaderboard';

<UserRank />
```

---

## UI Components

Located in `src/components/ui/`:

Reusable UI components following design system:
- `Button` - Standard button with variants
- `Card` - Container card
- `Modal` - Modal dialog
- `Loading` - Loading spinner
- `ErrorMessage` - Error display

---

## Component Exports

All components are exported from their respective index files:

```tsx
// Game components
import { 
  GameBoard, 
  PieceSelector, 
  ScoreDisplay,
  GameOverModal 
} from '@/components/game';

// Auth components
import { LoginButton, UserProfile } from '@/components/auth';

// Leaderboard components
import { LeaderboardTable, UserRank } from '@/components/leaderboard';

// UI components
import { Button, Card, Modal } from '@/components/ui';
```

---

## Creating New Components

### File Structure

```
components/
└── feature/
    ├── index.ts           # Exports
    ├── ComponentName.tsx  # Component
    └── ComponentName.test.tsx  # Tests (optional)
```

### Template

```tsx
'use client'; // Only if needed

import { type FC } from 'react';

interface ComponentNameProps {
  // props
}

export const ComponentName: FC<ComponentNameProps> = ({ ...props }) => {
  return (
    <div>
      {/* component content */}
    </div>
  );
};
```

### Best Practices

1. **Use Client Components sparingly** - Only add `'use client'` when needed for hooks or interactivity
2. **Type all props** - Use TypeScript interfaces
3. **Keep components focused** - Single responsibility
4. **Co-locate related code** - Keep styles, tests, and types near components
5. **Export from index** - Use barrel exports for cleaner imports
