#!/usr/bin/env bash
# Full test pipeline: regenerate proxies, build, and run all tests.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo '==> Step 1/3: Regenerating proxies...'
"$SCRIPT_DIR/regenerate.sh"

echo '==> Step 2/3: Building solution...'
dotnet build "$SCRIPT_DIR" -v quiet

echo '==> Step 3/3: Running all tests...'
dotnet test "$SCRIPT_DIR" --no-build

echo '==> All done. Regeneration + build + tests passed.'
