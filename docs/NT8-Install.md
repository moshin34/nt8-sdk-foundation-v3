# NT8 Install & Test (Portable SDK + Strategy Bridge)

## Build the portable SDK (DLL)
From repo root (Windows PowerShell):

1) Restore & build:
   - `dotnet restore SDK.csproj`
   - `msbuild SDK.csproj /t:Build /p:Configuration=Release /v:m`

2) Artifact path:
   - `bin\\Release\\net48\\NT8.SDK.dll`

## Install into NinjaTrader 8
1) Close NinjaTrader if running.
2) Copy the DLL into your user folder:
   - `%USERPROFILE%\\Documents\\NinjaTrader 8\\bin\\Custom\\NT8.SDK.dll`
3) Launch NinjaTrader 8 → open **NinjaScript Editor**.
4) Right-click → **References...** → **Add** → select `NT8.SDK.dll` from the path above → OK.
5) In the editor, **File → New NinjaScript**; paste the contents of `NT8Bridge/SdkStrategyBridge.cs` into a new Strategy named `SdkStrategyBridge`.
6) Click **Compile** (top left of editor). You should see `Compile completed with 0 error(s)`.

## Run it on a Chart (SIM)
1) Open a chart (instrument of your choice, e.g., ES 1-minute).
2) Right-click chart → **Strategies...** → add **SdkStrategyBridge**.
3) Parameters:
   - `Fast SMA = 5`
   - `Slow SMA = 20` (must be > Fast)
   - `DryRun = true` (safe mode: logs only)
4) Enable the strategy.
5) Watch **Output** window for:
   - banner line (e.g., `NT8.SDK v0.1.0`)
   - periodic `[SDK] latest tick` lines
   - bias transitions `[SDK] bias -> ...`

## Turn on orders (optional)
- Set `DryRun = false` to place **sim** orders aligned with bias flips.
- Keep in Simulator until you validate live connections/risk.

## Troubleshooting
- If NinjaScript compile errors mention the SDK reference:
  - Re-check **References...** includes `NT8.SDK.dll`.
  - Ensure DLL target is **net48**.
  - If you update the DLL, copy it again and re-open NinjaTrader or recompile.