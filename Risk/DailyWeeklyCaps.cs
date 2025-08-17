using System;
using System.Collections.Generic;

namespace NT8.SDK.Risk
{
    /// <summary>
    /// Tracks realized profit and loss and reports when daily or weekly
    /// loss caps have been breached.
    /// </summary>
    public sealed class DailyWeeklyCaps
    {
        private readonly Dictionary<DateTime, double> _dailyPnL;
        private readonly object _sync;

        /// <summary>Initializes a new instance of the <see cref="DailyWeeklyCaps"/> class.</summary>
        /// <param name="dailyLimit">Maximum loss allowed per day.</param>
        /// <param name="weeklyLimit">Maximum loss allowed per week.</param>
        public DailyWeeklyCaps(double dailyLimit, double weeklyLimit)
        {
            DailyLimit = dailyLimit;
            WeeklyLimit = weeklyLimit;
            _dailyPnL = new Dictionary<DateTime, double>();
            _sync = new object();
        }

        /// <summary>Gets the configured daily loss limit.</summary>
        public double DailyLimit { get; private set; }

        /// <summary>Gets the configured weekly loss limit.</summary>
        public double WeeklyLimit { get; private set; }

        /// <summary>
        /// Records a realized PnL entry for the specified date.
        /// </summary>
        /// <param name="realizedPnL">Amount realized.</param>
        /// <param name="dateTime">Time of the realization.</param>
        public void ApplyPnL(double realizedPnL, DateTime dateTime)
        {
            var day = dateTime.Date;
            lock (_sync)
            {
                double current;
                if (_dailyPnL.TryGetValue(day, out current))
                    _dailyPnL[day] = current + realizedPnL;
                else
                    _dailyPnL[day] = realizedPnL;
            }
        }

        /// <summary>
        /// Determines whether the daily loss limit has been breached for the provided date.
        /// </summary>
        /// <param name="dt">Date to check.</param>
        /// <returns>True if the daily loss limit has been exceeded.</returns>
        public bool IsDailyLocked(DateTime dt)
        {
            var day = dt.Date;
            lock (_sync)
            {
                double total;
                if (_dailyPnL.TryGetValue(day, out total))
                    return total <= -DailyLimit;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the weekly loss limit has been breached for the provided date.
        /// </summary>
        /// <param name="dt">Date to check.</param>
        /// <returns>True if the weekly loss limit has been exceeded.</returns>
        public bool IsWeeklyLocked(DateTime dt)
        {
            var start = GetWeekStart(dt.Date);
            var end = start.AddDays(7);
            double total = 0.0;

            lock (_sync)
            {
                foreach (var kv in _dailyPnL)
                {
                    if (kv.Key >= start && kv.Key < end)
                        total += kv.Value;
                }
            }

            return total <= -WeeklyLimit;
        }

        private static DateTime GetWeekStart(DateTime date)
        {
            int diff = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            return date.AddDays(-diff);
        }
    }
}

