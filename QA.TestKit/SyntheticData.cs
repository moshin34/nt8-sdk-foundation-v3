// QA.TestKit/SyntheticData.cs
using System;

namespace NT8.SDK.QA.TestKit
{
    /// <summary>
    /// Generates small synthetic bar streams for testing (no 'yield' usage).
    /// </summary>
    public static class SyntheticData
    {
        /// <summary>
        /// Generates an upward trending series of bars.
        /// </summary>
        /// <param name="symbol">Symbol name (unused).</param>
        /// <param name="startEt">Start eastern time.</param>
        /// <param name="bars">Number of bars to generate.</param>
        /// <param name="startPrice">Starting price.</param>
        /// <param name="step">Price step per bar.</param>
        /// <returns>Array of bars.</returns>
        public static Bar[] TrendingUp(string symbol, DateTime startEt, int bars, decimal startPrice, decimal step)
        {
            if (bars < 0) bars = 0;
            var result = new Bar[bars];
            decimal price = startPrice;

            for (int i = 0; i < bars; i++)
            {
                decimal open = price;
                decimal close = price + step;
                decimal high = open > close ? open : close;
                decimal low = open < close ? open : close;

                Bar bar = new Bar();
                bar.Et = startEt.AddMinutes(i);
                bar.Open = open;
                bar.High = high;
                bar.Low = low;
                bar.Close = close;

                result[i] = bar;
                price = close;
            }

            return result;
        }

        /// <summary>
        /// Generates a downward trending series of bars.
        /// </summary>
        public static Bar[] TrendingDown(string symbol, DateTime startEt, int bars, decimal startPrice, decimal step)
        {
            if (bars < 0) bars = 0;
            var result = new Bar[bars];
            decimal price = startPrice;

            for (int i = 0; i < bars; i++)
            {
                decimal open = price;
                decimal close = price - step;
                decimal high = open > close ? open : close;
                decimal low = open < close ? open : close;

                Bar bar = new Bar();
                bar.Et = startEt.AddMinutes(i);
                bar.Open = open;
                bar.High = high;
                bar.Low = low;
                bar.Close = close;

                result[i] = bar;
                price = close;
            }

            return result;
        }

        /// <summary>
        /// Generates a choppy bar sequence alternating direction.
        /// </summary>
        public static Bar[] Choppy(string symbol, DateTime startEt, int bars, decimal startPrice, decimal step)
        {
            if (bars < 0) bars = 0;
            var result = new Bar[bars];
            decimal price = startPrice;
            bool up = true;

            for (int i = 0; i < bars; i++)
            {
                decimal open = price;
                decimal close = up ? price + step : price - step;
                decimal high = open > close ? open : close;
                decimal low = open < close ? open : close;

                Bar bar = new Bar();
                bar.Et = startEt.AddMinutes(i);
                bar.Open = open;
                bar.High = high;
                bar.Low = low;
                bar.Close = close;

                result[i] = bar;
                price = close;
                up = !up;
            }

            return result;
        }
    }

    /// <summary>
    /// Plain bar struct used by synthetic data generators.
    /// </summary>
    public struct Bar
    {
        /// <summary>Eastern time of the bar.</summary>
        public DateTime Et;

        /// <summary>Open price.</summary>
        public decimal Open;

        /// <summary>High price.</summary>
        public decimal High;

        /// <summary>Low price.</summary>
        public decimal Low;

        /// <summary>Close price.</summary>
        public decimal Close;
    }
}
