using System;

namespace NT8.SDK.Risk
{
    /// <summary>Runtime-tunable risk parameters with sensible defaults.</summary>
    public sealed class RiskOptions
    {
        /// <summary>Consecutive losses that trigger a lockout.</summary>
        public int LossStreakLockout { get; set; }

        /// <summary>Duration of the lockout once triggered.</summary>
        public TimeSpan LockoutDuration { get; set; }

        public RiskOptions()
        {
            LossStreakLockout = RiskConfig.LossStreakLockoutDefault;
            LockoutDuration   = RiskConfig.LockoutDurationDefault;
        }
    }
}
