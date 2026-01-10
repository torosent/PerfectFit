# Backend Documentation

## Overview

The PerfectFit backend is built with ASP.NET Core 10 following Clean Architecture principles. It provides a RESTful API for game management, user authentication, and leaderboard functionality.

## Project Structure

```
backend/
├── PerfectFit.sln             # Solution file
├── src/
│   ├── PerfectFit.Core/       # Domain layer
│   │   ├── Entities/          # Domain entities (incl. gamification)
│   │   ├── Enums/             # Enumerations
│   │   ├── GameLogic/         # Core game logic
│   │   ├── Interfaces/        # Repository contracts
│   │   └── Services/          # Domain services (6 gamification services)
│   ├── PerfectFit.UseCases/   # Application layer
│   │   ├── Auth/              # Authentication use cases
│   │   ├── Games/             # Game use cases
│   │   ├── Gamification/      # Gamification commands/queries
│   │   └── Leaderboard/       # Leaderboard use cases
│   ├── PerfectFit.Infrastructure/ # Data access layer
│   │   ├── Data/              # EF Core, repositories, seed data
│   │   ├── Jobs/              # Background jobs (4 jobs)
│   │   ├── Identity/          # Auth infrastructure
│   │   └── Services/          # External services
│   └── PerfectFit.Web/        # API layer
│       ├── Endpoints/         # Minimal API endpoints (incl. gamification)
│       └── DTOs/              # Request/response DTOs
└── tests/
    ├── PerfectFit.UnitTests/      # Unit tests (730+)
    └── PerfectFit.IntegrationTests/ # Integration tests (100+)
```

## Quick Start

### Prerequisites
- .NET 10 SDK
- Docker (for PostgreSQL)

### Run with PostgreSQL

1. Start PostgreSQL:
```bash
docker compose up -d
```

2. Update `appsettings.Development.json`:
```json
{
  "DatabaseProvider": "PostgreSQL"
}
```

3. Run the application:
```bash
dotnet run --project src/PerfectFit.Web
```

## Testing

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test tests/PerfectFit.UnitTests

# Run only integration tests
dotnet test tests/PerfectFit.IntegrationTests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Related Documentation

- [API Reference](./api-reference.md) - Complete endpoint documentation
- [Authentication](./authentication.md) - Authentication methods and security
- [Admin API](./admin-api.md) - Admin endpoints and user management
- [Configuration](./configuration.md) - All configuration options
- [Database](./database.md) - Database schema and migrations
- [Gamification](../gamification.md) - Gamification system documentation
