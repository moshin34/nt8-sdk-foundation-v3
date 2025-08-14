using System;

namespace NT8.SDK
{
    /// <summary>Computes non-loosening trailing stops.</summary>
    public interface ITrailing
    {
        /// <summary>
        /// Compute the next stop level; must never be worse than <paramref name="priorStop"/>.
        /// Profile kind is in <see cref="TrailingProfile.Type"/>.
        /// </summary>
        decimal ComputeStop(decimal entry, decimal current, bool isLong, TrailingProfile profile, decimal priorStop);
    }
}
