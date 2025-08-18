namespace NT8.SDK.Abstractions.Risk
{
    using System;

    /// <summary>Portable risk manager contract (no platform refs).</summary>
    public interface IRiskManager
    {
        /// <summary>Evaluate snapshot against caps; returns a gating decision.</summary>
        RiskResult Evaluate(in RiskCaps caps, in RiskSnapshot snap);
    }
}
