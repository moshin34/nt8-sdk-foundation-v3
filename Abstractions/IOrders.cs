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
        /// <returns>True if a cancel request was issued.</returns>
        bool Cancel(OrderIds ids);

        /// <summary>
        /// Modifies existing orders using a new intent.
        /// </summary>
        /// <param name="ids">Identifiers of orders to modify.</param>
        /// <param name="intent">New order parameters.</param>
        /// <returns>True if a modification request was issued.</returns>
        bool Modify(OrderIds ids, OrderIntent intent);
    }
}
