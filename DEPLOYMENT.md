# VibeLink - Deployment Guide

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                         INTERNET                             │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     NGINX (Port 443/80)                      │
│                   - SSL Termination                          │
│                   - Rate Limiting                            │
│                   - Reverse Proxy                            │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   BACKEND API (Port 8080)                    │
│                   - .NET 8 / ASP.NET Core                    │
│                   - JWT Authentication                       │
│                   - Health Checks                            │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   POSTGRESQL (Port 5432)                     │
│                   - User data                                │
│                   - Matches & Chats                          │
└─────────────────────────────────────────────────────────────┘
```

## Quick Start

### 1. Server Requirements

- Ubuntu 22.04+ / Debian 12+ (or any Linux with Docker)
- 2 GB RAM minimum
- 20 GB storage
- Docker & Docker Compose installed

### 2. Initial Setup

```bash
# Clone repository
git clone https://github.com/yourusername/vibelink.git
cd vibelink

# Copy environment template
cp .env.example .env

# Edit .env with your values
nano .env
```

### 3. Configure Environment Variables

Edit `.env` with these required values:

```bash
# Database password (generate a strong one)
DB_PASSWORD=your_secure_password_here

# JWT Secret (minimum 64 characters)
# Generate with: openssl rand -base64 64
JWT_SECRET_KEY=your_very_long_secret_key...

# Stripe keys (from dashboard.stripe.com)
STRIPE_SECRET_KEY=sk_live_...
STRIPE_WEBHOOK_SECRET=whsec_...

# Your domain
CORS_ORIGINS=https://vibelink.app
```

### 4. Deploy

```bash
# Make scripts executable
chmod +x scripts/*.sh

# Deploy
./scripts/deploy.sh
```

### 5. Setup SSL (Let's Encrypt)

```bash
# Make sure your domain points to the server
./scripts/setup-ssl.sh vibelink.app
```

## Maintenance

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f postgres
```

### Database Backup

```bash
# Manual backup
./scripts/backup-db.sh

# Setup daily backup (cron)
crontab -e
# Add: 0 3 * * * /path/to/vibelink/scripts/backup-db.sh
```

### Restore Database

```bash
# Stop backend
docker-compose stop backend

# Restore from backup
gunzip < backups/vibelink_backup_XXXXXX.sql.gz | \
  docker-compose exec -T postgres psql -U vibelink -d vibelink

# Restart
docker-compose up -d backend
```

### Update Deployment

```bash
# Pull latest changes and redeploy
git pull origin main
./scripts/deploy.sh
```

## Mobile App Builds

Build mobile apps on macOS:

```bash
# Build all platforms
./scripts/build-mobile.sh all

# Build Android only
./scripts/build-mobile.sh android

# Build iOS only
./scripts/build-mobile.sh ios
```

### Android Release

1. Build APK: `./scripts/build-mobile.sh android`
2. APK location: `builds/VibeLink.apk`
3. AAB for Play Store: `builds/VibeLink.aab`

### iOS Release

1. Build: `./scripts/build-mobile.sh ios`
2. Open in Xcode for archiving
3. Upload to App Store Connect

## Stripe Webhook Setup

1. Go to [Stripe Dashboard](https://dashboard.stripe.com/webhooks)
2. Add endpoint: `https://vibelink.app/api/payment/webhook`
3. Select events:
   - `checkout.session.completed`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
4. Copy webhook secret to `.env`

## Monitoring

### Health Checks

```bash
# Full health check
curl https://vibelink.app/health

# Readiness (includes DB)
curl https://vibelink.app/health/ready

# Liveness
curl https://vibelink.app/health/live
```

### API Status

```bash
# Check API is responding
curl https://vibelink.app/api/health
```

## Security Checklist

- [ ] Strong DB_PASSWORD (20+ characters)
- [ ] JWT_SECRET_KEY (64+ characters)
- [ ] SSL certificate installed
- [ ] Firewall configured (only 80, 443 open)
- [ ] Stripe webhook secret configured
- [ ] Rate limiting enabled (nginx)
- [ ] Regular backups scheduled
- [ ] Log monitoring setup

## Troubleshooting

### Backend won't start

```bash
# Check logs
docker-compose logs backend

# Common issues:
# - Database not ready: wait and retry
# - Missing env vars: check .env file
# - Port conflict: check if 8080 is in use
```

### Database connection issues

```bash
# Test connection
docker-compose exec postgres psql -U vibelink -d vibelink -c "SELECT 1"

# Check postgres logs
docker-compose logs postgres
```

### SSL certificate issues

```bash
# Renew manually
sudo certbot renew

# Copy new certs
sudo cp /etc/letsencrypt/live/vibelink.app/*.pem nginx/ssl/
docker-compose restart nginx
```

## Scaling (Future)

For higher traffic:

1. Add Redis for caching (uncomment in docker-compose.yml)
2. Use PostgreSQL read replicas
3. Deploy multiple backend instances with load balancer
4. Use CDN for static assets
