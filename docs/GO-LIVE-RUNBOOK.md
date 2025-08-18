# NT8 Go-Live Runbook — RiskCappedStrategy

## 1) Import + Compile
1. Open NinjaTrader 8 → Tools → NinjaScript Editor → New → Strategy
2. Create a new file named `RiskCappedStrategy` and paste from `NT8Strategies/Live/RiskCappedStrategy.cs` in this repo.
3. Click **Compile**. Ensure no errors.

## 2) Configure Strategy (Chart → Strategies)
- **MaxContracts**: set per account rules (e.g., 1–3)
- **DailyLossLimit**: in account currency (e.g., 500)
- **WeeklyLossLimit**: in account currency (e.g., 1500)
- **TrailingDrawdown**: in account currency (e.g., 1500)
- **Debug Mode**: `true` for verbose logs during first session

## 3) SIM Smoke
- Apply to 1-minute chart (ES/NQ/MES/MNQ etc.)
- Confirm:
  - Entries occur only when risk not breached.
  - When loss thresholds are reached, no new entries and open position exits.

## 4) Live Toggle
- Disable `Debug Mode` after validation.
- Confirm broker connection and instrument mapping in NT8.
- Start strategy on live account with MaxContracts set conservatively.
- **Position Sync**: Set Start behavior = **Wait until flat**; ensure account is flat before switching to Live.

Notes:
- Daily/Weekly PnL baselines approximate via session/week anchors.
- Replace demo entry with your production signals as needed (logic block in `OnBarUpdate`).
- Enforcement runs on market data ticks; on breach, working orders are cancelled and (when **UseAccountFlatten = true**) the account is flattened automatically.
- Manual orders that violate caps while the strategy is enabled will be cancelled/flattened.
