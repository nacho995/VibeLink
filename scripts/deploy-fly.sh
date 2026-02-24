#!/bin/bash
# VibeLink - Deploy to Fly.io
# First time: ./scripts/deploy-fly.sh setup
# Updates: ./scripts/deploy-fly.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
BACKEND_DIR="$PROJECT_DIR/backend"

echo "=========================================="
echo "  VibeLink - Fly.io Deployment"
echo "=========================================="

# Check if flyctl is installed
if ! command -v flyctl &> /dev/null; then
    echo "Installing Fly CLI..."
    curl -L https://fly.io/install.sh | sh
    export PATH="$HOME/.fly/bin:$PATH"
fi

cd "$BACKEND_DIR"

# First time setup
if [ "$1" == "setup" ]; then
    echo ""
    echo "[1/5] Logging in to Fly.io..."
    flyctl auth login
    
    echo ""
    echo "[2/5] Creating app..."
    flyctl launch --no-deploy --name vibelink-api --region mad
    
    echo ""
    echo "[3/5] Creating PostgreSQL database..."
    flyctl postgres create --name vibelink-db --region mad --vm-size shared-cpu-1x --initial-cluster-size 1 --volume-size 1
    
    echo ""
    echo "[4/5] Attaching database to app..."
    flyctl postgres attach vibelink-db --app vibelink-api
    
    echo ""
    echo "[5/5] Setting secrets..."
    echo "Enter your JWT secret key (min 64 chars):"
    read -s JWT_KEY
    echo "Enter your Stripe secret key:"
    read -s STRIPE_KEY
    echo "Enter your Stripe webhook secret:"
    read -s STRIPE_WEBHOOK
    
    flyctl secrets set \
        JWT_KEY="$JWT_KEY" \
        STRIPE_SECRET_KEY="$STRIPE_KEY" \
        STRIPE_WEBHOOK_SECRET="$STRIPE_WEBHOOK" \
        --app vibelink-api
    
    echo ""
    echo "Setup complete! Now deploying..."
fi

# Deploy
echo ""
echo "Deploying to Fly.io..."
flyctl deploy --app vibelink-api

echo ""
echo "=========================================="
echo "  Deployment Complete!"
echo "=========================================="
echo ""
echo "Your API is live at:"
flyctl status --app vibelink-api | grep "Hostname"
echo ""
echo "View logs: flyctl logs --app vibelink-api"
echo "Open app:  flyctl open --app vibelink-api"
