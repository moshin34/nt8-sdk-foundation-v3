param(
  [Parameter(Mandatory=$false)][string] $OutDir = "artifacts",
  [Parameter(Mandatory=$false)][string] $ZipName = "NT8-Import-Pack.zip"
)

$ErrorActionPreference = "Stop"

$files = @(
  "NT8Strategies/Live/RiskCappedStrategy.cs",
  "NT8Strategies/Templates/MyStrategyTemplate.cs"
)

if (!(Test-Path $OutDir)) { New-Item -ItemType Directory -Path $OutDir | Out-Null }

$zipPath = Join-Path $OutDir $ZipName

if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

$missing = @()
foreach ($f in $files) { if (!(Test-Path $f)) { $missing += $f } }
if ($missing.Count -gt 0) {
  Write-Host "Missing files:"; $missing | ForEach-Object { Write-Host " - $_" }
  exit 1
}

Compress-Archive -Path $files -DestinationPath $zipPath -Force
Write-Host "Created $zipPath"
exit 0
