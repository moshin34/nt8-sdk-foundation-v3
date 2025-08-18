# Cutover: RiskCappedStrategy → RiskDelegatedStrategy

## Preconditions
- NT8 SDK DLL added to **Tools → NinjaScript → References** and compilation succeeds.
- PRs #132, #135, #138 merged on `main`.

## SIM Validation (required)
1. Apply `RiskDelegatedStrategy` on SIM.
2. Set: `MaxContracts=1`, small `DailyLossLimit`, `UseAccountFlatten=true`, `DebugMode=true`, Start=**Wait until flat**.
3. Expect Output: `[SDK] Hooked ...`
4. Place a 2-lot manual order → expect cancel/flatten; `[RiskCheck] ... breached=True`.

## Live Cutover
1. Disable `RiskCappedStrategy`; confirm account is **flat**.
2. Enable `RiskDelegatedStrategy` on Live (same caps). Set `DebugMode=false`, Start=**Wait until flat**.
3. Monitor Output 2–3 minutes.

## Rollback
- Disable `RiskDelegatedStrategy` → **Flatten Everything** → re-enable `RiskCappedStrategy` with prior settings.

## Notes
- Both strategies enforce account-level caps; Delegated version routes decisions through SDK when the DLL is present.
- Keep `UseAccountFlatten=true` for production safety.
