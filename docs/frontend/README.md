# Frontend Documentation

## Overview

The PerfectFit frontend is built with Next.js 16 using the App Router. It provides an interactive block puzzle game experience with drag-and-drop functionality, animations, and real-time state management.

## Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| Next.js | 16.1.1 | React framework with App Router |
| React | 19.2.3 | UI library |
| TypeScript | 5.x | Type safety |
| Tailwind CSS | 4.x | Styling |
| Zustand | 5.0.5 | State management |
| @dnd-kit | 6.3.1 | Drag and drop |
| Motion | 12.17.0 | Animations |

## Project Structure

```
frontend/
├── src/
│   ├── app/                    # Next.js App Router pages
│   │   ├── layout.tsx          # Root layout
│   │   ├── page.tsx            # Home page
│   │   ├── globals.css         # Global styles
│   │   ├── (auth)/             # Auth route group
│   │   └── (game)/             # Game route group
│   │
│   ├── components/             # React components
│   │   ├── auth/               # Auth components
│   │   ├── game/               # Game components
│   │   ├── leaderboard/        # Leaderboard components
│   │   ├── providers/          # Context providers
│   │   └── ui/                 # Reusable UI
│   │
│   ├── hooks/                  # Custom hooks
│   ├── lib/                    # Utilities
│   │   ├── api/                # API clients
│   │   ├── stores/             # Zustand stores
│   │   ├── game-logic/         # Game utilities
│   │   └── animations/         # Animation utilities
│   │
│   ├── contexts/               # React contexts
│   └── types/                  # TypeScript types
│
├── public/                     # Static assets
├── package.json
├── next.config.ts
├── tsconfig.json
├── tailwind.config.ts
└── postcss.config.mjs
```

## Quick Start

### Prerequisites
- Node.js 18+
- npm or yarn

### Installation

```bash
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

The frontend will be available at `http://localhost:3000`.

### Available Scripts

| Script | Description |
|--------|-------------|
| `npm run dev` | Start development server |
| `npm run build` | Build for production |
| `npm start` | Start production server |
| `npm run lint` | Run ESLint |

## Related Documentation

- [Components](./components.md) - UI component documentation
- [State Management](./state-management.md) - Zustand stores guide
- [Configuration](./configuration.md) - Environment and settings
