using System;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NT8.SDK.Abstractions;
using NT8.SDK.Facade;
using NT8.SDK.NT8Bridge;
using NT8.SDK;
using NT8.SDK.Risk;

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// Minimal shell that constructs the SDK and forwards ticks (no orders yet).
    /// Compiled by NinjaTrader editor; SDK.dll provides the interfaces/facade.
    /// </summary>
    public class SdkStrategyShell : Strategy
    {
        private dynamic _sdk;

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

            double currentPnL = 0.0;
            var plan = new { Order = new OrderIntent(string.Empty, true, 0, OrderIntentType.Market, 0m, string.Empty) };

            var caps = new DailyWeeklyCaps(1000, 3000); // Daily and weekly caps
            caps.ApplyPnL(currentPnL, Time[0]);

            if (caps.IsDailyLocked(Time[0]) || caps.IsWeeklyLocked(Time[0]))
            {
                Print("⚠️ Risk lockout: Trade blocked due to loss caps.");
                return;
            }

            _sdk.Orders.Submit(plan.Order);
        }
    }
}