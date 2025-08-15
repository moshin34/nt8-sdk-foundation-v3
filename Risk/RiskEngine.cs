using System;
using NT8.SDK;          // IRisk, PositionIntent, RiskMode, RiskLockoutState, IClock
using NT8.SDK.Common;    // SystemClock
using NT8.SDK.Risk;

namespace NT8.SDK.Risk
{
    /// <summary>
    /// Reference IRisk implementation with conservative defaults and deterministic timing.
    /// - Tracks consecutive losses and applies a time-based lockout.
    /// - EvaluateEntry returns "" when accepted (never null).
    /// - Uses an injected <see cref="IClock"/> (defaults to SystemClock.Instance).
    /// - Exposes read-only diagnostics (State, LossStreak, CooldownUntilUtc).
    /// </summary>
    public sealed class RiskEngine : IRisk
    {
        private readonly object _sync = new object();
        private readonly IClock _clock;
        private readonly RiskOptions _options;

        private int _lossStreak;
        private DateTime _cooldownUntilUtc;
        private RiskLockoutState _state;

        public RiskEngine(RiskMode mode)
            : this(mode, null, null)
        {
        }

        public RiskEngine(RiskMode mode, RiskOptions options, IClock clock)
        {
            Mode = mode;
            _options = options ?? new RiskOptions
            {
                LossStreakLockout = RiskConfig.LossStreakLockoutDefault,
                LockoutDuration = RiskConfig.LockoutDurationDefault
            };
            _clock = clock ?? SystemClock.Instance;

            _lossStreak = 0;
            _cooldownUntilUtc = DateTime.MinValue;
            _state = RiskLockoutState.None;
        }

        /// <summary>Active risk mode.</summary>
        public RiskMode Mode { get; private set; }

        /// <summary>Last-known state (for diagnostics).</summary>
        public RiskLockoutState State { get { lock (_sync) { return _state; } } }

        /// <summary>Current consecutive loss count (for diagnostics).</summary>
        public int LossStreak { get { lock (_sync) { return _lossStreak; } } }

        /// <summary>Cooldown expiry (UTC) when locked out; <see cref="DateTime.MinValue"/> otherwise.</summary>
        public DateTime CooldownUntilUtc { get { lock (_sync) { return _cooldownUntilUtc; } } }

        /// <summary>
        /// Forces an immediate lockout and starts the cooldown timer.
        /// </summary>
        public RiskLockoutState Lockout()
        {
            lock (_sync)
            {
                _state = RiskLockoutState.LockedOut;
                _cooldownUntilUtc = _clock.UtcNow + _options.LockoutDuration;
                return _state;
            }
        }

        /// <summary>
        /// True if trading is currently permitted. Clears lockout when cooldown expires.
        /// </summary>
        public bool CanTradeNow()
        {
            lock (_sync)
            {
                if (_state == RiskLockoutState.LockedOut && _clock.UtcNow >= _cooldownUntilUtc)
                {
                    _state = RiskLockoutState.None;
                    _lossStreak = 0;
                    _cooldownUntilUtc = DateTime.MinValue;
                }
                return _state == RiskLockoutState.None;
            }
        }

        /// <summary>
        /// Evaluates an entry intent. Returns "" if accepted; otherwise a textual reason.
        /// </summary>
        public string EvaluateEntry(PositionIntent intent)
        {
            if (string.IsNullOrEmpty(intent.Symbol)) return "symbol missing";
            if (!CanTradeNow()) return "risk lockout in effect";
            // Mode hooks can be added later (ECP/PCP etc). For Step 5 we accept by default.
            return "";
        }

        /// <summary>
        /// Records the outcome of the most recent trade and updates lockout state.
        /// </summary>
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

                _lossStreak++;
                if (_lossStreak >= _options.LossStreakLockout)
                {
                    _state = RiskLockoutState.LockedOut;
                    _cooldownUntilUtc = _clock.UtcNow + _options.LockoutDuration;
                }
                else if (_lossStreak == 1)
                {
                    _state = RiskLockoutState.CoolingDown; // advisory; CanTradeNow still true
                }
            }
        }

        /// <summary>Resets internal counters and clears any lockout (for tests).</summary>
        public void Reset()
        {
            lock (_sync)
            {
                _lossStreak = 0;
                _state = RiskLockoutState.None;
                _cooldownUntilUtc = DateTime.MinValue;
            }
        }
    }
}
