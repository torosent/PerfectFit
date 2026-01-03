# Development Guide

## Prerequisites

- **Node.js** 22+ ([Download](https://nodejs.org/))
- **.NET 10 SDK** ([Download](https://dotnet.microsoft.com/download))
- **Docker** (for PostgreSQL and full-stack development) ([Download](https://docker.com/))
- **Git** ([Download](https://git-scm.com/))

## Initial Setup

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/perfectfit.git
cd perfectfit
```

### 2. Start the Database (or Full Stack)

```bash
# Start PostgreSQL only
docker compose up -d postgres

# Or start the full stack (PostgreSQL + Backend + Frontend)
docker compose up -d

# Verify services are running
docker compose ps
```

**Full Stack URLs:**
- Frontend: http://localhost:3000
- Backend API: http://localhost:8080
- PostgreSQL: localhost:5432

### 3. Configure Backend

```bash
cd backend/src/PerfectFit.Web

# Initialize user secrets
dotnet user-secrets init

# Set required secrets
dotnet user-secrets set "Jwt:Secret" "your-secret-key-at-least-32-characters-long"

# Configure Microsoft OAuth (optional - needed for Microsoft sign-in)
dotnet user-secrets set "OAuth:Microsoft:ClientId" "your-microsoft-client-id"
dotnet user-secrets set "OAuth:Microsoft:ClientSecret" "your-microsoft-client-secret"
```

> **Note**: PerfectFit supports three authentication methods:
> - **Local auth** (email/password) - works out of the box
> - **Microsoft OAuth** - requires Azure AD app registration
> - **Guest** - no configuration needed

### 4. Start the Backend

```bash
cd backend
dotnet run --project src/PerfectFit.Web
```

Backend will be available at:
- API: http://localhost:5050 (local dev) or http://localhost:8080 (Docker)
- Swagger: http://localhost:5050/swagger

### 5. Start the Frontend

```bash
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

Frontend will be available at http://localhost:3000.

---

## Development Workflow

### Project Structure

```
PerfectFit/
├── backend/           # ASP.NET Core backend
├── frontend/          # Next.js frontend
├── docs/              # Documentation
├── plans/             # Project planning
└── docker-compose.yml # Database setup
```

### Running Both Servers

Open two terminals:

**Terminal 1 - Backend:**
```bash
cd backend
dotnet watch --project src/PerfectFit.Web
```

**Terminal 2 - Frontend:**
```bash
cd frontend
npm run dev
```

### Using SQLite for Development

For simpler development without Docker:

1. Edit `appsettings.Development.json`:
```json
{
  "DatabaseProvider": "SQLite"
}
```

2. Run the backend - SQLite database will be created automatically.

---

## Testing

### Backend Tests

```bash
cd backend

# Run all tests
dotnet test

# Run with verbosity
dotnet test --verbosity normal

# Run specific test project
dotnet test tests/PerfectFit.UnitTests
dotnet test tests/PerfectFit.IntegrationTests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend Tests

```bash
cd frontend

# Run linter
npm run lint

# Run tests (when configured)
npm test
```

---

## Code Style

### Backend (C#)

- Use **PascalCase** for public members
- Use **_camelCase** for private fields
- Follow .NET naming conventions
- Use async/await for all I/O operations
- Use meaningful variable names

```csharp
// Good
public async Task<GameSession> CreateGameAsync(int? userId)
{
    var session = GameSession.Create(userId);
    await _repository.AddAsync(session);
    return session;
}

// Bad
public GameSession Create(int? id)
{
    var s = GameSession.Create(id);
    _repo.Add(s);
    return s;
}
```

### Frontend (TypeScript)

- Use **camelCase** for variables and functions
- Use **PascalCase** for components and types
- Prefer functional components with hooks
- Use TypeScript strict mode

```typescript
// Good
interface GameBoardProps {
  grid: Grid;
  onCellClick: (row: number, col: number) => void;
}

const GameBoard: FC<GameBoardProps> = ({ grid, onCellClick }) => {
  // ...
};

// Bad
const gameboard = (props) => {
  // ...
};
```

---

## Git Workflow

### Branch Naming

- `feature/description` - New features
- `fix/description` - Bug fixes
- `refactor/description` - Code refactoring
- `docs/description` - Documentation updates

### Commit Messages

Use conventional commits:

```
feat: add leaderboard submission
fix: resolve piece placement validation
docs: update API documentation
refactor: simplify game engine
test: add integration tests for auth
```

### Pull Request Process

1. Create feature branch from `main`
2. Make changes
3. Run tests
4. Create PR with description
5. Address review feedback
6. Merge after approval

---

## Debugging

### Backend Debugging

**VS Code**:
1. Open `backend/` folder
2. Press F5 to start debugging
3. Set breakpoints as needed

**Visual Studio**:
1. Open `PerfectFit.sln`
2. Press F5 to debug
3. Use breakpoints and watch windows

**Logging**:
```csharp
_logger.LogInformation("Creating game for user {UserId}", userId);
_logger.LogError(ex, "Failed to place piece");
```

### Frontend Debugging

**Browser DevTools**:
- React DevTools for component inspection
- Network tab for API requests
- Console for errors and logs

**VS Code**:
1. Add breakpoints in TypeScript files
2. Use Chrome DevTools with source maps

**Debug Logging**:
```typescript
console.log('Game state:', useGameStore.getState());
console.log('API response:', response);
```

---

## Common Tasks

### Add a New API Endpoint

1. Create command/query in `PerfectFit.UseCases`
2. Add endpoint in `PerfectFit.Web/Endpoints`
3. Update Swagger documentation
4. Add tests

### Add a New Component

1. Create component in `frontend/src/components/`
2. Add TypeScript types
3. Export from index file
4. Add to page/layout

### Add Database Migration

```bash
cd backend/src/PerfectFit.Web

dotnet ef migrations add MigrationName \
  --project ../PerfectFit.Infrastructure

dotnet ef database update \
  --project ../PerfectFit.Infrastructure
```

### Update Dependencies

**Backend**:
```bash
cd backend
dotnet restore
dotnet outdated  # if dotnet-outdated is installed
```

**Frontend**:
```bash
cd frontend
npm outdated
npm update
```

---

## Troubleshooting

### Backend Won't Start

```
System.InvalidOperationException: Unable to resolve service
```
**Solution**: Check dependency injection in `Program.cs` and `DependencyInjection.cs`.

### Database Connection Failed

```
Npgsql.NpgsqlException: Failed to connect
```
**Solution**: Ensure Docker is running: `docker compose up -d`

### Frontend API Errors

```
TypeError: Failed to fetch
```
**Solution**: 
1. Check backend is running
2. Verify `NEXT_PUBLIC_API_URL` in `.env.local`
3. Check CORS configuration

### JWT Token Invalid

```
401 Unauthorized
```
**Solution**: 
1. Clear browser localStorage
2. Login again
3. Check JWT secret is configured

---

## IDE Setup

### VS Code Extensions (Recommended)

**Backend**:
- C# Dev Kit
- .NET Extension Pack

**Frontend**:
- ESLint
- Prettier
- Tailwind CSS IntelliSense
- TypeScript Vue Plugin (Volar)

### VS Code Settings

```json
{
  "editor.formatOnSave": true,
  "editor.defaultFormatter": "esbenp.prettier-vscode",
  "[csharp]": {
    "editor.defaultFormatter": "ms-dotnettools.csdevkit"
  },
  "typescript.tsdk": "frontend/node_modules/typescript/lib"
}
```

### Rider / Visual Studio

Open `backend/PerfectFit.sln` for the best C# development experience.
