using System;

namespace NT8.SDK.Config
{
    /// <summary>A single day’s metadata for a symbol: settlement and blackout slices (ET).</summary>
    public class CmeDay
    {
        /// <summary>Date in yyyy-MM-dd (ET).</summary>
        public string Date { get; set; }
        /// <summary>Settlement window "HH:mm-HH:mm" (ET), e.g., "16:00-16:45".</summary>
        public string Settlement { get; set; }
        /// <summary>Zero or more blackout windows, each "HH:mm-HH:mm" (ET).</summary>
        public string[] Blackouts { get; set; }
    }
}
