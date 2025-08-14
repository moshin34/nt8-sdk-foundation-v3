#region Using declarations
using System;
using NinjaTrader.NinjaScript;
#endregion

// NOTE: This shell just proves the SDK DLL is wired up.
// It uses the SDK's defaults except orders, which are routed via Nt8Orders.

namespace NinjaTrader.NinjaScript.Strategies
{
    public class SdkStrategyShell : Strategy
    {
        private NT8.SDK.ISdk _sdk;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "SdkStrategyShell";
                Calculate = Calculate.OnBarClose;
                IsInstantiatedOnEachOptimizationIteration = false;
            }
            else if (State == State.DataLoaded)
            {
                var orders = new NT8.SDK.NT8Bridge.Nt8Orders(this);
                _sdk = new NT8.SDK.Facade.SdkBuilder()
                    .WithOrders(orders)
                    .Build();

                Print("SDK wired with Nt8Orders (no-trade bridge)");
            }
        }

        protected override void OnBarUpdate()
        {
            // no-op for now — add calls into your SDK when ready
        }
    }
}
