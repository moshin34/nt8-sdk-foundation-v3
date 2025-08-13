using System;
using NT8.SDK;

namespace NT8.SDK.Risk
{
    /// <summary>
    /// Represents the result of a risk evaluation determining whether an entry is allowed.
    /// </summary>
    [Serializable]
    public struct RiskDecision
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RiskDecision"/> struct.
        /// </summary>
        /// <param name="allowEntry">True to allow the entry; otherwise false.</param>
        /// <param name="reason">Reason for rejection. Ignored when <paramref name="allowEntry"/> is true.</param>
        /// <param name="riskModeUsed">Risk mode used during evaluation.</param>
        public RiskDecision(bool allowEntry, string reason, RiskMode riskModeUsed)
        {
            AllowEntry = allowEntry;
            Reason = allowEntry ? string.Empty : (reason ?? string.Empty);
            RiskModeUsed = riskModeUsed;
        }

        /// <summary>Gets or sets a value indicating whether the entry is allowed.</summary>
        public bool AllowEntry { get; set; }

        /// <summary>
        /// Gets or sets the reason for the decision.
        /// An empty string indicates acceptance; this value is NEVER null.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>Gets or sets the risk mode used during evaluation.</summary>
        public RiskMode RiskModeUsed { get; set; }
    }

#if DEBUG
    internal static class DebugRiskDecision
    {
        internal static void Main()
        {
            var allowed = new RiskDecision(true, "ignored", RiskMode.DCP);
            var blocked = new RiskDecision(false, "Too risky", RiskMode.DCP);
            Console.WriteLine("Allowed Reason: '" + allowed.Reason + "'");
            Console.WriteLine("Blocked Reason: '" + blocked.Reason + "'");
        }
    }
#endif
}
