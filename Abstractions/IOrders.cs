using System;

namespace NT8.SDK
{
    /// <summary>
    /// Order management abstraction.
    /// </summary>
    public interface IOrders
    {
        /// <summary>
        /// Submits a new order intent.
        /// </summary>
        /// <param name="intent">Order parameters.</param>
        /// <returns>Identifiers for resulting orders.</returns>
        OrderIds Submit(OrderIntent intent);

        /// <summary>
        /// Cancels existing orders.
        /// </summary>
        /// <param name="ids">Identifiers of orders to cancel.</param>
        /// <returns>Nothing.</returns>
        void Cancel(OrderIds ids);

        /// <summary>
        /// Modifies existing orders using a new intent.
        /// </summary>
        /// <param name="ids">Identifiers of orders to modify.</param>
        /// <param name="intent">New order parameters.</param>
        /// <returns>Nothing.</returns>
        void Modify(OrderIds ids, OrderIntent intent);
    }
}
