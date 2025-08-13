using NT8.SDK;

namespace NT8.SDK.Sizing
{
    /// <summary>
    /// Defines a composable sizing rule that can attempt to produce a sizing decision.
    /// </summary>
    public interface ISizeRule
    {
        /// <summary>
        /// Attempts to decide a sizing outcome for the specified risk mode and intent.
        /// </summary>
        /// <param name="mode">The active risk mode.</param>
        /// <param name="intent">The position intent being evaluated.</param>
        /// <param name="decision">
        /// When this method returns, contains the decision made by the rule if successful; otherwise the default value.
        /// </param>
        /// <returns><c>true</c> if the rule produced a decision; otherwise <c>false</c>.</returns>
        bool TryDecide(RiskMode mode, PositionIntent intent, out SizeDecision decision);
    }
}

