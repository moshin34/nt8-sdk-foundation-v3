using System;
using NT8.SDK;

namespace NT8.SDK.Common
{
    /// <summary>
    /// IRisk guard that blocks new entries after a configured number of consecutive losses.
    /// Resets the streak on a win.
    /// </summary>
    public sealed class LossStreakGuard : IRisk
    {
        private readonly int _maxLosses;
        private readonly string _reasonTag;
        private readonly RiskMode _mode;
        private int _lossStreak;

        /// <summary>
        /// Creates a new loss-streak guard.
        /// </summary>
        /// <param name="maxLosses">Maximum consecutive losses allowed before blocking entries (>= 1).</param>
        /// <param name="mode">Risk mode reported by this guard.</param>
        /// <param name="reasonTag">Reason text returned by <see cref="EvaluateEntry"/> when blocked.</param>
        public LossStreakGuard(int maxLosses, RiskMode mode, string reasonTag = "LossStreak")
        {
            _maxLosses = maxLosses < 1 ? 1 : maxLosses;
            _mode = mode;
            _reasonTag = reasonTag ?? string.Empty;
            _lossStreak = 0;
        }

        /// <inheritdoc/>
        public RiskMode Mode { get { return _mode; } }

        /// <inheritdoc/>
        public RiskLockoutState Lockout()
        {
            return _lossStreak >= _maxLosses ? RiskLockoutState.LockedOut : RiskLockoutState.None;
        }

        /// <inheritdoc/>
        public bool CanTradeNow()
        {
            return _lossStreak < _maxLosses;
        }

        /// <inheritdoc/>
        /// <remarks>Return EMPTY STRING ("") when accepted; otherwise a reason tag.</remarks>
        public string EvaluateEntry(PositionIntent intent)
        {
            return _lossStreak >= _maxLosses ? _reasonTag : string.Empty;
        }

        /// <inheritdoc/>
        public void RecordWinLoss(bool win)
        {
            if (win) _lossStreak = 0;
            else _lossStreak++;
        }

#if DEBUG
        internal static class LossStreakGuardTests
        {
            internal static void Smoke()
            {
                var g = new LossStreakGuard(2, RiskMode.DCP);
                System.Diagnostics.Debug.Assert(g.EvaluateEntry(new PositionIntent("ES", PositionSide.Long)) == "");
                g.RecordWinLoss(false);
                System.Diagnostics.Debug.Assert(g.CanTradeNow());
                g.RecordWinLoss(false);
                System.Diagnostics.Debug.Assert(!g.CanTradeNow());
                System.Diagnostics.Debug.Assert(g.EvaluateEntry(new PositionIntent("ES", PositionSide.Long)) != "");
                g.RecordWinLoss(true);
                System.Diagnostics.Debug.Assert(g.CanTradeNow());
            }
        }
#endif
    }
}

