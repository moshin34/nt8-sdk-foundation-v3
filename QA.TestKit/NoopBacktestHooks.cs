using System;

namespace NT8.SDK.QA.TestKit
{
    /// <summary>No-op backtest hooks for portable execution.</summary>
    public sealed class NoopBacktestHooks : IBacktestHooks
    {
        public void Stamp(string key, string value)
        {
            // swallow
        }
    }
}

