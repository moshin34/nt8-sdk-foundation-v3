using System;
using NT8.SDK.Abstractions;
using NT8.SDK.Common;

namespace NT8.SDK
{
    /// <summary>
    /// Production-facing SDK façade implementing ISdk. Exposes identity info,
    /// accepts price ticks, and maintains a simple SMA crossover signal.
    /// </summary>
    public sealed class SdkFacade : ISdk
    {
        private Tick _lastTick;
        private bool _hasTick;

        // Simple price signal (portable)
        private readonly SmaCrossSignal _signal;

        /// <summary>Create a façade with default SMA params (fast=5, slow=20).</summary>
        public SdkFacade() : this(5, 20)
        {
        }

        /// <summary>Create a façade with explicit SMA params.</summary>
        public SdkFacade(int fastSmaPeriod, int slowSmaPeriod)
        {
            _signal = new SmaCrossSignal(fastSmaPeriod, slowSmaPeriod);
        }

        /// <summary>Semantic SDK version (e.g., 0.1.0).</summary>
        public string SdkVersion
        {
            get { return SdkInfo.Version; }
        }

        /// <summary>Startup banner string suitable for logging or UI display.</summary>
        public string StartupBanner
        {
            get { return StartupBanner.Get(); }
        }

        /// <summary>Accepts a price tick. Stores the last tick and feeds the signal.</summary>
        public void OnPriceTick(DateTime time, double price)
        {
            _lastTick = new Tick(time, price);
            _hasTick = true;

            if (_signal != null)
            {
                _signal.OnPriceTick(time, price);
            }
        }

        /// <summary>Returns the latest observed tick if available.</summary>
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

        /// <summary>Returns the current signal bias if available.</summary>
        public bool TryGetSignal(out bool isLong, out bool isShort)
        {
            isLong = false;
            isShort = false;

            if (_signal == null || !_signal.HasValue)
                return false;

            isLong = _signal.IsLong;
            isShort = _signal.IsShort;
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