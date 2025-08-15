# Release build
msbuild SDK.sln /p:Configuration=Release /m

# Copy outputs to NinjaTrader (if present)
$dst = Join-Path $env:USERPROFILE 'Documents\NinjaTrader 8\bin\Custom'
if (Test-Path $dst) {
  Get-ChildItem -Recurse -Filter *.dll | Where-Object { $_.FullName -match '\\bin\\Release\\' } | ForEach-Object { Copy-Item $_.FullName $dst -Force }
  Get-ChildItem -Recurse -Filter *.xml | Where-Object { $_.FullName -match '\\bin\\Release\\' } | ForEach-Object { Copy-Item $_.FullName $dst -Force }
  Write-Host 'Copied Release DLL/XML to NinjaTrader.'
} else {
  Write-Host 'NinjaTrader Custom folder not found; skipped copy.'
}
