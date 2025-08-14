using System;
using NT8.SDK;

namespace NT8.SDK.Facade
{
    /// <summary>
    /// Fluent builder for constructing an <see cref="ISdk"/> facade from components.
    /// </summary>
    public sealed class SdkAdapterV1
    {
        private IRisk _risk;
        private ISizing _sizing;
        private IOrders _orders;
        private ISession _session;
        private ITrailing _trailing;
        private ITelemetry _telemetry;
        private IDiagnostics _diagnostics;
        private IBacktestHooks _backtest;

        /// <summary>Sets the risk engine.</summary>
        public SdkAdapterV1 WithRisk(IRisk risk) { _risk = risk; return this; }

        /// <summary>Sets the sizing engine.</summary>
        public SdkAdapterV1 WithSizing(ISizing sizing) { _sizing = sizing; return this; }

        /// <summary>Sets the order router.</summary>
        public SdkAdapterV1 WithOrders(IOrders orders) { _orders = orders; return this; }

        /// <summary>Sets the session service.</summary>
        public SdkAdapterV1 WithSession(ISession session) { _session = session; return this; }

        /// <summary>Sets the trailing service.</summary>
        public SdkAdapterV1 WithTrailing(ITrailing trailing) { _trailing = trailing; return this; }

        /// <summary>Sets the telemetry sink.</summary>
        public SdkAdapterV1 WithTelemetry(ITelemetry telemetry) { _telemetry = telemetry; return this; }

        /// <summary>Sets the diagnostics sink.</summary>
        public SdkAdapterV1 WithDiagnostics(IDiagnostics diagnostics) { _diagnostics = diagnostics; return this; }

        /// <summary>Sets the backtest hooks.</summary>
        public SdkAdapterV1 WithBacktest(IBacktestHooks backtest) { _backtest = backtest; return this; }

        /// <summary>
        /// Builds a facade and a matching capabilities descriptor.
        /// </summary>
        public SdkFacade Build(out SdkCapabilities caps)
        {
            var facade = new SdkFacade(_risk, _sizing, _orders, _session, _trailing, _telemetry, _diagnostics, _backtest);
            caps = new SdkCapabilities
            {
                HasRisk = _risk != null,
                HasSizing = _sizing != null,
                HasOrders = _orders != null,
                HasSession = _session != null,
                HasTrailing = _trailing != null,
                HasTelemetry = _telemetry != null,
                HasDiagnostics = _diagnostics != null,
                HasBacktest = _backtest != null
            };
            return facade;
        }
    }
}
