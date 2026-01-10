# API Reference

## Base URL

- **Development**: `http://localhost:5050`
- **Production**: Configure via environment

## Authentication

Most endpoints support optional authentication. Protected endpoints require a valid JWT token.

### Headers

```
Authorization: Bearer <jwt_token>
Content-Type: application/json
```

---

## Health & Status

### Health Check

Check if the API is running.

```http
GET /health
```

**Response**: `200 OK`
```json
"Healthy"
```

### API Status

Get detailed API status information.

```http
GET /api/status
```

**Response**: `200 OK`
```json
{
  "status": "Healthy",
  "service": "PerfectFit API",
  "version": "1.0.0",
  "timestamp": "2026-01-02T12:00:00Z"
}
```

---

## Game Endpoints

### Create Game

Create a new game session.

```http
POST /api/games
```

**Headers** (Optional):
```
Authorization: Bearer <token>
```

**Response**: `201 Created`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "grid": [[0,0,0,...], ...],
  "currentPieces": [
    {
      "type": "T",
      "shape": [[1,1,1],[0,1,0]],
      "color": "#a855f7"
    },
    {
      "type": "LINE3",
      "shape": [[1,1,1]],
      "color": "#3b82f6"
    },
    {
      "type": "SQUARE_2X2",
      "shape": [[1,1],[1,1]],
      "color": "#eab308"
    }
  ],
  "score": 0,
  "combo": 0,
  "status": "Playing",
  "linesCleared": 0
}
```

### Get Game

Retrieve current game state.

```http
GET /api/games/{id}
```

**Parameters**:
| Name | Type | Description |
|------|------|-------------|
| id | GUID | Game session ID |

**Response**: `200 OK`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "grid": [[0,0,0,...], ...],
  "currentPieces": [...],
  "score": 150,
  "combo": 2,
  "status": "Playing",
  "linesCleared": 3
}
```

**Errors**:
- `404 Not Found` - Game not found

### Place Piece

Place a piece on the game board.

```http
POST /api/games/{id}/place
```

**Parameters**:
| Name | Type | Description |
|------|------|-------------|
| id | GUID | Game session ID |

**Request Body**:
```json
{
  "pieceIndex": 0,
  "position": {
    "row": 5,
    "col": 3
  },
  "clientTimestamp": 1704207600000
}
```

| Field | Type | Description |
|-------|------|-------------|
| pieceIndex | number | Index of piece (0-2) |
| position.row | number | Target row (0-9) |
| position.col | number | Target column (0-9) |
| clientTimestamp | number? | Client timestamp in ms (optional, for anti-cheat) |

**Response**: `200 OK`
```json
{
  "success": true,
  "gameState": {
    "id": "...",
    "grid": [...],
    "currentPieces": [...],
    "score": 200,
    "combo": 3,
    "status": "Playing",
    "linesCleared": 4
  },
  "linesCleared": 1,
  "pointsEarned": 50,
  "isGameOver": false,
  "piecesRemainingInTurn": 2,
  "newTurnStarted": false
}
```

**Errors**:
- `400 Bad Request` - Invalid placement, game not active, or anti-cheat rejection
- `404 Not Found` - Game not found

**Anti-Cheat Rejections**:
The server may reject requests with `400 Bad Request` for:
- `Move rate limit exceeded` - Too many moves in quick succession (min 50ms between moves)
- `Maximum moves reached` - Game exceeded 500 moves
- `Game exceeded maximum duration` - Game lasted longer than 24 hours
- `Invalid piece index` - Piece index out of range
- `Invalid board position` - Row/column out of bounds

### End Game

Manually end an active game.

```http
POST /api/games/{id}/end
```

**Parameters**:
| Name | Type | Description |
|------|------|-------------|
| id | GUID | Game session ID |

**Response**: `200 OK`
```json
{
  "id": "...",
  "grid": [...],
  "currentPieces": [...],
  "score": 500,
  "combo": 0,
  "status": "Ended",
  "linesCleared": 12
}
```

---

## Authentication Endpoints

### Initiate OAuth

Redirect to OAuth provider login.

```http
GET /api/auth/{provider}
```

**Parameters**:
| Name | Type | Description |
|------|------|-------------|
| provider | string | `microsoft` |

**Query Parameters** (Optional):
| Name | Type | Description |
|------|------|-------------|
| returnUrl | string | URL to redirect after auth |

**Response**: `302 Redirect` to OAuth provider

### OAuth Callback

Handle OAuth callback (internal use).

```http
GET /api/auth/callback/{provider}
```

**Response**: `302 Redirect` to frontend with token

### Create Guest Session

Create a guest user account.

```http
POST /api/auth/guest
```

**Response**: `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "displayName": "Guest_A1B2C3",
    "email": null,
    "provider": "Guest",
    "highScore": 0,
    "gamesPlayed": 0
  }
}
```

### Get Current User

Get the authenticated user's profile.

```http
GET /api/auth/me
```

