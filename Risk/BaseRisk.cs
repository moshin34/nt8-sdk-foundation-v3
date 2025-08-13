using System;
using NT8.SDK;

namespace NT8.SDK.Risk
{
    /// <summary>
    /// Provides a base implementation for risk rules.
    /// </summary>
    public abstract class BaseRisk : IRisk
    {
        /// <summary>Initializes a new instance of the <see cref="BaseRisk"/> class.</summary>
        /// <param name="mode">Risk mode represented by this rule.</param>
        protected BaseRisk(RiskMode mode)
        {
            Mode = mode;
        }

        /// <summary>Gets the risk mode represented by this rule.</summary>
        public RiskMode Mode { get; }

        /// <inheritdoc/>
        public virtual RiskLockoutState Lockout() { return RiskLockoutState.None; }

        /// <inheritdoc/>
        public virtual bool CanTradeNow() { return true; }

        /// <inheritdoc/>
        public virtual void RecordWinLoss(bool win) { /* no-op */ }

        /// <inheritdoc/>
        public abstract string EvaluateEntry(PositionIntent intent);
    }

#if DEBUG
    internal sealed class FakeRiskAllow : BaseRisk
    {
        public FakeRiskAllow() : base(RiskMode.DCP) { }
        public override string EvaluateEntry(PositionIntent intent) { return string.Empty; }
    }

    internal static class DebugBaseRisk
    {
        internal static void Main()
        {
            var risk = new FakeRiskAllow();
            Console.WriteLine("Mode: " + risk.Mode);
            Console.WriteLine("CanTrade: " + risk.CanTradeNow());
            Console.WriteLine("EvaluateEntry: '" + risk.EvaluateEntry(new PositionIntent("ES", PositionSide.Long)) + "'");
        }
    }
#endif
}
