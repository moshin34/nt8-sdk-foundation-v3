using System;
using System.Collections.Generic;
using NT8.SDK;

namespace NT8.SDK.Trailing
{
    /// <summary>
    /// Aggregates multiple trailing stop strategies and returns the most protective stop.
    /// </summary>
    public sealed class CompositeTrailingStop : ITrailingStop
    {
        private readonly List<ITrailingStop> _stops;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeTrailingStop"/> class.
        /// </summary>
        /// <param name="stops">Collection of child trailing stops. Null becomes empty.</param>
        public CompositeTrailingStop(IEnumerable<ITrailingStop> stops)
        {
            _stops = stops != null ? new List<ITrailingStop>(stops) : new List<ITrailingStop>();
        }

        /// <inheritdoc />
        public double? GetStopPrice(double entryPrice, double currentPrice, PositionSide side)
        {
            if (side != PositionSide.Long && side != PositionSide.Short) return null;

            double? candidate = null;
            for (int i = 0; i < _stops.Count; i++)
            {
                var value = _stops[i].GetStopPrice(entryPrice, currentPrice, side);
                if (!value.HasValue) continue;
                if (!candidate.HasValue)
                {
                    candidate = value;
                }
                else if (side == PositionSide.Long)
                {
                    if (value.Value > candidate.Value) candidate = value;
                }
                else
                {
                    if (value.Value < candidate.Value) candidate = value;
                }
            }
            return candidate;
        }
    }

#if DEBUG
    internal static class DebugCompositeTrailingStop
    {
        internal static void Main()
        {
            var composite = new CompositeTrailingStop(new ITrailingStop[]
            {
                new FixedTicksTrailingStop(10, 0.25),
                new PercentTrailingStop(0.1)
            });
            Console.WriteLine("Composite long: " + composite.GetStopPrice(100.0, 120.0, PositionSide.Long));
            Console.WriteLine("Composite short: " + composite.GetStopPrice(100.0, 80.0, PositionSide.Short));
        }
    }
#endif
}
