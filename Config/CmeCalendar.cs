using System;

namespace NT8.SDK.Config
{
    /// <summary>Top-level calendar data loaded from JSON.</summary>
    public class CmeCalendar
    {
        /// <summary>All symbols included in the seed.</summary>
        public CmeSymbol[] Symbols { get; set; }
    }
}
