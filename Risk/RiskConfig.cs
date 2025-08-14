using System;

namespace NT8.SDK.Risk
{
    /// <summary>
    /// Defaults for the risk engine. Runtime overrides are provided via <see cref="RiskOptions"/>.
    /// </summary>
    public static class RiskConfig
    {
        /// <summary>Default maximum allowed consecutive losses before lockout.</summary>
        public const int LossStreakLockoutDefault = 2;

        /// <summary>Default duration of lockout once triggered.</summary>
        public static readonly TimeSpan LockoutDurationDefault = TimeSpan.FromMinutes(15);
    }
}
