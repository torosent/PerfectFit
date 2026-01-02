# Database Documentation

## Overview

PerfectFit uses Entity Framework Core for data access with support for multiple database providers:

- **PostgreSQL** - Production-ready, recommended for deployment
- **SQLite** - Lightweight, ideal for local development

## Database Selection

Configure the database provider in `appsettings.json` or `appsettings.Development.json`:

```json
{
  "DatabaseProvider": "SQLite",  // or "PostgreSQL"
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=perfectfit;Username=perfectfit;Password=perfectfit_dev",
    "SqliteConnection": "Data Source=perfectfit.db"
  }
}
```

## Schema

### Users Table

Stores user account information.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | int | PK, auto-increment | User ID |
| `ExternalId` | string | Required, unique | OAuth provider user ID |
| `Email` | string | Nullable | User email (optional for guests) |
| `DisplayName` | string | Required | Display name |
| `Provider` | int | Required | Auth provider enum value |
| `CreatedAt` | datetime | Required | Account creation timestamp |
| `LastLoginAt` | datetime | Nullable | Last login timestamp |
| `HighScore` | int | Default: 0 | User's highest score |
| `GamesPlayed` | int | Default: 0 | Total games played |

### GameSessions Table

Stores game session data.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | GUID | PK | Game session ID |
| `UserId` | int | FK (nullable) | Associated user (null for anonymous) |
| `BoardState` | string (JSON) | Required | 10x10 grid state |
| `CurrentPieces` | string (JSON) | Required | Current available pieces |
| `PieceBagState` | string (JSON) | Required | Piece generator state |
| `Score` | int | Default: 0 | Current score |
| `Combo` | int | Default: 0 | Current combo count |
| `LinesCleared` | int | Default: 0 | Total lines cleared |
| `MaxCombo` | int | Default: 0 | Highest combo achieved |
| `Status` | int | Required | Game status enum |
| `StartedAt` | datetime | Required | Game start timestamp |
| `EndedAt` | datetime | Nullable | Game end timestamp |
| `LastActivityAt` | datetime | Required | Last action timestamp |
| `MoveCount` | int | Default: 0 | Total moves made (anti-cheat) |
| `LastMoveAt` | datetime | Nullable | Timestamp of last move (anti-cheat) |
| `MoveHistory` | string (JSON) | Default: "[]" | History of all moves (anti-cheat) |
| `ClientFingerprint` | string | Default: "" | Client identification (anti-cheat) |

### LeaderboardEntries Table

Stores leaderboard scores.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | int | PK, auto-increment | Entry ID |
| `UserId` | int | FK, Required | User who achieved the score |
| `GameSessionId` | GUID | FK, Required, **UNIQUE** | Associated game session (one entry per game) |
| `Score` | int | Required | Final score |
| `LinesCleared` | int | Required | Lines cleared |
| `MaxCombo` | int | Required | Max combo achieved |
| `AchievedAt` | datetime | Required | Timestamp of achievement |

**Note**: The unique constraint on `GameSessionId` prevents duplicate leaderboard submissions for the same game session.

## Entity Relationships

```
Users 1 ←─────→ N GameSessions
  │
  │
  └──── 1 ←─────→ N LeaderboardEntries
                        │
                        └──── 1 ←─────→ 1 GameSessions
```

## JSON Data Formats

### BoardState

```json
{
  "grid": [
    [0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0],
    [0,0,0,0,0,0,0,0,0,0]
  ]
}
```

- `0` = empty cell
- Non-zero values = occupied cell with color index

### CurrentPieces

```json
[
  {"type": "T", "color": "#a855f7"},
  {"type": "LINE3", "color": "#3b82f6"},
  {"type": "SQUARE_2X2", "color": "#eab308"}
]
```

### PieceBagState

```json
{
  "index": 15,
  "seed": 12345
}
```

### MoveHistory (Anti-Cheat)

```json
[
  {"i": 0, "r": 5, "c": 3, "p": 30, "l": 1, "t": "2026-01-02T12:00:00Z"},
  {"i": 1, "r": 2, "c": 7, "p": 0, "l": 0, "t": "2026-01-02T12:00:02Z"}
]
```

| Field | Description |
|-------|-------------|
| `i` | Piece index (0-2) |
| `r` | Row position |
| `c` | Column position |
| `p` | Points earned |
| `l` | Lines cleared |
| `t` | Timestamp (ISO 8601) |

## Database Migrations

### Using EF Core Migrations

