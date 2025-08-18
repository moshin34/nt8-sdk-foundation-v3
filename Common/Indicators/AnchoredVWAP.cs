namespace Common.Indicators
{
    using System;

    /// <summary>Anchored VWAP utilities (portable, deterministic).</summary>
    public static class AnchoredVWAP
    {
        /// <summary>
        /// Compute anchored VWAP for a window [anchorIdx..endIdx], inclusive.
        /// price[i] and volume[i] expected non-null; indices 0..n-1.
        /// Returns 0 if inputs invalid or total volume == 0.
        /// </summary>
        public static double Compute(double[] price, double[] volume, int anchorIdx, int endIdx)
        {
            if (price == null || volume == null) return 0.0;
            if (anchorIdx < 0 || endIdx < 0 || anchorIdx >= price.Length || endIdx >= price.Length) return 0.0;
            if (endIdx < anchorIdx) return 0.0;

            double pv = 0.0;
            double vol = 0.0;
            for (int i = anchorIdx; i <= endIdx; i++)
            {
                var v = volume[i];
                pv += price[i] * v;
                vol += v;
            }
            return vol > 0.0 ? pv / vol : 0.0;
        }
    }
}
