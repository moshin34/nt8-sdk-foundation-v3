using System;

namespace NT8.SDK
{
    /// <summary>
    /// Computes non-loosening trailing stops (never widens the stop).
    /// </summary>
    public interface ITrailing
    {
        /// <summary>
        /// Compute the next stop level given entry/current prices, direction,
        /// profile, and the prior stop. The returned value MUST NOT be worse
        /// than <paramref name="priorStop"/> (i.e., non-loosening semantics).
        /// </summary>
        /// <param name="entry">Entry price.</param>
        /// <param name="current">Current price.</param>
        /// <param name="isLong">True if long; false if short.</param>
        /// <param name="profile">Trailing profile (type and parameters).</param>
        /// <param name="priorStop">Previously active stop level.</param>
        /// <returns>New stop level (price).</returns>
        decimal ComputeStop(decimal entry, decimal current, bool isLong, TrailingProfile profile, decimal priorStop);
    }
}
