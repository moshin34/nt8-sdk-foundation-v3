using System;

namespace NT8.SDK.Trailing
{
    /// <summary>Non-loosening trailing stop engine.</summary>
    public sealed class TrailingEngine : ITrailing
    {
        private static decimal TightenLong(decimal priorStop, decimal candidate)
        {
            if (priorStop <= 0m) return candidate;          // initial placement
            return Math.Max(priorStop, candidate);           // never loosen (only tighten upward)
        }

        private static decimal TightenShort(decimal priorStop, decimal candidate)
        {
            if (priorStop <= 0m) return candidate;          // initial placement
            return Math.Min(priorStop, candidate);           // never loosen (only tighten downward)
        }

        public decimal ComputeStop(decimal entry, decimal current, bool isLong, TrailingProfile profile, decimal priorStop)
        {
            // For now, ATR/OR degrade to fixed-distance using Param1.
            // Param1 = distance; sanitize to small positive value.
            var distance = profile.Param1 <= 0m ? 10m : profile.Param1;

            switch (profile.Type)
            {
                case TrailingProfileType.FixedTicks:
                    // distance already set from Param1
                    break;
                case TrailingProfileType.AtrMultiple:
                case TrailingProfileType.OpeningRangeWidth:
                default:
                    // Future: compute from ATR / opening range; keep fixed fallback for Step 4B.
                    break;
            }

            if (isLong)
            {
                var candidate = current - distance;
                if (priorStop <= 0m) priorStop = entry - distance; // initial placement
                return TightenLong(priorStop, candidate);
            }
            else
            {
                var candidate = current + distance;
                if (priorStop <= 0m) priorStop = entry + distance; // initial placement
                return TightenShort(priorStop, candidate);
            }
        }
    }
}

