using System;

namespace NT8.SDK.Session
{
    /// <summary>
    /// Represents a time-of-day range in ET. Supports wrap past midnight.
    /// Equality of start and end denotes a full 24-hour block.
    /// </summary>
    public struct TimeRange
    {
        public readonly TimeSpan Start;
        public readonly TimeSpan End;

        public TimeRange(TimeSpan start, TimeSpan end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Returns true if <paramref name="t"/> falls within this range.
        /// </summary>
        public bool Contains(TimeSpan t)
        {
            if (Start == End) return true; // 24h block
            if (Start <= End) return t >= Start && t <= End;
            return t >= Start || t <= End; // wrap
        }

        public override string ToString()
        {
            bool wrap = Start > End;
            return string.Format("{0}–{1} ({2})",
                Start.ToString(@"hh\:mm\:ss"),
                End.ToString(@"hh\:mm\:ss"),
                wrap ? "wrap" : "linear");
        }
    }

    /// <summary>
    /// Combines settlement and blackout windows.
    /// </summary>
    public sealed class SettlementGate
    {
        private readonly TimeRange? _settlement;
        private readonly TimeRange[] _blackouts;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettlementGate"/> class.
        /// </summary>
        /// <param name="settlement">Settlement window (ET) or null.</param>
        /// <param name="blackouts">Blackout windows (ET) or empty array.</param>
        public SettlementGate(TimeRange? settlement, TimeRange[] blackouts)
        {
            _settlement = settlement;
            _blackouts = blackouts ?? new TimeRange[0];
        }

        /// <summary>
        /// Returns true when the time is outside all windows.
        /// </summary>
        public bool IsOpen(DateTime etTime)
        {
            var t = etTime.TimeOfDay;
            if (_settlement.HasValue && _settlement.Value.Contains(t)) return false;
            for (int i = 0; i < _blackouts.Length; i++)
            {
                if (_blackouts[i].Contains(t)) return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the first blocking range for the time, or null if none.
        /// </summary>
        public TimeRange? Blocking(DateTime etTime)
        {
            var t = etTime.TimeOfDay;
            if (_settlement.HasValue && _settlement.Value.Contains(t)) return _settlement;
            for (int i = 0; i < _blackouts.Length; i++)
            {
                if (_blackouts[i].Contains(t)) return _blackouts[i];
            }
            return null;
        }
    }
}

