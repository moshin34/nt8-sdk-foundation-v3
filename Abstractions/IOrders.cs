namespace NT8.SDK.Abstractions
{
    /// <summary>Minimal portable order API (implemented by the NT8 bridge inside NinjaTrader).</summary>
    public interface IOrders
    {
        /// <summary>Submit or exit using a high-level intent (implementation maps to actual NT8 orders).</summary>
        void Submit(OrderIntent intent);

        /// <summary>Modify existing orders identified by <paramref name="ids"/> (stop/target adjust, etc.).</summary>
        void Modify(OrderIds ids, OrderIntent intent);

        /// <summary>Cancel orders identified by <paramref name="ids"/> (no-op if null/empty).</summary>
        void Cancel(OrderIds ids);
    }
}