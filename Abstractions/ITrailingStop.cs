using System;

namespace NT8.SDK
{
    /// <summary>
    /// Defines a trailing stop strategy that suggests stop prices based on
    /// entry and current prices.
    /// </summary>
    public interface ITrailingStop
    {
        /// <summary>
        /// Gets a suggested stop price.
        /// </summary>
        /// <param name="entryPrice">The original entry price.</param>
        /// <param name="currentPrice">The current market price.</param>
        /// <param name="side">The side of the position.</param>
        /// <returns>The suggested stop price, or <c>null</c> if inactive or cannot compute.</returns>
        double? GetStopPrice(double entryPrice, double currentPrice, PositionSide side);
    }

#if DEBUG
    internal sealed class DebugTrailingStop : ITrailingStop
    {
        public double? GetStopPrice(double entryPrice, double currentPrice, PositionSide side)
        {
            return currentPrice;
        }
    }

    internal static class DebugITrailingStop
    {
        internal static void Main()
        {
            ITrailingStop stop = new DebugTrailingStop();
            Console.WriteLine("Debug stop: " + stop.GetStopPrice(1.0, 2.0, PositionSide.Long));
        }
    }
#endif
}
