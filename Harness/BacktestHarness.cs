using System;
using System.Collections.Generic;
using NT8.SDK.QA.TestKit;
using NT8.SDK.Strategies;

namespace NT8.SDK.Harness
{
    /// <summary>
    /// Simple deterministic harness driving a strategy over a stream of bars.
    /// </summary>
    public class BacktestHarness
    {
        private readonly ISdk _sdk;
        private readonly StrategyBase _strategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="BacktestHarness"/> class.
        /// </summary>
        /// <param name="sdk">SDK facade.</param>
        /// <param name="strategy">Strategy instance.</param>
        public BacktestHarness(ISdk sdk, StrategyBase strategy)
        {
            _sdk = sdk;
            _strategy = strategy;
        }

        /// <summary>
        /// Runs the strategy over the provided bar sequence.
        /// </summary>
        /// <param name="bars">Bars to process.</param>
        /// <param name="observer">Optional callback invoked on each processed bar.</param>
        /// <returns>Number of bars processed.</returns>
        public int Run(IEnumerable<Bar> bars, Action<Bar> observer = null)
        {
            if (bars == null || _strategy == null)
                return 0;

            decimal last = 0m;
            int count = 0;
            foreach (Bar b in bars)
            {
                _strategy.OnBeforeBar(b.Et, last);
                _strategy.OnBar(b.Et, b.Open, b.High, b.Low, b.Close);
                _strategy.OnAfterBar(b.Et);
                if (observer != null) observer(b);
                last = b.Close;
                count++;
            }

            return count;
        }
    }
}

#if DEBUG
namespace NT8.SDK.Harness
{
    internal static class BacktestHarnessDebug
    {
        public static void Smoke()
        {
            var data = NT8.SDK.QA.TestKit.SyntheticData.TrendingUp("SYM", new DateTime(2024, 1, 1), 3, 100m, 1m);
            var strategy = new NT8.SDK.Strategies.TemplateStrategy(null, "SYM");
            var harness = new BacktestHarness(null, strategy);
            int bars = harness.Run(data);
            System.Console.WriteLine("Harness bars: " + bars);
        }
    }
}
#endif
