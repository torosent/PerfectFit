# Architecture

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Client Layer                             │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                    Next.js Frontend                       │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐ │   │
│  │  │   App    │  │  Zustand │  │  @dnd-kit│  │  Motion  │ │   │
│  │  │  Router  │  │  Stores  │  │   DnD    │  │ Animate  │ │   │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘ │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ HTTP/REST (JSON)
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                        API Layer                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │               ASP.NET Core Minimal API                    │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐ │   │
│  │  │  Game    │  │   Auth   │  │  Leader  │  │  Health  │ │   │
│  │  │Endpoints │  │Endpoints │  │  board   │  │  Check   │ │   │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘ │   │
│  └─────────────────────────────────────────────────────────┘   │
│                              │                                   │
│                              │ MediatR (CQRS)                   │
│                              ▼                                   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                  Application Layer                        │   │
│  │  ┌──────────────────┐  ┌──────────────────────────────┐ │   │
│  │  │     Commands     │  │          Queries             │ │   │
│  │  │ - CreateGame     │  │ - GetGame                    │ │   │
│  │  │ - PlacePiece     │  │ - GetTopScores               │ │   │
│  │  │ - EndGame        │  │ - GetUserStats               │ │   │
│  │  │ - SubmitScore    │  │                              │ │   │
│  │  └──────────────────┘  └──────────────────────────────┘ │   │
│  └─────────────────────────────────────────────────────────┘   │
│                              │                                   │
│                              ▼                                   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                    Domain Layer                           │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐ │   │
│  │  │  Game    │  │  Piece   │  │  Score   │  │   Line   │ │   │
│  │  │  Engine  │  │   Bag    │  │Calculator│  │  Clearer │ │   │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘ │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐              │   │
│  │  │   User   │  │  Game    │  │Leaderboard│              │   │
│  │  │  Entity  │  │ Session  │  │  Entry   │              │   │
│  │  └──────────┘  └──────────┘  └──────────┘              │   │
│  └─────────────────────────────────────────────────────────┘   │
│                              │                                   │
│                              ▼                                   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │               Infrastructure Layer                        │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐ │   │
│  │  │EF Core   │  │   JWT    │  │  OAuth   │  │Repository│ │   │
│  │  │DbContext │  │ Service  │  │ Handlers │  │  Impls   │ │   │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘ │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Data Layer                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              PostgreSQL / SQLite Database                  │  │
│  │  ┌──────────┐  ┌──────────────┐  ┌────────────────────┐  │  │
│  │  │  Users   │  │ GameSessions │  │ LeaderboardEntries │  │  │
│  │  └──────────┘  └──────────────┘  └────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## Backend Architecture (Clean Architecture)

The backend follows **Clean Architecture** principles with clear separation of concerns:

### Layer Structure

```
backend/src/
├── PerfectFit.Core/           # Domain Layer (innermost)
│   ├── Entities/              # Domain entities
│   ├── Enums/                 # Domain enumerations
│   ├── GameLogic/             # Core game logic
│   │   ├── Board/             # Game board operations
│   │   ├── PieceBag/          # Piece generation
│   │   ├── Pieces/            # Piece definitions
│   │   └── Scoring/           # Score calculation
│   ├── Interfaces/            # Repository & service contracts
│   └── Services/              # Domain services
│
├── PerfectFit.UseCases/       # Application Layer
│   ├── Auth/                  # Authentication use cases
│   │   └── Commands/          # Login, register commands
│   ├── Games/                 # Game use cases
│   │   ├── Commands/          # Create, place, end game
│   │   ├── Queries/           # Get game state
│   │   └── DTOs/              # Data transfer objects
│   └── Leaderboard/           # Leaderboard use cases
│       ├── Commands/          # Submit score
│       └── Queries/           # Get rankings
│
├── PerfectFit.Infrastructure/ # Infrastructure Layer
│   ├── Data/                  # EF Core implementation
│   │   ├── AppDbContext.cs    # Database context
│   │   └── Repositories/      # Repository implementations
│   ├── Identity/              # Auth infrastructure
│   │   ├── JwtService.cs      # JWT token handling
│   │   └── JwtSettings.cs     # JWT configuration
│   └── Services/              # External service implementations
│
└── PerfectFit.Web/            # API Layer (outermost)
    ├── Endpoints/             # Minimal API endpoints
    ├── DTOs/                  # API request/response DTOs
    └── Program.cs             # Application entry point
```

### Dependency Flow

```
Web → UseCases → Core ← Infrastructure
         ↓           ↑
         └───────────┘
```

- **Core** has no dependencies (pure domain logic)
- **UseCases** depends only on Core interfaces
- **Infrastructure** implements Core interfaces
- **Web** orchestrates everything via dependency injection

## Frontend Architecture

### Directory Structure

