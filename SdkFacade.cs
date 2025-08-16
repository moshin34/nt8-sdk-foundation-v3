using System;
using NT8.SDK.Abstractions;
using NT8.SDK.Common;
using StartupBannerCommon = NT8.SDK.Common.StartupBanner;

namespace NT8.SDK
{
    /// <summary>
    /// Production-facing SDK façade implementing ISdk. Exposes identity info
    /// and accepts price ticks for downstream processing (no order placement).
    /// </summary>
    public sealed class SdkFacade : ISdk
    {
        private Tick _lastTick;
        private bool _hasTick;

        /// <summary>Semantic SDK version (e.g., 0.1.0).</summary>
        public string SdkVersion
        {
            get { return SdkInfo.Version; }
        }

        /// <summary>Startup banner string suitable for logging or UI display.</summary>
        public string StartupBanner
        {
            get { return StartupBannerCommon.Get(); }
        }

        /// <summary>
        /// Accepts a price tick. Stores only the last tick for quick diagnostics.
        /// </summary>
        public void OnPriceTick(DateTime time, double price)
        {
            _lastTick = new Tick(time, price);
            _hasTick = true;
        }

        /// <summary>
        /// Returns the latest observed tick if available.
        /// </summary>
        public bool TryGetLatestTick(out DateTime time, out double price)
        {
            if (!_hasTick)
            {
                time = default(DateTime);
                price = default(double);
                return false;
            }

            time = _lastTick.Time;
            price = _lastTick.Price;
            return true;
        }

        // Internal immutable value type for the last tick.
        private struct Tick
        {
            public readonly DateTime Time;
            public readonly double Price;

            public Tick(DateTime time, double price)
            {
                Time = time;
                Price = price;
            }
        }
    }
}