using System;

namespace NT8.SDK.Common
{
    /// <summary>Price math helpers for tick-size normalization.</summary>
    public static class PriceMath
    {
        /// <summary>Rounds a price to the nearest tick. Returns input when tickSize &lt;= 0.</summary>
        public static decimal RoundToTick(decimal price, decimal tickSize)
        {
            if (tickSize <= 0m) return price;
            var q = price / tickSize;
            // MidpointAwayFromZero rounding behavior for stability under Mono
            var roundedQ = Math.Round(q, 0, MidpointRounding.AwayFromZero);
            return roundedQ * tickSize;
        }
    }
}
