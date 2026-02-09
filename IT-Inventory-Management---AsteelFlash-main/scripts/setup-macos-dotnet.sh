#!/usr/bin/env bash
set -euo pipefail

# Script to install .NET SDK 8 on macOS via Homebrew (Apple Silicon / Intel)
# Usage: bash scripts/setup-macos-dotnet.sh

if ! command -v brew >/dev/null 2>&1; then
  echo "Homebrew not found. Please install Homebrew first: https://brew.sh/"
  exit 1
fi

echo "Installing .NET SDK 8 via Homebrew cask..."
brew install --cask dotnet-sdk || brew upgrade --cask dotnet-sdk

echo "If 'dotnet' is not on your PATH, add it. For example (zsh):"
echo "  export PATH=\"$PATH:/opt/homebrew/share/dotnet\""

echo "Verify installation with: dotnet --info"