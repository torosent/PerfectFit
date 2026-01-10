# PerfectFit

A modern, full-stack grid-based block placement puzzle game built with Next.js 16 and ASP.NET Core 10.

## 🎮 About the Game

PerfectFit is a strategic puzzle game where players place tetromino-like pieces on a 8x8 grid. The goal is to clear lines vertically or horizontally to score points and keep the board from filling up.

## Demo
[Demo](https://perfectfit.tomer.dev)

![Screenshot of PerfectFit game showing a 8x8 grid with colorful tetromino-like pieces and the game UI](https://github.com/user-attachments/assets/52fea048-c1c9-4ed5-b4b8-c592fb5fc3df)

### Key Features

- **Strategic Gameplay**: Place pieces on a 8x8 grid to clear lines.
- **Diverse Pieces**: Over 15 unique shapes including Tetrominoes, Lines, Corners, and Squares.
- **Combo System**: Chain line clears for massive bonus points.
- **Smooth Experience**: Drag-and-drop controls powered by `@dnd-kit` and fluid animations with `Motion`.
- **Competitive**: Global leaderboards and personal high scores.
- **Secure**: Robust anti-cheat system with server-side validation and rate limiting.
- **User Accounts**: Sign in with Email or Microsoft Account to save your progress.

### Gamification System

- **Daily & Weekly Challenges**: Complete challenges to earn XP and rewards.
- **Streak System**: Maintain daily play streaks with streak freeze tokens to protect your progress.
- **Season Pass**: Progress through 50 tiers of rewards with themed seasons (7-day cycles).
- **Achievements & Badges**: Unlock 18+ achievements across 5 categories with cosmetic rewards.
- **Cosmetics**: Customize your experience with board themes, avatar frames, and profile badges.
- **Personal Goals**: Beat your average, improve accuracy, and set new personal bests.

## 🛠️ Tech Stack

### Frontend
- **Framework**: [Next.js 16](https://nextjs.org/) (App Router)
- **Language**: TypeScript
- **Styling**: [Tailwind CSS 4](https://tailwindcss.com/)
- **State Management**: [Zustand](https://github.com/pmndrs/zustand)
- **Interactivity**: [`@dnd-kit`](https://dndkit.com/) for drag-and-drop
- **Animations**: [Motion](https://motion.dev/)

### Backend
- **Framework**: [ASP.NET Core 10](https://dotnet.microsoft.com/en-us/apps/aspnet)
- **Language**: C# 13
- **Architecture**: Clean Architecture with CQRS (MediatR)
- **Database**: PostgreSQL 16 with Entity Framework Core
- **API**: Minimal APIs with Swagger/OpenAPI

### Infrastructure
- **Containerization**: Docker & Docker Compose
- **Cloud**: Azure Container Apps & Cloudflare Pages

## 🚀 Getting Started

### Prerequisites
- [Node.js 22+](https://nodejs.org/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Quick Start (Recommended)

Run the entire stack with Docker Compose:

```bash
docker compose up -d
```

- **Frontend**: [http://localhost:3000](http://localhost:3000)
- **Backend API**: [http://localhost:8080](http://localhost:8080)
- **Swagger UI**: [http://localhost:8080/swagger](http://localhost:8080/swagger)

### Manual Setup

If you prefer running services individually:

1.  **Start Database**:
    ```bash
    docker compose up -d postgres
    ```

2.  **Start Backend**:
    ```bash
    cd backend
    dotnet restore
    dotnet run --project src/PerfectFit.Web
    ```
    *Runs on http://localhost:5050*

3.  **Start Frontend**:
    ```bash
    cd frontend
    npm install
    npm run dev
    ```
    *Runs on http://localhost:3000*

## 📂 Project Structure

```
PerfectFit/
├── backend/                # ASP.NET Core Solution
│   ├── src/
│   │   ├── PerfectFit.Core/           # Domain Entities & Logic
│   │   ├── PerfectFit.UseCases/       # Application Business Rules (CQRS)
│   │   ├── PerfectFit.Infrastructure/ # Database, Jobs & External Services
│   │   └── PerfectFit.Web/            # API Endpoints
│   └── tests/              # Unit & Integration Tests (830+ tests)
├── frontend/               # Next.js Application
│   ├── src/
│   │   ├── app/            # Pages & Routes
│   │   ├── components/     # React Components (incl. gamification)
│   │   ├── stores/         # Zustand State Management
│   │   └── lib/            # Game Logic & API Clients
│   └── __tests__/          # Frontend Tests (580+ tests)
├── deploy/                 # Deployment Scripts (Azure/Cloudflare)
└── docs/                   # Detailed Documentation
```

## 📖 Documentation

- [**Overview**](docs/overview.md): Game mechanics and features.
- [**Architecture**](docs/architecture.md): System design and patterns.
- [**Gamification**](docs/gamification.md): Challenges, streaks, seasons, and achievements.
- [**Development Guide**](docs/development.md): Setup and contribution guidelines.
- [**API Reference**](docs/backend/api-reference.md): Backend endpoints.

## 🚢 Deployment

Scripts are provided for deploying to Azure and Cloudflare:

- **Azure Container Apps**: `./deploy/azure/deploy-container-apps.sh`
- **Cloudflare Pages**: `./deploy/cloudflare/deploy-cloudflare-pages.sh`

See [Deployment Guide](docs/deployment.md) for details.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
