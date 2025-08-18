namespace Common.Indicators
{
    using System;

    /// <summary>NR4 (Narrow Range 4) detector: true if last 4 bars' high-low range is the narrowest of the last N=4.</summary>
    public static class NR4Detector
    {
        public static bool IsNR4(double[] high, double[] low, int lastIndex)
        {
            if (high == null || low == null) return false;
            if (lastIndex < 3 || lastIndex >= high.Length || lastIndex >= low.Length) return false;

            double minRange = double.MaxValue;
            for (int i = lastIndex - 3; i <= lastIndex; i++)
            {
                double range = high[i] - low[i];
                if (range < minRange) minRange = range;
            }
            // NR4 if the most recent bar has the min range among the last 4
            double lastRange = high[lastIndex] - low[lastIndex];
            return Math.Abs(lastRange - minRange) < 1e-12 || lastRange == minRange;
        }
    }
}
