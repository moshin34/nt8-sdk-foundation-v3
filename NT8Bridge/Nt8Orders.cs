using System;
using System.Collections.Generic;
using NinjaTrader.Cbi;
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
        private readonly Dictionary<Guid, Order> _orders = new Dictionary<Guid, Order>();

        public Nt8Orders(Strategy strategy)
        {
            if (strategy == null) throw new ArgumentNullException("strategy");
            _strategy = strategy;
        }

        public void Submit(OrderIntent intent)
        {
            if (intent == null)
                throw new ArgumentNullException("intent");

            dynamic i = intent;

            Guid clientId = i.ClientOrderId;
            bool isLong = i.IsLong;
            int quantity = i.Quantity;
            decimal stop = i.Stop;
            decimal target = i.Target;
            string oco = i.OcoId;

            if (stop != 0m)
                _strategy.SetStopLoss(oco, CalculationMode.Price, (double)stop, false);

            if (target != 0m)
                _strategy.SetProfitTarget(oco, CalculationMode.Price, (double)target);

            Order order = isLong
                ? _strategy.EnterLong(quantity, oco)
                : _strategy.EnterShort(quantity, oco);

            if (order != null && clientId != Guid.Empty)
                _orders[clientId] = order;
        }

        public void Cancel(Guid clientOrderId)
        {
            Order order;
            if (_orders.TryGetValue(clientOrderId, out order) && order != null)
                _strategy.CancelOrder(order);
        }

        public void Modify(Guid clientOrderId, decimal newStop, decimal newTarget)
        {
            Order order;
            if (!_orders.TryGetValue(clientOrderId, out order) || order == null)
                return;

            if (newStop != 0m)
                _strategy.ChangeOrder(order, order.Quantity, order.LimitPrice, (double)newStop);

            if (newTarget != 0m)
                _strategy.ChangeOrder(order, order.Quantity, (double)newTarget, order.StopPrice);
        }

        void IOrders.Modify(OrderIds ids, OrderIntent intent)
        {
            if (ids.IsEmpty)
                return;

            dynamic i = intent;
            decimal stop = i.Stop;
            decimal target = i.Target;
            Guid g;

            if (!string.IsNullOrEmpty(ids.Stop) && Guid.TryParse(ids.Stop, out g))
                Modify(g, stop, 0m);

            if (!string.IsNullOrEmpty(ids.Target) && Guid.TryParse(ids.Target, out g))
                Modify(g, 0m, target);
        }

        void IOrders.Cancel(OrderIds ids)
        {
            if (ids.IsEmpty)
                return;

            Guid g;
            if (!string.IsNullOrEmpty(ids.Entry) && Guid.TryParse(ids.Entry, out g))
                Cancel(g);

            if (!string.IsNullOrEmpty(ids.Stop) && Guid.TryParse(ids.Stop, out g))
                Cancel(g);

            if (!string.IsNullOrEmpty(ids.Target) && Guid.TryParse(ids.Target, out g))
                Cancel(g);
        }
    }
}

