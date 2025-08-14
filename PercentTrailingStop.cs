using System;
using System.Diagnostics;
using NT8.SDK;

namespace NT8.SDK.Trailing
{
    /// <summary>
    /// Trailing stop that tracks price using a percentage move.
    /// </summary>
    public sealed class PercentTrailingStop : ITrailingStop
    {
        private readonly double _percent;

        /// <summary>
        /// Initializes a new instance of the <see cref="PercentTrailingStop"/> class.
        /// </summary>
        /// <param name="percent">Fractional percent (e.g., 0.01 for 1%).</param>
        public PercentTrailingStop(double percent)
        {
            _percent = percent;
        }

        /// <inheritdoc />
        public double? GetStopPrice(double entryPrice, double currentPrice, PositionSide side)
        {
            if (_percent <= 0.0) return null;

            double factor = _percent;
            if (side == PositionSide.Long)
            {
                double fromEntry = entryPrice * (1.0 - factor);
                double fromCurrent = currentPrice * (1.0 - factor);
                return Math.Max(fromEntry, fromCurrent);
            }
            if (side == PositionSide.Short)
            {
                double fromEntry = entryPrice * (1.0 + factor);
                double fromCurrent = currentPrice * (1.0 + factor);
                return Math.Min(fromEntry, fromCurrent);
            }
            return null;
        }
    }

#if DEBUG
    internal static class DebugPercentTrailingStop
    {
        internal static void Main()
        {
            var stop = new PercentTrailingStop(0.1);
            double? longStop = stop.GetStopPrice(100.0, 120.0, PositionSide.Long);
            double? shortStop = stop.GetStopPrice(100.0, 80.0, PositionSide.Short);
            Debug.Assert(longStop.HasValue && Math.Abs(longStop.Value - 108.0) < 0.0001);
            Debug.Assert(shortStop.HasValue && Math.Abs(shortStop.Value - 88.0) < 0.0001);
            Console.WriteLine("PercentTrailingStop smoke test passed");
        }
    }
#endif
}