**Headers** (Required):
```
Authorization: Bearer <token>
```

**Response**: `200 OK`
```json
{
  "id": 1,
  "displayName": "John Doe",
  "email": "john@example.com",
  "provider": "Local"
}
```

**Errors**:
- `401 Unauthorized` - Invalid or missing token

### Refresh Token

Refresh an expired JWT token.

```http
POST /api/auth/refresh
```

**Request Body**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response**: `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": { ... }
}
```

**Errors**:
- `401 Unauthorized` - Invalid token

### Logout

Logout (client-side token disposal).

```http
POST /api/auth/logout
```

**Response**: `200 OK`
```json
{
  "message": "Logged out successfully. Please discard your token."
}
```

---

## Leaderboard Endpoints

### Get Top Scores

Retrieve the global leaderboard.

```http
GET /api/leaderboard
```

**Query Parameters**:
| Name | Type | Default | Description |
|------|------|---------|-------------|
| count | number | 100 | Number of entries to return |

**Response**: `200 OK`
```json
[
  {
    "rank": 1,
    "displayName": "ProPlayer",
    "score": 15000,
    "linesCleared": 45,
    "maxCombo": 8,
    "achievedAt": "2026-01-01T10:30:00Z"
  },
  {
    "rank": 2,
    "displayName": "Champion",
    "score": 12500,
    "linesCleared": 38,
    "maxCombo": 6,
    "achievedAt": "2026-01-01T14:20:00Z"
  }
]
```

### Get My Stats

Get the current user's leaderboard statistics.

```http
GET /api/leaderboard/me
```

**Headers** (Required):
```
Authorization: Bearer <token>
```

**Response**: `200 OK`
```json
{
  "highScore": 5000,
  "gamesPlayed": 25,
  "globalRank": 150,
  "bestGame": {
    "rank": 150,
    "displayName": "John Doe",
    "score": 5000,
    "linesCleared": 18,
    "maxCombo": 4,
    "achievedAt": "2025-12-30T16:45:00Z"
  }
}
```

**Errors**:
- `401 Unauthorized` - Not authenticated
- `404 Not Found` - User not found

### Submit Score

Submit a game score to the leaderboard.

```http
POST /api/leaderboard/submit
```

**Headers** (Required):
```
Authorization: Bearer <token>
```

