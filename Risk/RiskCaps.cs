using System;

namespace NT8.SDK.Risk
{
    /// <summary>
    /// Aggregates multiple risk cap checks (daily, weekly, trailing) and
    /// reports when trading should be locked out.
    /// </summary>
    public class RiskCaps
    {
        private readonly DailyWeeklyCaps _dailyWeekly;
        private readonly TrailingDrawdown _trailing;
        private double _netEquity;

        /// <summary>
        /// Initializes a new instance of the <see cref="RiskCaps"/> class.
        /// </summary>
        /// <param name="dailyLimit">Maximum loss allowed per day.</param>
        /// <param name="weeklyLimit">Maximum loss allowed per week.</param>
        /// <param name="trailingLimit">Maximum allowed trailing drawdown.</param>
        /// <param name="initialEquity">Initial account equity.</param>
        public RiskCaps(double dailyLimit, double weeklyLimit, double trailingLimit, double initialEquity)
        {
            _dailyWeekly = new DailyWeeklyCaps(dailyLimit, weeklyLimit);
            _trailing = trailingLimit > 0 ? new TrailingDrawdown(initialEquity, trailingLimit) : null;
            _netEquity = initialEquity;
        }

        /// <summary>
        /// Applies realized PnL and updates internal caps.
        /// </summary>
        /// <param name="realizedPnL">The realized profit or loss.</param>
        /// <param name="when">Time the PnL was realized.</param>
        /// <param name="netEquity">Current net equity value.</param>
        public void ApplyPnL(double realizedPnL, DateTime when, double netEquity)
        {
            _dailyWeekly?.ApplyPnL(realizedPnL, when);
            _netEquity = netEquity;
            _trailing?.ApplyPnL(netEquity);
        }

        /// <summary>
        /// Determines whether any configured cap has been breached.
        /// </summary>
        /// <returns>True if a lockout is active; otherwise, false.</returns>
        public bool IsLocked()
        {
            var now = DateTime.UtcNow;
            if (_dailyWeekly != null && (_dailyWeekly.IsDailyLocked(now) || _dailyWeekly.IsWeeklyLocked(now)))
                return true;
            if (_trailing != null && _trailing.IsLocked(_netEquity))
                return true;
            return false;
        }
    }
}

