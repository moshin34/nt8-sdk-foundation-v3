using System;

namespace NT8.SDK.Config
{
    /// <summary>Trading-day flags used by Session layer (CME).</summary>
    public sealed class CmeCalendarDay
    {
        public DateTime Date { get; set; }
        public bool IsHoliday { get; set; }
        public bool IsEarlyClose { get; set; }

        // Optional headroom fields in case Session references them:
        public int EarlyCloseMinutes { get; set; }
        public string Notes { get; set; }
    }
}
