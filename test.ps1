# Full test pipeline: regenerate proxies, build, and run all tests.
$ErrorActionPreference = 'Stop'

$scriptDir = $PSScriptRoot

Write-Host '==> Step 1/3: Regenerating proxies...'
& "$scriptDir\regenerate.ps1"
if ($LASTEXITCODE -ne 0) { throw 'Regeneration failed' }

Write-Host '==> Step 2/3: Building solution...'
dotnet build "$scriptDir" -v quiet
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }

Write-Host '==> Step 3/3: Running all tests...'
dotnet test "$scriptDir" --no-build
if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }

Write-Host '==> All done. Regeneration + build + tests passed.'
