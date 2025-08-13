using System;
using System.Collections.Generic;

namespace NT8.SDK.Config
{
    /// <summary>
    /// CME calendar for a given symbol.
    /// </summary>
    [Serializable]
    public class CmeCalendar
    {
        /// <summary>Instrument symbol (e.g., "NQ").</summary>
        public string Symbol { get; set; }

        /// <summary>Per-day schedule for the symbol.</summary>
        public List<CmeCalendarDay> Days { get; set; }

        /// <summary>Initializes with non-null collections.</summary>
        public CmeCalendar()
        {
            Days = new List<CmeCalendarDay>();
        }
    }
}
