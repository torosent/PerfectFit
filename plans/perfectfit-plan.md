## Plan: PerfectFit Block Puzzle Game

A full-stack grid-based block placement puzzle game using Next.js frontend and C# ASP.NET Core backend. Players receive 3 random block shapes per turn, place them on a 10×10 grid, and score points by completing rows/columns. Features OAuth authentication, global leaderboard, and responsive design.

**Phases: 8**

---

### Phase 1: Project Scaffolding & Infrastructure

- **Objective:** Create both project structures, configure tooling, Docker for PostgreSQL, and establish shared types/DTOs
- **Files/Functions to Create:**
  - `frontend/` - Next.js 15 App Router project with TypeScript, Tailwind CSS, ESLint
  - `backend/PerfectFit.sln` - .NET solution file
  - `backend/src/PerfectFit.Core/` - Domain layer (entities, enums)
  - `backend/src/PerfectFit.UseCases/` - Application layer (commands, queries)
  - `backend/src/PerfectFit.Infrastructure/` - Data access layer (EF Core)
  - `backend/src/PerfectFit.Web/` - API layer (controllers, endpoints)
  - `backend/tests/PerfectFit.UnitTests/` - Unit test project
  - `backend/tests/PerfectFit.IntegrationTests/` - Integration test project
  - `docker-compose.yml` - PostgreSQL container
  - `frontend/types/game.ts` - Shared TypeScript interfaces
  - `backend/src/PerfectFit.Web/DTOs/` - C# DTOs matching frontend types
- **Tests to Write:**
  - Backend project compiles and runs
  - Frontend project compiles and runs
  - Docker PostgreSQL container starts
- **Steps:**
  1. Create Next.js project with `create-next-app` (App Router, TypeScript, Tailwind)
  2. Create .NET solution with Clean Architecture folder structure
  3. Add project references between layers
  4. Create docker-compose.yml with PostgreSQL service
  5. Define shared TypeScript types (GameState, Piece, Position, etc.)
  6. Define matching C# DTOs
  7. Verify both projects compile and run

---

### Phase 2: Database Schema & Entity Framework Core

- **Objective:** Design and implement database models with EF Core migrations
- **Files/Functions to Create:**
  - `PerfectFit.Core/Entities/User.cs` - User entity
  - `PerfectFit.Core/Entities/GameSession.cs` - Game session entity
  - `PerfectFit.Core/Entities/LeaderboardEntry.cs` - Leaderboard entry entity
  - `PerfectFit.Core/Enums/AuthProvider.cs` - OAuth provider enum
  - `PerfectFit.Core/Enums/GameStatus.cs` - Game status enum
  - `PerfectFit.Infrastructure/Data/AppDbContext.cs` - EF Core context
  - `PerfectFit.Infrastructure/Data/Configurations/` - Entity configurations
  - `PerfectFit.Infrastructure/Data/Repositories/` - Repository implementations
  - `PerfectFit.Core/Interfaces/` - Repository interfaces
- **Tests to Write:**
  - `UserEntityTests.cs` - User entity validation
  - `GameSessionEntityTests.cs` - Game session entity validation
  - `RepositoryTests.cs` - Repository CRUD operations
- **Steps:**
  1. Write tests for User entity creation and validation
  2. Implement User entity with properties (Id, ExternalId, Email, DisplayName, Provider, HighScore)
  3. Write tests for GameSession entity
  4. Implement GameSession entity (Id, UserId, BoardState, Score, Combo, Status, etc.)
  5. Write tests for LeaderboardEntry
  6. Implement LeaderboardEntry entity
  7. Create AppDbContext with DbSets
  8. Create entity configurations (indexes, constraints)
  9. Create and run initial migration
  10. Implement repository interfaces and classes

---

### Phase 3: Core Game Logic (Backend)

- **Objective:** Implement backend-authoritative game mechanics with comprehensive tests
- **Files/Functions to Create:**
  - `PerfectFit.Core/GameLogic/Pieces/PieceDefinitions.cs` - All piece shapes (7 tetrominoes + extended)
  - `PerfectFit.Core/GameLogic/Pieces/PieceType.cs` - Piece type enum
  - `PerfectFit.Core/GameLogic/Pieces/Piece.cs` - Piece class with shape matrix
  - `PerfectFit.Core/GameLogic/Board/GameBoard.cs` - 10×10 board representation
  - `PerfectFit.Core/GameLogic/Board/BoardValidator.cs` - Placement validation
  - `PerfectFit.Core/GameLogic/Board/LineClearer.cs` - Row/column clearing
  - `PerfectFit.Core/GameLogic/Scoring/ScoreCalculator.cs` - Score and combo logic
  - `PerfectFit.Core/GameLogic/PieceBag/PieceBagGenerator.cs` - 7-bag random system
  - `PerfectFit.Core/GameLogic/GameEngine.cs` - Main game orchestrator
