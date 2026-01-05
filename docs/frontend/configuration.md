# Frontend Configuration

## Environment Variables

Create a `.env.local` file in the `frontend/` directory:

```bash
# API Configuration
NEXT_PUBLIC_API_URL=http://localhost:5050

# Feature Flags (optional)
NEXT_PUBLIC_ENABLE_ANALYTICS=false
NEXT_PUBLIC_ENABLE_DEBUG=true
```

### Available Variables

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `NEXT_PUBLIC_API_URL` | No | `http://localhost:5050` | Backend API base URL |
| `NEXT_PUBLIC_ENABLE_ANALYTICS` | No | `false` | Enable analytics tracking |
| `NEXT_PUBLIC_ENABLE_DEBUG` | No | `false` | Enable debug features |
| `BUILD_STANDALONE` | No | `false` | Enable standalone output for Docker builds |

**Note**: Variables prefixed with `NEXT_PUBLIC_` are exposed to the browser.

---

## Next.js Configuration

### next.config.ts

```typescript
import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Enable standalone output for Docker deployments
  output: process.env.BUILD_STANDALONE === 'true' ? 'standalone' : undefined,
  
  async headers() {
    return [
      {
        source: '/:path*',
        headers: [
          {
            key: 'X-DNS-Prefetch-Control',
            value: 'on'
          },
          {
            key: 'Strict-Transport-Security',
            value: 'max-age=63072000; includeSubDomains; preload'
          },
          {
            key: 'X-Frame-Options',
            value: 'SAMEORIGIN'
          },
          {
            key: 'X-Content-Type-Options',
            value: 'nosniff'
          },
          {
            key: 'Referrer-Policy',
            value: 'origin-when-cross-origin'
          },
          {
            key: 'Content-Security-Policy',
            value: "default-src 'self'; script-src 'self' 'unsafe-eval' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' blob: data:; font-src 'self' data:; connect-src 'self' http://localhost:5050;"
          }
        ]
      }
    ];
  }
};

export default nextConfig;
```

### Configuration Options

| Option | Description |
|--------|-------------|
| `headers` | Security headers for all routes |
| `images` | Image optimization settings |
| `experimental` | Experimental features |
| `env` | Build-time environment variables |

### Security Headers

| Header | Purpose |
|--------|---------|
| `X-DNS-Prefetch-Control` | Controls DNS prefetching |
| `Strict-Transport-Security` | Forces HTTPS |
| `X-Frame-Options` | Prevents clickjacking |
| `X-Content-Type-Options` | Prevents MIME sniffing |
| `Referrer-Policy` | Controls referrer information |
| `Content-Security-Policy` | Controls resource loading |

### Updating CSP for Production

Update the `connect-src` directive to include your production API:

```typescript
{
  key: 'Content-Security-Policy',
  value: "default-src 'self'; ... connect-src 'self' https://api.perfectfit.com;"
}
```

---

## TypeScript Configuration

### tsconfig.json

```json
{
  "compilerOptions": {
    "target": "ES2017",
    "lib": ["dom", "dom.iterable", "esnext"],
    "allowJs": true,
    "skipLibCheck": true,
    "strict": true,
    "noEmit": true,
    "esModuleInterop": true,
    "module": "esnext",
    "moduleResolution": "bundler",
    "resolveJsonModule": true,
    "isolatedModules": true,
    "jsx": "preserve",
    "incremental": true,
    "plugins": [
      {
        "name": "next"
      }
    ],
    "paths": {
      "@/*": ["./src/*"]
    }
  },
  "include": ["next-env.d.ts", "**/*.ts", "**/*.tsx", ".next/types/**/*.ts"],
  "exclude": ["node_modules"]
}
```

### Path Aliases

The `@/*` alias maps to `./src/*`:

```typescript
// Instead of:
import { useGameStore } from '../../../lib/stores/game-store';

// Use:
import { useGameStore } from '@/lib/stores/game-store';
```

---

## Tailwind CSS Configuration

### Tailwind v4 (CSS-based)

Tailwind v4 uses CSS-based configuration. Customize in `globals.css`:

