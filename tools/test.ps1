# Ensures qa/summary.json exists with dummy metrics
$repoRoot = Split-Path -Parent $PSScriptRoot
$qaDir = Join-Path $repoRoot 'qa'
New-Item -ItemType Directory -Path $qaDir -Force | Out-Null

$metrics = [ordered]@{
    MAR         = 1.2
    MaxDrawdown = 1500.00
    HitRate     = 65.0
}

$summaryPath = Join-Path $qaDir 'summary.json'
$metrics | ConvertTo-Json | Set-Content -Path $summaryPath

Write-Host "QA summary written to $summaryPath"

