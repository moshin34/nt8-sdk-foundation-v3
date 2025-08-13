using System;
using System.Collections.Generic;
using NT8.SDK;

namespace NT8.SDK.Risk
{
    /// <summary>
    /// Combines multiple <see cref="IRisk"/> implementations into a single composite rule set.
    /// </summary>
    public class CompositeRisk : IRisk
    {
        private readonly List<IRisk> _rules;
        private readonly RiskMode _mode;

        /// <summary>Initializes a new instance of the <see cref="CompositeRisk"/> class.</summary>
        /// <param name="rules">Collection of risk rules. Null becomes empty.</param>
        /// <param name="mode">Risk mode exposed by this composite.</param>
        public CompositeRisk(IEnumerable<IRisk> rules, RiskMode mode)
        {
            _rules = rules != null ? new List<IRisk>(rules) : new List<IRisk>();
            _mode = mode;
        }

        /// <summary>Gets the risk mode exposed by this composite.</summary>
        public RiskMode Mode { get { return _mode; } }

        /// <inheritdoc/>
        public string EvaluateEntry(PositionIntent intent)
        {
            for (int i = 0; i < _rules.Count; i++)
            {
                var reason = _rules[i].EvaluateEntry(intent);
                if (!string.IsNullOrEmpty(reason)) return reason;
            }
            return string.Empty;
        }

        /// <inheritdoc/>
        public RiskLockoutState Lockout()
        {
            var state = RiskLockoutState.None;
            for (int i = 0; i < _rules.Count; i++)
            {
                var child = _rules[i].Lockout();
                if (child == RiskLockoutState.LockedOut) return RiskLockoutState.LockedOut;
                if (child == RiskLockoutState.CoolingDown) state = RiskLockoutState.CoolingDown;
            }
            return state;
        }

        /// <inheritdoc/>
        public bool CanTradeNow()
        {
            for (int i = 0; i < _rules.Count; i++)
            {
                if (!_rules[i].CanTradeNow()) return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public void RecordWinLoss(bool win)
        {
            for (int i = 0; i < _rules.Count; i++)
            {
                _rules[i].RecordWinLoss(win);
            }
        }
    }

#if DEBUG
    // Renamed to avoid collisions with other files.
    internal sealed class DbgFakeRiskAllowComposite : IRisk
    {
        public RiskMode Mode { get { return RiskMode.DCP; } }
        public RiskLockoutState Lockout() { return RiskLockoutState.None; }
        public bool CanTradeNow() { return true; }
        public string EvaluateEntry(PositionIntent intent) { return string.Empty; }
        public void RecordWinLoss(bool win) { }
    }

    internal sealed class FakeRiskBlock : IRisk
    {
        private readonly string _reason;
        public FakeRiskBlock(string reason) { _reason = reason; }
        public RiskMode Mode { get { return RiskMode.DCP; } }
        public RiskLockoutState Lockout() { return RiskLockoutState.None; }
        public bool CanTradeNow() { return true; }
        public string EvaluateEntry(PositionIntent intent) { return _reason; }
        public void RecordWinLoss(bool win) { }
    }

    internal sealed class FakeRiskCooling : IRisk
    {
        public RiskMode Mode { get { return RiskMode.DCP; } }
        public RiskLockoutState Lockout() { return RiskLockoutState.CoolingDown; }
        public bool CanTradeNow() { return true; }
        public string EvaluateEntry(PositionIntent intent) { return string.Empty; }
        public void RecordWinLoss(bool win) { }
    }

    internal sealed class FakeRiskLocked : IRisk
    {
        public RiskMode Mode { get { return RiskMode.DCP; } }
        public RiskLockoutState Lockout() { return RiskLockoutState.LockedOut; }
        public bool CanTradeNow() { return true; }
        public string EvaluateEntry(PositionIntent intent) { return string.Empty; }
        public void RecordWinLoss(bool win) { }
    }

    internal static class DebugCompositeRisk
    {
        internal static void Main()
        {
            var composite = new CompositeRisk(new IRisk[] { new DbgFakeRiskAllowComposite(), new FakeRiskBlock("RULE_X") }, RiskMode.DCP);
            Console.WriteLine("Entry decision: '" + composite.EvaluateEntry(new PositionIntent("ES", PositionSide.Long)) + "'");

            var cooling = new CompositeRisk(new IRisk[] { new DbgFakeRiskAllowComposite(), new FakeRiskCooling() }, RiskMode.DCP);
            Console.WriteLine("Lockout state (cooling): " + cooling.Lockout());

            var locked = new CompositeRisk(new IRisk[] { new DbgFakeRiskAllowComposite(), new FakeRiskCooling(), new FakeRiskLocked() }, RiskMode.DCP);
            Console.WriteLine("Lockout state (locked): " + locked.Lockout());
        }
    }
#endif
}
