#!/bin/bash
# VibeLink Mobile Build Script
# Run this on macOS to build the mobile apps
# Usage: ./scripts/build-mobile.sh [android|ios|all]

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
FRONTEND_DIR="$PROJECT_DIR/frontend/VibeLink.App"
BUILD_DIR="$PROJECT_DIR/builds"
TARGET="${1:-all}"

echo "=========================================="
echo "  VibeLink Mobile Build - $TARGET"
echo "=========================================="

cd "$FRONTEND_DIR"

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "Using .NET SDK: $DOTNET_VERSION"

# Create builds directory
mkdir -p "$BUILD_DIR"

# Restore packages
echo ""
echo "[1/4] Restoring packages..."
dotnet restore

build_android() {
    echo ""
    echo "[Building Android APK...]"
    
    # Build release APK
    dotnet publish -f net10.0-android \
        -c Release \
        -p:AndroidPackageFormat=apk \
        -o "$BUILD_DIR/android"
    
    # Find and copy APK
    APK_FILE=$(find "$BUILD_DIR/android" -name "*.apk" | head -1)
    if [ -n "$APK_FILE" ]; then
        cp "$APK_FILE" "$BUILD_DIR/VibeLink.apk"
        echo "✓ Android APK: $BUILD_DIR/VibeLink.apk"
    fi
    
    # Also build AAB for Play Store
    echo ""
    echo "[Building Android AAB for Play Store...]"
    dotnet publish -f net10.0-android \
        -c Release \
        -p:AndroidPackageFormat=aab \
        -o "$BUILD_DIR/android-aab"
    
    AAB_FILE=$(find "$BUILD_DIR/android-aab" -name "*.aab" | head -1)
    if [ -n "$AAB_FILE" ]; then
        cp "$AAB_FILE" "$BUILD_DIR/VibeLink.aab"
        echo "✓ Android AAB: $BUILD_DIR/VibeLink.aab"
    fi
}

build_ios() {
    echo ""
    echo "[Building iOS...]"
    
    # Check if running on macOS
    if [[ "$OSTYPE" != "darwin"* ]]; then
        echo "⚠ iOS builds require macOS. Skipping."
        return
    fi
    
    # Build for iOS
    dotnet publish -f net10.0-ios \
        -c Release \
        -p:ArchiveOnBuild=true \
        -p:RuntimeIdentifier=ios-arm64 \
        -o "$BUILD_DIR/ios"
    
    echo "✓ iOS build complete: $BUILD_DIR/ios"
    echo "  Note: For App Store, use Xcode to archive and upload"
}

build_maccatalyst() {
    echo ""
    echo "[Building Mac Catalyst...]"
    
    if [[ "$OSTYPE" != "darwin"* ]]; then
        echo "⚠ Mac builds require macOS. Skipping."
        return
    fi
    
    dotnet publish -f net10.0-maccatalyst \
        -c Release \
        -o "$BUILD_DIR/mac"
    
    echo "✓ Mac build complete: $BUILD_DIR/mac"
}

# Build based on target
case "$TARGET" in
    android)
        build_android
        ;;
    ios)
        build_ios
        ;;
    mac)
        build_maccatalyst
        ;;
    all)
        build_android
        build_ios
        build_maccatalyst
        ;;
    *)
        echo "Unknown target: $TARGET"
        echo "Usage: $0 [android|ios|mac|all]"
        exit 1
        ;;
esac

echo ""
echo "=========================================="
echo "  Build Complete!"
echo "=========================================="
echo ""
echo "Build outputs in: $BUILD_DIR"
ls -lh "$BUILD_DIR"/*.apk "$BUILD_DIR"/*.aab 2>/dev/null || true
