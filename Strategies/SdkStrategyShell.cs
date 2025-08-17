using System;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NT8.SDK.Abstractions;
using NT8.SDK.Facade;
using NT8.SDK.NT8Bridge;
using NT8.SDK.Telemetry;
using NT8.SDK.Risk;

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// Minimal shell that constructs the SDK and forwards ticks (no orders yet).
    /// Compiled by NinjaTrader editor; SDK.dll provides the interfaces/facade.
    /// </summary>
    public class SdkStrategyShell : Strategy
    {
        private ISdk _sdk;
        private RiskCaps riskCaps;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "SdkStrategyShell";
                Calculate = Calculate.OnEachTick;
            }
            else if (State == State.Configure)
            {
                Print(NT8.SDK.Common.StartupBanner.Get());
                Print($"[NT8 SDK] Starting strategy on {Instrument.FullName} at {Time[0]}");
            }
            else if (State == State.DataLoaded)
            {
                _sdk = new SdkBuilder()
                    .WithDefaults()
                    .WithOrders(new Nt8Orders(this))
                    .Build();
                riskCaps = new RiskCaps(double.MaxValue, double.MaxValue, double.MaxValue, 0);
                Print(_sdk.StartupBanner);
            }
        }

        protected override void OnBarUpdate()
        {
            if (_sdk == null || CurrentBar < 0) return;

            if (riskCaps != null && riskCaps.IsLocked())
            {
                Print("⚠️ Trade skipped: risk cap lockout active.");
                return;
            }

            _sdk.OnPriceTick(Time[0], Close[0]);
        }
    }
}