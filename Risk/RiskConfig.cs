using System;

namespace NT8.SDK.Risk
{
    /// <summary>Static configuration for risk engine defaults.</summary>
    public static class RiskConfig
    {
        /// <summary>Maximum allowed consecutive losses before triggering lockout.</summary>
        public static readonly int LossStreakLockout = 2;

        /// <summary>Duration of lockout applied after reaching the loss streak.</summary>
        public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    }
}
