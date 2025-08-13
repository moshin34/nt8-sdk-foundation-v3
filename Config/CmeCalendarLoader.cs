using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace NT8.SDK.Config
{
    /// <summary>
    /// Loads CME calendar data from a seed file if present, else returns safe defaults.
    /// </summary>
    public static class CmeCalendarLoader
    {
        private const string SeedFile = "./seeds/cme_calendar_2025-08_to_2026-07.json";

        /// <summary>
        /// Loads calendars for all symbols. Never throws; returns empty list on failure.
        /// </summary>
        public static List<CmeCalendar> LoadAll()
        {
            try
            {
                if (File.Exists(SeedFile))
                {
                    var serializer = new JavaScriptSerializer();
                    var json = File.ReadAllText(SeedFile);
                    var list = serializer.Deserialize<List<CmeCalendar>>(json);
                    if (list != null)
                    {
                        // Ensure non-null lists on each calendar/day
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i] == null) { list[i] = new CmeCalendar(); continue; }
                            if (list[i].Days == null) list[i].Days = new List<CmeCalendarDay>();
                            for (int d = 0; d < list[i].Days.Count; d++)
                            {
                                var day = list[i].Days[d];
                                if (day != null && day.Blackouts == null) day.Blackouts = new List<string>();
                            }
                        }
                        return list;
                    }
                }
            }
            catch
            {
                // ignore malformed/missing seed
            }
            return new List<CmeCalendar>();
        }

        /// <summary>
        /// Loads calendar for a specific symbol; never throws. Returns safe defaults when missing.
        /// </summary>
        public static CmeCalendar Load(string symbol)
        {
            var calendars = LoadAll();
            for (int i = 0; i < calendars.Count; i++)
            {
                var cal = calendars[i];
                if (cal != null && string.Equals(cal.Symbol, symbol, StringComparison.OrdinalIgnoreCase))
                    return cal;
            }
            return new CmeCalendar { Symbol = symbol };
        }
    }
}

