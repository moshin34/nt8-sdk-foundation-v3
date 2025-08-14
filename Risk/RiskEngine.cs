using System;

namespace NT8.SDK.Risk
{
    /// <summary>
    /// Reference <see cref="IRisk"/> implementation with conservative defaults:
    /// - Tracks consecutive losses and applies a time-based lockout.
    /// - <see cref="EvaluateEntry(NT8.SDK.PositionIntent)"/> returns empty string ("") when accepted.
    /// - Uses UTC for cooldown timing.
    /// </summary>
    public sealed class RiskEngine : IRisk
    {
        private readonly object _sync = new object();
        private int _lossStreak;
        private DateTime _cooldownUntilUtc;
        private RiskLockoutState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="RiskEngine"/> class.
        /// </summary>
        /// <param name="mode">Active risk mode (ECP/PCP/DCP/HR).</param>
        public RiskEngine(RiskMode mode)
        {
            Mode = mode;
            _lossStreak = 0;
            _cooldownUntilUtc = DateTime.MinValue;
            _state = RiskLockoutState.None;
        }

        /// <summary>
        /// Gets the active risk mode.
        /// </summary>
        public RiskMode Mode { get; private set; }

        /// <summary>
        /// Forces a lockout immediately and starts the cooldown timer.
        /// </summary>
        /// <returns>The new lockout state (LockedOut).</returns>
        public RiskLockoutState Lockout()
        {
            lock (_sync)
            {
                _state = RiskLockoutState.LockedOut;
                _cooldownUntilUtc = DateTime.UtcNow + RiskConfig.LockoutDuration;
                return _state;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if trading is permitted at this moment (i.e., not locked out).
        /// If the cooldown has expired, clears the lockout and resets the loss streak.
        /// </summary>
        public bool CanTradeNow()
        {
            lock (_sync)
            {
                if (_state == RiskLockoutState.LockedOut && DateTime.UtcNow >= _cooldownUntilUtc)
                {
                    _state = RiskLockoutState.None;
                    _lossStreak = 0;
                }
                return _state == RiskLockoutState.None;
            }
        }

        /// <summary>
        /// Evaluates an entry intent and returns an empty string ("") to ACCEPT or a reason to REJECT.
        /// Implementations must not return null.
        /// </summary>
        /// <param name="intent">The position intent to evaluate.</param>
        /// <returns>"" if accepted; otherwise a textual rejection reason.</returns>
        public string EvaluateEntry(PositionIntent intent)
        {
            // Defensive checks
            if (string.IsNullOrEmpty(intent.Symbol)) return "symbol missing";

            // Lock state gate
            if (!CanTradeNow()) return "risk lockout in effect";

            // Mode-specific gates (placeholder hooks; conservative accept for Step 3)
            switch (Mode)
            {
                case RiskMode.ECP:
                case RiskMode.PCP:
                case RiskMode.DCP:
                case RiskMode.HR:
                default:
                    return ""; // accept
            }
        }

        /// <summary>
        /// Records the outcome of the most recent trade.
        /// Increments loss streak on loss and applies/updates cooldown when thresholds are met.
        /// </summary>
        /// <param name="win"><c>true</c> if the trade was a win; <c>false</c> if it was a loss.</param>
        public void RecordWinLoss(bool win)
        {
            lock (_sync)
            {
                if (win)
                {
                    _lossStreak = 0;
                    if (_state == RiskLockoutState.CoolingDown) _state = RiskLockoutState.None;
                    return;
                }

                // Loss path
                _lossStreak++;
                if (_lossStreak >= RiskConfig.LossStreakLockout)
                {
                    _state = RiskLockoutState.LockedOut;
                    _cooldownUntilUtc = DateTime.UtcNow + RiskConfig.LockoutDuration;
                }
                else if (_lossStreak == 1)
                {
                    // Soft hint; CanTradeNow still true
                    _state = RiskLockoutState.CoolingDown;
                }
            }
        }
    }
}

