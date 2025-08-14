using System;

namespace NT8.SDK.Facade
{
    /// <summary>Composes risk + session checks into a single entry gate.</summary>
    public static class EntryGates
    {
        /// <summary>
        /// Returns "" when entry is allowed; otherwise a reason ("sdk missing", "symbol missing",
        /// "flat intent", "risk lockout in effect", "settlement window", "blackout window").
        /// Caller passes ET in <paramref name="etNow"/>.
        /// </summary>
        public static string CheckEntry(ISdk sdk, DateTime etNow, PositionIntent intent)
        {
            if (sdk == null) return "sdk missing";
            if (string.IsNullOrEmpty(intent.Symbol)) return "symbol missing";
            if (intent.Side == PositionSide.Flat) return "flat intent";

            // Risk gate
            var riskReason = sdk.Risk != null ? sdk.Risk.EvaluateEntry(intent) : "";
            if (!string.IsNullOrEmpty(riskReason)) return riskReason;

            // Session gates (ET)
            if (sdk.Session != null)
            {
                if (sdk.Session.IsSettlementWindow(etNow, intent.Symbol)) return "settlement window";
                if (sdk.Session.IsBlackout(etNow, intent.Symbol)) return "blackout window";
            }

            return "";
        }
    }
}
