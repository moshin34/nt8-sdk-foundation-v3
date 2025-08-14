using System;
using System.Collections.Generic;
using NT8.SDK;         // for IClock, SystemClock
using NT8.SDK.Config;

namespace NT8.SDK.Session
{
    /// <summary>
    /// Session/blackout logic backed by a simple CME calendar seed.
    /// Assumes caller passes times in US/Eastern; clock is for deterministic UTC anchoring.
    /// </summary>
    public sealed class CmeBlackoutService : ISession
    {
        private readonly List<CmeCalendar> _calendars;
        private readonly IClock _clock;

        /// <summary>
        /// Initializes a new instance and loads the calendar seed.
        /// </summary>
        /// <param name="clock">Optional clock for deterministic timing; defaults to <see cref="SystemClock.Instance"/>.</param>
        public CmeBlackoutService(IClock clock = null)
        {
            _calendars = CmeCalendarLoader.LoadAll();
            _clock = clock ?? SystemClock.Instance;
        }

        public bool IsBlackout(DateTime etNow, string symbol)
        {
            var day = FindDay(etNow, symbol);
            if (day == null || day.Blackouts == null) return false;

            var tod = etNow.TimeOfDay;
            for (int i = 0; i < day.Blackouts.Count; i++)
            {
                TimeSpan s, e;
                if (TryParseRange(day.Blackouts[i], out s, out e))
                {
                    if (InRange(tod, s, e)) return true;
                }
            }
            return false;
        }

        public bool IsSettlementWindow(DateTime etNow, string symbol)
        {
            var day = FindDay(etNow, symbol);
            if (day == null) return false;

            TimeSpan s, e;
            if (!TryParseRange(day.Settlement, out s, out e))
                return false;

            var tod = etNow.TimeOfDay;
            return InRange(tod, s, e);
        }

        public DateTime SessionOpen(SessionKey key)
        {
            var d = _clock.UtcNow.Date; // deterministic anchor
            return new DateTime(d.Year, d.Month, d.Day, 9, 0, 0);
        }

        public DateTime SessionClose(SessionKey key)
        {
            var d = _clock.UtcNow.Date; // deterministic anchor
            return new DateTime(d.Year, d.Month, d.Day, 16, 0, 0);
        }

        private CmeCalendarDay FindDay(DateTime etNow, string symbol)
        {
            if (_calendars == null || string.IsNullOrEmpty(symbol))
                return null;

            var date = etNow.ToString("yyyy-MM-dd");
            for (int i = 0; i < _calendars.Count; i++)
            {
                var cal = _calendars[i];
                if (cal == null || string.IsNullOrEmpty(cal.Symbol) || cal.Days == null) continue;
                if (!string.Equals(cal.Symbol, symbol, StringComparison.OrdinalIgnoreCase)) continue;

                for (int d = 0; d < cal.Days.Count; d++)
                {
                    var day = cal.Days[d];
                    if (day == null || string.IsNullOrEmpty(day.Date)) continue;
                    if (day.Date == date) return day;
                }
            }
            return null;
        }

        private static bool TryParseRange(string range, out TimeSpan start, out TimeSpan end)
        {
            start = TimeSpan.Zero;
            end = TimeSpan.Zero;
            if (string.IsNullOrEmpty(range)) return false;
            var parts = range.Split('-');
            if (parts.Length != 2) return false;
            return TimeSpan.TryParse(parts[0], out start) && TimeSpan.TryParse(parts[1], out end);
        }

        private static bool InRange(TimeSpan tod, TimeSpan start, TimeSpan end)
        {
            if (start <= end)
            {
                return tod >= start && tod <= end;
            }
            return tod >= start || tod <= end;
        }
    }
}
