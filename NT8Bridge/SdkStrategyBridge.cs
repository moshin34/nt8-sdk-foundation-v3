using System;
using System.ComponentModel;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NT8.SDK;
using NT8.SDK.Abstractions;

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// SdkStrategyBridge
    /// - On load: instantiate SDK and print startup banner & config.
    /// - On each tick: forward time/price to SDK (no orders).
    /// - Every 5 seconds: print latest tick; also print bias transitions.
    /// </summary>
    public class SdkStrategyBridge : Strategy
    {
        private ISdk _sdk;
        private DateTime _lastPrint = DateTime.MinValue;
        private readonly TimeSpan _printEvery = TimeSpan.FromSeconds(5);

        private string _lastBias = null; // "LONG", "SHORT", "FLAT", or null until known

        // === Exposed NinjaTrader properties (editable in Strategy UI) ===
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Fast SMA Period", Order = 1, GroupName = "SDK")]
        public int FastSmaPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(2, int.MaxValue)]
        [Display(Name = "Slow SMA Period", Order = 2, GroupName = "SDK")]
        public int SlowSmaPeriod { get; set; }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "SdkStrategyBridge";
                Calculate = Calculate.OnEachTick;
                IsUnmanaged = false;
                IsInstantiatedOnEachOptimizationIteration = false;

                // Defaults (safe; can be changed in UI)
                FastSmaPeriod = 5;
                SlowSmaPeriod = 20;
            }
            else if (State == State.DataLoaded)
            {
                // Instantiate the SDK façade with configured SMA params
                _sdk = new SdkFacade(FastSmaPeriod, SlowSmaPeriod);

                Print(_sdk.StartupBanner);
                Print("[SDK] config: SMA fast=" + FastSmaPeriod + " slow=" + SlowSmaPeriod);
            }
        }

        protected override void OnBarUpdate()
        {
            if (_sdk == null || CurrentBar < 0)
                return;

            DateTime t = Time[0];
            double price = Close[0];
            _sdk.OnPriceTick(t, price);

            // Periodic latest-tick print
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

            // Bias transitions
            bool isLong;
            bool isShort;
            if (_sdk.TryGetSignal(out isLong, out isShort))
            {
                string bias = isLong ? "LONG" : (isShort ? "SHORT" : "FLAT");
                if (!string.Equals(bias, _lastBias, StringComparison.Ordinal))
                {
                    Print("[SDK] bias => " + bias);
                    _lastBias = bias;
                }
            }
            else if (_lastBias != "WARMING")
            {
                Print("[SDK] bias => WARMING");
                _lastBias = "WARMING";
            }
        }
    }
}