# Deployment Configuration

This directory contains deployment configurations and scripts for PerfectFit.

## Deployment Options

### Azure Container Apps

Full containerized deployment for both frontend and backend.

**Files:**
- `azure/deploy-container-apps.sh` - Shell script for manual deployment
- `azure/bicep/main.bicep` - Infrastructure as Code (IaC) template

**Usage:**
```bash
# Set required environment variables
export RESOURCE_GROUP=perfectfit-rg
export LOCATION=eastus
export ACR_NAME=perfectfitacr

# Run deployment
./azure/deploy-container-apps.sh production
```

**Required Secrets (for GitHub Actions):**
- `AZURE_CREDENTIALS` - Azure service principal credentials
- `API_URL` - Backend API URL for frontend build

### Cloudflare Pages

Static deployment for the frontend only (requires separate backend hosting).

**Files:**
- `cloudflare/deploy-cloudflare-pages.sh` - Shell script for manual deployment
- `cloudflare/wrangler.toml` - Cloudflare configuration

**Usage:**
```bash
# Set required environment variables
export CLOUDFLARE_API_TOKEN=your_token
export CLOUDFLARE_ACCOUNT_ID=your_account_id
export NEXT_PUBLIC_API_URL=https://your-backend-api.com

# Run deployment
./cloudflare/deploy-cloudflare-pages.sh production
```

**Required Secrets (for GitHub Actions):**
- `CLOUDFLARE_API_TOKEN` - Cloudflare API token
- `CLOUDFLARE_ACCOUNT_ID` - Cloudflare account ID
- `NEXT_PUBLIC_API_URL` - Backend API URL

## GitHub Actions Workflows

### Azure Deployment (`.github/workflows/deploy-azure.yml`)

Triggered on:
- Push to `main` or `release/*` branches
- Manual workflow dispatch

Deploys:
- Backend to Azure Container Apps
- Frontend to Azure Container Apps

### Cloudflare Deployment (`.github/workflows/deploy-cloudflare.yml`)

Triggered on:
- Push to `main` (only when frontend files change)
- Manual workflow dispatch

Deploys:
- Frontend to Cloudflare Pages

## Local Development with Docker

Run the full stack locally:

```bash
# From project root
docker-compose up -d

# Services:
# - Frontend: http://localhost:3000
# - Backend: http://localhost:8080
# - PostgreSQL: localhost:5432
```

## Environment Variables

### Backend
| Variable | Description | Required |
|----------|-------------|----------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | Yes |
| `ConnectionStrings__DefaultConnection` | Database connection string | Yes |
| `CORS__AllowedOrigins` | Allowed CORS origins | Yes |

### Frontend
| Variable | Description | Required |
|----------|-------------|----------|
| `NEXT_PUBLIC_API_URL` | Backend API URL | Yes |
