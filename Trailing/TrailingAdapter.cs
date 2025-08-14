using System;
using NT8.SDK;

namespace NT8.SDK.Trailing
{
    /// <summary>
    /// Adapter that bridges an <see cref="ITrailingStop"/> strategy to the <see cref="ITrailing"/> interface.
    /// </summary>
    public sealed class TrailingAdapter : ITrailing
    {
        private readonly ITrailingStop _stop;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrailingAdapter"/> class.
        /// </summary>
        /// <param name="stop">Underlying trailing stop strategy.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stop"/> is <c>null</c>.</exception>
        public TrailingAdapter(ITrailingStop stop)
        {
            if (stop == null) throw new ArgumentNullException("stop");
            _stop = stop;
        }

        /// <inheritdoc />
        /// <remarks>
        /// The <paramref name="profile"/> parameter is currently ignored.
        /// </remarks>
        public decimal ComputeStop(decimal entry, decimal current, bool isLong, TrailingProfile profile, decimal priorStop)
        {
            double entryD = (double)entry;
            double currentD = (double)current;
            PositionSide side = isLong ? PositionSide.Long : PositionSide.Short;
            double? candidate = _stop.GetStopPrice(entryD, currentD, side);
            if (!candidate.HasValue) return priorStop;

            decimal candidateDec;
            try
            {
                candidateDec = (decimal)candidate.Value;
            }
            catch
            {
                return priorStop;
            }

            if (isLong)
            {
                return priorStop > candidateDec ? priorStop : candidateDec;
            }
            return priorStop < candidateDec ? priorStop : candidateDec;
        }
    }

#if DEBUG
    internal static class DebugTrailingAdapter
    {
        private sealed class DummyStop : ITrailingStop
        {
            public double? GetStopPrice(double entryPrice, double currentPrice, PositionSide side)
            {
                if (side == PositionSide.Long) return currentPrice - 1.0;
                if (side == PositionSide.Short) return currentPrice + 1.0;
                return null;
            }
        }

        internal static void Main()
        {
            ITrailingStop stop = new DummyStop();
            var adapter = new TrailingAdapter(stop);
            TrailingProfile profile = new TrailingProfile();

            decimal priorLong = 100m;
            decimal longStop1 = adapter.ComputeStop(100m, 120m, true, profile, priorLong);
            decimal longStop2 = adapter.ComputeStop(100m, 110m, true, profile, longStop1);
            System.Diagnostics.Debug.Assert(longStop2 >= longStop1);

            decimal priorShort = 100m;
            decimal shortStop1 = adapter.ComputeStop(100m, 80m, false, profile, priorShort);
            decimal shortStop2 = adapter.ComputeStop(100m, 90m, false, profile, shortStop1);
            System.Diagnostics.Debug.Assert(shortStop2 <= shortStop1);

            Console.WriteLine("TrailingAdapter smoke test completed");
        }
    }
#endif
}

