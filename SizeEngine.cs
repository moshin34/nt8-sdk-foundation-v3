using System;

namespace NT8.SDK.Sizing
{
    /// <summary>Reference sizing engine with per-mode base sizes and defensive checks.</summary>
    public sealed class SizeEngine : ISizing
    {
        public SizeDecision Decide(RiskMode mode, PositionIntent intent)
        {
            if (string.IsNullOrEmpty(intent.Symbol))
                return new SizeDecision(0, "symbol missing", mode);

            if (intent.Side == PositionSide.Flat)
                return new SizeDecision(0, "flat intent", mode);

            int qty;
            string reason;

            switch (mode)
            {
                case RiskMode.ECP: qty = 1; reason = "ECP base 1"; break;
                case RiskMode.PCP: qty = 1; reason = "PCP base 1"; break;
                case RiskMode.DCP: qty = 2; reason = "DCP base 2"; break;
                case RiskMode.HR:  qty = 3; reason = "HR base 3";  break;
                default:           qty = 1; reason = "default 1";  break;
            }

            // Clamp defensively and return.
            if (qty < 0) qty = 0;
            return new SizeDecision(qty, reason, mode);
        }
    }
}