- **Tests to Write:**
  - `PieceDefinitionsTests.cs` - All pieces have exactly 4 cells (tetrominoes)
  - `BoardValidatorTests.cs` - Bounds check, overlap check, valid placements
  - `LineClearerTests.cs` - Row clearing, column clearing, combo clearing
  - `ScoreCalculatorTests.cs` - Single/multi-line scores, combo multipliers
  - `PieceBagGeneratorTests.cs` - Bag contains all pieces, proper randomization
  - `GameEngineTests.cs` - Full game flow, game-over detection
- **Steps:**
  1. Write tests for piece definitions (shape validation)
  2. Implement all tetromino shapes (I, O, T, S, Z, J, L) plus extended shapes
  3. Write tests for board placement validation
  4. Implement BoardValidator with bounds and overlap checking
  5. Write tests for line clearing (rows and columns, no gravity)
  6. Implement LineClearer
  7. Write tests for score calculation with combos
  8. Implement ScoreCalculator
  9. Write tests for piece bag generation (7-bag system)
  10. Implement PieceBagGenerator
  11. Write tests for GameEngine orchestration
  12. Implement GameEngine (place piece, check clears, check game over)

---

### Phase 4: Game API Endpoints

- **Objective:** Expose REST APIs for game operations with proper DTOs
- **Files/Functions to Create:**
  - `PerfectFit.Web/Endpoints/Games/CreateGameEndpoint.cs` - POST /api/games
  - `PerfectFit.Web/Endpoints/Games/GetGameEndpoint.cs` - GET /api/games/{id}
  - `PerfectFit.Web/Endpoints/Games/PlacePieceEndpoint.cs` - POST /api/games/{id}/place
  - `PerfectFit.Web/Endpoints/Games/EndGameEndpoint.cs` - POST /api/games/{id}/end
  - `PerfectFit.Web/DTOs/Requests/` - Request DTOs
  - `PerfectFit.Web/DTOs/Responses/` - Response DTOs
  - `PerfectFit.UseCases/Games/Commands/` - CQRS command handlers
  - `PerfectFit.UseCases/Games/Queries/` - CQRS query handlers
- **Tests to Write:**
  - `CreateGameEndpointTests.cs` - Creates game, returns initial state
  - `GetGameEndpointTests.cs` - Returns correct game state
  - `PlacePieceEndpointTests.cs` - Valid placement, invalid placement, clearing
  - `EndGameEndpointTests.cs` - Ends game, submits score
- **Steps:**
  1. Write integration tests for POST /api/games
  2. Implement CreateGameCommand and endpoint
  3. Write tests for GET /api/games/{id}
  4. Implement GetGameQuery and endpoint
  5. Write tests for POST /api/games/{id}/place
  6. Implement PlacePieceCommand and endpoint (with validation, clearing, scoring)
  7. Write tests for POST /api/games/{id}/end
  8. Implement EndGameCommand and endpoint
  9. Add error handling middleware
  10. Add request/response logging

---

### Phase 5: Frontend Game Board & Pieces

- **Objective:** Build the interactive game UI with drag-and-drop
- **Files/Functions to Create:**
  - `frontend/app/(game)/play/page.tsx` - Main game page
  - `frontend/app/(game)/layout.tsx` - Game layout
  - `frontend/components/game/GameBoard.tsx` - 10×10 grid component
  - `frontend/components/game/GameCell.tsx` - Individual cell component
  - `frontend/components/game/PieceSelector.tsx` - Available pieces display
  - `frontend/components/game/DraggablePiece.tsx` - Draggable piece component
  - `frontend/components/game/PiecePreview.tsx` - Ghost preview on hover
  - `frontend/components/game/ScoreDisplay.tsx` - Score component
  - `frontend/components/game/GameOverModal.tsx` - Game over modal
  - `frontend/lib/stores/game-store.ts` - Zustand game state
  - `frontend/lib/game-logic/pieces.ts` - Piece definitions (mirror backend)
  - `frontend/lib/game-logic/validation.ts` - Client-side validation (for UI feedback)
  - `frontend/lib/api/game-client.ts` - API client for backend calls
  - `frontend/components/providers/DndProvider.tsx` - @dnd-kit provider setup
