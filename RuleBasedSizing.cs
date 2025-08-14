#if DEBUG
using System.Diagnostics;
#endif
using System;
using System.Collections.Generic;
using NT8.SDK;

namespace NT8.SDK.Sizing
{
    /// <summary>
    /// Sizing implementation that evaluates a sequence of rules before falling back to a default sizing strategy.
    /// </summary>
    public class RuleBasedSizing : ISizing
    {
        private readonly List<ISizeRule> _rules;
        private readonly ISizing _fallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleBasedSizing"/> class.
        /// </summary>
        /// <param name="rules">The ordered set of sizing rules to evaluate.</param>
        /// <param name="fallback">The fallback sizing to use when no rule provides a decision.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fallback"/> is <c>null</c>.</exception>
        public RuleBasedSizing(IEnumerable<ISizeRule> rules, ISizing fallback)
        {
            if (fallback == null) throw new ArgumentNullException(nameof(fallback));
            _fallback = fallback;
            _rules = rules != null ? new List<ISizeRule>(rules) : new List<ISizeRule>();
        }

        /// <inheritdoc />
        public SizeDecision Decide(RiskMode mode, PositionIntent intent)
        {
            SizeDecision decision;
            for (int i = 0; i < _rules.Count; i++)
            {
                if (_rules[i].TryDecide(mode, intent, out decision))
                {
                    return decision;
                }
            }
            return _fallback.Decide(mode, intent);
        }

#if DEBUG
        private sealed class InlineRule : ISizeRule
        {
            public bool TryDecide(RiskMode mode, PositionIntent intent, out SizeDecision decision)
            {
                decision = new SizeDecision(7, "inline", mode);
                return true;
            }
        }

        internal static class RuleBasedSizingTests
        {
            internal static void SmokeTest()
            {
                var sizing = new RuleBasedSizing(new ISizeRule[] { new InlineRule() }, new FixedQuantitySizing(1));
                var decision = sizing.Decide(RiskMode.ECP, new PositionIntent("ES", PositionSide.Long));
                Debug.Assert(decision.Quantity == 7);
                Debug.Assert(decision.Reason == "inline");
                Debug.Assert(decision.RiskModeUsed == RiskMode.ECP);
            }
        }
#endif
    }
}

