using System;

namespace NT8.SDK
{
    /// <summary>
    /// Optional hooks for backtesting environments.
    /// </summary>
    public interface IBacktestHooks
    {
        /// <summary>
        /// Stamps an arbitrary key/value pair into the backtest context.
        /// </summary>
        /// <param name="key">Identifier for the stamp.</param>
        /// <param name="value">Value to associate.</param>
        void Stamp(string key, string value);
    }
}
