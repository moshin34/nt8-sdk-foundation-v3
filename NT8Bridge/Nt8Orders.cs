#region Using declarations
using System;
using NT8.SDK;
using NinjaTrader.NinjaScript;
#endregion

namespace NT8.SDK.NT8Bridge
{
    /// <summary>
    /// NinjaTrader Managed-orders adapter for the portable IOrders interface.
    /// Maps OrderIntent to Strategy-managed order methods. Returns the signal as EntryId.
    /// </summary>
    public sealed class Nt8Orders : IOrders
    {
        private readonly Strategy _strategy;

        public Nt8Orders(Strategy strategy)
        {
            if (strategy == null) throw new ArgumentNullException("strategy");
            _strategy = strategy;
        }

        public OrderIds Submit(OrderIntent intent)
        {
            // Defensive checks
            if (intent.Quantity <= 0)
                return new OrderIds(string.Empty, string.Empty, string.Empty);

            // Ensure a stable entry signal; SetStopLoss links by this value.
            var signal = string.IsNullOrEmpty(intent.Signal) ? "SDK_" + _strategy.Name : intent.Signal;

            // Round any provided price to tick size
            double price = (double)intent.Price;
            if (price > 0)
                price = _strategy.Instrument.MasterInstrument.RoundToTickSize(price);

            switch (intent.Type)
            {
                case OrderIntentType.Market:
                    if (intent.IsLong) _strategy.EnterLong(intent.Quantity, signal);
                    else _strategy.EnterShort(intent.Quantity, signal);
                    break;

                case OrderIntentType.Limit:
                    if (price <= 0) return new OrderIds(string.Empty, string.Empty, string.Empty);
                    if (intent.IsLong) _strategy.EnterLongLimit(intent.Quantity, price, signal);
                    else _strategy.EnterShortLimit(intent.Quantity, price, signal);
                    break;

                case OrderIntentType.StopMarket:
                    if (price <= 0) return new OrderIds(string.Empty, string.Empty, string.Empty);
                    if (intent.IsLong) _strategy.EnterLongStopMarket(intent.Quantity, price, signal);
                    else _strategy.EnterShortStopMarket(intent.Quantity, price, signal);
                    break;

                case OrderIntentType.StopLimit:
                    // v1 simplification: treat StopLimit as StopMarket using the provided price.
                    if (price <= 0) return new OrderIds(string.Empty, string.Empty, string.Empty);
                    if (intent.IsLong) _strategy.EnterLongStopMarket(intent.Quantity, price, signal);
                    else _strategy.EnterShortStopMarket(intent.Quantity, price, signal);
                    break;
            }

            // Managed orders don’t expose IDs at submit time; return the signal for traceability.
            return new OrderIds(signal, string.Empty, string.Empty);
        }

        public void Cancel(OrderIds ids)
        {
            // v1 Managed approach: selective cancel by ID isn’t supported; no-op.
        }

        public void Modify(OrderIds ids, OrderIntent intent)
        {
            // v1 Managed approach: post-submit modification isn’t supported; no-op.
        }
    }
}

