#!/bin/bash
# VibeLink Database Backup Script
# Usage: ./scripts/backup-db.sh

set -e

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BACKUP_DIR="$PROJECT_DIR/backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="vibelink_backup_$TIMESTAMP.sql.gz"

echo "=========================================="
echo "  VibeLink Database Backup"
echo "=========================================="

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Load environment
source "$PROJECT_DIR/.env"

echo ""
echo "Creating backup..."

# Dump database
docker-compose exec -T postgres pg_dump \
    -U vibelink \
    -d vibelink \
    --clean \
    --if-exists \
    | gzip > "$BACKUP_DIR/$BACKUP_FILE"

# Get file size
SIZE=$(du -h "$BACKUP_DIR/$BACKUP_FILE" | cut -f1)

echo ""
echo "Backup created: $BACKUP_DIR/$BACKUP_FILE ($SIZE)"

# Keep only last 7 backups
echo ""
echo "Cleaning old backups (keeping last 7)..."
ls -t "$BACKUP_DIR"/vibelink_backup_*.sql.gz 2>/dev/null | tail -n +8 | xargs -r rm

# List current backups
echo ""
echo "Current backups:"
ls -lh "$BACKUP_DIR"/vibelink_backup_*.sql.gz 2>/dev/null || echo "  No backups found"

echo ""
echo "=========================================="
echo "  Backup Complete!"
echo "=========================================="
