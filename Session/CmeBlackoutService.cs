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

        public CmeBlackoutService()
        {
            _calendar = CmeCalendarLoader.Load();
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
            var d = etNowToday();
            return new DateTime(d.Year, d.Month, d.Day, 9, 0, 0);
        }

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
