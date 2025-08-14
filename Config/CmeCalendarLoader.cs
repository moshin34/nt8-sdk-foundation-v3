using System;
using System.IO;
using System.Web.Script.Serialization;

namespace NT8.SDK.Config
{
    /// <summary>Loads CME calendar JSON from disk and provides simple helpers.</summary>
    public static class CmeCalendarLoader
    {
        private static readonly object Sync = new object();
        private static CmeCalendar _cached;

        /// <summary>
        /// Loads the calendar from the given path (default ./seeds/cme_calendar.json).
        /// On any error returns an empty calendar (no throws).
        /// </summary>
        public static CmeCalendar Load(string path = "./seeds/cme_calendar.json")
        {
            lock (Sync)
            {
                if (_cached != null) return _cached;
                try
                {
                    if (!File.Exists(path))
                        return _cached = Empty();

                    var json = File.ReadAllText(path);
                    var ser = new JavaScriptSerializer();
                    var cal = ser.Deserialize<CmeCalendar>(json);
                    if (cal == null || cal.Symbols == null)
                        return _cached = Empty();

                    // Normalize nulls to empty arrays
                    for (int i = 0; i < cal.Symbols.Length; i++)
                    {
                        var s = cal.Symbols[i];
                        if (s == null) { cal.Symbols[i] = new CmeSymbol { Symbol = "", Days = new CmeDay[0] }; continue; }
                        if (s.Days == null) s.Days = new CmeDay[0];
                        for (int d = 0; d < s.Days.Length; d++)
                        {
                            var day = s.Days[d];
                            if (day == null) { s.Days[d] = new CmeDay { Date = "", Settlement = "", Blackouts = new string[0] }; continue; }
                            if (day.Blackouts == null) day.Blackouts = new string[0];
                            if (day.Settlement == null) day.Settlement = "";
                            if (day.Date == null) day.Date = "";
                        }
                    }
                    return _cached = cal;
                }
                catch
                {
                    return _cached = Empty();
                }
            }
        }

        /// <summary>Parses "HH:mm-HH:mm" into start/end TimeSpan. Returns false on failure.</summary>
        public static bool TryParseRange(string hhmmRange, out TimeSpan start, out TimeSpan end)
        {
            start = TimeSpan.Zero; end = TimeSpan.Zero;
            if (string.IsNullOrEmpty(hhmmRange)) return false;
            var parts = hhmmRange.Split('-');
            if (parts.Length != 2) return false;
            TimeSpan s, e;
            if (!TimeSpan.TryParse(parts[0], out s)) return false;
            if (!TimeSpan.TryParse(parts[1], out e)) return false;
            start = s; end = e;
            return true;
        }

        /// <summary>
        /// True if time-of-day t falls within [start,end], supporting wrap past midnight.
        /// </summary>
        public static bool InRange(TimeSpan t, TimeSpan start, TimeSpan end)
        {
            if (start <= end) return t >= start && t <= end;
            return t >= start || t <= end;
        }

        private static CmeCalendar Empty()
        {
            return new CmeCalendar { Symbols = new CmeSymbol[0] };
        }

#if DEBUG
        internal static bool InternalSelfTest()
        {
            try
            {
                var cal = Load();
                return cal != null && cal.Symbols != null;
            }
            catch { return false; }
        }
#endif
    }
}
