using System;
using NT8.SDK;

namespace NT8.SDK.Risk
{
    /// <summary>
    /// IRisk wrapper that adds enable/disable and manual lockout around an inner risk engine.
    /// If disabled, entries are blocked with reason "RiskDisabled".
    /// If manual lockout is set, entries are blocked with reason "RiskManualLockout".
    /// </summary>
    public sealed class RiskManager : IRisk
    {
        private readonly IRisk _inner;
        private readonly RiskMode _mode;

        /// <summary>True to enable risk checks; false to block all entries.</summary>
        public bool Enabled { get; set; }

        /// <summary>True to force lockout regardless of inner state.</summary>
        public bool ManualLockout { get; set; }

        /// <summary>
        /// Creates a new risk manager.
        /// </summary>
        /// <param name="inner">Inner risk engine (can be a composite). May be null.</param>
        /// <param name="mode">Mode reported by this manager; defaults to inner.Mode if inner provided.</param>
        public RiskManager(IRisk inner, RiskMode mode)
        {
            _inner = inner;
            _mode = mode;
            Enabled = true;
            ManualLockout = false;
        }

        /// <inheritdoc/>
        public RiskMode Mode { get { return _mode; } }

        /// <inheritdoc/>
        public RiskLockoutState Lockout()
        {
            if (!Enabled || ManualLockout) return RiskLockoutState.LockedOut;
            return _inner != null ? _inner.Lockout() : RiskLockoutState.None;
        }

        /// <inheritdoc/>
        public bool CanTradeNow()
        {
            if (!Enabled || ManualLockout) return false;
            return _inner != null ? _inner.CanTradeNow() : true;
        }

        /// <inheritdoc/>
        /// <remarks>Return EMPTY STRING ("") when accepted; otherwise a reason string.</remarks>
        public string EvaluateEntry(PositionIntent intent)
        {
            if (!Enabled) return "RiskDisabled";
            if (ManualLockout) return "RiskManualLockout";
            return _inner != null ? _inner.EvaluateEntry(intent) : string.Empty;
        }

        /// <inheritdoc/>
        public void RecordWinLoss(bool win)
        {
            if (_inner != null) _inner.RecordWinLoss(win);
        }
    }
}

