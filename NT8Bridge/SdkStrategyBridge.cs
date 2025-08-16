using System;
using NinjaTrader.NinjaScript;          // Strategy base type
using NinjaTrader.NinjaScript.Strategies;
using NT8.SDK;                          // SdkFacade
using NT8.SDK.Abstractions;             // ISdk

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// SdkStrategyBridge: Minimal bridge that proves end-to-end wiring.
    /// - On DataLoaded: instantiate SDK and print the startup banner.
    /// - On each tick/bar update: forward time/price to SDK (no orders).
    /// </summary>
    public class SdkStrategyBridge : Strategy
    {
        private ISdk _sdk;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "SdkStrategyBridge";
                Calculate = Calculate.OnEachTick; // tick-level updates
                IsUnmanaged = false;              // no orders here
                IsInstantiatedOnEachOptimizationIteration = false;
            }
            else if (State == State.DataLoaded)
            {
                // Create SDK façade and print banner
                _sdk = new SdkFacade();
                Print(_sdk.StartupBanner);
            }
        }

        protected override void OnBarUpdate()
        {
            if (_sdk == null || CurrentBar < 0)
                return;

            // Forward the latest bar's time/price into the SDK (no orders).
            // Using Strategy's Time and Close series to avoid NT market-data specifics.
            DateTime t = Time[0];
            double price = Close[0];
            _sdk.OnPriceTick(t, price);
        }
    }
}