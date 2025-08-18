namespace Common.Indicators
{
    using System;

    /// <summary>Lightweight pullback rules (portable).</summary>
    public static class ShortTermPullbackRules
    {
        /// <summary>
        /// Return true if a pullback of at least 'ticks' occurred within 'lookback' bars from a local swing.
        /// </summary>
        public static bool PullbackOccurred(double[] close, int lastIndex, int lookback, double ticks)
        {
            if (close == null) return false;
            if (lastIndex <= 0 || lastIndex >= close.Length) return false;
            if (lookback <= 0) return false;

            double recentMax = close[lastIndex];
            double minAfter = recentMax;

            int start = lastIndex - lookback + 1;
            if (start < 1) start = 1;

            // Track max then min since that max
            for (int i = start; i <= lastIndex; i++)
            {
                if (close[i] > recentMax)
                {
                    recentMax = close[i];
                    minAfter = recentMax;
                }
                if (close[i] < minAfter)
                    minAfter = close[i];
            }
            return (recentMax - minAfter) >= ticks;
        }
    }
}
