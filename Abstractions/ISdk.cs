using System;

namespace NT8.SDK.Abstractions
{
    /// <summary>
    /// Unified SDK surface exposed to strategies and host glue (portable; no NinjaTrader types).
    /// </summary>
    public interface ISdk
    {
        /// <summary>Semantic SDK version (e.g., 0.1.0).</summary>
        string SdkVersion { get; }

        /// <summary>Startup banner string suitable for logging or UI display.</summary>
        string StartupBanner { get; }

        /// <summary>
        /// Minimal price tick hook for end-to-end wiring (no orders).
        /// </summary>
        /// <param name=""time"">UTC timestamp of the tick.</param>
        /// <param name=""price"">Last traded price.</param>
        void OnPriceTick(DateTime time, double price);

        /// <summary>
        /// Returns the latest tick if one has been observed since startup.
        /// </summary>
        /// <param name=""time"">UTC timestamp of the latest tick.</param>
        /// <param name=""price"">Price of the latest tick.</param>
        /// <returns>True if a tick has been observed; otherwise false.</returns>
        bool TryGetLatestTick(out DateTime time, out double price);
    }
}