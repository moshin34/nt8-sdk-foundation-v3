using System;

namespace NT8.SDK.Abstractions
{
    /// <summary>
    /// Unified SDK surface exposed to strategies and host glue. Keep this interface portable
    /// (no NinjaTrader types, no cross-layer dependencies) so the Guard can compile it in isolation.
    /// </summary>
    public interface ISdk
    {
        /// <summary>Semantic SDK version (e.g., 0.1.0).</summary>
        string SdkVersion { get; }

        /// <summary>Startup banner string suitable for logging or UI display.</summary>
        string StartupBanner { get; }

        /// <summary>
        /// Minimal price tick hook for wiring end-to-end without placing orders.
        /// Implementations may buffer or route the tick internally.
        /// </summary>
        /// <param name=""time"">UTC timestamp of the tick.</param>
        /// <param name=""price"">Last traded price.</param>
        void OnPriceTick(DateTime time, double price);
    }
}