using System;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NT8.SDK.Abstractions;
using NT8.SDK.Facade;
using NT8.SDK.NT8Bridge;

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// Minimal shell that constructs the SDK and forwards ticks (no orders yet).
    /// Compiled by NinjaTrader editor; SDK.dll provides the interfaces/facade.
    /// </summary>
    public class SdkStrategyShell : Strategy
    {
        private ISdk _sdk;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "SdkStrategyShell";
                Calculate = Calculate.OnEachTick;
            }
            else if (State == State.DataLoaded)
            {
                _sdk = new SdkBuilder()
                    .WithDefaults()
                    .WithOrders(new Nt8Orders(this))
                    .Build();
                Print(_sdk.StartupBanner);
            }
        }

        protected override void OnBarUpdate()
        {
            if (_sdk == null || CurrentBar < 0) return;
            _sdk.OnPriceTick(Time[0], Close[0]);
        }
    }
}