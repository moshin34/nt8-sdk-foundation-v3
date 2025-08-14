using System;
using System.Diagnostics;
using NT8.SDK;

namespace NT8.SDK.Trailing
{
    /// <summary>
    /// Trailing stop that tracks price using a fixed number of ticks.
    /// </summary>
    public sealed class FixedTicksTrailingStop : ITrailingStop
    {
        private readonly int _ticks;
        private readonly double _tickSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedTicksTrailingStop"/> class.
        /// </summary>
        /// <param name="ticks">The number of ticks for the stop distance.</param>
        /// <param name="tickSize">The size of a single tick.</param>
        public FixedTicksTrailingStop(int ticks, double tickSize)
        {
            _ticks = ticks;
            _tickSize = tickSize;
        }

        /// <inheritdoc />
        public double? GetStopPrice(double entryPrice, double currentPrice, PositionSide side)
        {
            if (_ticks <= 0 || _tickSize <= 0.0) return null;

            double offset = _ticks * _tickSize;
            if (side == PositionSide.Long)
            {
                double fromEntry = entryPrice - offset;
                double fromCurrent = currentPrice - offset;
                return Math.Max(fromEntry, fromCurrent);
            }
            if (side == PositionSide.Short)
            {
                double fromEntry = entryPrice + offset;
                double fromCurrent = currentPrice + offset;
                return Math.Min(fromEntry, fromCurrent);
            }
            return null;
        }
    }

#if DEBUG
    internal static class DebugFixedTicksTrailingStop
    {
        internal static void Main()
        {
            var stop = new FixedTicksTrailingStop(10, 0.25);
            double? longStop = stop.GetStopPrice(100.0, 110.0, PositionSide.Long);
            double? shortStop = stop.GetStopPrice(100.0, 90.0, PositionSide.Short);
            Debug.Assert(longStop.HasValue && Math.Abs(longStop.Value - 107.5) < 0.0001);
            Debug.Assert(shortStop.HasValue && Math.Abs(shortStop.Value - 92.5) < 0.0001);
            Console.WriteLine("FixedTicksTrailingStop smoke test passed");
        }
    }
#endif
}
