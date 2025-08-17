using System;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NT8.SDK;

namespace NT8Bridge
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

        public OrderIds Submit(OrderIntent intent)
        {
            _strategy.Print("[Nt8Orders] Submit " + (intent.Signal ?? string.Empty));

            Order order = null;

            if (intent.Type == OrderIntentType.Market)
            {
                if (intent.IsLong)
                    order = _strategy.EnterLong(intent.Quantity, intent.Signal);
                else
                    order = _strategy.EnterShort(intent.Quantity, intent.Signal);
            }
            else if (intent.Type == OrderIntentType.Limit)
            {
                double price = (double)intent.Price;
                if (intent.IsLong)
                    order = _strategy.EnterLongLimit(intent.Quantity, price, intent.Signal);
                else
                    order = _strategy.EnterShortLimit(intent.Quantity, price, intent.Signal);
            }
            else if (intent.Type == OrderIntentType.StopMarket)
            {
                double price = (double)intent.Price;
                if (intent.IsLong)
                    order = _strategy.EnterLongStopMarket(intent.Quantity, price, intent.Signal);
                else
                    order = _strategy.EnterShortStopMarket(intent.Quantity, price, intent.Signal);
            }
            else if (intent.Type == OrderIntentType.StopLimit)
            {
                double price = (double)intent.Price;
                if (intent.IsLong)
                    order = _strategy.EnterLongStopLimit(intent.Quantity, price, price, intent.Signal);
                else
                    order = _strategy.EnterShortStopLimit(intent.Quantity, price, price, intent.Signal);
            }

            if (order != null && !string.IsNullOrEmpty(intent.OcoGroup))
                order.Oco = intent.OcoGroup;

            string entryId = order != null ? order.OrderId : string.Empty;
            return new OrderIds(entryId, string.Empty, string.Empty);
        }

        public void Modify(OrderIds ids, OrderIntent intent)
        {
            _strategy.Print("[Nt8Orders] Modify " + ids.EntryId);

            if (intent.Price <= 0)
                return;

            double price = (double)intent.Price;

            if (intent.Type == OrderIntentType.Limit)
            {
                if (!string.IsNullOrEmpty(ids.EntryId))
                    _strategy.SetProfitTarget(ids.EntryId, CalculationMode.Price, price);
            }
            else if (intent.Type == OrderIntentType.StopMarket || intent.Type == OrderIntentType.StopLimit)
            {
                if (!string.IsNullOrEmpty(ids.EntryId))
                    _strategy.SetStopLoss(ids.EntryId, CalculationMode.Price, price, false);
            }
        }

        public void Cancel(OrderIds ids)
        {
            _strategy.Print("[Nt8Orders] Cancel " + ids.EntryId + "," + ids.StopId + "," + ids.TargetId);
            CancelById(ids.EntryId);
            CancelById(ids.StopId);
            CancelById(ids.TargetId);
        }

        private void CancelById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            Order order = _strategy.GetOrder(id);
            if (order != null)
                _strategy.CancelOrder(order);
        }
    }
}

