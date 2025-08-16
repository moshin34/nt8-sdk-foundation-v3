using System;

namespace NT8.SDK.Session
{
    /// <summary>
    /// Evaluates ET time-of-day windows specified as "HH:mm-HH:mm", including wrap past midnight.
    /// </summary>
    public sealed class TimeFilter
    {
        /// <summary>
        /// Returns true if <paramref name="etNow"/> falls within the specified "HH:mm-HH:mm" range.
        /// Handles windows that cross midnight (e.g., "23:55-00:10").
        /// Returns false for null/empty/malformed ranges.
        /// </summary>
        public static bool InRange(DateTime etNow, string range)
        {
            if (string.IsNullOrEmpty(range)) return false;
            var parts = range.Split('-');
            if (parts.Length != 2) return false;

            TimeSpan start, end;
            if (!TimeSpan.TryParse(parts[0], out start)) return false;
            if (!TimeSpan.TryParse(parts[1], out end)) return false;

            var t = etNow.TimeOfDay;
            if (start <= end)
                return t >= start && t <= end;

            // Wrap past midnight: either after start or before end.
            return t >= start || t <= end;
        }
    }
}

