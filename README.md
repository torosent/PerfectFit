# PerfectFit Project

A full-stack grid-based block placement puzzle game using Next.js frontend and C# ASP.NET Core backend.

## Project Structure

```
PerfectFit/
├── frontend/           # Next.js 16 App Router frontend
│   ├── src/
│   │   ├── app/       # Next.js app routes
│   │   ├── components/ # React components
│   │   │   ├── game/  # Game-specific components
│   │   │   └── ui/    # Reusable UI components
│   │   ├── contexts/  # React context providers
│   │   ├── hooks/     # Custom React hooks
│   │   ├── lib/       # Utility libraries
│   │   │   ├── api/   # API client
│   │   │   ├── game-logic/ # Game logic utilities
│   │   │   └── stores/ # Zustand state stores
│   │   └── types/     # TypeScript type definitions
│   └── package.json
├── backend/           # ASP.NET Core backend
│   ├── src/
│   │   ├── PerfectFit.Core/           # Domain layer
│   │   ├── PerfectFit.UseCases/       # Application layer
│   │   ├── PerfectFit.Infrastructure/ # Data access layer
│   │   └── PerfectFit.Web/            # API layer
│   └── tests/
│       ├── PerfectFit.UnitTests/
│       └── PerfectFit.IntegrationTests/
├── docker-compose.yml  # PostgreSQL container
└── plans/             # Project planning documents
```

## Prerequisites

- Node.js 18+
- .NET 9 SDK
- Docker (for PostgreSQL)

## Quick Start

### 1. Start PostgreSQL

```bash
docker compose up -d
```

### 2. Start Backend

```bash
cd backend
dotnet run --project src/PerfectFit.Web
```

The API will be available at `http://localhost:5000` and Swagger UI at `http://localhost:5000/swagger`.

### 3. Start Frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend will be available at `http://localhost:3000`.

## Running Tests

### Backend Tests

```bash
cd backend
dotnet test
```

### Frontend (future)

```bash
cd frontend
npm test
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check |
| GET | `/api/status` | API status |

## Technology Stack

- **Frontend**: Next.js 16, TypeScript, Tailwind CSS, @dnd-kit, Zustand, Motion
- **Backend**: ASP.NET Core 9, C#, Entity Framework Core
- **Database**: PostgreSQL 16
- **Architecture**: Clean Architecture with CQRS

## License

Private project.
