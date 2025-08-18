namespace Common.Indicators
{
    using System;

    /// <summary>Simple gap classification utilities.</summary>
    public static class GapClassifier
    {
        public enum GapType { None = 0, Up = 1, Down = -1 }

        /// <summary>Classify gap between prior close and current open.</summary>
        public static GapType Classify(double priorClose, double open, double thresholdTicks)
        {
            double diff = open - priorClose;
            if (diff > thresholdTicks) return GapType.Up;
            if (diff < -thresholdTicks) return GapType.Down;
            return GapType.None;
        }
    }
}
