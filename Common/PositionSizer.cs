using System;
using NT8.SDK;

namespace NT8.SDK.Common
{
    /// <summary>
    /// Safety wrapper around an <see cref="ISizing"/> engine that clamps quantity into [MinQty, MaxQty]
    /// and annotates the decision reason when a clamp occurs.
    /// </summary>
    public sealed class PositionSizer : ISizing
    {
        /// <summary>Inner sizing engine.</summary>
        private readonly ISizing _inner;

        /// <summary>Minimum allowed quantity (inclusive).</summary>
        public int MinQty { get; private set; }

        /// <summary>Maximum allowed quantity (inclusive). Use <c>int.MaxValue</c> for no upper bound.</summary>
        public int MaxQty { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionSizer"/> class.
        /// </summary>
        /// <param name="inner">Inner sizing engine to wrap.</param>
        /// <param name="minQty">Minimum allowed quantity (inclusive).</param>
        /// <param name="maxQty">Maximum allowed quantity (inclusive). Use int.MaxValue for "no cap".</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> is null.</exception>
        public PositionSizer(ISizing inner, int minQty = 0, int maxQty = int.MaxValue)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (minQty < 0) minQty = 0;
            if (maxQty < minQty) maxQty = minQty;
            _inner = inner;
            MinQty = minQty;
            MaxQty = maxQty;
        }

        /// <inheritdoc/>
        public SizeDecision Decide(RiskMode mode, PositionIntent intent)
        {
            SizeDecision d = _inner.Decide(mode, intent);
            int q = d.Quantity;
            string reason = d.Reason ?? string.Empty;

            if (q < MinQty)
            {
                reason = Append(reason, "ClampedMin");
                q = MinQty;
            }
            if (q > MaxQty)
            {
                reason = Append(reason, "ClampedMax");
                q = MaxQty;
            }

            return new SizeDecision(q, reason, d.RiskModeUsed);
        }

        private static string Append(string baseReason, string tag)
        {
            if (string.IsNullOrEmpty(baseReason)) return tag;
            return baseReason + "+" + tag;
        }
    }
}
