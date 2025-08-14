using System;
using NT8.SDK;                 // SessionKey, IClock, SystemClock
using NT8.SDK.Config;          // CmeCalendar, CmeCalendarLoader

namespace NT8.SDK.Session
{
    /// <summary>
    /// Session/blackout logic backed by a simple CME calendar seed.
    /// - Assumes caller passes times in US/Eastern.
    /// - Uses an injected <see cref="IClock"/> (defaults to <see cref="SystemClock.Instance"/>)
    ///   for deterministic "today" anchoring of placeholder session open/close.
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

        public bool IsBlackout(DateTime etNow, string symbol)
        {
            var day = FindDay(etNow, symbol);
            if (day == null || day.Blackouts == null) return false;

            var tod = etNow.TimeOfDay;
            for (int i = 0; i < day.Blackouts.Length; i++)
            {
                TimeSpan s, e;
                if (CmeCalendarLoader.TryParseRange(day.Blackouts[i], out s, out e))
                {
                    if (CmeCalendarLoader.InRange(tod, s, e)) return true;
                }
            }
            return false;
        }

        public bool IsSettlementWindow(DateTime etNow, string symbol)
        {
            var day = FindDay(etNow, symbol);
            if (day == null) return false;

            TimeSpan s, e;
            if (!CmeCalendarLoader.TryParseRange(day.Settlement, out s, out e))
                return false;

            var tod = etNow.TimeOfDay;
            return CmeCalendarLoader.InRange(tod, s, e);
        }

        public DateTime SessionOpen(SessionKey key)
        {
            // Conservative placeholder RTH open (09:00 ET) using clock-anchored "today".
            var d = _clock.UtcNow; // caller passes ET elsewhere; here we only need a stable "day"
            return new DateTime(d.Year, d.Month, d.Day, 9, 0, 0);
        }

        public DateTime SessionClose(SessionKey key)
        {
            // Conservative placeholder RTH close (16:00 ET) using clock-anchored "today".
            var d = _clock.UtcNow;
            return new DateTime(d.Year, d.Month, d.Day, 16, 0, 0);
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
