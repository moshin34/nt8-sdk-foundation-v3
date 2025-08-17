$ErrorActionPreference = 'Stop'

$repo     = Split-Path -Parent $PSScriptRoot
$outDir   = Join-Path $repo 'out'
$buildDir = Join-Path $outDir 'build'
$zipPath  = Join-Path $outDir 'nt8-sdk-bundle.zip'

Remove-Item $buildDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $buildDir | Out-Null
New-Item -ItemType Directory -Force -Path $outDir   | Out-Null

Copy-Item (Join-Path $repo 'bin\Release\net48\NT8.SDK.dll') $buildDir -Force
Copy-Item (Join-Path $repo 'NT8Bridge') (Join-Path $buildDir 'NT8Bridge') -Recurse -Force
Copy-Item (Join-Path $repo 'Strategies') (Join-Path $buildDir 'Strategies') -Recurse -Force
Copy-Item (Join-Path $repo 'docs\NT8-Install.md') (Join-Path $buildDir 'docs') -Force

Copy-Item qa\summary.json $buildDir\qa_summary.json -Force

Compress-Archive -Path "$buildDir\*" -DestinationPath $zipPath -Force
Write-Host "✅ NT8 SDK bundle created: $zipPath"
