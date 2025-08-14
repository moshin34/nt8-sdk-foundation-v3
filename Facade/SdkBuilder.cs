using System;
using NT8.SDK;         // for IClock
using NT8.SDK.Risk;
using NT8.SDK.Sizing;
using NT8.SDK.Session;
using NT8.SDK.Trailing;

namespace NT8.SDK.Facade
{
    /// <summary>Lightweight builder to assemble a default SDK quickly.</summary>
    public sealed class SdkBuilder
    {
        private RiskMode _mode = RiskMode.PCP;
        private readonly RiskOptions _riskOptions = new RiskOptions();
        private IClock _clock; // null => SystemClock inside components

        public SdkBuilder WithMode(RiskMode mode)
        {
            _mode = mode;
            return this;
        }

        public SdkBuilder WithLossStreakLockout(int losses)
        {
            _riskOptions.LossStreakLockout = losses;
            return this;
        }

        public SdkBuilder WithLockoutDuration(TimeSpan duration)
        {
            _riskOptions.LockoutDuration = duration;
            return this;
        }

        /// <summary>Inject a custom UTC clock for deterministic tests/UIs.</summary>
        public SdkBuilder WithClock(IClock clock)
        {
            _clock = clock;
            return this;
        }

        public ISdk Build()
        {
            return new Sdk(
                orders: new Orders.NullOrders(),
                risk: new RiskEngine(_mode, _riskOptions, _clock),
                sizing: new SizeEngine(),
                session: new CmeBlackoutService(_clock), // use the same clock
                trailing: new TrailingEngine(),
                telemetry: new Telemetry.NoopTelemetry(),
                diagnostics: new Diagnostics.NoopDiagnostics(),
                backtest: new QA.TestKit.NoopBacktestHooks());
        }
    }
}
