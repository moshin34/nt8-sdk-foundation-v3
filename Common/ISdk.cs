using System;

namespace NT8.SDK
{
    /// <summary>
    /// Facade exposing SDK services.
    /// </summary>
    public interface ISdk
    {
        /// <summary>Risk subsystem.</summary>
        IRisk Risk { get; }

        /// <summary>Sizing subsystem.</summary>
        ISizing Sizing { get; }

        /// <summary>Order subsystem.</summary>
        IOrders Orders { get; }

        /// <summary>Session subsystem.</summary>
        ISession Session { get; }

        /// <summary>Trailing subsystem.</summary>
        ITrailing Trailing { get; }

        /// <summary>Telemetry subsystem.</summary>
        ITelemetry Telemetry { get; }

        /// <summary>Diagnostics subsystem.</summary>
        IDiagnostics Diagnostics { get; }

        /// <summary>Backtest hooks.</summary>
        IBacktestHooks Backtest { get; }
    }
}
