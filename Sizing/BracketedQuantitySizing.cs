#if DEBUG
using System.Diagnostics;
#endif
using NT8.SDK;

namespace NT8.SDK.Sizing
{
    /// <summary>
    /// Sizing implementation that selects quantity based on the active risk mode.
    /// </summary>
    public class BracketedQuantitySizing : ISizing
    {
        private readonly int _ecpQty;
        private readonly int _pcpQty;
        private readonly int _dcpQty;
        private readonly int _hrQty;
        private readonly string _tag;

        /// <summary>
        /// Initializes a new instance of the <see cref="BracketedQuantitySizing"/> class.
        /// </summary>
        /// <param name="ecpQty">Quantity when risk mode is <see cref="RiskMode.ECP"/>.</param>
        /// <param name="pcpQty">Quantity when risk mode is <see cref="RiskMode.PCP"/>.</param>
        /// <param name="dcpQty">Quantity when risk mode is <see cref="RiskMode.DCP"/>.</param>
        /// <param name="hrQty">Quantity when risk mode is <see cref="RiskMode.HR"/>.</param>
        /// <param name="tag">Optional tag describing the sizing reason.</param>
        public BracketedQuantitySizing(int ecpQty, int pcpQty, int dcpQty, int hrQty, string tag = "BracketedQuantity")
        {
            _ecpQty = ecpQty;
            _pcpQty = pcpQty;
            _dcpQty = dcpQty;
            _hrQty = hrQty;
            _tag = tag ?? string.Empty;
        }

        /// <inheritdoc />
        public SizeDecision Decide(RiskMode mode, PositionIntent intent)
        {
            int qty;
            switch (mode)
            {
                case RiskMode.ECP:
                    qty = _ecpQty;
                    break;
                case RiskMode.PCP:
                    qty = _pcpQty;
                    break;
                case RiskMode.DCP:
                    qty = _dcpQty;
                    break;
                case RiskMode.HR:
                default:
                    qty = _hrQty;
                    break;
            }
            return new SizeDecision(qty, _tag, mode);
        }

#if DEBUG
        internal static class BracketedQuantitySizingTests
        {
            internal static void SmokeTest()
            {
                var sizing = new BracketedQuantitySizing(1, 2, 3, 4);
                var decision = sizing.Decide(RiskMode.HR, new PositionIntent("ES", PositionSide.Long));
                Debug.Assert(decision.Quantity == 4);
                Debug.Assert(decision.Reason == "BracketedQuantity");
                Debug.Assert(decision.RiskModeUsed == RiskMode.HR);
            }
        }
#endif
    }
}

