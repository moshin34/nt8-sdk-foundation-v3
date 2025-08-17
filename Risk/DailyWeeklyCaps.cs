namespace NT8.SDK.Risk
{
    /// <summary>
    /// Simple risk utility that tracks daily and weekly loss caps.
    /// </summary>
    public sealed class DailyWeeklyCaps
    {
        private readonly double _dailyCap;
        private readonly double _weeklyCap;

        /// <summary>Initializes a new instance of the <see cref="DailyWeeklyCaps"/> class.</summary>
        /// <param name="dailyCap">Maximum allowed loss for the current day.</param>
        /// <param name="weeklyCap">Maximum allowed loss for the current week.</param>
        public DailyWeeklyCaps(double dailyCap, double weeklyCap)
        {
            _dailyCap = dailyCap;
            _weeklyCap = weeklyCap;
        }

        /// <summary>True if today's P&amp;L breaches the daily loss cap.</summary>
        public bool IsDailyLimitBreached(double todayPnL)
        {
            return _dailyCap > 0 && todayPnL <= -_dailyCap;
        }

        /// <summary>True if weekly P&amp;L breaches the weekly loss cap.</summary>
        public bool IsWeeklyLimitBreached(double weeklyPnL)
        {
            return _weeklyCap > 0 && weeklyPnL <= -_weeklyCap;
        }

        /// <summary>True if either the daily or weekly limit has been breached.</summary>
        public bool IsLocked(double todayPnL, double weeklyPnL)
        {
            return IsDailyLimitBreached(todayPnL) || IsWeeklyLimitBreached(weeklyPnL);
        }
    }
}
