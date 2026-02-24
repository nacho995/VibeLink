#!/bin/bash
# VibeLink Deployment Script
# Usage: ./scripts/deploy.sh [environment]

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
ENVIRONMENT="${1:-production}"

echo "=========================================="
echo "  VibeLink Deployment - $ENVIRONMENT"
echo "=========================================="

cd "$PROJECT_DIR"

# Check for .env file
if [ ! -f ".env" ]; then
    echo "ERROR: .env file not found!"
    echo "Copy .env.example to .env and fill in the values."
    exit 1
fi

# Load environment variables
source .env

# Validate required variables
required_vars=("DB_PASSWORD" "JWT_SECRET_KEY" "STRIPE_SECRET_KEY")
for var in "${required_vars[@]}"; do
    if [ -z "${!var}" ]; then
        echo "ERROR: $var is not set in .env"
        exit 1
    fi
done

echo ""
echo "[1/5] Pulling latest changes..."
git pull origin main 2>/dev/null || echo "Not a git repo or no remote, skipping pull"

echo ""
echo "[2/5] Building Docker images..."
docker-compose build --no-cache

echo ""
echo "[3/5] Stopping existing containers..."
docker-compose down

echo ""
echo "[4/5] Starting services..."
docker-compose up -d

echo ""
echo "[5/5] Waiting for services to be healthy..."
sleep 10

# Check health
echo ""
echo "Checking service health..."

if curl -sf http://localhost:8080/health > /dev/null 2>&1; then
    echo "✓ Backend API is healthy"
else
    echo "✗ Backend API health check failed"
    echo "  Checking logs..."
    docker-compose logs --tail=50 backend
    exit 1
fi

if docker-compose exec -T postgres pg_isready -U vibelink > /dev/null 2>&1; then
    echo "✓ PostgreSQL is healthy"
else
    echo "✗ PostgreSQL health check failed"
    exit 1
fi

echo ""
echo "=========================================="
echo "  Deployment Complete!"
echo "=========================================="
echo ""
echo "Services running:"
docker-compose ps
echo ""
echo "API URL: http://localhost:8080"
echo "Health:  http://localhost:8080/health"
