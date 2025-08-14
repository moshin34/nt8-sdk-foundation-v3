using System;

namespace NT8.SDK.Risk
{
    /// <summary>Simple risk thresholds; constants for Step 3.</summary>
    public static class RiskConfig
    {
        public const int LossStreakLockout = 3;      // lockout after N consecutive losses
        public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    }
}
