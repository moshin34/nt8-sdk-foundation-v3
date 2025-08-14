using System;
using NT8.SDK;

/// <summary>
/// Concrete implementation of ISdk that aggregates core subsystems.
/// </summary>
namespace NT8.SDK.Facade
{
    /// <summary>
    /// Concrete implementation of <see cref="ISdk"/> that aggregates core subsystems.
    /// </summary>
    public sealed class SdkFacade : ISdk
    {
        /// <summary>Constructs an immutable facade over the provided subsystems (nulls allowed).</summary>
        public SdkFacade(IRisk risk, ISizing sizing, IOrders orders, ISession session, ITrailing trailing, ITelemetry telemetry, IDiagnostics diagnostics, IBacktestHooks backtest)
        {
            Risk = risk;
            Sizing = sizing;
            Orders = orders;
            Session = session;
            Trailing = trailing;
            Telemetry = telemetry;
            Diagnostics = diagnostics;
            Backtest = backtest;
        }

        /// <inheritdoc/>
        public IRisk Risk { get; private set; }

        /// <inheritdoc/>
        public ISizing Sizing { get; private set; }

        /// <inheritdoc/>
        public IOrders Orders { get; private set; }

        /// <inheritdoc/>
        public ISession Session { get; private set; }

        /// <inheritdoc/>
        public ITrailing Trailing { get; private set; }

        /// <inheritdoc/>
        public ITelemetry Telemetry { get; private set; }

        /// <inheritdoc/>
        public IDiagnostics Diagnostics { get; private set; }

        /// <inheritdoc/>
        public IBacktestHooks Backtest { get; private set; }
    }
}
