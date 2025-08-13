using System;

namespace NT8.SDK
{
    /// <summary>
    /// Defines a risk management engine (pure C#; no NT8 types).
    /// </summary>
    public interface IRisk
    {
        /// <summary>Active risk mode.</summary>
        RiskMode Mode { get; }

        /// <summary>Returns the current lockout state.</summary>
        RiskLockoutState Lockout();

        /// <summary>True if trading is allowed at this moment.</summary>
        bool CanTradeNow();

        /// <summary>
        /// Evaluate an entry. Return EMPTY STRING ("") if accepted; otherwise a rejection reason.
        /// NOTE: Implementations must NOT return null; use "" to indicate acceptance.
        /// </summary>
        string EvaluateEntry(PositionIntent intent);

        /// <summary>
        /// Record the outcome of the most recent trade for streak tracking and controls.
        /// </summary>
        void RecordWinLoss(bool win);
    }
}
