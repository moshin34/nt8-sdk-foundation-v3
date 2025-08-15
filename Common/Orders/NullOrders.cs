using System;

namespace NT8.SDK.Orders
{
    /// <summary>No-op orders adapter for portable/testing contexts.</summary>
    public sealed class NullOrders : IOrders
    {
        public OrderIds Submit(OrderIntent intent)
        {
            return new OrderIds(string.Empty, string.Empty, string.Empty);
        }

        public void Cancel(OrderIds ids)
        {
            // no-op
        }

        public void Modify(OrderIds ids, OrderIntent intent)
        {
            // no-op
        }
    }
}
