using System;

namespace NT8.SDK.Common
{
    /// <summary>
    /// Defines inclusive quantity bounds (min/max) for each <see cref="RiskMode"/>.
    /// Use with <see cref="PositionSizer"/> to clamp sizing decisions to tier limits.
    /// </summary>
    public sealed class RiskTiers
    {
        /// <summary>
        /// Describes the bounds and a tag for a single tier.
        /// </summary>
        [Serializable]
        public struct Tier
        {
            /// <summary>Minimum allowed quantity (inclusive).</summary>
            public int Min;

            /// <summary>Maximum allowed quantity (inclusive).</summary>
            public int Max;

            /// <summary>Optional tag to annotate clamp reasons.</summary>
            public string Tag;

            /// <summary>
            /// Initializes a new <see cref="Tier"/>.
            /// </summary>
            public Tier(int min, int max, string tag)
            {
                if (min < 0) min = 0;
                if (max < min) max = min;
                Min = min;
                Max = max;
                Tag = tag ?? string.Empty;
            }
        }

        private readonly Tier _ecp;
        private readonly Tier _pcp;
        private readonly Tier _dcp;
        private readonly Tier _hr;

        /// <summary>
        /// Creates a new <see cref="RiskTiers"/> set.
        /// </summary>
        public RiskTiers(Tier ecp, Tier pcp, Tier dcp, Tier hr)
        {
            _ecp = ecp;
            _pcp = pcp;
            _dcp = dcp;
            _hr  = hr;
        }

        /// <summary>
        /// Returns the tier for the specified <paramref name="mode"/>.
        /// </summary>
        public Tier For(RiskMode mode)
        {
            switch (mode)
            {
                case RiskMode.ECP: return _ecp;
                case RiskMode.PCP: return _pcp;
                case RiskMode.DCP: return _dcp;
                case RiskMode.HR:
                default: return _hr;
            }
        }

        /// <summary>
        /// Provides a conservative default tier set.
        /// </summary>
        public static RiskTiers Defaults()
        {
            return new RiskTiers(
                new Tier(1, 1, "ECP"),
                new Tier(1, 2, "PCP"),
                new Tier(1, 3, "DCP"),
                new Tier(1, 1, "HR"));
        }
    }
}

