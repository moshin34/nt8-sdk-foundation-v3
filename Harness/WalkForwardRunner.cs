using System;
using System.Collections.Generic;
using NT8.SDK.Common;
using NT8.SDK.QA.TestKit;
using NT8.SDK.Strategies;

namespace NT8.SDK.Harness
{
    /// <summary>
    /// Runs walk-forward segments using the backtest harness.
    /// </summary>
    public class WalkForwardRunner
    {
        /// <summary>Walk-forward window definition.</summary>
        public struct Window
        {
            /// <summary>Training bars.</summary>
            public IEnumerable<Bar> Train;

            /// <summary>Testing bars.</summary>
            public IEnumerable<Bar> Test;
        }

        /// <summary>Factory result for creating strategy instances.</summary>
        public struct Pair
        {
            /// <summary>SDK facade.</summary>
            public ISdk Sdk;

            /// <summary>Strategy instance.</summary>
            public StrategyBase Strategy;
        }

        /// <summary>Aggregated walk-forward results.</summary>
        public struct Result
        {
            /// <summary>Total number of test bars processed.</summary>
            public int BarsTested;

            /// <summary>Number of trade intents submitted.</summary>
            public int TradesSubmitted;

            /// <summary>Winning trades (placeholder).</summary>
            public int Wins;

            /// <summary>Losing trades (placeholder).</summary>
            public int Losses;
        }

        /// <summary>
        /// Executes the walk-forward process.
        /// </summary>
        /// <param name="windows">Sequence of training/testing windows.</param>
        /// <param name="factory">Factory for strategy/SDK pairs.</param>
        /// <returns>Aggregated results.</returns>
        public Result Run(IEnumerable<Window> windows, Func<Pair> factory)
        {
            Result result = new Result();
            if (windows == null || factory == null)
                return result;

            foreach (Window w in windows)
            {
                Pair pair = factory();
                BacktestHarness harness = new BacktestHarness(pair.Sdk, pair.Strategy);

                harness.Run(w.Train);

                OrderRouter router = pair.Sdk != null ? pair.Sdk.Orders as OrderRouter : null;
                int before = router != null ? router.Snapshot().Length : 0;
                int bars = harness.Run(w.Test);
                int after = router != null ? router.Snapshot().Length : 0;

                result.BarsTested += bars;
                result.TradesSubmitted += after - before;
            }

            return result;
        }
    }
}

#if DEBUG
namespace NT8.SDK.Harness
{
    internal static class WalkForwardRunnerDebug
    {
        public static void Smoke()
        {
            DateTime start = new DateTime(2024, 1, 1);
            List<Bar> data = new List<Bar>(NT8.SDK.QA.TestKit.SyntheticData.TrendingUp("SYM", start, 6, 100m, 1m));
            WalkForwardRunner.Window[] windows = new WalkForwardRunner.Window[2];
            windows[0] = new WalkForwardRunner.Window { Train = data.GetRange(0, 3).ToArray(), Test = data.GetRange(3, 1).ToArray() };
            windows[1] = new WalkForwardRunner.Window { Train = data.GetRange(1, 3).ToArray(), Test = data.GetRange(4, 2).ToArray() };
            WalkForwardRunner runner = new WalkForwardRunner();
            WalkForwardRunner.Result result = runner.Run(windows, delegate { return new WalkForwardRunner.Pair { Sdk = null, Strategy = new NT8.SDK.Strategies.TemplateStrategy(null, "SYM") }; });
            System.Console.WriteLine("WF bars: " + result.BarsTested);
        }
    }
}
#endif
