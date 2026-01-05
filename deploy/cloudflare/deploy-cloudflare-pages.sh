#!/bin/bash
set -e

# =============================================================================
# Cloudflare Pages Deployment Script for PerfectFit Frontend
# =============================================================================
# Usage: ./deploy-cloudflare-pages.sh [environment]
# Example: ./deploy-cloudflare-pages.sh production
# =============================================================================

# Configuration
ENVIRONMENT="${1:-production}"
PROJECT_NAME="${CLOUDFLARE_PROJECT_NAME:-perfectfit-web}"
BRANCH="${BRANCH:-main}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Check required environment variables
check_environment() {
    log_info "Checking environment variables..."
    
    if [ -z "$CLOUDFLARE_API_TOKEN" ]; then
        log_error "CLOUDFLARE_API_TOKEN is not set"
        exit 1
    fi
    
    if [ -z "$CLOUDFLARE_ACCOUNT_ID" ]; then
        log_error "CLOUDFLARE_ACCOUNT_ID is not set"
        exit 1
    fi
    
    if [ -z "$NEXT_PUBLIC_API_URL" ]; then
        log_warn "NEXT_PUBLIC_API_URL is not set. Using default."
    fi
    
    log_info "Environment check passed."
}

# Check prerequisites
check_prerequisites() {
    log_info "Checking prerequisites..."
    
    if ! command -v node &> /dev/null; then
        log_error "Node.js is not installed. Please install it first."
        exit 1
    fi
    
    if ! command -v npm &> /dev/null; then
        log_error "npm is not installed. Please install it first."
        exit 1
    fi
    
    # Install wrangler if not available
    if ! command -v wrangler &> /dev/null; then
        log_info "Installing Wrangler CLI..."
        npm install -g wrangler
    fi
    
    log_info "Prerequisites check passed."
}

# Build the frontend
build_frontend() {
    log_info "Building frontend for Cloudflare Pages..."
    
    cd "$(dirname "$0")/../../frontend"
    
    # Install dependencies
    log_info "Installing dependencies..."
    npm ci
    
    # Build with environment variables
    log_info "Building Next.js application..."
    NEXT_PUBLIC_API_URL="${NEXT_PUBLIC_API_URL:-}" npm run build
    
    log_info "Frontend built successfully."
}

# Deploy to Cloudflare Pages
deploy_to_cloudflare() {
    log_info "Deploying to Cloudflare Pages..."
    
    cd "$(dirname "$0")/../../frontend"
    
    # Deploy using wrangler
    wrangler pages deploy .next/static \
        --project-name "$PROJECT_NAME" \
        --branch "$BRANCH" \
        --commit-dirty=true
    
    log_info "Deployment to Cloudflare Pages completed."
}

# Create Cloudflare Pages project if it doesn't exist
create_project_if_needed() {
    log_info "Checking if Cloudflare Pages project exists..."
    
    # Check if project exists
    if wrangler pages project list | grep -q "$PROJECT_NAME"; then
        log_info "Project $PROJECT_NAME already exists."
    else
        log_info "Creating Cloudflare Pages project: $PROJECT_NAME"
        wrangler pages project create "$PROJECT_NAME" \
            --production-branch "$BRANCH"
    fi
}

# Main deployment flow
main() {
    log_info "Starting PerfectFit frontend deployment to Cloudflare Pages..."
    log_info "Environment: $ENVIRONMENT"
    log_info "Project: $PROJECT_NAME"
    
    check_environment
    check_prerequisites
    create_project_if_needed
    build_frontend
    deploy_to_cloudflare
    
    log_info "=========================================="
    log_info "Deployment completed successfully!"
    log_info "=========================================="
    log_info "Your site will be available at:"
    log_info "  https://$PROJECT_NAME.pages.dev"
    log_info ""
    log_info "For custom domains, configure them in the Cloudflare dashboard."
}

# Run main function
main "$@"
