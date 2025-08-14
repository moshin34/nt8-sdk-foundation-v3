using System;

namespace NT8.SDK.Config
{
    /// <summary>A single day’s metadata for a symbol: settlement window and blackout slices (ET).</summary>
    public class CmeDay
    {
        /// <summary>Date in yyyy-MM-dd (ET).</summary>
        public string Date { get; set; }
        /// <summary>Settlement window "HH:mm-HH:mm" (ET), e.g., "16:00-16:45".</summary>
        public string Settlement { get; set; }
        /// <summary>Zero or more blackout windows, each "HH:mm-HH:mm" (ET).</summary>
        public string[] Blackouts { get; set; }
    }

    /// <summary>Calendar entries for a single symbol (e.g., "NQ").</summary>
    public class CmeSymbol
    {
        /// <summary>Instrument symbol (e.g., "NQ", "ES").</summary>
        public string Symbol { get; set; }
        /// <summary>Day records for the symbol.</summary>
        public CmeDay[] Days { get; set; }
    }

    /// <summary>Top-level calendar data loaded from JSON.</summary>
    public class CmeCalendar
    {
        /// <summary>All symbols included in the seed.</summary>
        public CmeSymbol[] Symbols { get; set; }
    }
}
