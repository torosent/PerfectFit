# Backend Documentation

## Overview

The PerfectFit backend is built with ASP.NET Core 10 following Clean Architecture principles. It provides a RESTful API for game management, user authentication, and leaderboard functionality.

## Project Structure

```
backend/
├── PerfectFit.sln             # Solution file
├── src/
│   ├── PerfectFit.Core/       # Domain layer
│   ├── PerfectFit.UseCases/   # Application layer
│   ├── PerfectFit.Infrastructure/ # Data access layer
│   └── PerfectFit.Web/        # API layer
└── tests/
    ├── PerfectFit.UnitTests/      # Unit tests
    └── PerfectFit.IntegrationTests/ # Integration tests
```

## Quick Start

### Prerequisites
- .NET 10 SDK
- Docker (for PostgreSQL) or SQLite for local development

### Run with SQLite (Development)

```bash
cd backend
dotnet run --project src/PerfectFit.Web
```

The API will be available at:
- API: `http://localhost:5050`
- Swagger UI: `http://localhost:5050/swagger`

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
