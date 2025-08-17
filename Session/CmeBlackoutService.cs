using System;
using System.Collections.Generic;
using System.IO;               // File, Directory
using System.Web.Script.Serialization; // JavaScriptSerializer
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

        private static readonly HashSet<string> SupportedBaseSymbols = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ES", "NQ", "CL", "GC"
        };

        public CmeBlackoutService()
            : this(null)
        {
        }

        /// <summary>Creates a new instance, loading the seed once and wiring the clock.</summary>
        public CmeBlackoutService(IClock clock)
        {
            _clock = clock ?? SystemClock.Instance;
            _calendar = LoadCalendar(); // reads seeds/cme_calendar_*.json
        }

        private static CmeCalendar LoadCalendar()
        {
            var symbols = new Dictionary<string, Dictionary<string, CmeDay>>(StringComparer.OrdinalIgnoreCase);
            var ser = new JavaScriptSerializer();
            try
            {
                var files = Directory.GetFiles("./seeds", "cme_calendar_*.json");
                for (int i = 0; i < files.Length; i++)
                {
                    var json = File.ReadAllText(files[i]);
                    if (string.IsNullOrWhiteSpace(json)) continue;
                    if (json.TrimStart().StartsWith("{"))
                    {
                        var cal = ser.Deserialize<CmeCalendar>(json);
                        if (cal?.Symbols != null) Merge(cal.Symbols);
                    }
                    else
                    {
                        var calSymbols = ser.Deserialize<CmeSymbol[]>(json);
                        if (calSymbols != null) Merge(calSymbols);
                    }
                }
            }
            catch
            {
                // ignore any single seed failure
            }

            var list = new List<CmeSymbol>();
            foreach (var kv in symbols)
            {
                var days = new List<CmeDay>(kv.Value.Values);
                list.Add(new CmeSymbol { Symbol = kv.Key, Days = days.ToArray() });
            }
            return new CmeCalendar { Symbols = list.ToArray() };

            void Merge(IEnumerable<CmeSymbol> src)
            {
                foreach (var sym in src)
                {
                    if (sym == null || string.IsNullOrEmpty(sym.Symbol) || sym.Days == null) continue;
                    if (!SupportedBaseSymbols.Contains(sym.Symbol)) continue;
                    Dictionary<string, CmeDay> dayMap;
                    if (!symbols.TryGetValue(sym.Symbol, out dayMap))
                    {
                        dayMap = new Dictionary<string, CmeDay>(StringComparer.OrdinalIgnoreCase);
                        symbols[sym.Symbol] = dayMap;
                    }
                    for (int d = 0; d < sym.Days.Length; d++)
                    {
                        var day = sym.Days[d];
                        if (day == null || string.IsNullOrEmpty(day.Date)) continue;
                        dayMap[day.Date] = day; // override duplicates
                    }
                }
            }
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

        public bool IsBlackout(string symbol, DateTime now)
        {
            return IsWithin(now, BlackoutRanges(now, symbol));
        }

        public bool IsForceFlat(string symbol, DateTime now)
        {
            var range = SettlementRange(now, symbol);
            return range.HasValue && range.Value.Contains(now.TimeOfDay);
        }

        // Legacy interface contracts
        public bool IsBlackout(DateTime etNow, string symbol)
        {
            return IsBlackout(symbol, etNow);
        }

        public bool IsSettlementWindow(DateTime etNow, string symbol)
        {
            return IsForceFlat(symbol, etNow);
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

        private static string NormalizeSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol)) return symbol;
            var up = symbol.ToUpperInvariant();
            switch (up)
            {
                case "MNQ": return "NQ";
                case "MES": return "ES";
                default: return up;
            }
        }

        private CmeDay FindDay(DateTime etNow, string symbol)
        {
            symbol = NormalizeSymbol(symbol);
            if (_calendar == null || _calendar.Symbols == null || string.IsNullOrEmpty(symbol) || !SupportedBaseSymbols.Contains(symbol))
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

