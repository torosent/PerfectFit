# Database Documentation

## Overview

PerfectFit uses Entity Framework Core for data access with PostgreSQL as the database provider.

## Database Configuration

Configure the database connection in `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=perfectfit;Username=perfectfit;Password=perfectfit_dev"
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

### Migration Strategy

PerfectFit uses Entity Framework Core migrations to manage database schema changes. The application is configured to **automatically apply pending migrations on startup**, ensuring the database schema stays in sync with the application code.

**Key Benefits:**
- No data loss when schema changes - existing data is preserved
- Version-controlled schema changes
- Rollback capability
- Consistent schema across environments

### Configuration

Migration behavior can be configured in `appsettings.json`:

```json
{
  "DatabaseMigration": {
    "RunMigrationsOnStartup": true,
    "FailOnMigrationError": true,
    "MigrationTimeoutSeconds": 300,
    "LogMigrationSql": false,
    "ConnectionRetryCount": 5,
    "ConnectionRetryDelaySeconds": 5
  }
}
```

| Setting | Description | Default |
|---------|-------------|---------|
| `RunMigrationsOnStartup` | Run migrations when the app starts | `true` |
| `FailOnMigrationError` | Stop the app if migrations fail | `true` |
| `MigrationTimeoutSeconds` | Maximum time to wait for migrations | `300` |
| `LogMigrationSql` | Log SQL commands during migration | `false` |
| `ConnectionRetryCount` | Retry attempts if database is unavailable | `5` |
| `ConnectionRetryDelaySeconds` | Delay between connection retries | `5` |

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

The application uses a hosted service (`DatabaseMigrationHostedService`) to apply migrations:

1. **Connection Retry** - Waits for the database to be available (useful for containerized deployments)
2. **Pending Migration Check** - Lists any unapplied migrations
3. **Migration Application** - Applies pending migrations in order
4. **Error Handling** - Configurable behavior on failure

```csharp
// Migrations are automatically applied via DatabaseMigrationHostedService
// Configure behavior in appsettings.json under "DatabaseMigration"
```

**Important:** 
- For production, always review migrations before deployment
- Use `dotnet ef migrations script` to generate SQL for review
- Back up your database before applying migrations in production

### Migration Best Practices

1. **Always Create Migrations for Schema Changes**
   ```bash
   dotnet ef migrations add DescriptiveMigrationName \
     --project ../PerfectFit.Infrastructure
   ```

2. **Review Generated Migration Code** - Check the `Up()` and `Down()` methods

3. **Test Migrations Locally** - Apply migrations to a local database before deployment

4. **Include Data Migrations When Needed** - For data transformations, add code to the migration

5. **Never Modify Applied Migrations** - Create a new migration instead

### Manual Migration (Production)

For production deployments where you want more control over migrations:

**Option 1: Disable Auto-Migration**

Set `RunMigrationsOnStartup` to `false` and run migrations separately:

```bash
# Generate SQL script for review
dotnet ef migrations script --idempotent \
  --project ../PerfectFit.Infrastructure \
  --output migration.sql

# Review the script, then apply manually
psql -U perfectfit -d perfectfit -f migration.sql
```

**Option 2: Use a Migration Job/Init Container**

In Kubernetes or container orchestration, run migrations as a separate job before deployment:

```bash
# Run migrations in a container
docker run --rm \
  -e ConnectionStrings__DefaultConnection="your-connection-string" \
  perfectfit-backend \
  dotnet ef database update --project src/PerfectFit.Infrastructure
```

**Option 3: CI/CD Pipeline Migration**

Add a migration step to your deployment pipeline:

```yaml
# Example GitHub Actions step
- name: Apply Database Migrations
  run: |
    dotnet ef database update \
      --project backend/src/PerfectFit.Infrastructure \
      --startup-project backend/src/PerfectFit.Web
```

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

### Migration Errors

```
The entity type 'X' requires a primary key to be defined.
```

**Solution**: Ensure all entities have a properly configured primary key in `AppDbContext.OnModelCreating()`.
