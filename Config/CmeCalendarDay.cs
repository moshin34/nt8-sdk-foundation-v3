using System;
using System.Collections.Generic;

namespace NT8.SDK.Config
{
    /// <summary>
    /// Calendar day definition with settlement and blackout windows.
    /// Matches seed JSON shape:
    /// { "date":"YYYY-MM-DD", "settlement":"HH:mm-HH:mm", "blackouts":["HH:mm-HH:mm"] }
    /// </summary>
    [Serializable]
    public class CmeCalendarDay
    {
        /// <summary>Trading date in YYYY-MM-DD (ET).</summary>
        public string Date { get; set; }

        /// <summary>Settlement window as "HH:mm-HH:mm" (ET). Optional.</summary>
        public string Settlement { get; set; }

        /// <summary>Blackout windows as "HH:mm-HH:mm" (ET). Optional.</summary>
        public List<string> Blackouts { get; set; }
    }
}
