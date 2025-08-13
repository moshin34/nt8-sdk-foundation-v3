using System;

namespace NT8.SDK
{
    public interface ISizing
    {
        SizeDecision Decide(RiskMode mode, PositionIntent intent);
    }
}
