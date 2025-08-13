using System;
using System.Collections.Generic;
using NT8.SDK.Config;

namespace NT8.SDK.Session
{
    /// <summary>
    /// ISession implementation backed by CME calendar seed data (US/Eastern).
    /// </summary>
    public class CmeBlackoutService : ISession
    {
        private readonly Dictionary<string, CmeCalendar> _cache =
            new Dictionary<string, CmeCalendar>(StringComparer.OrdinalIgnoreCase);

        private CmeCalendar GetCalendar(string symbol)
        {
            CmeCalendar cal;
            if (!_cache.TryGetValue(symbol, out cal))
            {
                cal = CmeCalendarLoader.Load(symbol) ?? new CmeCalendar { Symbol = symbol };
                if (cal.Days == null) cal.Days = new List<CmeCalendarDay>();
                _cache[symbol] = cal;
            }
            return cal;
        }

        /// <summary>
        /// Returns true if <paramref name="etNow"/> falls within any configured blackout range for the symbol.
        /// </summary>
        public bool IsBlackout(DateTime etNow, string symbol)
        {
            var cal = GetCalendar(symbol);
            var days = cal.Days ?? new List<CmeCalendarDay>();
            for (int i = 0; i < days.Count; i++)
            {
                var day = days[i];
                if (day == null) continue;
                if (IsSameDay(day.Date, etNow))
                {
                    var blackouts = day.Blackouts ?? new List<string>();
                    for (int j = 0; j < blackouts.Count; j++)
                    {
                        if (IsInRange(etNow, blackouts[j])) return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if <paramref name="etNow"/> is within the configured settlement window for the symbol.
        /// </summary>
        public bool IsSettlementWindow(DateTime etNow, string symbol)
        {
            var cal = GetCalendar(symbol);
            var days = cal.Days ?? new List<CmeCalendarDay>();
            for (int i = 0; i < days.Count; i++)
            {
                var day = days[i];
                if (day == null) continue;
                if (IsSameDay(day.Date, etNow) && !string.IsNullOrEmpty(day.Settlement))
                {
                    if (IsInRange(etNow, day.Settlement)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the session open for the key, or <see cref="DateTime.MinValue"/> when unknown.
        /// </summary>
        public DateTime SessionOpen(SessionKey key)
        {
            // Safe default for Step 2; refined in later steps.
            return DateTime.MinValue;
        }

        /// <summary>
        /// Returns the session close for the key, or <see cref="DateTime.MaxValue"/> when unknown.
        /// </summary>
        public DateTime SessionClose(SessionKey key)
        {
            // Safe default for Step 2; refined in later steps.
            return DateTime.MaxValue;
        }

        /// <summary>
        /// True if the given ET time falls within the "HH:mm-HH:mm" range.
        /// Handles windows that cross midnight (e.g., "23:55-00:10").
        /// </summary>
        private static bool IsInRange(DateTime etNow, string range)
        {
            if (string.IsNullOrEmpty(range)) return false;
            var parts = range.Split('-');
            if (parts.Length != 2) return false;

            TimeSpan start, end;
            if (!TimeSpan.TryParse(parts[0], out start)) return false;
            if (!TimeSpan.TryParse(parts[1], out end)) return false;

            var t = etNow.TimeOfDay;
            if (start <= end)
            {
                // normal same-day window
                return t >= start && t <= end;
            }
            else
            {
                // window wraps past midnight
                return (t >= start) || (t <= end);
            }
        }

        /// <summary>
        /// Compares a seed date string (YYYY-MM-DD) to the ET date of <paramref name="etNow"/>.
        /// </summary>
        private static bool IsSameDay(string yyyyMmDd, DateTime etNow)
        {
            if (string.IsNullOrEmpty(yyyyMmDd)) return false;
            int y, m, d;
            var parts = yyyyMmDd.Split('-');
            if (parts.Length != 3) return false;
            if (!int.TryParse(parts[0], out y)) return false;
            if (!int.TryParse(parts[1], out m)) return false;
            if (!int.TryParse(parts[2], out d)) return false;

            var date = new DateTime(y, m, d);
            return date.Date == etNow.Date;
        }
    }
}