```
frontend/src/
├── app/                       # Next.js App Router
│   ├── layout.tsx            # Root layout
│   ├── page.tsx              # Home page
│   ├── (auth)/               # Auth route group
│   │   └── login/            # Login page
│   └── (game)/               # Game route group
│       └── play/             # Game play page
│
├── components/               # React components
│   ├── auth/                 # Authentication components
│   ├── game/                 # Game UI components
│   │   ├── GameBoard.tsx     # Main game board
│   │   ├── DraggablePiece.tsx # Draggable piece
│   │   ├── DroppableBoard.tsx # Drop target
│   │   ├── PieceSelector.tsx  # Piece selection
│   │   ├── ScoreDisplay.tsx   # Score UI
│   │   └── GameOverModal.tsx  # End game modal
│   ├── leaderboard/          # Leaderboard components
│   ├── providers/            # Context providers
│   └── ui/                   # Reusable UI components
│
├── hooks/                    # Custom React hooks
│   └── index.ts              # Hook exports
│
├── lib/                      # Utilities and logic
│   ├── api/                  # API client layer
│   │   ├── game-client.ts    # Game API
│   │   ├── auth-client.ts    # Auth API
│   │   └── leaderboard-client.ts # Leaderboard API
│   ├── stores/               # Zustand state stores
│   │   ├── game-store.ts     # Game state
│   │   └── auth-store.ts     # Auth state
│   ├── game-logic/           # Client-side game logic
│   └── animations/           # Animation utilities
│
├── contexts/                 # React contexts
│   └── index.ts              # Context exports
│
└── types/                    # TypeScript definitions
    ├── game.ts               # Game types
    └── index.ts              # Type exports
```

### State Management

```
┌─────────────────────────────────────────────────────────────┐
│                     Zustand Stores                           │
│                                                             │
│  ┌───────────────────────┐  ┌───────────────────────────┐  │
│  │      Game Store       │  │       Auth Store          │  │
│  │                       │  │                           │  │
│  │ • gameState           │  │ • user                    │  │
│  │ • currentPieces       │  │ • token                   │  │
│  │ • score/combo         │  │ • isAuthenticated         │  │
│  │ • selectedPieceIndex  │  │ • isLoading              │  │
│  │ • hoverPosition       │  │                           │  │
│  │ • animationState      │  │ Actions:                  │  │
│  │                       │  │ • login()                 │  │
│  │ Actions:              │  │ • logout()                │  │
│  │ • startNewGame()      │  │ • loginAsGuest()          │  │
│  │ • placePiece()        │  │ • initializeAuth()        │  │
│  │ • selectPiece()       │  │                           │  │
│  │ • endCurrentGame()    │  └───────────────────────────┘  │
│  └───────────────────────┘                                  │
│                                                             │
│  Persistence: Auth store uses zustand/persist               │
│               to localStorage                               │
└─────────────────────────────────────────────────────────────┘
```

## CQRS Pattern

The backend uses Command Query Responsibility Segregation (CQRS) via MediatR:

### Commands (Write Operations)
```csharp
// Create a new game
CreateGameCommand → CreateGameHandler → GameSession

// Place a piece
PlacePieceCommand → PlacePieceHandler → PlacementResult

// End a game
EndGameCommand → EndGameHandler → GameSession

// Submit score
SubmitScoreCommand → SubmitScoreHandler → LeaderboardEntry
```

### Queries (Read Operations)
```csharp
// Get game state
GetGameQuery → GetGameHandler → GameStateDto

// Get top scores
GetTopScoresQuery → GetTopScoresHandler → List<LeaderboardEntryDto>

// Get user stats
GetUserStatsQuery → GetUserStatsHandler → UserStatsDto
```

## Authentication Flow

```
┌─────────┐      ┌─────────┐      ┌─────────┐      ┌─────────┐
│ Client  │      │ Backend │      │  OAuth  │      │ Backend │
│         │      │         │      │Provider │      │         │
└────┬────┘      └────┬────┘      └────┬────┘      └────┬────┘
     │                │                 │                │
     │ 1. GET /api/auth/google          │                │
     │────────────────>                 │                │
     │                │                 │                │
     │ 2. Redirect to OAuth provider    │                │
     │<────────────────                 │                │
     │                │                 │                │
     │ 3. User authenticates            │                │
     │──────────────────────────────────>                │
     │                │                 │                │
     │ 4. OAuth callback with code      │                │
     │<──────────────────────────────────                │
     │                │                 │                │
     │ 5. GET /api/auth/callback/google │                │
     │────────────────>                 │                │
     │                │                 │                │
     │                │ 6. Exchange code for user info   │
     │                │─────────────────>                │
     │                │                 │                │
     │                │ 7. User profile │                │
     │                │<─────────────────                │
     │                │                 │                │
     │ 8. Redirect to frontend with JWT │                │
     │<────────────────                 │                │
     │                │                 │                │
```

## Database Schema

```
┌──────────────────────┐
│        Users         │
├──────────────────────┤
│ Id (PK)              │
│ ExternalId           │
│ Email                │
│ DisplayName          │
│ Provider             │
│ CreatedAt            │
│ LastLoginAt          │
│ HighScore            │
│ GamesPlayed          │
└──────────┬───────────┘
           │
           │ 1:N
           ▼
┌──────────────────────┐
│    GameSessions      │
├──────────────────────┤
│ Id (PK, GUID)        │
│ UserId (FK, nullable)│
│ BoardState (JSON)    │
│ CurrentPieces (JSON) │
│ PieceBagState (JSON) │
│ Score                │
│ Combo                │
│ LinesCleared         │
│ MaxCombo             │
│ Status               │
│ StartedAt            │
│ EndedAt              │
│ LastActivityAt       │
└──────────────────────┘

┌──────────────────────┐
│  LeaderboardEntries  │
├──────────────────────┤
│ Id (PK)              │
│ UserId (FK)          │
│ GameSessionId (FK)   │
│ Score                │
│ LinesCleared         │
│ MaxCombo             │
│ AchievedAt           │
└──────────────────────┘
```
