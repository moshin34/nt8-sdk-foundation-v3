using System;
using System.Collections.Generic;

namespace NT8.SDK.Config
{
    /// <summary>
    /// Placeholder for CME calendar day data used during compilation.
    /// </summary>
    public class CmeCalendarDay
    {
        /// <summary>Date string in yyyy-MM-dd.</summary>
        public string Date { get; set; }

        /// <summary>Blackout windows.</summary>
        public List<string> Blackouts { get; set; }

        /// <summary>Settlement window.</summary>
        public string Settlement { get; set; }
    }

    /// <summary>
    /// Placeholder calendar root object.
    /// </summary>
    public class CmeCalendar
    {
        /// <summary>Symbol for the calendar.</summary>
        public string Symbol { get; set; }

        /// <summary>Calendar days.</summary>
        public List<CmeCalendarDay> Days { get; set; }

        /// <summary>Initializes a new instance of the <see cref="CmeCalendar"/> class.</summary>
        public CmeCalendar()
        {
            Days = new List<CmeCalendarDay>();
        }
    }

    /// <summary>
    /// Minimal loader returning empty calendars.
    /// </summary>
    public static class CmeCalendarLoader
    {
        /// <summary>Loads calendar data for the symbol.</summary>
        public static CmeCalendar Load(string symbol)
        {
            CmeCalendar cal = new CmeCalendar();
            cal.Symbol = symbol;
            return cal;
        }
    }
}
