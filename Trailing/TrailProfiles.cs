using System;
using NT8.SDK;
using NT8.SDK.Trailing;

namespace NT8.SDK.Trailing
{
    /// <summary>
    /// Factory for creating non-loosening trailing engines from <see cref="TrailingProfile"/> definitions.
    /// Uses Step-5 ITrailingStop strategies and wraps them with the Step-6 non-loosening adapter.
    /// </summary>
    public sealed class TrailProfiles
    {
        /// <summary>
        /// Creates an <see cref="ITrailing"/> engine that enforces non-loosening semantics.
        /// Only <see cref="TrailingProfileType.FixedTicks"/> is active at this step.
        /// Other profile types return a passthrough engine (returns prior stop).
        /// </summary>
        /// <param name="profile">Trailing profile DTO.</param>
        /// <param name="tickSize">Instrument tick size (e.g., 0.25 for ES).</param>
        /// <returns>Non-loosening trailing engine.</returns>
        public static ITrailing Create(TrailingProfile profile, decimal tickSize)
        {
            if (profile.Type == TrailingProfileType.FixedTicks)
            {
                int ticks = (int)profile.Param1;
                double ts = (double)tickSize;
                ITrailingStop stop = new FixedTicksTrailingStop(ticks, ts);
                return new TrailingAdapter(stop);
            }

            return new PassthroughTrailing();
        }

        /// <summary>
        /// Passthrough trailing (identity) — returns the prior stop unchanged.
        /// </summary>
        private sealed class PassthroughTrailing : ITrailing
        {
            /// <inheritdoc/>
            public decimal ComputeStop(decimal entry, decimal current, bool isLong, TrailingProfile profile, decimal priorStop)
            {
                return priorStop;
            }
        }
    }
}
