using System;
using System.Collections.Generic;
using NT8.SDK;            // SessionKey
using NT8.SDK.Config;     // CmeCalendar, CmeSymbol, CmeDay, CmeCalendarLoader

namespace NT8.SDK.Session
{
    /// <summary>
    /// Normalized view over CME seed data for session-related queries.
    /// Wraps <see cref="CmeCalendarLoader"/> and exposes simple lookups.
    /// </summary>
    public sealed class SessionCalendar
    {
        // Cache by symbol -> CmeSymbol (from Config model)
        private readonly Dictionary<string, CmeSymbol> _cache;
        // Loader returns the top-level calendar (which contains Symbols[])
        private readonly Func<string, CmeCalendar> _load;

        /// <summary>
        /// Initializes a new <see cref="SessionCalendar"/> with the given loader.
        /// </summary>
        /// <param name="loader">Loader function; defaults to <see cref="CmeCalendarLoader.Load(string)"/>.</param>
        public SessionCalendar(Func<string, CmeCalendar> loader = null)
        {
            _cache = new Dictionary<string, CmeSymbol>(StringComparer.OrdinalIgnoreCase);
            _load = loader ?? CmeCalendarLoader.Load;
        }

        /// <summary>Returns true if the ET time falls within any configured blackout window for the symbol.</summary>
        public bool IsBlackout(DateTime et, string symbol)
        {
            var day = FindDay(symbol, et.Date);
            if (day == null || day.Blackouts == null) return false;

            for (int i = 0; i < day.Blackouts.Length; i++)
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
            return day?.Blackouts ?? Array.Empty<string>();
        }

        /// <summary>Returns the settlement window for the ET date, or empty string.</summary>
        public string SettlementFor(string symbol, DateTime etDate)
        {
            var day = FindDay(symbol, etDate.Date);
            return day?.Settlement ?? string.Empty;
        }

        /// <summary>
        /// Placeholder session open time for the key. Returns <see cref="DateTime.MinValue"/> when unknown.
        /// (Real session opens may be provided by future profile logic.)
        /// </summary>
        public DateTime SessionOpen(SessionKey _) => DateTime.MinValue;   // IDE0060: underscore means intentional discard

        /// <summary>
        /// Placeholder session close time for the key. Returns <see cref="DateTime.MaxValue"/> when unknown.
        /// </summary>
        public DateTime SessionClose(SessionKey _) => DateTime.MaxValue;  // IDE0060: underscore means intentional discard

        // ---------- internals ----------

        private CmeDay FindDay(string symbol, DateTime etDate)
        {
            var sym = GetSymbol(symbol);
            if (sym == null || sym.Days == null) return null;

            for (int i = 0; i < sym.Days.Length; i++)
            {
                var d = sym.Days[i];
                if (d == null || string.IsNullOrEmpty(d.Date)) continue;

                // Expect "yyyy-MM-dd"
                var parts = d.Date.Split('-');
                if (parts.Length != 3) continue;

                if (!int.TryParse(parts[0], out var yy)) continue;
                if (!int.TryParse(parts[1], out var mm)) continue;
                if (!int.TryParse(parts[2], out var dd)) continue;

                var dt = new DateTime(yy, mm, dd);
                if (dt.Date == etDate.Date) return d;
            }

            return null;
        }

        private CmeSymbol GetSymbol(string symbol)
        {
            if (_cache.TryGetValue(symbol, out var cached))
                return cached;

            // Load the calendar for this symbol (loader returns top-level CmeCalendar)
            var cal = _load != null ? _load(symbol) : null;

            CmeSymbol found = null;
            if (cal != null && cal.Symbols != null)
            {
                for (int i = 0; i < cal.Symbols.Length; i++)
                {
                    var s = cal.Symbols[i];
                    if (s != null && !string.IsNullOrEmpty(s.Symbol) &&
                        s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase))
                    {
                        found = s;
                        break;
                    }
                }
            }

            // If not found, park an empty placeholder so callers still work
            if (found == null)
                found = new CmeSymbol { Symbol = symbol, Days = Array.Empty<CmeDay>() };

            if (found.Days == null)
                found.Days = Array.Empty<CmeDay>();

            _cache[symbol] = found;
            return found;
        }
    }
}
