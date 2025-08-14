#if DEBUG
using System.Diagnostics;
#endif
using NT8.SDK;

namespace NT8.SDK.Sizing
{
    /// <summary>
    /// Sizing implementation that always returns a fixed quantity.
    /// </summary>
    public class FixedQuantitySizing : ISizing
    {
        private readonly int _quantity;
        private readonly string _tag;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedQuantitySizing"/> class.
        /// </summary>
        /// <param name="quantity">The quantity to always return.</param>
        /// <param name="tag">An optional tag describing the sizing reason.</param>
        public FixedQuantitySizing(int quantity, string tag = "FixedQuantity")
        {
            _quantity = quantity;
            _tag = tag ?? string.Empty;
        }

        /// <inheritdoc />
        public SizeDecision Decide(RiskMode mode, PositionIntent intent)
        {
            return new SizeDecision(_quantity, _tag, mode);
        }

#if DEBUG
        internal static class FixedQuantitySizingTests
        {
            internal static void SmokeTest()
            {
                var sizing = new FixedQuantitySizing(10);
                var decision = sizing.Decide(RiskMode.DCP, new PositionIntent("ES", PositionSide.Long));
                Debug.Assert(decision.Quantity == 10);
                Debug.Assert(decision.Reason == "FixedQuantity");
                Debug.Assert(decision.RiskModeUsed == RiskMode.DCP);
            }
        }
#endif
    }
}

