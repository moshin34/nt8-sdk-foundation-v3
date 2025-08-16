using System;
using System.Collections.Generic;
using NT8.SDK;                 // SessionKey, IClock, SystemClock
using NT8.SDK.Config;          // CmeCalendar, CmeCalendarLoader

namespace NT8.SDK.Session
{
    /// <summary>
    /// Session/blackout logic backed by a simple CME calendar seed.
    /// - Assumes caller passes times in US/Eastern.
    /// - Uses an injected <see cref="IClock"/> (defaults to <see cref="SystemClock.Instance"/>)
    ///   for deterministic "today" anchoring of placeholder session open/close.
    /// Provides parsed time ranges for settlement and blackouts.
    /// </summary>
    public sealed class CmeBlackoutService : ISession
    {
        private readonly IClock _clock;
        private readonly CmeCalendar _calendar;

        public CmeBlackoutService()
            : this(null)
        {
        }

        /// <summary>Creates a new instance, loading the seed once and wiring the clock.</summary>
        public CmeBlackoutService(IClock clock)
        {
            _clock = clock ?? SystemClock.Instance;
            _calendar = CmeCalendarLoader.Load(); // never throws; normalized arrays
        }

        /// <summary>Returns blackout windows for the ET date.</summary>
        public TimeRange[] BlackoutRanges(DateTime etNow, string symbol)
        {
            var day = FindDay(etNow, symbol);
            if (day == null || day.Blackouts == null) return new TimeRange[0];

            var list = new List<TimeRange>();
            for (int i = 0; i < day.Blackouts.Length; i++)
            {
                TimeSpan s, e;
                if (CmeCalendarLoader.TryParseRange(day.Blackouts[i], out s, out e))
                    list.Add(new TimeRange(s, e));
            }
            return list.ToArray();
        }

        /// <summary>Returns the settlement window for the ET date, or null when none.</summary>
        public TimeRange? SettlementRange(DateTime etNow, string symbol)
        {
            var day = FindDay(etNow, symbol);
            if (day == null) return null;

            TimeSpan s, e;
            if (!CmeCalendarLoader.TryParseRange(day.Settlement, out s, out e))
                return null;

            return new TimeRange(s, e);
        }

        public bool IsBlackout(DateTime etNow, string symbol)
        {
            return IsWithin(etNow, BlackoutRanges(etNow, symbol));
        }

        public bool IsSettlementWindow(DateTime etNow, string symbol)
        {
            var range = SettlementRange(etNow, symbol);
            return range.HasValue && range.Value.Contains(etNow.TimeOfDay);
        }

        /// <summary>
        /// Returns true if <paramref name="etTime"/> falls within any of the ranges.
        /// </summary>
        internal static bool IsWithin(DateTime etTime, TimeRange[] ranges)
        {
            var t = etTime.TimeOfDay;
            for (int i = 0; i < ranges.Length; i++)
            {
                if (ranges[i].Contains(t)) return true;
            }
            return false;
        }

        public DateTime SessionOpen(SessionKey key)
        {
            // Conservative placeholder RTH open (09:00 ET) using clock-anchored "today".
            var d = _clock.UtcNow; // caller passes ET elsewhere; here we only need a stable "day"
            return new DateTime(d.Year, d.Month, d.Day, 9, 0, 0); // TODO VERIFY CME
        }

        public DateTime SessionClose(SessionKey key)
        {
            // Conservative placeholder RTH close (16:00 ET) using clock-anchored "today".
            var d = _clock.UtcNow;
            return new DateTime(d.Year, d.Month, d.Day, 16, 0, 0); // TODO VERIFY CME
        }

        private CmeDay FindDay(DateTime etNow, string symbol)
        {
            if (_calendar == null || _calendar.Symbols == null || string.IsNullOrEmpty(symbol))
                return null;

            var date = etNow.ToString("yyyy-MM-dd"); // ET date string
            for (int i = 0; i < _calendar.Symbols.Length; i++)
            {
                var s = _calendar.Symbols[i];
                if (s == null || string.IsNullOrEmpty(s.Symbol) || s.Days == null) continue;
                if (!string.Equals(s.Symbol, symbol, StringComparison.OrdinalIgnoreCase)) continue;

                for (int d = 0; d < s.Days.Length; d++)
                {
                    var day = s.Days[d];
                    if (day == null || string.IsNullOrEmpty(day.Date)) continue;
                    if (day.Date == date) return day;
                }
            }
            return null;
        }
    }
}

