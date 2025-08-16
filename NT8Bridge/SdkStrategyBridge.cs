using System;
using NinjaTrader.NinjaScript;          // Strategy base type
using NinjaTrader.NinjaScript.Strategies;
using NT8.SDK;                          // SdkFacade
using NT8.SDK.Abstractions;             // ISdk

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// SdkStrategyBridge
    /// - On load: instantiate SDK and print startup banner.
    /// - On each tick: forward time/price to SDK (no orders).
    /// - Every 5 seconds (chart time): print the latest SDK tick for visibility.
    /// </summary>
    public class SdkStrategyBridge : Strategy
    {
        private ISdk _sdk;
        private DateTime _lastPrint = DateTime.MinValue;
        private readonly TimeSpan _printEvery = TimeSpan.FromSeconds(5);

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "SdkStrategyBridge";
                Calculate = Calculate.OnEachTick;   // tick-level updates
                IsUnmanaged = false;                // no orders here
                IsInstantiatedOnEachOptimizationIteration = false;
            }
            else if (State == State.DataLoaded)
            {
                _sdk = new SdkFacade();
                Print(_sdk.StartupBanner);
            }
        }

        protected override void OnBarUpdate()
        {
            if (_sdk == null || CurrentBar < 0)
                return;

            // Forward the latest bar's time/price into the SDK (no orders).
            DateTime t = Time[0];
            double price = Close[0];
            _sdk.OnPriceTick(t, price);

            // Every 5 seconds of chart time, print latest SDK tick if available.
            if (_lastPrint == DateTime.MinValue || (t - _lastPrint) >= _printEvery)
            {
                DateTime lt;
                double lp;
                if (_sdk.TryGetLatestTick(out lt, out lp))
                {
                    Print("[SDK] latest tick: " + lt.ToString("o") + "  price=" + lp);
                }
                else
                {
                    Print("[SDK] no ticks yet");
                }
                _lastPrint = t;
            }
        }
    }
}