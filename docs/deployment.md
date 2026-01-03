# Deployment Guide

## Overview

PerfectFit consists of three deployable components:
1. **Frontend** - Next.js 16 application
2. **Backend** - ASP.NET Core 10 API
3. **Database** - PostgreSQL 16

---

## Quick Start

### Local Development with Docker Compose

Run the entire stack locally:

```bash
# From project root
docker compose up -d

# Services:
# - Frontend: http://localhost:3000
# - Backend: http://localhost:8080
# - PostgreSQL: localhost:5432
```

Stop services:
```bash
docker compose down
```

---

## Deployment Options

### Option 1: Azure Container Apps (Recommended for Production)

Full containerized deployment with automatic scaling, managed TLS, and integrated monitoring.

#### Prerequisites
- Azure CLI installed and logged in (\`az login\`)
- Docker installed
- Azure subscription

#### Automated Deployment

```bash
# Set configuration
export RESOURCE_GROUP=perfectfit-rg
export LOCATION=eastus
export ACR_NAME=perfectfitacr

# Run deployment script
./deploy/azure/deploy-container-apps.sh production
```

#### Manual Deployment Steps

1. **Create Azure Resources**
   ```bash
   # Create resource group
   az group create --name perfectfit-rg --location eastus
   
   # Create Container Registry
   az acr create --resource-group perfectfit-rg --name perfectfitacr --sku Basic --admin-enabled true
   
   # Create Container Apps Environment
   az containerapp env create --name perfectfit-env --resource-group perfectfit-rg --location eastus
   ```

2. **Build and Push Imdocker
   ```bash
   # Login to ACR
   az acr login --name perfectfitacr
   
   # Build and push backend
   cd backend
   docker build -t perfectfitacr.azurecr.io/perfectfit-api:latest .
   docker push perfectfitacr.azurecr.io/perfectfit-api:latest
   
   # Build and push frontend
   cd ../frontend
   docker build --build-arg NEXT_PUBLIC_API_URL=https://perfectfit-api.eastus.azurecontainerapps.io \
     -t perfectfitacr.azurecr.io/perfectfit-web:latest .
   docker push perfectfitacr.azurecr.io/perfectfit-web:latest
   ```

3. **Deploy Container Apps**
   ```bash
   # Get ACR credentials
   ACR_PASSWORD=$(az acr credential show --name perfectfitacr --query "passwords[0].value" -o tsv)
   
   # Deploy backend
   az acr create --resource-g    --name perfectfit-api \
     --resource-group perfectfit-rg \
     --environment perfectfit-env \
     --image perfectfitacr.azurecr.io/perfectfit-api:latest \
     --registry-server perfectfitacr.azurecr.io \
     --registry-username perfectfitacr \
     --registry-password "$ACR_PASSWORD" \
     --target-port 8080 \
     --ingress external \
     --min-replicas 1 \
     --max-replicas 10
   
   # Deploy frontend
   az containerapp create \
     --name perfectfit-web \
     --resource-group perfectfit-rg \
     --environment perfectfit-env \
     --image perfectfitacr.azurecr.io/perfectfit-web:latest \
     --registry-server perfectfitacr.azurecr.io \
     --registry-username perfectfitacr \
     --registry-password "$ACR_PASSWORD" \
     --target-port 3000 \
     --ingress external \
     --min-replicas 1 \
     --max-replicas 10
   ```

#### Infrastructure as Code (Bicep)

Deploy using Azure Bicep templates:

```bash
az deployment group create \
  --resource-group perfectfit-rg \
  --template-file deploy/azure/bicep/main.bicep \
  --parameters environment=production \
               backendImage=perfectfitacr.azurecr.io/perfectfit-api:latest \
               frontendImage=perfectfitacr.azurecr.io/perfectfit-web:latest \
               dbConnectionString="Host=...;Database=perfectfit;..."
```

---

### Option 2: Cloudflare Pages (Frontend Only)

Deploy the frontend to Cloudflare's edge network for optimal global performance. Requires separate backend hosting.

#### Prerequisites
- Cloudflare account
- Wrangler CLI installed (\`npm install -g wrangler\`)
- API token with Pages permissions

#### Automated Deployment

```bash
# Set credentials
export CLOUDFLARE_API_TOKEN=your_token
export CLOUDFLARE_ACCOUNT_ID=your_account_id
export NEXT_PUBLIC_API_URL=https://your-backend-api.com

# Run deployment
./deploy/cloudflare/deploy-cloudflare-pages.sh production
```

#### Manual Deployment

```bash
cd frontend

# Install dependencies and build
npm ci
NEXT_PUBLIC_API_URL=https://your-backend-api.com npm run build

# Deploy to Cloudflare Pages
wrangler pages deploy out --project-name perfectfit-web
```

#### Cloudflare Pages Configuration

The project includes a \`wrangler.toml\` configuration at \`deploy/cloudflare/wrangler.toml\`:

```toml
name = "perfectfit-web"
compatibility_date = "2024-01-01"
pages_build_output_dir = ".next"

[vars]
NODE_VERSION = "22"
```

---

### Option 3: Docker Compose (Self-Hosted)

Complete deployment with all services on a single server or VM.

#### Production Docker Compose

```yaml
# docker-compose.production.yml
version: '3.8'

services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: perfectfit
      POSTGRES_USER: perfectfit
      POSTGRES_PASSWORD: \${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U perfectfit -d perfectfit"]
      interval: 10s
      timeout: 5s
      retries: 5
    restart: unless-stopped

  backend:
    build:
      context: ./backend
      dockerfile: Dockerfile
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=perfectfit;Username=perfectfit;Password=\${DB_PASSWORD}
      - Jwt__Secret=\${JWT_SECRET}
      - CORS__AllowedOrigins=https://your-domain.com
    depends_on:
      postgres:
        condition: service_healthy
    restart: unless-stopped

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
      args:
        - NEXT_PUBLIC_API_URL=https://api.your-domain.com
    ports:
      - "3000:3000"
    depends_on:
      - backend
    restart: unless-stopped

volumes:
  postgres_data:
```

**Deploy:**
```bash
export DB_PASSWORD=your-secure-password
export JWT_SECRET=your-jwt-secret-at-least-32-characters

docker compose -f docker-compose.production.yml up -d
```

---

## Dockerfiles

### Backend Dockerfile

The backend uses a multi-stage build optimized for .NET 10:

```dockerfile
# backend/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY PerfectFit.sln ./
COPY src/PerfectFit.Core/PerfectFit.Core.csproj ./src/PerfectFit.Core/
COPY src/PerfectFit.UseCases/PerfectFit.UseCases.csproj ./src/PerfectFit.UseCases/
COPY src/PerfectFit.Infrastructure/PerfectFit.Infrastructure.csproj ./src/PerfectFit.Infrastructure/
COPY src/PerfectFit.Web/PerfectFit.Web.csproj ./src/PerfectFit.Web/

RUN dotnet restore

COPY src/ ./src/
RUN dotnet publish src/PerfectFit.Web/PerfectFit.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN adduser --disabled-password --gecos "" --uid 1000 appuser
COPY --from=publish /app/publish .
RUN chown -R appuser:appuser /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s 
```dockerfile
# backend/Dockerfile
FROM mcr.microsoft.com --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "PerfectFit.Web.dll"]
```

### Frontend Dockerfile

The frontend uses a multi-stage build with standalone output for optimal size:

```dockerfile
# frontend/Dockerfile
FROM node:22-alpine AS builder
WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm ci

COPY . .

ARG NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_API_URL=\${NEXT_PUBLIC_API_URL}
ENV BUILD_STANDALONE=true

RUN npm run build

FROM node:22-alpine AS runner
WORKDIR /app

RUN addgroup --system --gid 1001 nodejs && \
    adduser --system --uid 1001 nextjs

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1

COPY --from=builder /app/public ./public
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static

RUN chown -R nextjs:nodejs /app
USER nextjs

EXPOSE 3000
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:3000/ || exit 1

CMD ["node", "server.js"]
```

---

## CI/CD with GitHub Actions

### Azure Container Apps Workflow

The project includes a GitHub Actions workflow at \`.github/workflows/deploy-azure.yml\`:

**Triggers:**
- Push to \`main\` or \`release/*\` branches
- Manual workflow dispatch

**Workflow Steps:**
1. Build and push Docker images to ACR
2. Deploy backend Container App
3. Deploy frontend Container App
4. Configure CORS settings

**Required Secrets:**
| Secret | Description |
|--------|-------------|
| \`AZURE_CREDENTIALS\` | Azure service principal credentials (JSON) |
| \`API_URL\` | Backend API URL for frontend build |

**Create Azure Service Principal:**
```bash
az ad sp create-for-rbac --name "perfectfit-github-actions" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/perfectfit-rg \
  --sdk-auth
```

### Cloudflare Pages Workflow

The project includes a workflow at \`.github/workflows/deploy-cloudflare.yml\`:

**Triggers:**
- Push to \`main\` (when frontend files change)
- Manual workflow dispatch

**Required Secrets:**
| Secret | Description |
|--------|-------------|
| \`CLOUDFLARE_API_TOKEN\` | Cloudflare API token |
| \`CLOUDFLARE_ACCOUNT_ID\` | Cloudflare account ID |
| \`NEXT_PUBLIC_API_URL\` | Backend API URL |

---

## Environment Configuration

### Backend Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| \`ASPNETCORE_ENVIRONMENT\` | Yes | \`Production\`, \`Staging\`, or \`Development\` |
| \`ConnectionStrings__DefaultConnection\` | Yes | PostgreSQL connection string |
| \`Jwt__Secret\` | Yes | JWT signing key (32+ characters) |
| \`Jwt__Issuer\` | No | JWT issuer (default: PerfectFit) |
| \`Jwt__Audience\` | No | JWT audience (default: PerfectFit) |
| \`CORS__AllowedOrigins\` | Yes | Frontend URL(s), comma-separated |
| `OAuth__Microsoft__ClientId` | No | Microsoft OAuth client ID |
| `OAuth__Microsoft__ClientSecret` | No | Microsoft OAuth client secret |
| `Email__ConnectionString` | No | Azure Communication Services connection string |
| `Email__SenderAddress` | No | Sender email address (e.g., DoNotReply@xxx.azurecomm.net) |
| `Email__FrontendUrl` | No | Frontend URL for verification links (default: http://localhost:3000) |
| `Auth__LockoutThreshold` | No | Failed login attempts before lockout (default: 5) |
| `Auth__LockoutDurationMinutes` | No | Lockout duration in minutes (default: 15) |

### Frontend Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| \`NEXT_PUBLIC_API_URL\` | Yes | Backend API URL |
| \`BUILD_STANDALONE\` | No | Set to \`true\` for Docker builds |

---

## SSL/TLS Configuration

### Azure Container Apps
TLS is automatically configured with managed certificates.

### Cloudflare Pages
TLS is automatically configured at the edge.

### Self-Hosted with Nginx

```nginx
server {
    listen 80;
    server_name perfectfit.com www.perfectfit.com;
    return 301 https://\$server_name\$request_uri;
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
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
    }

    # Backend API
    location /api {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
```

---

## Database Management

### Migrations

```bash
# Generate migration script
cd backend
dotnet ef migrations script -i -o migrate.sql --project src/PerfectFit.Infrastructure

# Apply using psql
psql -h your-host -U perfectfit -d perfectfit -f migrate.sql
```

### Backup & Recovery

```bash
# Backup
pg_dump -h localhost -U perfectfit perfectfit > backup_\$(date +%Y%m%d).sql

# Restore
psql -h localhost -U perfectfit perfectfit < backup_20260102.sql
```

### Automated Backups (cron)

```bash
# /etc/cron.d/perfectfit-backup
0 2 * * * pg_dump -h localhost -U perfectfit perfectfit | gzip > /backups/perfectfit_\$(date +\%Y\%m\%d).sql.gz
```

---

## Health Checks

### Endpoints

| Service | Endpoint | Port |
|---------|----------|------|
| Backend | \`/health\` | 8080 |
| Frontend | \`/\` | 3000 |
| PostgreSQL | \`pg_isready\` | 5432 |

### Docker Health Check

Health checks are built into both Dockerfiles and trigger automatically.

---

## Scaling

### Azure Container Apps
Configure in Azure Portal or via CLI:
```bash
az containerapp upd
```bash
# Gerfectfit-api \
  --resource-group perfectfit-rg \
  --min-replicas 2 \
  --max-replicas 20
```

### Docker Compose
```yaml
services:
  backend:
    deploy:
      replicas: 3
```

### Database Connection Pooling

```
Host=...;Pooling=true;MinPoolSize=5;MaxPoolSize=100
```

---

## Monitoring & Logging

### Azure Container Apps
- Built-in metrics in Azure Portal
- Application Insights integration available

### Application Insights

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### Structured Logging (Serilog)

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "/var/log/perfectfit/log-.txt", "rollingInterval": "Day" } }
    ]
  }
}
```

---

## Security Checklist

- [ ] Use HTTPS everywhere
- [ ] Set strong JWT secret (32+ characters)
- [ ] Configure CORS properly (specific origins, not \`*\`)
- [ ] Enable rate limiting
- [ ] Use database connection encryption (SSL)
- # Gerfore secrets in secure vault (Azure Key Vault, etc.)
- [ ] Enable security headers (configured in Next.js)
- [ ] Regular dependency updates
- [ ] Database backup strategy
- [ ] Access logging enabled
- [ ] Run containers as non-root user (configured in Dockerfiles)

---

## Troubleshooting

### Container Won't Start

```bash
# Check logs
docker logs perfectfit-backend
docker logs perfectfit-frontend

# Azure Container Apps
az containerapp logs show --name perfectfit-api --resource-group perfectfit-rg
```

### Database Connection Failed

```bash
# Verify PostgreSQL is running
docker compose ps
pg_isready -h localhost -U perfectfit

# Check connection string format
Host=<host>;Port=5432;Database=perfectfit;Username=perfectfit;Password=<password>
```

### CORS Errors

Ensure \`CORS__AllowedOrigins\` includes the exact frontend URL (with protocol).

### Health Check Failing

```bash
# Test health endpoint manually
curl http://localhost:8080/health
curl http://localhost:3000/
```
