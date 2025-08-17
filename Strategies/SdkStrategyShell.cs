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
        private DailyWeeklyCaps _caps;

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
                _caps = new DailyWeeklyCaps(double.MaxValue, double.MaxValue);
                Print(_sdk.StartupBanner);
            }
        }

        protected override void OnBarUpdate()
        {
            if (_sdk == null || CurrentBar < 0) return;

            double currentPnL = 0;
            if (_caps != null && (_caps.IsDailyLocked(Time[0]) || _caps.IsWeeklyLocked(Time[0])))
            {
                var telemetry = new FileTelemetry("risk.log", "jsonl");
                telemetry.Emit("RiskLockout", "Trade blocked by Daily or Weekly Cap", new {
                    date = Time[0],
                    pnl = currentPnL
                });
                return;
            }

            _sdk.OnPriceTick(Time[0], Close[0]);
        }
    }
}