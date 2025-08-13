using System;
using NT8.SDK.Facade;
using NT8.SDK.Common;
using NT8.SDK.Strategies;
using NT8.SDK.QA.TestKit;

namespace NT8.SDK.Harness
{
    /// <summary>
    /// Minimal runner demonstrating the default SDK wiring on synthetic data.
    /// </summary>
    public static class QuickStartRunner
    {
        /// <summary>
        /// Executes a single backtest using the default SDK components and synthetic data.
        /// </summary>
        public static void RunOnce()
        {
            SdkCapabilities caps;
            SdkFacade sdk = Defaults.Build(out caps);
            TemplateStrategy strategy = new TemplateStrategy(sdk, "ES");
            BacktestHarness harness = new BacktestHarness(sdk, strategy);
            Bar[] data = SyntheticData.TrendingUp("ES", new DateTime(2024, 1, 1, 9, 30, 0), 10, 100m, 1m);
            harness.Run(data);
            OrderRouter router = sdk.Orders as OrderRouter;
            int _ = router != null ? router.Snapshot().Length : 0;
        }

#if DEBUG
        internal static void Smoke()
        {
            RunOnce();
        }
#endif
    }
}

