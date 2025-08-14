using System;
using NT8.SDK; // for IClock, SystemClock

namespace NT8.SDK.Risk
{
    /// <summary>
    /// Reference IRisk implementation with conservative defaults:
    /// - Tracks consecutive losses and applies a time-based lockout.
    /// - EvaluateEntry returns "" when accepted (never null).
    /// - Uses a pluggable clock for UTC timing (defaults to SystemClock).
    /// - Thresholds and durations are configurable via RiskOptions.
    /// </summary>
    public sealed class RiskEngine : IRisk
    {
        private readonly object _sync = new object();
        private readonly int _lossStreakThreshold;
        private readonly TimeSpan _lockoutDuration;
        private readonly IClock _clock;

        private int _lossStreak;
        private DateTime _cooldownUntilUtc;
        private RiskLockoutState _state;

        /// <summary>Create with defaults (LossStreak=RiskConfig default; Lockout=RiskConfig default).</summary>
        public RiskEngine(RiskMode mode) : this(mode, null, null) { }

        /// <summary>Create with explicit options and optional clock (UI/test configurable).</summary>
        public RiskEngine(RiskMode mode, RiskOptions options) : this(mode, options, null) { }

        /// <summary>Primary ctor.</summary>
        public RiskEngine(RiskMode mode, RiskOptions options, IClock clock)
        {
            Mode = mode;

            var opts = options ?? new RiskOptions();
            _lossStreakThreshold = opts.LossStreakLockout <= 0 ? RiskConfig.LossStreakLockout : opts.LossStreakLockout;
            _lockoutDuration = opts.LockoutDuration <= TimeSpan.Zero ? RiskConfig.LockoutDuration : opts.LockoutDuration;

            _clock = clock ?? SystemClock.Instance;

            _lossStreak = 0;
            _cooldownUntilUtc = DateTime.MinValue;
            _state = RiskLockoutState.None;
        }

        /// <summary>Active risk mode.</summary>
        public RiskMode Mode { get; private set; }

        /// <summary>Current lockout state.</summary>
        public RiskLockoutState State { get { lock (_sync) { return _state; } } }

        /// <summary>Current consecutive loss count.</summary>
        public int LossStreak { get { lock (_sync) { return _lossStreak; } } }

        /// <summary>UTC time when lockout ends (or MinValue when not locked out).</summary>
        public DateTime CooldownUntilUtc { get { lock (_sync) { return _cooldownUntilUtc; } } }

        public RiskLockoutState Lockout()
        {
            lock (_sync)
            {
                _state = RiskLockoutState.LockedOut;
                _cooldownUntilUtc = _clock.UtcNow + _lockoutDuration;
                return _state;
            }
        }

        public bool CanTradeNow()
        {
            lock (_sync)
            {
                if (_state == RiskLockoutState.LockedOut && _clock.UtcNow >= _cooldownUntilUtc)
                {
                    _state = RiskLockoutState.None;
                    _lossStreak = 0;
                }
                return _state == RiskLockoutState.None;
            }
        }

        public string EvaluateEntry(PositionIntent intent)
        {
            if (string.IsNullOrEmpty(intent.Symbol)) return "symbol missing";
            if (!CanTradeNow()) return "risk lockout in effect";
            // Per-mode rules can be extended later; accept by default.
            return "";
        }

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
                if (_lossStreak >= _lossStreakThreshold)
                {
                    _state = RiskLockoutState.LockedOut;
                    _cooldownUntilUtc = _clock.UtcNow + _lockoutDuration;
                }
                else if (_lossStreak == 1)
                {
                    _state = RiskLockoutState.CoolingDown; // soft hint; CanTradeNow still true
                }
            }
        }

        /// <summary>Optional test helper: manually clear state.</summary>
        public void Reset()
        {
            lock (_sync)
            {
                _lossStreak = 0;
                _cooldownUntilUtc = DateTime.MinValue;
                _state = RiskLockoutState.None;
            }
        }
    }
}
