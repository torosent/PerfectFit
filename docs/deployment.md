# Deployment Guide

## Overview

PerfectFit consists of three deployable components:
1. **Frontend** - Next.js application
2. **Backend** - ASP.NET Core API
3. **Database** - PostgreSQL

---

## Deployment Options

### Option 1: Docker Compose (Recommended for Self-Hosting)

Complete deployment with all services:

```yaml
# docker-compose.production.yml
version: '3.8'

services:
  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    ports:
      - "3000:3000"
    environment:
      - NEXT_PUBLIC_API_URL=http://api:5050
    depends_on:
      - backend

  backend:
    build:
      context: ./backend
      dockerfile: Dockerfile
    ports:
      - "5050:5050"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=perfectfit;Username=perfectfit;Password=${DB_PASSWORD}
      - Jwt__Secret=${JWT_SECRET}
      - Cors__AllowedOrigins__0=http://localhost:3000
    depends_on:
      postgres:
        condition: service_healthy

  postgres:
    image: postgres:16
    environment:
      - POSTGRES_DB=perfectfit
      - POSTGRES_USER=perfectfit
      - POSTGRES_PASSWORD=${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U perfectfit -d perfectfit"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres_data:
```

**Deploy:**
```bash
# Set environment variables
export DB_PASSWORD=your-secure-password
export JWT_SECRET=your-jwt-secret-at-least-32-characters

# Deploy
docker compose -f docker-compose.production.yml up -d
```

### Option 2: Cloud Platform Deployment

#### Frontend on Vercel

1. Connect GitHub repository to Vercel
2. Configure build settings:
   - Framework: Next.js
   - Build Command: `npm run build`
   - Output Directory: `.next`
3. Add environment variables:
   ```
   NEXT_PUBLIC_API_URL=https://api.perfectfit.com
   ```
4. Deploy

#### Backend on Azure App Service

1. Create Azure App Service (Linux, .NET 9)
2. Configure deployment:
   ```bash
   az webapp up --name perfectfit-api --resource-group perfectfit-rg
   ```
3. Configure settings:
   ```bash
   az webapp config appsettings set --name perfectfit-api \
     --settings \
     ConnectionStrings__DefaultConnection="your-connection-string" \
     Jwt__Secret="your-jwt-secret"
   ```

#### Database on Azure PostgreSQL

1. Create Azure Database for PostgreSQL
2. Configure firewall rules
3. Update connection string in App Service

---

## Dockerfiles

### Backend Dockerfile

```dockerfile
# backend/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY src/PerfectFit.Core/*.csproj ./PerfectFit.Core/
COPY src/PerfectFit.UseCases/*.csproj ./PerfectFit.UseCases/
COPY src/PerfectFit.Infrastructure/*.csproj ./PerfectFit.Infrastructure/
COPY src/PerfectFit.Web/*.csproj ./PerfectFit.Web/
RUN dotnet restore PerfectFit.Web/PerfectFit.Web.csproj

# Copy source and build
COPY src/ .
RUN dotnet publish PerfectFit.Web/PerfectFit.Web.csproj -c Release -o /app

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:5050
EXPOSE 5050

ENTRYPOINT ["dotnet", "PerfectFit.Web.dll"]
```

### Frontend Dockerfile

```dockerfile
# frontend/Dockerfile
FROM node:18-alpine AS builder
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

# Production image
FROM node:18-alpine AS runner
WORKDIR /app

ENV NODE_ENV=production

# Copy necessary files
COPY --from=builder /app/package*.json ./
COPY --from=builder /app/.next ./.next
COPY --from=builder /app/public ./public
COPY --from=builder /app/node_modules ./node_modules

EXPOSE 3000

CMD ["npm", "start"]
```

---

## Environment Configuration

### Production Environment Variables

#### Backend

| Variable | Required | Description |
|----------|----------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Yes | Set to `Production` |
| `ConnectionStrings__DefaultConnection` | Yes | PostgreSQL connection string |
| `Jwt__Secret` | Yes | JWT signing key (32+ chars) |
| `Jwt__Issuer` | No | JWT issuer (default: PerfectFit) |
| `Jwt__Audience` | No | JWT audience (default: PerfectFit) |
| `Cors__AllowedOrigins__0` | Yes | Frontend URL |
| `OAuth__Google__ClientId` | No | Google OAuth client ID |
| `OAuth__Google__ClientSecret` | No | Google OAuth client secret |
| `OAuth__Microsoft__ClientId` | No | Microsoft OAuth client ID |
| `OAuth__Microsoft__ClientSecret` | No | Microsoft OAuth client secret |

#### Frontend

| Variable | Required | Description |
|----------|----------|-------------|
| `NEXT_PUBLIC_API_URL` | Yes | Backend API URL |

---

## SSL/TLS Configuration

### Using Let's Encrypt with Nginx

```nginx
# /etc/nginx/sites-available/perfectfit
server {
    listen 80;
    server_name perfectfit.com www.perfectfit.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name perfectfit.com www.perfectfit.com;

    ssl_certificate /etc/letsencrypt/live/perfectfit.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/perfectfit.com/privkey.pem;

    # Frontend
    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    # Backend API
    location /api {
        proxy_pass http://localhost:5050;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## Database Migrations

### Apply Migrations in Production

```bash
# Generate migration script
dotnet ef migrations script -i -o migrate.sql

# Apply using psql
psql -h your-host -U perfectfit -d perfectfit -f migrate.sql
```

### Or apply via application (not recommended for production)

```csharp
// Only in controlled environments
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
```

---

## Health Checks

### Backend Health Endpoint

```
GET /health
```

Returns `200 OK` when healthy.

### Docker Health Check

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:5050/health"]
  interval: 30s
  timeout: 10s
  retries: 3
```

---

## Monitoring & Logging

### Application Insights (Azure)

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### Structured Logging

Configure Serilog for production:

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": { "path": "/var/log/perfectfit/log-.txt", "rollingInterval": "Day" }
      }
    ]
  }
}
```

---

## Scaling

### Horizontal Scaling

The backend is stateless and can be scaled horizontally:

```yaml
# docker-compose.scale.yml
services:
  backend:
    deploy:
      replicas: 3
```

### Load Balancing

Use Nginx or cloud load balancers to distribute traffic.

### Database Connection Pooling

Configure connection pooling in the connection string:

```
Host=...;Pooling=true;MinPoolSize=5;MaxPoolSize=100
```

---

## Backup & Recovery

### Database Backup

```bash
# Backup
pg_dump -h localhost -U perfectfit perfectfit > backup_$(date +%Y%m%d).sql

# Restore
psql -h localhost -U perfectfit perfectfit < backup_20260102.sql
```

### Automated Backups

Use cloud provider features or cron jobs:

```bash
# /etc/cron.d/perfectfit-backup
0 2 * * * pg_dump -h localhost -U perfectfit perfectfit | gzip > /backups/perfectfit_$(date +\%Y\%m\%d).sql.gz
```

---

## Security Checklist

- [ ] Use HTTPS everywhere
- [ ] Set strong JWT secret (32+ characters)
- [ ] Configure CORS properly
- [ ] Enable rate limiting
- [ ] Use database connection encryption
- [ ] Store secrets in secure vault
- [ ] Enable security headers
- [ ] Regular dependency updates
- [ ] Database backup strategy
- [ ] Access logging enabled
