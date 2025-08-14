using System;
using NT8.SDK.Config;

namespace NT8.SDK.Session
{
    /// <summary>
    /// Session/blackout logic backed by a simple CME calendar seed.
    /// Assumes caller passes times in US/Eastern.
    /// </summary>
    public sealed class CmeBlackoutService : ISession
    {
        private readonly CmeCalendar _calendar;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmeBlackoutService"/> class and loads the calendar seed.
        /// </summary>
        public CmeBlackoutService()
        {
            _calendar = CmeCalendarLoader.Load();
        }

        /// <summary>
        /// Returns <c>true</c> if the provided ET timestamp falls within any configured blackout range for the symbol.
        /// </summary>
        /// <param name="etNow">Current time in US/Eastern.</param>
        /// <param name="symbol">Instrument symbol (e.g., "NQ").</param>
        /// <returns><c>true</c> if in a blackout range; otherwise <c>false</c>.</returns>
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

        /// <summary>
        /// Returns <c>true</c> if the provided ET timestamp falls within the configured settlement window for the symbol.
        /// </summary>
        /// <param name="etNow">Current time in US/Eastern.</param>
        /// <param name="symbol">Instrument symbol (e.g., "NQ").</param>
        /// <returns><c>true</c> if in the settlement window; otherwise <c>false</c>.</returns>
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

        /// <summary>
        /// Returns a conservative placeholder RTH session open time (09:00 ET) for the provided key’s date.
        /// </summary>
        /// <param name="key">Session key (symbol + named session).</param>
        /// <returns>Session open time in ET.</returns>
        public DateTime SessionOpen(SessionKey key)
        {
            var d = etNowToday();
            return new DateTime(d.Year, d.Month, d.Day, 9, 0, 0);
        }

        /// <summary>
        /// Returns a conservative placeholder RTH session close time (16:00 ET) for the provided key’s date.
        /// </summary>
        /// <param name="key">Session key (symbol + named session).</param>
        /// <returns>Session close time in ET.</returns>
        public DateTime SessionClose(SessionKey key)
        {
            var d = etNowToday();
            return new DateTime(d.Year, d.Month, d.Day, 16, 0, 0);
        }

        private static DateTime etNowToday()
        {
            return DateTime.Now.Date;
        }

        private CmeDay FindDay(DateTime etNow, string symbol)
        {
            if (_calendar == null || _calendar.Symbols == null || string.IsNullOrEmpty(symbol))
                return null;

            var date = etNow.ToString("yyyy-MM-dd");
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
