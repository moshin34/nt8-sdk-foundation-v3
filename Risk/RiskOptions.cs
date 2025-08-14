using System;

namespace NT8.SDK.Risk
{
    /// <summary>Configurable risk parameters with safe defaults.</summary>
    public sealed class RiskOptions
    {
        /// <summary>Lock out after this many consecutive losses. Default = RiskConfig.LossStreakLockout (2).</summary>
        public int LossStreakLockout { get; set; }

        /// <summary>Duration of lockout once triggered. Default = RiskConfig.LockoutDuration.</summary>
        public TimeSpan LockoutDuration { get; set; }

        public RiskOptions()
        {
            LossStreakLockout = RiskConfig.LossStreakLockout;
            LockoutDuration = RiskConfig.LockoutDuration;
        }
    }
}
