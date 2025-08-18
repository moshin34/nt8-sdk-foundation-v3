namespace NT8.SDK.Abstractions.Risk
{
    using System;

    /// <summary>Deterministic portable evaluator for hard caps.</summary>
    public sealed class PortableRiskManager : IRiskManager
    {
        public RiskResult Evaluate(in RiskCaps caps, in RiskSnapshot snap)
        {
            // Max contracts: gate new entries if account quantity already at or above MaxContracts.
            if (caps.MaxContracts > 0 && snap.AccountQuantity > caps.MaxContracts)
                return RiskResult.Block(RiskDecision.BlockMaxContracts, "MaxContracts",
                    "Account position exceeds MaxContracts");

            // Daily loss
            if (caps.DailyLossLimit > 0m && (-snap.DailyPnL) >= caps.DailyLossLimit)
                return RiskResult.Block(RiskDecision.BlockDailyLoss, "DailyLossLimit",
                    "Daily loss limit breached");

            // Weekly loss
            if (caps.WeeklyLossLimit > 0m && (-snap.WeeklyPnL) >= caps.WeeklyLossLimit)
                return RiskResult.Block(RiskDecision.BlockWeeklyLoss, "WeeklyLossLimit",
                    "Weekly loss limit breached");

            // Trailing drawdown from peak equity
            var dd = snap.PeakEquity - snap.Equity;
            if (caps.TrailingDrawdown > 0m && dd >= caps.TrailingDrawdown)
                return RiskResult.Block(RiskDecision.BlockTrailingDD, "TrailingDrawdown",
                    "Trailing drawdown breached");

            return RiskResult.AllowOk();
        }
    }
}