- **Tests to Write:**
  - `GameBoard.test.tsx` - Renders 100 cells
  - `DraggablePiece.test.tsx` - Drag behavior
  - `validation.test.ts` - Client-side placement validation
  - `game-store.test.ts` - State management
- **Steps:**
  1. Install dependencies (@dnd-kit/core, zustand, motion)
  2. Create TypeScript piece definitions matching backend
  3. Write tests for client-side validation logic
  4. Implement validation functions
  5. Write tests for game store
  6. Implement Zustand game store
  7. Create GameBoard component with CSS Grid (10×10)
  8. Create GameCell component with occupied/empty states
  9. Create DraggablePiece component with @dnd-kit
  10. Create PieceSelector showing 3 available pieces
  11. Implement drag-and-drop with snap-to-grid collision detection
  12. Add valid/invalid drop highlighting
  13. Create ScoreDisplay component
  14. Create GameOverModal component
  15. Wire up API client to sync with backend
  16. Make responsive for mobile (touch support)

---

### Phase 6: Frontend Animations & Polish

- **Objective:** Add animations, visual feedback, and UI polish
- **Files/Functions to Create:**
  - `frontend/components/game/AnimatedCell.tsx` - Cell with animation states
  - `frontend/components/game/ClearingAnimation.tsx` - Row/column clear effect
  - `frontend/components/game/ComboDisplay.tsx` - Combo notification
  - `frontend/components/game/PlacementAnimation.tsx` - Piece placement effect
  - `frontend/lib/animations/game-animations.ts` - Motion variants
  - `frontend/app/globals.css` - Enhanced styles and animations
- **Tests to Write:**
  - Animation components render without errors
  - Animations complete callbacks fire
- **Steps:**
  1. Create motion variants for placement animation
  2. Implement AnimatedCell with enter/exit animations
  3. Create ClearingAnimation component (flash/fade effect)
  4. Create ComboDisplay component (floating score popup)
  5. Add score counter animation (counting up effect)
  6. Add shake animation for invalid placement attempts
  7. Polish color scheme and visual design
  8. Add hover states and visual feedback
  9. Test on mobile devices
  10. Optimize animation performance

---

### Phase 7: OAuth Authentication

- **Objective:** Implement OAuth login with Google, Apple, Microsoft and JWT tokens
- **Files/Functions to Create:**
  - `PerfectFit.Web/Endpoints/Auth/GoogleAuthEndpoint.cs` - Google OAuth
  - `PerfectFit.Web/Endpoints/Auth/AppleAuthEndpoint.cs` - Apple OAuth
  - `PerfectFit.Web/Endpoints/Auth/MicrosoftAuthEndpoint.cs` - Microsoft OAuth
  - `PerfectFit.Web/Endpoints/Auth/CallbackEndpoint.cs` - OAuth callback
  - `PerfectFit.Web/Endpoints/Auth/RefreshTokenEndpoint.cs` - Token refresh
  - `PerfectFit.Infrastructure/Identity/JwtService.cs` - JWT generation
  - `PerfectFit.Infrastructure/Identity/OAuthService.cs` - OAuth handling
  - `PerfectFit.UseCases/Users/Commands/CreateOrUpdateUserCommand.cs` - User upsert
  - `frontend/app/(auth)/login/page.tsx` - Login page
  - `frontend/app/(auth)/callback/page.tsx` - OAuth callback page
  - `frontend/lib/stores/auth-store.ts` - Auth state store
  - `frontend/lib/api/auth-client.ts` - Auth API client
  - `frontend/components/auth/LoginButton.tsx` - OAuth login buttons
  - `frontend/components/auth/UserMenu.tsx` - User dropdown menu
- **Tests to Write:**
  - `JwtServiceTests.cs` - Token generation and validation
  - `OAuthServiceTests.cs` - OAuth flow handling
  - `AuthEndpointTests.cs` - Integration tests for auth endpoints
  - `auth-store.test.ts` - Frontend auth state
