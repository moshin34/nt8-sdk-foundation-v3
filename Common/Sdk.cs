using System;

namespace NT8.SDK
{
    /// <summary>Concrete SDK façade wiring core components.</summary>
    public sealed class Sdk : ISdk
    {
        public Sdk(
            IOrders orders,
            IRisk risk,
            ISizing sizing,
            ISession session,
            ITrailing trailing,
            ITelemetry telemetry,
            IDiagnostics diagnostics,
            IBacktestHooks backtest)
        {
            Orders = orders;
            Risk = risk;
            Sizing = sizing;
            Session = session;
            Trailing = trailing;
            Telemetry = telemetry;
            Diagnostics = diagnostics;
            Backtest = backtest;
        }

        public IOrders Orders { get; private set; }
        public IRisk Risk { get; private set; }
        public ISizing Sizing { get; private set; }
        public ISession Session { get; private set; }
        public ITrailing Trailing { get; private set; }
        public ITelemetry Telemetry { get; private set; }
        public IDiagnostics Diagnostics { get; private set; }
        public IBacktestHooks Backtest { get; private set; }
    }
}

