$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$outDir = Join-Path $repo 'out'
$zipPath = Join-Path $outDir 'nt8-sdk-bundle.zip'
$customBin = Join-Path $env:USERPROFILE 'Documents\NinjaTrader 8\bin\Custom'

New-Item -ItemType Directory -Force -Path $outDir | Out-Null
Remove-Item $zipPath -ErrorAction SilentlyContinue

$files = @(
    'bin\Release\net48\NT8.SDK.dll',
    'NT8Bridge\*.cs',
    'Strategies\*Shell*.cs',
    'docs\NT8-Install.md'
) | ForEach-Object { Join-Path $repo $_ }

Compress-Archive -Path $files -DestinationPath $zipPath
Write-Host "✅ NT8 SDK bundle created: $zipPath"

