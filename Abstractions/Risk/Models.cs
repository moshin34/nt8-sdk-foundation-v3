namespace NT8.SDK.Abstractions.Risk
{
    using System;

    /// <summary>Summary of hard caps for portable evaluation.</summary>
    public struct RiskCaps
    {
        public int MaxContracts;
        public decimal DailyLossLimit;
        public decimal WeeklyLossLimit;
        public decimal TrailingDrawdown;
    }

    /// <summary>Input snapshot for risk evaluation.</summary>
    public struct RiskSnapshot
    {
        public int AccountQuantity;          // Absolute position size for the instrument at account level
        public decimal Equity;               // Current cumulative PnL (base = 0 at some anchor)
        public decimal PeakEquity;           // Peak cumulative PnL seen so far
        public decimal DailyPnL;             // Cumulative PnL since daily anchor (>= -caps.DailyLossLimit)
        public decimal WeeklyPnL;            // Cumulative PnL since weekly anchor (>= -caps.WeeklyLossLimit)
    }

    /// <summary>Decision enum for gating entries / enforcing flatten.</summary>
    public enum RiskDecision
    {
        Allow = 0,
        BlockMaxContracts = 1,
        BlockDailyLoss = 2,
        BlockWeeklyLoss = 3,
        BlockTrailingDD = 4
    }

    /// <summary>Portable result with reason + message.</summary>
    public struct RiskResult
    {
        public RiskDecision Decision;
        public string Reason;
        public string Message;

        public static RiskResult AllowOk()
        {
            var r = new RiskResult();
            r.Decision = RiskDecision.Allow;
            r.Reason = "OK";
            r.Message = "Allowed";
            return r;
        }

        public static RiskResult Block(RiskDecision d, string reason, string message)
        {
            var r = new RiskResult();
            r.Decision = d;
            r.Reason = reason;
            r.Message = message;
            return r;
        }
    }
}
