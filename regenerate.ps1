# Regenerates the proxy source files in generated/Ndump.Generated/
# from a fresh TestApp dump.
$ErrorActionPreference = 'Stop'

$scriptDir = $PSScriptRoot
$publishDir = Join-Path ([System.IO.Path]::GetTempPath()) "ndump_pub_$([System.Guid]::NewGuid().ToString('N').Substring(0,8))"
$dumpPath = Join-Path ([System.IO.Path]::GetTempPath()) "ndump_gen_$([System.Guid]::NewGuid().ToString('N').Substring(0,8)).dmp"
$outputDir = Join-Path $scriptDir 'generated\Ndump.Generated'

try {
    Write-Host '==> Publishing TestApp...'
    dotnet publish "$scriptDir\src\Ndump.TestApp" -o $publishDir -c Release --no-self-contained -v quiet
    if ($LASTEXITCODE -ne 0) { throw 'Failed to publish TestApp' }

    Write-Host '==> Running TestApp to create dump...'
    & "$publishDir\Ndump.TestApp.exe" $dumpPath
    if ($LASTEXITCODE -ne 0) { throw 'TestApp failed' }

    Write-Host '==> Emitting proxy sources...'
    dotnet run --project "$scriptDir\src\Ndump.Cli" -- emit $dumpPath -o $outputDir
    if ($LASTEXITCODE -ne 0) { throw 'Emit failed' }

    Write-Host '==> Done. Sources written to generated\Ndump.Generated\'
}
finally {
    Remove-Item $dumpPath -ErrorAction SilentlyContinue
    Remove-Item $publishDir -Recurse -ErrorAction SilentlyContinue
}
