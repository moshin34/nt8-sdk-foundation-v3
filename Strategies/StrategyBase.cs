using System;

namespace NT8.SDK.Strategies
{
    /// <summary>
    /// Base strategy providing minimal wiring helpers around the SDK services.
    /// </summary>
    public abstract class StrategyBase
    {
        private PositionSide _lastSide = PositionSide.Flat;

        /// <summary>
        /// Initializes a new instance of the <see cref="StrategyBase"/> class.
        /// </summary>
        /// <param name="sdk">SDK facade (may have null subsystems during early wiring).</param>
        /// <param name="symbol">Symbol to operate on.</param>
        protected StrategyBase(ISdk sdk, string symbol)
        {
            Sdk = sdk;
            Symbol = symbol;
        }

        /// <summary>Gets the SDK facade.</summary>
        public ISdk Sdk { get; private set; }

        /// <summary>Gets the symbol this strategy operates on.</summary>
        public string Symbol { get; private set; }

        /// <summary>
        /// Called before a bar is processed.
        /// </summary>
        /// <param name="etNow">Current eastern time.</param>
        /// <param name="last">Last trade price.</param>
        public virtual void OnBeforeBar(DateTime etNow, decimal last)
        {
        }

        /// <summary>
        /// Called when a bar closes.
        /// </summary>
        /// <param name="etNow">Current eastern time.</param>
        /// <param name="open">Open price.</param>
        /// <param name="high">High price.</param>
        /// <param name="low">Low price.</param>
        /// <param name="close">Close price.</param>
        public virtual void OnBar(DateTime etNow, decimal open, decimal high, decimal low, decimal close)
        {
        }

        /// <summary>
        /// Called after a bar is processed.
        /// </summary>
        /// <param name="etNow">Current eastern time.</param>
        public virtual void OnAfterBar(DateTime etNow)
        {
        }

        /// <summary>
        /// Determines whether a position can be entered. If <see cref="ISdk.Risk"/> is null,
        /// this method allows entries (useful while wiring up components).
        /// </summary>
        /// <param name="side">Desired position side.</param>
        /// <returns><c>true</c> if entry is allowed; otherwise, <c>false</c>.</returns>
        protected bool CanEnter(PositionSide side)
        {
            _lastSide = side;

            // If no risk engine is wired yet, allow by default.
            if (Sdk == null || Sdk.Risk == null)
                return true;

            if (!Sdk.Risk.CanTradeNow())
                return false;

            // IRisk contract: empty string ("") means accepted.
            string reason = Sdk.Risk.EvaluateEntry(new PositionIntent(Symbol, side));
            return reason == string.Empty;
        }

        /// <summary>
        /// Decides position size using the last evaluated side. If <see cref="ISdk.Sizing"/> is null,
        /// this returns a conservative default size of 1 with reason "DefaultSize".
        /// </summary>
        /// <returns>Size decision.</returns>
        protected SizeDecision DecideSize()
        {
            RiskMode mode = (Sdk != null && Sdk.Risk != null) ? Sdk.Risk.Mode : RiskMode.DCP;

            if (Sdk != null && Sdk.Sizing != null)
                return Sdk.Sizing.Decide(mode, new PositionIntent(Symbol, _lastSide));

            return new SizeDecision(1, "DefaultSize", mode);
        }

        /// <summary>
        /// Submits an order intent. If <see cref="ISdk.Orders"/> is null, returns empty IDs without throwing.
        /// </summary>
        /// <param name="type">Order type.</param>
        /// <param name="isLong">True for long orders.</param>
        /// <param name="qty">Quantity to submit.</param>
        /// <param name="price">Price level (0 for market).</param>
        /// <param name="signal">Signal name.</param>
        /// <param name="ocoGroup">OCO group identifier.</param>
        /// <returns>Identifiers for resulting orders, or empty identifiers if orders are not wired.</returns>
        protected OrderIds Submit(OrderIntentType type, bool isLong, int qty, decimal price, string signal, string ocoGroup = null)
        {
            if (Sdk == null || Sdk.Orders == null)
                return new OrderIds(string.Empty, string.Empty, string.Empty);

            return Sdk.Orders.Submit(new OrderIntent(Symbol, isLong, qty, type, price, signal, ocoGroup));
        }

        /// <summary>
        /// Captures a diagnostics message if the diagnostics sink is present and enabled.
        /// </summary>
        /// <param name="tag">Tag for the entry.</param>
        /// <param name="message">Message to capture.</param>
        protected void LogDiag(string tag, string message)
        {
            if (Sdk != null && Sdk.Diagnostics != null && Sdk.Diagnostics.Enabled)
            {
                Sdk.Diagnostics.Capture(new DiagnosticsEvent(tag ?? string.Empty, message ?? string.Empty), tag ?? string.Empty);
            }
        }

        /// <summary>
        /// Emits a telemetry event if a telemetry sink is present.
        /// </summary>
        /// <param name="category">Telemetry category.</param>
        /// <param name="action">Telemetry action.</param>
        /// <param name="label">Telemetry label.</param>
        /// <param name="value">Telemetry value.</param>
        protected void EmitTel(string category, string action, string label, string value)
        {
            if (Sdk != null && Sdk.Telemetry != null)
            {
                Sdk.Telemetry.Emit(new TelemetryEvent(category ?? string.Empty, action ?? string.Empty, label ?? string.Empty, value ?? string.Empty));
            }
        }
    }
}

