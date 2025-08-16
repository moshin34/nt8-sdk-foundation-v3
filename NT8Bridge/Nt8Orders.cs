using System;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NT8.SDK.Abstractions;

namespace NT8.SDK.NT8Bridge
{
    /// <summary>
    /// Thin adapter that implements the portable IOrders interface using a live NinjaTrader Strategy.
    /// NOTE: This file is compiled by the NinjaTrader editor (NOT in the portable SDK.csproj).
    /// </summary>
    public sealed class Nt8Orders : IOrders
    {
        private readonly Strategy _strategy;

        public Nt8Orders(Strategy strategy)
        {
            if (strategy == null) throw new ArgumentNullException("strategy");
            _strategy = strategy;
        }

        public void Submit(OrderIntent intent)
        {
            // TODO: map intents to actual NT8 order methods (EnterLong/EnterShort/ExitXXX).
            // Guard-safe placeholder to keep compile green until order routing is wired.
        }

        public void Modify(OrderIds ids, OrderIntent intent)
        {
            // TODO: adjust stop/target for provided ids.
        }

        public void Cancel(OrderIds ids)
        {
            // TODO: cancel by ids where available; no-op if ids.IsEmpty.
        }
    }
}