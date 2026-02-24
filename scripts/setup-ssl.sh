#!/bin/bash
# VibeLink SSL Setup Script (Let's Encrypt)
# Usage: ./scripts/setup-ssl.sh your-domain.com

set -e

DOMAIN="${1:-vibelink.app}"
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "=========================================="
echo "  SSL Setup for $DOMAIN"
echo "=========================================="

# Create SSL directory
mkdir -p "$PROJECT_DIR/nginx/ssl"

# Check if running on a server with public IP
echo ""
echo "This script will obtain SSL certificates from Let's Encrypt."
echo "Make sure:"
echo "  1. Your domain ($DOMAIN) points to this server's IP"
echo "  2. Port 80 is open and accessible"
echo ""
read -p "Continue? (y/n) " -n 1 -r
echo ""

if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Aborted."
    exit 1
fi

# Install certbot if not present
if ! command -v certbot &> /dev/null; then
    echo "Installing certbot..."
    if command -v dnf &> /dev/null; then
        sudo dnf install -y certbot
    elif command -v apt &> /dev/null; then
        sudo apt install -y certbot
    else
        echo "Please install certbot manually"
        exit 1
    fi
fi

# Stop nginx temporarily if running
docker-compose stop nginx 2>/dev/null || true

# Obtain certificate
echo ""
echo "Obtaining SSL certificate..."
sudo certbot certonly --standalone \
    -d "$DOMAIN" \
    -d "www.$DOMAIN" \
    --agree-tos \
    --non-interactive \
    --email "admin@$DOMAIN"

# Copy certificates to nginx directory
echo ""
echo "Copying certificates..."
sudo cp "/etc/letsencrypt/live/$DOMAIN/fullchain.pem" "$PROJECT_DIR/nginx/ssl/"
sudo cp "/etc/letsencrypt/live/$DOMAIN/privkey.pem" "$PROJECT_DIR/nginx/ssl/"
sudo chown -R $USER:$USER "$PROJECT_DIR/nginx/ssl/"
chmod 600 "$PROJECT_DIR/nginx/ssl/privkey.pem"

# Restart nginx
echo ""
echo "Restarting nginx..."
docker-compose up -d nginx

echo ""
echo "=========================================="
echo "  SSL Setup Complete!"
echo "=========================================="
echo ""
echo "Your site is now available at:"
echo "  https://$DOMAIN"
echo ""
echo "Certificate auto-renewal is handled by certbot."
echo "To manually renew: sudo certbot renew"
