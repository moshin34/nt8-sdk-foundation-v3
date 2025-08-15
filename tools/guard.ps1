param([switch]$FailOnWarn=$true)
Write-Host 'Running repo guard...'
$errors = @()

# 1) Duplicate/colliding folders
$pairs = @(
  @{A='Docs';        B='docs'},
  @{A='Diagnostics'; B='Diag'},
  @{A='NT8Bridge';   B='Bridge'},
  @{A='QA.TestKit';  B='QA/ Trace'},  # space variant
  @{A='QA.TestKit';  B='QA_Trace'}    # underscore variant
)
foreach ($p in $pairs) {
  if ((Test-Path $p.A) -and (Test-Path $p.B)) {
    $errors += "Both '$($p.A)' and '$($p.B)' exist. Pick one."
  }
}

# 2) No NinjaTrader.* in portable layers
$portable = 'Abstractions','Common','Risk','Sizing','Session','Trailing','Diagnostics','Telemetry'
foreach ($dir in $portable) {
  if (Test-Path $dir) {
    $hits = Select-String -Path "$dir\**\*.cs" -Pattern 'NinjaTrader\.'
    if ($hits) { $errors += "NinjaTrader.* found in portable layer '$dir'." }
  }
}

# 3) One router implementation heuristic (warn)
if (Test-Path Orders) {
  $routerHits = Select-String -Path 'Orders\**\*.cs','Common\**\*.cs','NT8Bridge\**\*.cs' -Pattern 'interface\s+IOrders|class\s+.*Router'
  if ($routerHits.Count -gt 5) { $errors += 'Multiple router-like implementations detected. Confirm single canonical router.' }
}

# 4) Require AGENTS.md
if (-not (Test-Path AGENTS.md)) { $errors += 'AGENTS.md missing at repo root.' }

if ($errors.Count) {
  $errors | ForEach-Object { Write-Host "::error ::$_" }
  if ($FailOnWarn) { exit 1 }
} else {
  Write-Host 'Guard passed.'
}