```css
@import "tailwindcss";

/* Custom theme extensions */
@theme {
  --color-primary: #3b82f6;
  --color-secondary: #a855f7;
  --color-success: #22c55e;
  --color-warning: #eab308;
  --color-error: #ef4444;
  
  /* Game-specific colors */
  --color-cell-empty: #1e293b;
  --color-cell-hover: #334155;
  --color-board-border: #475569;
}
```

### PostCSS Configuration

```javascript
// postcss.config.mjs
export default {
  plugins: {
    '@tailwindcss/postcss': {},
  },
};
```

---

## ESLint Configuration

### eslint.config.mjs

```javascript
import { FlatCompat } from "@eslint/eslintrc";

const compat = new FlatCompat({
  baseDirectory: import.meta.dirname,
});

const eslintConfig = [
  ...compat.extends("next/core-web-vitals", "next/typescript"),
  {
    rules: {
      // Custom rules
      '@typescript-eslint/no-unused-vars': 'warn',
      'react-hooks/exhaustive-deps': 'warn',
    }
  }
];

export default eslintConfig;
```

---

## API Client Configuration

### Base URL

```typescript
// src/lib/api/index.ts
export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5050';
```

### Environment-Specific URLs

| Environment | URL |
|-------------|-----|
| Development | `http://localhost:5050` |
| Staging | `https://api.staging.perfectfit.com` |
| Production | `https://api.perfectfit.com` |

---

## Build Configuration

### Development

```bash
npm run dev
```

- Hot Module Replacement
- Fast Refresh
- Source maps
- Verbose logging

### Production Build

```bash
npm run build
npm start
```

**Build Optimizations**:
- Code minification
- Tree shaking
- Image optimization
- Static page generation

### Build Output

```
.next/
├── cache/              # Build cache
├── server/             # Server bundles
├── static/             # Static assets
└── types/              # Generated types
```

---

## Performance Configuration

### Image Optimization

```typescript
// next.config.ts
const nextConfig: NextConfig = {
  images: {
    domains: ['example.com'],  // External image domains
    formats: ['image/avif', 'image/webp'],
    minimumCacheTTL: 60,
  }
};
```

### Font Optimization

Using Next.js built-in font optimization:

```typescript
// src/app/layout.tsx
import { Geist, Geist_Mono } from "next/font/google";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});
```

---

## Debugging

### Enable Debug Mode

```bash
# .env.local
NEXT_PUBLIC_ENABLE_DEBUG=true
```

### Debug Store State

```typescript
// In browser console
import { useGameStore, useAuthStore } from '@/lib/stores';

// View current state
console.log(useGameStore.getState());
console.log(useAuthStore.getState());
```

### React DevTools

Install the [React Developer Tools](https://react.dev/learn/react-developer-tools) browser extension for component inspection.

### Network Debugging

- Use browser DevTools Network tab
- Filter by `/api/` to see API requests
- Check for CORS errors in Console

---

## Deployment Configuration

### Docker (Recommended)

The frontend includes a production-ready Dockerfile with multi-stage build:

```bash
# Build with API URL
docker build --build-arg NEXT_PUBLIC_API_URL=https://api.perfectfit.com \
  -t perfectfit-web:latest .

# Run container
docker run -p 3000:3000 perfectfit-web:latest
```

See [Deployment Guide](../deployment.md) for full deployment instructions.

### Vercel

```json
// vercel.json
{
  "framework": "nextjs",
  "buildCommand": "npm run build",
  "devCommand": "npm run dev",
  "installCommand": "npm install",
  "env": {
    "NEXT_PUBLIC_API_URL": "@api-url"
  }
}
```

### Cloudflare Pages

Use the provided deployment script:

```bash
./deploy/cloudflare/deploy-cloudflare-pages.sh production
```

Or deploy manually:

```bash
npm run build
wrangler pages deploy out --project-name perfectfit-web
```

### Azure Container Apps

Use the provided deployment script:

```bash
./deploy/azure/deploy-container-apps.sh production
```

### Static Export

For static hosting (no server-side features):

```typescript
// next.config.ts
const nextConfig: NextConfig = {
  output: 'export',
  images: { unoptimized: true }
};
```
