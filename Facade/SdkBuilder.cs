using System;
using NT8.SDK.Risk;
using NT8.SDK.Sizing;
using NT8.SDK.Session;
using NT8.SDK.Trailing;
using NT8.SDK.Common;

namespace NT8.SDK.Facade
{
    /// <summary>Lightweight builder to assemble a default SDK quickly.</summary>
    public sealed class SdkBuilder
    {
        private RiskMode _mode = RiskMode.PCP;
        private int _lossStreakLockout = 2;
        private TimeSpan _lockoutDuration = TimeSpan.FromMinutes(15);
        private IClock _clock = SystemClock.Instance;
        private IOrders _orders = new Orders.NullOrders();

        public SdkBuilder WithMode(RiskMode mode) { _mode = mode; return this; }
        public SdkBuilder WithLossStreakLockout(int value) { if (value > 0) _lossStreakLockout = value; return this; }
        public SdkBuilder WithLockoutDuration(TimeSpan duration) { if (duration.TotalSeconds > 0) _lockoutDuration = duration; return this; }
        public SdkBuilder WithClock(IClock clock) { _clock = clock ?? SystemClock.Instance; return this; }
        public SdkBuilder WithOrders(IOrders orders) { _orders = orders ?? new Orders.NullOrders(); return this; }

        public ISdk Build()
        {
            var riskOptions = new RiskOptions
            {
                LossStreakLockout = _lossStreakLockout,
                LockoutDuration = _lockoutDuration
            };

            return new Sdk(
                orders: _orders,
                risk: new RiskEngine(_mode, riskOptions, _clock),
                sizing: new SizeEngine(),
                session: new CmeBlackoutService(_clock),
                trailing: new TrailingEngine(),
                telemetry: new Telemetry.NoopTelemetry(),
                diagnostics: new Diagnostics.NoopDiagnostics(),
                backtest: new QA.TestKit.NoopBacktestHooks());
        }
    }
}

