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
├── deploy/            # Deployment configurations
│   ├── azure/         # Azure Container Apps scripts & Bicep
│   └── cloudflare/    # Cloudflare Pages scripts
├── docker-compose.yml # Full stack container setup
└── docs/              # Project documentation
```

## Prerequisites

- Node.js 22+
- .NET 10 SDK
- Docker (for local development)

## Quick Start

### Option 1: Full Stack with Docker Compose (Recommended)

```bash
# Start all services (frontend, backend, database)
docker compose up -d

# Services:
# - Frontend: http://localhost:3000
# - Backend: http://localhost:8080
# - PostgreSQL: localhost:5432
```

### Option 2: Individual Development

#### 1. Start PostgreSQL

```bash
docker compose up -d postgres
```

#### 2. Start Backend

```bash
cd backend
dotnet run --project src/PerfectFit.Web
```

The API will be available at `http://localhost:5050` and Swagger UI at `http://localhost:5050/swagger`.

#### 3. Start Frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend will be available at `http://localhost:3000`.

## Deployment

### Azure Container Apps

```bash
./deploy/azure/deploy-container-apps.sh production
```

### Cloudflare Pages (Frontend Only)

```bash
./deploy/cloudflare/deploy-cloudflare-pages.sh production
```

See [Deployment Guide](docs/deployment.md) for full documentation.

## Running Tests

### Backend Tests

```bash
cd backend
dotnet test
```

### Frontend Tests

```bash
cd frontend
npm test
```

## Technology Stack

| Layer | Technology |
|-------|------------|
| Frontend | Next.js 16, React 19, TypeScript, Tailwind CSS 4, Zustand, @dnd-kit, Motion |
| Backend | ASP.NET Core 10, C# 13, Entity Framework Core, MediatR |
| Database | PostgreSQL 16 |
| Deployment | Docker, Azure Container Apps, Cloudflare Pages |
| Architecture | Clean Architecture, CQRS |

## Documentation

- [Project Overview](docs/overview.md)
- [Architecture](docs/architecture.md)
- [Development Guide](docs/development.md)
- [Deployment Guide](docs/deployment.md)
- [Backend API Reference](docs/backend/api-reference.md)
- [Frontend Components](docs/frontend/components.md)

## License

Private project.
