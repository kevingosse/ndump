#!/usr/bin/env bash
# Regenerates the proxy source files in generated/Ndump.Generated/
# from a fresh TestApp dump.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PUBLISH_DIR="$(mktemp -d)"
DUMP_PATH="$(mktemp -u).dmp"
OUTPUT_DIR="$SCRIPT_DIR/generated/Ndump.Generated"

cleanup() {
    rm -f "$DUMP_PATH"
    rm -rf "$PUBLISH_DIR"
}
trap cleanup EXIT

echo "==> Publishing TestApp..."
dotnet publish "$SCRIPT_DIR/src/Ndump.TestApp" -o "$PUBLISH_DIR" -c Release --no-self-contained -v quiet

echo "==> Running TestApp to create dump..."
"$PUBLISH_DIR/Ndump.TestApp.exe" "$DUMP_PATH"

echo "==> Emitting proxy sources..."
dotnet run --project "$SCRIPT_DIR/src/Ndump.Cli" -- init "$DUMP_PATH" "$OUTPUT_DIR"

echo "==> Done. Sources written to generated/Ndump.Generated/"
