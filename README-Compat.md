# NT8 SDK Compat DLL Setup

## Quick Install
1. Clone the repository and open Windows PowerShell.
2. Optional but recommended: run `tools/guard.ps1` to ensure repo hygiene.
3. Execute the compat build/deploy script:
   ```powershell
   scripts\build-deploy-compat.ps1
   ```

## Build + Deploy
- Delete `Documents\NinjaTrader 8\bin\Custom\bin` and `Documents\NinjaTrader 8\bin\Custom\obj` to remove stale artifacts.
- Run `scripts\build-deploy-compat.ps1` from the repo root. This compiles `NT8.SDK.Compat.dll` and drops it into your NinjaTrader `bin/Custom` folder.

## NinjaTrader Reference Sanity
1. Launch NinjaTrader 8 and open the NinjaScript editor.
2. Right-click → **References** and confirm `NT8.SDK.Compat.dll` appears with no warning icons.
3. Recompile (F5) and ensure the Output window shows **0 errors**.

## Smoke Test
- Create a blank strategy or indicator, add `using NT8.SDK.Compat;`, compile, and confirm no compiler errors.
- Enable the script on a Sim chart to verify no runtime exceptions.

## Troubleshooting
- If the build script fails, confirm PowerShell execution policy allows local scripts: `Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned -Force`.
- Always clear `bin/Custom/bin` and `bin/Custom/obj` when encountering weird compile issues.
- If NinjaTrader cannot find the DLL, rerun `scripts\build-deploy-compat.ps1` and re-add the reference.

## Version
- Applies to NT8 SDK Foundation v3 Compat build as of this commit.
