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
│  │               ASP.NET Core 10 Minimal API                 │   │
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
│  │  ┌──────────────────────────────────────────────────┐   │   │
│  │  │           Score Validation Service                │   │   │
│  │  │  • Anti-cheat validation  • Score plausibility   │   │   │
│  │  │  • Rate limiting          • Move history check   │   │   │
│  │  └──────────────────────────────────────────────────┘   │   │
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

## Anti-Cheat System

The game implements a multi-layer anti-cheat system to ensure fair play and score integrity:

### Defense Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                     Client-Side Deterrence                       │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │              lib/anti-cheat.ts                           │   │
│  │  • Session tracking           • Input validation         │   │
│  │  • Client-side rate limiting  • Suspicious behavior log  │   │
│  │  • Client fingerprinting      • Move attempt validation  │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ All validation is server-authoritative
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Server-Side Enforcement                      │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │              PlacePieceCommand Handler                    │   │
│  │  • Rate limiting (50ms min between moves)                │   │
│  │  • Max moves per game (500)                              │   │
│  │  • Max game duration (24 hours)                          │   │
│  │  • Input bounds validation                               │   │
│  │  • Move history recording                                │   │
│  └─────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │              ScoreValidationService                       │   │
│  │  • Game duration validation (min 5 seconds)              │   │
│  │  • Move count requirements for high scores               │   │
│  │  • Score plausibility checks                             │   │
│  │  • Mathematical relationship validation                  │   │
│  │    - Max score per move: 500 points                      │   │
│  │    - Max average score per second: 50 points             │   │
│  │    - Max lines per move: 2                               │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

### Move History Tracking

Each game session tracks all moves for post-game analysis:

```json
{
  "moves": [
    {
      "pieceIndex": 0,
      "row": 3,
      "col": 4,
      "pointsEarned": 150,
      "linesCleared": 1,
      "timestamp": "2024-01-15T10:30:45.123Z"
    }
  ]
}
```

### Validation Flow

```
Client Move Request → Rate Limit Check → Input Validation → 
    → Game Logic Processing → Move Recording → Response
    
Score Submit → Duration Check → Move Count Check → 
    → Score Plausibility → Leaderboard Entry
```

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     Production Deployment                        │
│                                                                 │
│  Option 1: Azure Container Apps                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                   Azure Container Apps                    │   │
│  │  ┌──────────────────┐    ┌──────────────────────────┐   │   │
│  │  │ perfectfit-web   │    │   perfectfit-api          │   │   │
│  │  │ (Next.js 16)     │───▶│   (ASP.NET Core 10)       │   │   │
│  │  │ Port: 3000       │    │   Port: 8080              │   │   │
│  │  └──────────────────┘    └──────────────────────────┘   │   │
│  │           │                           │                  │   │
│  │           │              ┌────────────▼────────────┐    │   │
│  │           │              │  Azure PostgreSQL       │    │   │
│  │           │              │  (Flexible Server)      │    │   │
│  │           │              └─────────────────────────┘    │   │
│  └───────────┼──────────────────────────────────────────────┘   │
│              │                                                  │
│  ┌───────────▼──────────────────────────────────────────────┐  │
│  │              Azure Container Registry                     │  │
│  │  perfectfitacr.azurecr.io/perfectfit-web:latest          │  │
│  │  perfectfitacr.azurecr.io/perfectfit-api:latest          │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                 │
│  Option 2: Cloudflare Pages + Azure Container Apps             │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  ┌───────────────────┐    ┌─────────────────────────┐   │   │
│  │  │ Cloudflare Pages  │    │ Azure Container Apps    │   │   │
│  │  │ (Edge Network)    │───▶│ (perfectfit-api)        │   │   │
│  │  │ Global CDN        │    │ Auto-scaling Backend    │   │   │
│  │  └───────────────────┘    └─────────────────────────┘   │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  Option 3: Self-Hosted Docker Compose                          │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  ┌──────────┐  ┌──────────┐  ┌────────────────────┐    │   │
│  │  │ Frontend │  │ Backend  │  │     PostgreSQL     │    │   │
│  │  │  :3000   │─▶│  :8080   │─▶│       :5432        │    │   │
│  │  └──────────┘  └──────────┘  └────────────────────┘    │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

### Container Configuration

| Container | Base Image | Port | Health Check |
|-----------|-----------|------|--------------|
| Frontend | node:22-alpine | 3000 | GET / |
| Backend | mcr.microsoft.com/dotnet/aspnet:10.0 | 8080 | GET /health |
| PostgreSQL | postgres:16 | 5432 | pg_isready |

### CI/CD Pipeline

```
┌─────────┐    ┌─────────────┐    ┌──────────────┐    ┌─────────────┐
│  Push   │───▶│   GitHub    │───▶│  Build &     │───▶│  Deploy to  │
│  Code   │    │   Actions   │    │  Push Images │    │  Container  │
└─────────┘    └─────────────┘    └──────────────┘    │    Apps     │
                                                       └─────────────┘
```

**Workflows:**
- `.github/workflows/deploy-azure.yml` - Full stack to Azure Container Apps
- `.github/workflows/deploy-cloudflare.yml` - Frontend to Cloudflare Pages
