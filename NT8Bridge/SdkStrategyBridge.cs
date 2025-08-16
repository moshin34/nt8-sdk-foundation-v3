using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NT8.SDK;

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// SdkStrategyBridge
    /// - Uses portable NT8.SDK (SMA cross) to decide bias.
    /// - Default is DryRun=true (logs only). Set DryRun=false to place sim orders.
    /// </summary>
    public class SdkStrategyBridge : Strategy
    {
        private SdkFacade _sdk;
        private DateTime _lastPrint = DateTime.MinValue;
        private TimeSpan _printEvery = TimeSpan.FromSeconds(5);
        private string _lastBias;

        [NinjaScriptProperty]
        [Range(2, 200)]
        [Display(Name = "Fast SMA", GroupName = "SDK", Order = 0)]
        public int FastSma { get; set; }

        [NinjaScriptProperty]
        [Range(3, 400)]
        [Display(Name = "Slow SMA", GroupName = "SDK", Order = 1)]
        public int SlowSma { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "DryRun (no orders)", GroupName = "SDK", Order = 2)]
        public bool DryRun { get; set; }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "SdkStrategyBridge";
                Calculate = Calculate.OnEachTick;
                IsUnmanaged = false;
                FastSma = 5;
                SlowSma = 20;
                DryRun  = true;    // default safe: logs only
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
            }
            else if (State == State.DataLoaded)
            {
                if (FastSma >= SlowSma)
                    throw new ArgumentException("FastSma must be < SlowSma.");

                _sdk = new SdkFacade(FastSma, SlowSma);
                Print(_sdk.StartupBanner);
                _lastBias = null;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 0 || _sdk == null)
                return;

            DateTime t = Time[0];
            double price = Close[0];

            // feed tick into portable SDK
            _sdk.OnPriceTick(t, price);

            // periodic heartbeat with latest tick
            if (_lastPrint == DateTime.MinValue || (t - _lastPrint) >= _printEvery)
            {
                DateTime lt; double lp;
                if (_sdk.TryGetLatestTick(out lt, out lp))
                    Print($"[SDK] latest tick: {lt:o}  price={lp}");
                _lastPrint = t;
            }

            // query bias and act/log
            bool isLong, isShort;
            if (_sdk.TryGetSignal(out isLong, out isShort))
            {
                string bias = isLong ? "LONG" : (isShort ? "SHORT" : "FLAT");
                if (bias != _lastBias)
                {
                    Print($"[SDK] bias -> {bias} @ {t:o}");
                    _lastBias = bias;
                }

                if (!DryRun)
                {
                    // simple flip logic: go with current bias (managed orders)
                    if (isLong && Position.MarketPosition != MarketPosition.Long)
                    {
                        if (Position.MarketPosition == MarketPosition.Short) ExitShort();
                        EnterLong();
                    }
                    else if (isShort && Position.MarketPosition != MarketPosition.Short)
                    {
                        if (Position.MarketPosition == MarketPosition.Long) ExitLong();
                        EnterShort();
                    }
                }
                else
                {
                    // log-only mode
                    if (isLong && Position.MarketPosition != MarketPosition.Long)
                        Print("[SDK] DryRun would EnterLong()");
                    else if (isShort && Position.MarketPosition != MarketPosition.Short)
                        Print("[SDK] DryRun would EnterShort()");
                }
            }
            else
            {
                // warming up until slow SMA is filled
                // (quiet by default to avoid spam)
            }
        }
    }
}