- **Steps:**
  1. Write tests for JWT token generation
  2. Implement JwtService
  3. Configure OAuth providers in Program.cs
  4. Write tests for OAuth callback handling
  5. Implement OAuth endpoints (initiate flow, handle callback)
  6. Implement CreateOrUpdateUserCommand (upsert on OAuth login)
  7. Add JWT authentication middleware
  8. Create frontend login page with OAuth buttons
  9. Create callback page to handle OAuth redirect
  10. Implement auth store with token management
  11. Add auth state to game pages (show user name)
  12. Implement guest mode (anonymous game sessions)
  13. Add protected route middleware for authenticated-only features

---

### Phase 8: Leaderboard System

- **Objective:** Implement global and personal leaderboards with score validation
- **Files/Functions to Create:**
  - `PerfectFit.Web/Endpoints/Leaderboard/GetLeaderboardEndpoint.cs` - GET /api/leaderboard
  - `PerfectFit.Web/Endpoints/Leaderboard/GetPersonalBestEndpoint.cs` - GET /api/users/me/stats
  - `PerfectFit.UseCases/Leaderboard/Queries/GetTopScoresQuery.cs` - Top scores query
  - `PerfectFit.UseCases/Leaderboard/Queries/GetUserStatsQuery.cs` - User stats query
  - `PerfectFit.UseCases/Leaderboard/Commands/SubmitScoreCommand.cs` - Score submission
  - `PerfectFit.Core/Services/ScoreValidationService.cs` - Server-side score validation
  - `frontend/app/(game)/leaderboard/page.tsx` - Leaderboard page
  - `frontend/components/leaderboard/LeaderboardTable.tsx` - Top scores table
  - `frontend/components/leaderboard/PersonalStats.tsx` - User's stats display
  - `frontend/components/leaderboard/RankBadge.tsx` - Rank display component
- **Tests to Write:**
  - `GetLeaderboardEndpointTests.cs` - Returns top scores sorted
  - `SubmitScoreCommandTests.cs` - Valid score submission, duplicate prevention
  - `ScoreValidationServiceTests.cs` - Detects invalid/suspicious scores
  - `LeaderboardTable.test.tsx` - Renders scores correctly
- **Steps:**
  1. Write tests for score submission validation
  2. Implement ScoreValidationService (verify score came from valid game session)
  3. Write tests for SubmitScoreCommand
  4. Implement score submission (update user high score if new record)
  5. Write tests for GetTopScoresQuery
  6. Implement leaderboard query (top 100, paginated)
  7. Write tests for GetUserStatsQuery
  8. Implement user stats query (games played, high score, rank)
  9. Create leaderboard page
  10. Create LeaderboardTable component
  11. Create PersonalStats component
  12. Add leaderboard link to game UI
  13. Show rank on game over if made leaderboard
  14. Require authentication for score submission

---

## Technology Stack

| Layer | Technology |
|-------|------------|
| Frontend | Next.js 15 (App Router), TypeScript, Tailwind CSS |
| Drag & Drop | @dnd-kit/core |
| Animations | motion (framer-motion) |
| State | zustand |
| Backend | ASP.NET Core 8, C# 12 |
| Architecture | Clean Architecture with CQRS |
| Database | PostgreSQL + EF Core 8 |
| Auth | OAuth2 (Google, Apple, Microsoft) + JWT |

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/games` | Optional | Create new game session |
| GET | `/api/games/{id}` | Optional | Get current game state |
| POST | `/api/games/{id}/place` | Optional | Place a piece on the board |
| POST | `/api/games/{id}/end` | Required* | End game and submit score |
| GET | `/api/leaderboard` | No | Get global leaderboard |
| GET | `/api/users/me` | Required | Get current user profile |
| GET | `/api/users/me/stats` | Required | Get personal stats |
| GET | `/api/auth/{provider}` | No | Initiate OAuth flow |
| GET | `/api/auth/callback` | No | OAuth callback handler |
| POST | `/api/auth/refresh` | No | Refresh access token |

*Score only submitted to leaderboard if authenticated

## Design Decisions

1. **Bag System:** 7-bag randomization for tetrominoes ensures fair piece distribution
2. **3 Pieces Per Turn:** Players choose from 3 pieces each turn (1010! style)
3. **Extended Shapes:** Include single dots, 2-cell lines, 3-cell lines, corners, 3×3 squares
4. **No Rotation:** Pieces cannot be rotated (simplifies gameplay)
5. **No Gravity:** Cleared cells become empty, blocks don't fall
6. **Guest Play:** Anyone can play, but scores only saved to leaderboard when authenticated