```bash
cd backend/src/PerfectFit.Web

# Create a new migration
dotnet ef migrations add <MigrationName> \
  --project ../PerfectFit.Infrastructure

# Apply migrations
dotnet ef database update \
  --project ../PerfectFit.Infrastructure

# Remove last migration (if not applied)
dotnet ef migrations remove \
  --project ../PerfectFit.Infrastructure

# Generate SQL script
dotnet ef migrations script \
  --project ../PerfectFit.Infrastructure \
  --output script.sql
```

### Auto-Migration on Startup

The application automatically ensures the database exists on startup:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}
```

**Note**: For production, use explicit migrations instead of `EnsureCreated()`.

## PostgreSQL Setup

### Using Docker

```bash
# Start PostgreSQL container
docker compose up -d postgres

# Connect to database
docker exec -it perfectfit-postgres psql -U perfectfit -d perfectfit

# View tables
\dt

# Describe table
\d "Users"
```

### Docker Compose Configuration

```yaml
services:
  postgres:
    image: postgres:16
    container_name: perfectfit-postgres
    environment:
      POSTGRES_DB: perfectfit
      POSTGRES_USER: perfectfit
      POSTGRES_PASSWORD: perfectfit_dev
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U perfectfit -d perfectfit"]
      interval: 10s
      timeout: 5s
      retries: 5
    restart: unless-stopped

volumes:
  postgres_data:
```

### Manual PostgreSQL Setup

```sql
-- Create database
CREATE DATABASE perfectfit;

-- Create user
CREATE USER perfectfit WITH PASSWORD 'perfectfit_dev';

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE perfectfit TO perfectfit;
```

## SQLite Setup

SQLite requires no setup. The database file is created automatically:

```
backend/src/PerfectFit.Web/perfectfit.db
```

### Viewing SQLite Database

```bash
# Using sqlite3 CLI
sqlite3 perfectfit.db

# List tables
.tables

# Describe schema
.schema Users

# Query data
SELECT * FROM Users;
```

Or use a GUI tool like:
- [DB Browser for SQLite](https://sqlitebrowser.org/)
- VS Code SQLite extension

## Performance Considerations

### Indexes

Recommended indexes for production:

```sql
-- Users
CREATE INDEX IX_Users_ExternalId ON "Users" ("ExternalId");
CREATE INDEX IX_Users_HighScore ON "Users" ("HighScore" DESC);

-- GameSessions
CREATE INDEX IX_GameSessions_UserId ON "GameSessions" ("UserId");
CREATE INDEX IX_GameSessions_Status ON "GameSessions" ("Status");
CREATE INDEX IX_GameSessions_StartedAt ON "GameSessions" ("StartedAt" DESC);

-- LeaderboardEntries
CREATE INDEX IX_LeaderboardEntries_Score ON "LeaderboardEntries" ("Score" DESC);
CREATE INDEX IX_LeaderboardEntries_UserId ON "LeaderboardEntries" ("UserId");
CREATE INDEX IX_LeaderboardEntries_AchievedAt ON "LeaderboardEntries" ("AchievedAt" DESC);
CREATE UNIQUE INDEX IX_LeaderboardEntries_GameSessionId ON "LeaderboardEntries" ("GameSessionId"); -- Anti-cheat: prevents duplicate submissions
```

### Query Optimization

The application uses:
- **Async queries** - All database operations are async
- **No tracking** - Read-only queries use `AsNoTracking()`
- **Projections** - Only necessary columns are selected

## Backup & Restore

### PostgreSQL

```bash
# Backup
docker exec perfectfit-postgres pg_dump -U perfectfit perfectfit > backup.sql

# Restore
docker exec -i perfectfit-postgres psql -U perfectfit perfectfit < backup.sql
```

### SQLite

```bash
# Backup (just copy the file)
cp perfectfit.db perfectfit.backup.db

# Restore
cp perfectfit.backup.db perfectfit.db
```

## Troubleshooting

### Connection Refused

```
Npgsql.NpgsqlException: Failed to connect to localhost:5432
```

**Solution**: Ensure PostgreSQL is running:
```bash
docker compose up -d postgres
docker compose logs postgres
```

### Database Locked (SQLite)

```
Microsoft.Data.Sqlite.SqliteException: database is locked
```

**Solution**: Close other connections (e.g., DB Browser) or restart the application.

### Migration Errors

```
The entity type 'X' requires a primary key to be defined.
```

**Solution**: Ensure all entities have a properly configured primary key in `AppDbContext.OnModelCreating()`.
