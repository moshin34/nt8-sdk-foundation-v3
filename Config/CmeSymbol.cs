using System;

namespace NT8.SDK.Config
{
    /// <summary>Calendar entries for a single symbol (e.g., "NQ").</summary>
    public class CmeSymbol
    {
        /// <summary>Instrument symbol (e.g., "NQ", "ES").</summary>
        public string Symbol { get; set; }
        /// <summary>Day records for the symbol.</summary>
        public CmeDay[] Days { get; set; }
    }
}
