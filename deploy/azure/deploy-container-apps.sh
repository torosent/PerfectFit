#!/bin/bash
set -e

# =============================================================================
# Azure Container Apps Deployment Script for PerfectFit
# =============================================================================
# Usage: ./deploy-container-apps.sh [environment]
# Example: ./deploy-container-apps.sh production
# =============================================================================

# Configuration
ENVIRONMENT="${1:-production}"
RESOURCE_GROUP="${RESOURCE_GROUP:-perfectfit-rg}"
LOCATION="${LOCATION:-eastus}"
ACR_NAME="${ACR_NAME:-perfectfitacr}"
CONTAINER_APP_ENV="${CONTAINER_APP_ENV:-perfectfit-env}"
BACKEND_APP_NAME="${BACKEND_APP_NAME:-perfectfit-api}"
FRONTEND_APP_NAME="${FRONTEND_APP_NAME:-perfectfit-web}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Check required tools
check_prerequisites() {
    log_info "Checking prerequisites..."
    
    if ! command -v az &> /dev/null; then
        log_error "Azure CLI is not installed. Please install it first."
        exit 1
    fi
    
    if ! command -v docker &> /dev/null; then
        log_error "Docker is not installed. Please install it first."
        exit 1
    fi
    
    # Check Azure login
    if ! az account show &> /dev/null; then
        log_error "Not logged into Azure. Please run 'az login' first."
        exit 1
    fi
    
    log_info "Prerequisites check passed."
}

# Create Azure resources
create_azure_resources() {
    log_info "Creating Azure resources..."
    
    # Create resource group
    log_info "Creating resource group: $RESOURCE_GROUP"
    az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none
    
    # Create Azure Container Registry
    log_info "Creating Azure Container Registry: $ACR_NAME"
    az acr create \
        --resource-group "$RESOURCE_GROUP" \
        --name "$ACR_NAME" \
        --sku Basic \
        --admin-enabled true \
        --output none
    
    # Create Container Apps Environment
    log_info "Creating Container Apps Environment: $CONTAINER_APP_ENV"
    az containerapp env create \
        --name "$CONTAINER_APP_ENV" \
        --resource-group "$RESOURCE_GROUP" \
        --location "$LOCATION" \
        --output none
    
    log_info "Azure resources created successfully."
}

# Build and push Docker images
build_and_push_images() {
    log_info "Building and pushing Docker images..."
    
    # Get ACR login server
    ACR_LOGIN_SERVER=$(az acr show --name "$ACR_NAME" --query loginServer -o tsv)
    
    # Login to ACR
    log_info "Logging into ACR..."
    az acr login --name "$ACR_NAME"
    
    # Get current timestamp for image tag
    IMAGE_TAG=$(date +%Y%m%d%H%M%S)
    
    # Build and push backend image
    log_info "Building backend image..."
    cd "$(dirname "$0")/../../backend"
    docker build -t "$ACR_LOGIN_SERVER/$BACKEND_APP_NAME:$IMAGE_TAG" -t "$ACR_LOGIN_SERVER/$BACKEND_APP_NAME:latest" .
    
    log_info "Pushing backend image..."
    docker push "$ACR_LOGIN_SERVER/$BACKEND_APP_NAME:$IMAGE_TAG"
    docker push "$ACR_LOGIN_SERVER/$BACKEND_APP_NAME:latest"
    
    # Build and push frontend image
    log_info "Building frontend image..."
    cd "$(dirname "$0")/../../frontend"
    
    # Get backend URL for frontend build
    BACKEND_URL="${NEXT_PUBLIC_API_URL:-https://$BACKEND_APP_NAME.$LOCATION.azurecontainerapps.io}"
    
    docker build \
        --build-arg NEXT_PUBLIC_API_URL="$BACKEND_URL" \
        -t "$ACR_LOGIN_SERVER/$FRONTEND_APP_NAME:$IMAGE_TAG" \
        -t "$ACR_LOGIN_SERVER/$FRONTEND_APP_NAME:latest" \
        .
    
    log_info "Pushing frontend image..."
    docker push "$ACR_LOGIN_SERVER/$FRONTEND_APP_NAME:$IMAGE_TAG"
    docker push "$ACR_LOGIN_SERVER/$FRONTEND_APP_NAME:latest"
    
    log_info "Docker images built and pushed successfully."
    
    # Export image tag for deployment
    export IMAGE_TAG
}

