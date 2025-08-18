param(
    [Parameter(Mandatory=$false)]
    [string] $Path = "NT8Strategies"
)

Write-Host "NinjaScript Lint: scanning $Path"

function Test-File {
    param([string]$file)

    $src = Get-Content $file -Raw
    $errors = @()

    if ($src -notmatch 'namespace\s+NinjaTrader\.NinjaScript\.Strategies') { $errors += "Wrong or missing namespace." }

    $requiredUsings = @(
        'using\s+System;',
        'using\s+NinjaTrader\.Cbi;',
        'using\s+NinjaTrader\.Data;',
        'using\s+NinjaTrader\.Gui\.Tools;',
        'using\s+NinjaTrader\.NinjaScript;',
        'using\s+NinjaTrader\.NinjaScript\.Strategies;',
        'using\s+NinjaTrader\.NinjaScript\.StrategyGenerator;',
        'using\s+System\.ComponentModel;',
        'using\s+System\.ComponentModel\.DataAnnotations;'
    )
    foreach ($u in $requiredUsings) {
        if ($src -notmatch $u) { $errors += "Missing using: $u" }
    }

    if ($src -notmatch 'Calculate\s*=\s*Calculate\.OnBarClose') { $errors += "Wrong or missing Calculate.OnBarClose." }
    if ($src -notmatch 'if\s*\(\s*CurrentBar\s*<\s*BarsRequiredToTrade\s*\)\s*return;') { $errors += "Missing BarsRequiredToTrade guard in OnBarUpdate." }

    $onOrderSig = 'OnOrderUpdate\s*\(\s*Order\s+order,\s*double\s+limitPrice,\s*double\s+stopPrice,\s*int\s+quantity,\s*int\s+filled,\s*double\s+averageFillPrice,\s*OrderState\s+orderState,\s*DateTime\s+time,\s*ErrorCode\s+error,\s*string\s+nativeError\s*\)'
    if ($src -notmatch $onOrderSig) { $errors += "OnOrderUpdate signature incorrect (must include int filled in correct position)." }

    if ($errors.Count -gt 0) {
        Write-Host "NinjaScript Lint Failed for $file:"
        $errors | ForEach-Object { Write-Host " - $_" }
        return $false
    }

    Write-Host "NinjaScript Lint Passed: $file"
    return $true
}

$targetFiles = @()

if (Test-Path $Path) {
    if ((Get-Item $Path).PSIsContainer) {
        $targetFiles = Get-ChildItem -Path $Path -Recurse -Filter *.cs | ForEach-Object { $_.FullName }
    } else {
        $targetFiles = @($Path)
    }
} else {
    Write-Host "Path not found: $Path"
    exit 0
}

$failed = $false
foreach ($f in $targetFiles) {
    if (-not (Test-File -file $f)) { $failed = $true }
}

if ($failed) { exit 1 } else { exit 0 }
