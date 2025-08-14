using System;

namespace NT8.SDK
{
    /// <summary>Determines order sizing based on risk and intent.</summary>
    public interface ISizing
    {
        /// <summary>Returns size plus context (reason, risk mode used).</summary>
        SizeDecision Decide(RiskMode mode, PositionIntent intent);
    }
}