# Deploy backend Container App
deploy_backend() {
    log_info "Deploying backend Container App..."
    
    ACR_LOGIN_SERVER=$(az acr show --name "$ACR_NAME" --query loginServer -o tsv)
    ACR_PASSWORD=$(az acr credential show --name "$ACR_NAME" --query "passwords[0].value" -o tsv)
    
    # Check if container app exists
    if az containerapp show --name "$BACKEND_APP_NAME" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
        log_info "Updating existing backend container app..."
        az containerapp update \
            --name "$BACKEND_APP_NAME" \
            --resource-group "$RESOURCE_GROUP" \
            --image "$ACR_LOGIN_SERVER/$BACKEND_APP_NAME:latest" \
            --output none
    else
        log_info "Creating new backend container app..."
        az containerapp create \
            --name "$BACKEND_APP_NAME" \
            --resource-group "$RESOURCE_GROUP" \
            --environment "$CONTAINER_APP_ENV" \
            --image "$ACR_LOGIN_SERVER/$BACKEND_APP_NAME:latest" \
            --registry-server "$ACR_LOGIN_SERVER" \
            --registry-username "$ACR_NAME" \
            --registry-password "$ACR_PASSWORD" \
            --target-port 8080 \
            --ingress external \
            --min-replicas 1 \
            --max-replicas 10 \
            --cpu 0.5 \
            --memory 1Gi \
            --env-vars \
                "ASPNETCORE_ENVIRONMENT=Production" \
                "ConnectionStrings__DefaultConnection=secretref:db-connection-string" \
            --output none
    fi
    
    # Get backend URL
    BACKEND_URL=$(az containerapp show --name "$BACKEND_APP_NAME" --resource-group "$RESOURCE_GROUP" --query "properties.configuration.ingress.fqdn" -o tsv)
    log_info "Backend deployed at: https://$BACKEND_URL"
    
    export BACKEND_URL
}

# Deploy frontend Container App
deploy_frontend() {
    log_info "Deploying frontend Container App..."
    
    ACR_LOGIN_SERVER=$(az acr show --name "$ACR_NAME" --query loginServer -o tsv)
    ACR_PASSWORD=$(az acr credential show --name "$ACR_NAME" --query "passwords[0].value" -o tsv)
    
    # Check if container app exists
    if az containerapp show --name "$FRONTEND_APP_NAME" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
        log_info "Updating existing frontend container app..."
        az containerapp update \
            --name "$FRONTEND_APP_NAME" \
            --resource-group "$RESOURCE_GROUP" \
            --image "$ACR_LOGIN_SERVER/$FRONTEND_APP_NAME:latest" \
            --output none
    else
        log_info "Creating new frontend container app..."
        az containerapp create \
            --name "$FRONTEND_APP_NAME" \
            --resource-group "$RESOURCE_GROUP" \
            --environment "$CONTAINER_APP_ENV" \
            --image "$ACR_LOGIN_SERVER/$FRONTEND_APP_NAME:latest" \
            --registry-server "$ACR_LOGIN_SERVER" \
            --registry-username "$ACR_NAME" \
            --registry-password "$ACR_PASSWORD" \
            --target-port 3000 \
            --ingress external \
            --min-replicas 1 \
            --max-replicas 10 \
            --cpu 0.5 \
            --memory 1Gi \
            --env-vars \
                "NEXT_PUBLIC_API_URL=https://$BACKEND_URL" \
            --output none
    fi
    
    FRONTEND_URL=$(az containerapp show --name "$FRONTEND_APP_NAME" --resource-group "$RESOURCE_GROUP" --query "properties.configuration.ingress.fqdn" -o tsv)
    log_info "Frontend deployed at: https://$FRONTEND_URL"
}

# Configure CORS on backend
configure_cors() {
    log_info "Configuring CORS..."
    
    FRONTEND_URL=$(az containerapp show --name "$FRONTEND_APP_NAME" --resource-group "$RESOURCE_GROUP" --query "properties.configuration.ingress.fqdn" -o tsv)
    
    az containerapp update \
        --name "$BACKEND_APP_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --set-env-vars "CORS__AllowedOrigins=https://$FRONTEND_URL" \
        --output none
    
    log_info "CORS configured."
}

# Main deployment flow
main() {
    log_info "Starting PerfectFit deployment to Azure Container Apps..."
    log_info "Environment: $ENVIRONMENT"
    
    check_prerequisites
    create_azure_resources
    build_and_push_images
    deploy_backend
    deploy_frontend
    configure_cors
    
    log_info "=========================================="
    log_info "Deployment completed successfully!"
    log_info "=========================================="
    FRONTEND_URL=$(az containerapp show --name "$FRONTEND_APP_NAME" --resource-group "$RESOURCE_GROUP" --query "properties.configuration.ingress.fqdn" -o tsv)
    BACKEND_URL=$(az containerapp show --name "$BACKEND_APP_NAME" --resource-group "$RESOURCE_GROUP" --query "properties.configuration.ingress.fqdn" -o tsv)
    log_info "Frontend URL: https://$FRONTEND_URL"
    log_info "Backend URL: https://$BACKEND_URL"
}

# Run main function
main "$@"
