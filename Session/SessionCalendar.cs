using System;
using System.Collections.Generic;
using NT8.SDK;
using NT8.SDK.Config;

namespace NT8.SDK.Session
{
    /// <summary>
    /// Normalized view over CME seed data for session-related queries.
    /// Wraps <see cref="CmeCalendarLoader"/> and exposes simple lookups.
    /// </summary>
    public sealed class SessionCalendar
    {
        private readonly Dictionary<string, CmeCalendar> _cache;
        private readonly Func<string, CmeCalendar> _load;

        /// <summary>
        /// Initializes a new <see cref="SessionCalendar"/> with the given loader.
        /// </summary>
        /// <param name="loader">Loader function; defaults to <see cref="CmeCalendarLoader.Load(string)"/>.</param>
        public SessionCalendar(Func<string, CmeCalendar> loader = null)
        {
            _cache = new Dictionary<string, CmeCalendar>(StringComparer.OrdinalIgnoreCase);
            _load = loader ?? CmeCalendarLoader.Load;
        }

        /// <summary>Returns true if the ET time falls within any configured blackout window for the symbol.</summary>
        public bool IsBlackout(DateTime et, string symbol)
        {
            var day = FindDay(symbol, et.Date);
            if (day == null || day.Blackouts == null) return false;
            for (int i = 0; i < day.Blackouts.Count; i++)
            {
                if (TimeFilter.InRange(et, day.Blackouts[i])) return true;
            }
            return false;
        }

        /// <summary>Returns true if the ET time is within the settlement window for the symbol.</summary>
        public bool IsSettlement(DateTime et, string symbol)
        {
            var day = FindDay(symbol, et.Date);
            if (day == null || string.IsNullOrEmpty(day.Settlement)) return false;
            return TimeFilter.InRange(et, day.Settlement);
        }

        /// <summary>Returns blackout windows for the ET date (may be empty).</summary>
        public string[] BlackoutsFor(string symbol, DateTime etDate)
        {
            var day = FindDay(symbol, etDate.Date);
            if (day == null || day.Blackouts == null) return new string[0];
            return day.Blackouts.ToArray();
        }

        /// <summary>Returns the settlement window for the ET date, or empty string.</summary>
        public string SettlementFor(string symbol, DateTime etDate)
        {
            var day = FindDay(symbol, etDate.Date);
            return day != null ? (day.Settlement ?? string.Empty) : string.Empty;
        }

        /// <summary>
        /// Placeholder session open time for the key. Returns <see cref="DateTime.MinValue"/> when unknown.
        /// (Real session opens may be provided by future profile logic.)
        /// </summary>
        public DateTime SessionOpen(SessionKey key) { return DateTime.MinValue; }

        /// <summary>
        /// Placeholder session close time for the key. Returns <see cref="DateTime.MaxValue"/> when unknown.
        /// </summary>
        public DateTime SessionClose(SessionKey key) { return DateTime.MaxValue; }

        private CmeCalendarDay FindDay(string symbol, DateTime etDate)
        {
            var cal = Get(symbol);
            if (cal == null || cal.Days == null) return null;
            for (int i = 0; i < cal.Days.Count; i++)
            {
                var d = cal.Days[i];
                if (d == null || string.IsNullOrEmpty(d.Date)) continue;
                int yy, mm, dd;
                var parts = d.Date.Split('-');
                if (parts.Length != 3) continue;
                if (!int.TryParse(parts[0], out yy)) continue;
                if (!int.TryParse(parts[1], out mm)) continue;
                if (!int.TryParse(parts[2], out dd)) continue;
                var dt = new DateTime(yy, mm, dd);
                if (dt.Date == etDate.Date) return d;
            }
            return null;
        }

        private CmeCalendar Get(string symbol)
        {
            CmeCalendar cal;
            if (_cache.TryGetValue(symbol, out cal)) return cal;
            cal = _load != null ? _load(symbol) : null;
            if (cal == null) cal = new CmeCalendar { Symbol = symbol, Days = new List<CmeCalendarDay>() };
            if (cal.Days == null) cal.Days = new List<CmeCalendarDay>();
            _cache[symbol] = cal;
            return cal;
        }
    }
}

