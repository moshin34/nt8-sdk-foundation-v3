using System;

namespace NT8.SDK.Strategies
{
    /// <summary>
    /// Simple example strategy demonstrating SDK wiring; attempts long entries on bullish bars.
    /// </summary>
    public class TemplateStrategy : StrategyBase
    {
        private int _bars;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateStrategy"/> class.
        /// </summary>
        /// <param name="sdk">SDK facade.</param>
        /// <param name="symbol">Symbol to operate on.</param>
        public TemplateStrategy(ISdk sdk, string symbol)
            : base(sdk, symbol)
        {
        }

        /// <inheritdoc/>
        public override void OnBar(DateTime etNow, decimal open, decimal high, decimal low, decimal close)
        {
            _bars++;
            if (Sdk.Session != null && Sdk.Session.IsBlackout(etNow, Symbol))
                return;
            if (close > open && CanEnter(PositionSide.Long))
            {
                SizeDecision size = DecideSize();
                if (size.Quantity > 0)
                {
                    Submit(OrderIntentType.Market, true, size.Quantity, 0m, "TPL_LONG");
                }
            }
        }
    }
}

