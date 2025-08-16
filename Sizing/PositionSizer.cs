using System;
using NT8.SDK;

namespace NT8.SDK.Sizing
{
    /// <summary>
    /// Safety wrapper around an <see cref="ISizing"/> engine that clamps quantity into [MinQty, MaxQty]
    /// and annotates the decision reason when a clamp occurs.
    /// </summary>
    public sealed class PositionSizer : ISizing
    {
        // Inner sizing engine
        private readonly ISizing _inner;

        /// <summary>Minimum allowed quantity (inclusive).</summary>
        public int MinQty { get; private set; }

        /// <summary>Maximum allowed quantity (inclusive). Use <c>int.MaxValue</c> for no upper bound.</summary>
        public int MaxQty { get; private set; }

        // Optional tier tag used to annotate clamp reason (e.g., the risk tier name)
        private readonly string _tierTag;

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionSizer"/> class.
        /// </summary>
        /// <param name="inner">Inner sizing engine to wrap.</param>
        /// <param name="minQty">Minimum allowed quantity (inclusive).</param>
        /// <param name="maxQty">Maximum allowed quantity (inclusive). Use int.MaxValue for "no cap".</param>
        /// <param name="clampTag">Optional tag appended to the reason when a clamp occurs.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> is null.</exception>
        public PositionSizer(ISizing inner, int minQty = 0, int maxQty = int.MaxValue, string clampTag = null)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (minQty < 0) minQty = 0;
            if (maxQty < minQty) maxQty = minQty;

            _inner = inner;
            MinQty = minQty;
            MaxQty = maxQty;
            _tierTag = clampTag ?? string.Empty;
        }

        /// <summary>
        /// Creates a <see cref="PositionSizer"/> that clamps using tier bounds for the supplied <paramref name="mode"/>.
        /// </summary>
        /// <param name="inner">Inner sizing engine.</param>
        /// <param name="tiers">Tier set (min/max) per risk mode.</param>
        /// <param name="mode">Risk mode to pick a tier from.</param>
        /// <returns>Wrapped sizing that enforces the tier limits.</returns>
        /// <remarks>
        /// If <paramref name="tiers"/> is null, returns a wrapper with no additional clamping (0..int.MaxValue).
        /// The tier's Tag (if any) is appended to the reason when a clamp occurs.
        /// </remarks>
        public static PositionSizer FromTiers(ISizing inner, RiskTiers tiers, RiskMode mode)
        {
            int min = 0;
            int max = int.MaxValue;
            string tag = string.Empty;

            if (tiers != null)
            {
                RiskTiers.Tier t = tiers.For(mode);
                min = t.Min;
                max = t.Max;
                tag = t.Tag;
            }

            return new PositionSizer(inner, min, max, tag);
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
                if (_tierTag.Length != 0) reason = Append(reason, _tierTag);
                q = MinQty;
            }
            if (q > MaxQty)
            {
                reason = Append(reason, "ClampedMax");
                if (_tierTag.Length != 0) reason = Append(reason, _tierTag);
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