**Request Body**:
```json
{
  "gameSessionId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response**: `200 OK`
```json
{
  "success": true,
  "errorMessage": null,
  "entry": {
    "rank": 42,
    "displayName": "John Doe",
    "score": 5500,
    "linesCleared": 20,
    "maxCombo": 5,
    "achievedAt": "2026-01-02T12:00:00Z"
  },
  "isNewHighScore": true,
  "newRank": 42
}
```

**Errors**:
- `400 Bad Request` - Invalid game session, already submitted, or anti-cheat validation failed
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Guest users cannot submit scores

**Anti-Cheat Validations**:
Score submissions are validated against:
- Game session ownership (must belong to authenticated user)
- Game completion status (game must be ended)
- Duplicate submission prevention (unique constraint on game session)
- Maximum entries per user (100 entries limit)
- Game age limit (must be submitted within 48 hours)
- Timestamp validation (rejects future-dated games)
- Minimum game duration (5 seconds)
- Score plausibility (mathematically achievable)
- Score rate limits (max 50 points/second average)
- Move count requirements for high scores

---

## Gamification Endpoints

### Get Gamification Status

Get the complete gamification status for the authenticated user.

```http
GET /api/gamification
```

**Headers** (Required):
```
Authorization: Bearer <token>
```

**Response**: `200 OK`
```json
{
  "streakData": {
    "currentStreak": 7,
    "longestStreak": 14,
    "lastPlayedAt": "2026-01-10T15:30:00Z",
    "streakFreezeCount": 2,
    "isStreakActive": true,
    "streakExpiresAt": "2026-01-11T00:00:00Z"
  },
  "activeChallenges": [...],
  "recentAchievements": [...],
  "seasonPass": {...},
  "equippedCosmetics": {...}
}
```

### Get Challenges

Get active daily and weekly challenges.

```http
GET /api/gamification/challenges
```

**Response**: `200 OK`
```json
{
  "daily": [
    {
      "id": 1,
      "name": "Score 500",
      "description": "Score at least 500 points in a single game",
      "type": "Daily",
      "targetType": "Score",
      "targetValue": 500,
      "currentProgress": 350,
      "xpReward": 50,
      "isCompleted": false,
      "expiresAt": "2026-01-11T00:00:00Z"
    }
  ],
  "weekly": [...]
}
```

### Get Achievements

Get all achievements with unlock status.

```http
GET /api/gamification/achievements
```

**Response**: `200 OK`
```json
[
  {
    "id": 1,
    "name": "First Win",
    "description": "Complete your first game",
    "category": "Progression",
    "rarity": "Common",
    "xpReward": 100,
    "isUnlocked": true,
    "unlockedAt": "2026-01-05T10:30:00Z",
    "iconUrl": "/icons/first-win.png"
  }
]
```

### Get Season Pass

Get current season and tier progress.

```http
GET /api/gamification/season-pass
```

**Response**: `200 OK`
```json
{
  "season": {
    "id": 1,
    "name": "Ocean Season",
    "theme": "ocean",
    "startDate": "2026-01-06T00:00:00Z",
    "endDate": "2026-01-13T00:00:00Z",
    "maxTiers": 50
  },
  "currentTier": 15,
  "currentXp": 1250,
  "xpToNextTier": 100,
  "rewards": [
    {
      "id": 1,
      "tier": 5,
      "rewardType": "Cosmetic",
      "description": "Bronze Frame",
      "isClaimed": true
    }
  ],
  "newRewardsCount": 2
}
```

### Get Cosmetics

Get owned and available cosmetics.

```http
GET /api/gamification/cosmetics
```

**Response**: `200 OK`
```json
{
  "owned": [
    {
      "id": 1,
      "code": "theme_classic",
      "name": "Classic Theme",
      "type": "BoardTheme",
      "rarity": "Common",
      "isEquipped": true
    }
  ],
  "equipped": {
    "boardTheme": 1,
    "avatarFrame": null,
    "badge": null
  }
}
```

### Get Personal Goals

Get current personal goals.

```http
GET /api/gamification/goals
```

**Response**: `200 OK`
```json
[
  {
    "id": 1,
    "type": "BeatAverage",
    "description": "Beat your average score of 450",
    "targetValue": 450,
    "currentValue": 0,
    "progressPercentage": 0,
    "isCompleted": false
  }
]
```

### Use Streak Freeze

Use a streak freeze token to protect your streak.

```http
POST /api/gamification/streak-freeze
```

**Response**: `200 OK`
```json
{
  "success": true,
  "remainingFreezes": 1,
  "message": "Streak freeze applied successfully"
}
```

**Errors**:
- `400 Bad Request` - No streak freeze tokens available
- `401 Unauthorized` - Not authenticated

### Equip Cosmetic

Equip a cosmetic item.

```http
POST /api/gamification/cosmetics/equip
```

**Request Body**:
```json
{
  "cosmeticId": 5
}
```

**Response**: `200 OK`
```json
{
  "success": true,
  "equipped": {
    "boardTheme": 5,
    "avatarFrame": 2,
    "badge": null
  }
}
```

**Errors**:
- `400 Bad Request` - Cosmetic not owned
- `404 Not Found` - Cosmetic not found

### Claim Season Reward

Claim a season pass reward.

```http
POST /api/gamification/season-pass/claim-reward
```

**Request Body**:
```json
{
  "rewardId": 15
}
```

**Response**: `200 OK`
```json
{
  "success": true,
  "reward": {
    "id": 15,
    "tier": 15,
    "rewardType": "Cosmetic",
    "description": "Ocean Theme",
    "value": 5
  }
}
```

**Errors**:
- `400 Bad Request` - Tier not reached or already claimed
- `404 Not Found` - Reward not found

### Set Timezone

Set the user's preferred timezone for streak calculations.

```http
POST /api/gamification/timezone
```

**Request Body**:
```json
{
  "timezone": "America/New_York"
}
```

**Response**: `200 OK`
```json
{
  "success": true,
  "timezone": "America/New_York"
}
```

**Errors**:
- `400 Bad Request` - Invalid timezone

---

## Error Responses

All errors follow this format:

```json
{
  "error": "Error description"
}
```

### Common HTTP Status Codes

| Code | Description |
|------|-------------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Authentication required |
| 403 | Forbidden - Access denied |
| 404 | Not Found - Resource doesn't exist |
| 500 | Internal Server Error |

---

## Data Types

### GameStatus

| Value | Description |
|-------|-------------|
| `Playing` | Game is active |
| `Ended` | Game completed (no moves left) |
| `Abandoned` | Game manually ended |

### AuthProvider

| Value | Description |
|-------|-------------|
| `Guest` | Anonymous guest user |
| `Local` | Local email/password authentication |
| `Microsoft` | Microsoft OAuth |

### PieceType

| Type | Description | Shape |
|------|-------------|-------|
| `I` | I-Tetromino | 4x1 line |
| `O` | O-Tetromino | 2x2 square |
| `T` | T-Tetromino | T-shape |
| `S` | S-Tetromino | S-shape |
| `Z` | Z-Tetromino | Z-shape |
| `J` | J-Tetromino | J-shape |
| `L` | L-Tetromino | L-shape |
| `DOT` | Single cell | 1x1 |
| `LINE2` | 2-cell line | 2x1 |
| `LINE3` | 3-cell line | 3x1 |
| `LINE5` | 5-cell line | 5x1 |
| `CORNER` | Small corner | L 2x2 |
| `BIG_CORNER` | Large corner | L 3x3 |
| `SQUARE_2X2` | Small square | 2x2 |
| `SQUARE_3X3` | Large square | 3x3 |
