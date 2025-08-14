using System;

namespace NT8.SDK.Strategies
{
    /// <summary>
    /// Crabel-style Opening Range Breakout strategy shell.
    /// </summary>
    public class CrabelORBStrategy : StrategyBase
    {
        private DateTime _sessionDate = DateTime.MinValue;
        private DateTime _orEnd = DateTime.MinValue;
        private decimal _orHigh;
        private decimal _orLow;
        private bool _orComplete;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrabelORBStrategy"/> class.
        /// </summary>
        /// <param name="sdk">SDK facade.</param>
        /// <param name="symbol">Symbol to operate on.</param>
        public CrabelORBStrategy(ISdk sdk, string symbol)
            : base(sdk, symbol)
        {
            UseSessionBlackout = true;
        }

        /// <summary>
        /// Gets or sets the opening range duration in minutes.
        /// </summary>
        public int OpeningRangeMinutes { get; set; }

        /// <summary>
        /// Gets or sets the breakout offset in ticks.
        /// </summary>
        public decimal BreakoutOffsetTicks { get; set; }

        /// <summary>
        /// Gets or sets the tick size.
        /// </summary>
        public decimal TickSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether session blackout checks are used.
        /// </summary>
        public bool UseSessionBlackout { get; set; }

        /// <inheritdoc/>
        public override void OnBar(DateTime etNow, decimal open, decimal high, decimal low, decimal close)
        {
            if (UseSessionBlackout && Sdk.Session != null && Sdk.Session.IsBlackout(etNow, Symbol))
                return;

            if (OpeningRangeMinutes <= 0 || BreakoutOffsetTicks <= 0m || TickSize <= 0m)
                return;

            if (etNow.Date != _sessionDate)
            {
                _sessionDate = etNow.Date;
                _orEnd = etNow.AddMinutes(OpeningRangeMinutes);
                _orHigh = high;
                _orLow = low;
                _orComplete = false;
                return;
            }

            if (!_orComplete)
            {
                if (etNow < _orEnd)
                {
                    if (high > _orHigh) _orHigh = high;
                    if (low < _orLow) _orLow = low;
                    return;
                }
                _orComplete = true;
            }

            decimal offset = BreakoutOffsetTicks * TickSize;
            if (close > _orHigh + offset && CanEnter(PositionSide.Long))
            {
                SizeDecision size = DecideSize();
                if (size.Quantity > 0)
                {
                    Submit(OrderIntentType.Market, true, size.Quantity, 0m, "ORB_LONG");
                }
            }
            else if (close < _orLow - offset && CanEnter(PositionSide.Short))
            {
                SizeDecision size = DecideSize();
                if (size.Quantity > 0)
                {
                    Submit(OrderIntentType.Market, false, size.Quantity, 0m, "ORB_SHORT");
                }
            }
        }

#if DEBUG
        internal static class CrabelORBStrategyTest
        {
            private class StubSdk : ISdk
            {
                public StubSdk()
                {
                    Risk = new StubRisk();
                    Sizing = new StubSizing();
                    Orders = new StubOrders();
                    Session = new StubSession();
                    Telemetry = new StubTelemetry();
                    Diagnostics = new StubDiagnostics();
                    Backtest = new StubBacktest();
                }

                public IRisk Risk { get; private set; }
                public ISizing Sizing { get; private set; }
                public IOrders Orders { get; private set; }
                public ISession Session { get; private set; }
                public ITrailing Trailing { get { return null; } }
                public ITelemetry Telemetry { get; private set; }
                public IDiagnostics Diagnostics { get; private set; }
                public IBacktestHooks Backtest { get; private set; }

                private class StubRisk : IRisk
                {
                    public RiskMode Mode { get { return RiskMode.DCP; } }
                    public RiskLockoutState Lockout() { return RiskLockoutState.None; }
                    public bool CanTradeNow() { return true; }
                    public string EvaluateEntry(PositionIntent intent) { return string.Empty; }
                    public void RecordWinLoss(bool win) { }
                }

                private class StubSizing : ISizing
                {
                    public SizeDecision Decide(RiskMode mode, PositionIntent intent)
                    {
                        return new SizeDecision(1, string.Empty, mode);
                    }
                }

                private class StubOrders : IOrders
                {
                    public OrderIds Submit(OrderIntent intent)
                    {
                        return new OrderIds("E", "S", "T");
                    }
                    public bool Cancel(OrderIds ids) { return true; }
                    public bool Modify(OrderIds ids, OrderIntent intent) { return true; }
                }

                private class StubSession : ISession
                {
                    public bool IsBlackout(DateTime etNow, string symbol) { return false; }
                    public bool IsSettlementWindow(DateTime etNow, string symbol) { return false; }
                    public DateTime SessionOpen(SessionKey key) { return DateTime.MinValue; }
                    public DateTime SessionClose(SessionKey key) { return DateTime.MaxValue; }
                }

                private class StubTelemetry : ITelemetry
                {
                    public void Emit(TelemetryEvent evt) { }
                }

                private class StubDiagnostics : IDiagnostics
                {
                    public bool Enabled { get; set; }
                    public void Capture(object snapshot, string tag) { }
                }

                private class StubBacktest : IBacktestHooks
                {
                    public void Stamp(string key, string value) { }
                }
            }

            public static void Run()
            {
                var sdk = new StubSdk();
                var strat = new CrabelORBStrategy(sdk, "ES")
                {
                    OpeningRangeMinutes = 1,
                    BreakoutOffsetTicks = 1m,
                    TickSize = 0.25m,
                    UseSessionBlackout = false
                };
                DateTime start = new DateTime(2020, 1, 1, 9, 30, 0);
                strat.OnBar(start, 100m, 101m, 99m, 100.5m);
                strat.OnBar(start.AddMinutes(1), 100.5m, 101.5m, 100m, 101.6m);
            }
        }
#endif
    }
}

