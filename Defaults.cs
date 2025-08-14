using System;
using NT8.SDK;
using NT8.SDK.Facade;
using NT8.SDK.Risk;
using NT8.SDK.Sizing;
using NT8.SDK.Session;
using NT8.SDK.Trailing;
using NT8.SDK.Telemetry;
using NT8.SDK.Diagnostics;

namespace NT8.SDK.Common
{
    /// <summary>
    /// Provides factory methods for constructing SDK facades with conservative defaults.
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// Builds a default SDK facade wired with conservative components.
        /// </summary>
        /// <param name="caps">Receives the capabilities descriptor describing which subsystems are present.</param>
        /// <returns>Fully wired SDK facade instance.</returns>
        public static SdkFacade Build(out SdkCapabilities caps)
        {
            // Risk
            var guard = new LossStreakGuard(2, RiskMode.DCP, "LossStreak");
            var riskComposite = new CompositeRisk(new IRisk[] { guard }, RiskMode.DCP);
            var risk = new RiskManager(riskComposite, RiskMode.DCP) { Enabled = true };

            // Sizing
            var fallback = new BracketedQuantitySizing(1, 2, 3, 1, "bracket");
            var ruleBased = new RuleBasedSizing(new ISizeRule[] { new InlineRule() }, fallback);
            var tiers = RiskTiers.Defaults();
            var sizer = PositionSizer.FromTiers(ruleBased, tiers, RiskMode.DCP);

            // Orders
            var orders = new OrderRouter(256);

            // Session
            var session = new CmeBlackoutService();

            // Trailing
            var trailing = new TrailingAdapter(new FixedTicksTrailingStop(10, 0.25));

            // Telemetry
            var telemetry = new InMemoryTelemetry(256);

            // Diagnostics
            var diagnostics = new InMemoryDiagnostics(256) { Enabled = true };

            var builder = new SdkAdapterV1()
                .WithRisk(risk)
                .WithSizing(sizer)
                .WithOrders(orders)
                .WithSession(session)
                .WithTrailing(trailing)
                .WithTelemetry(telemetry)
                .WithDiagnostics(diagnostics);

            return builder.Build(out caps);
        }

        /// <summary>
        /// Inline sizing rule that always returns a quantity of one.
        /// </summary>
        private sealed class InlineRule : ISizeRule
        {
            /// <summary>
            /// Always produces a sizing decision of one unit.
            /// </summary>
            public bool TryDecide(RiskMode mode, PositionIntent intent, out SizeDecision decision)
            {
                decision = new SizeDecision(1, "inline", mode);
                return true;
            }
        }
    }
}

