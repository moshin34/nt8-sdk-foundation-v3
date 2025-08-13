using System;

namespace NT8.SDK.Harness
{
    /// <summary>
    /// Produces bootstrap statistics from a population of returns.
    /// </summary>
    public class MonteCarloBootstrapper
    {
        private readonly Random _rng;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonteCarloBootstrapper"/> class.
        /// </summary>
        /// <param name="seed">Seed for the random generator.</param>
        public MonteCarloBootstrapper(int seed = 123456)
        {
            _rng = new Random(seed);
        }

        /// <summary>
        /// Performs bootstrap resampling and returns aggregate statistics.
        /// </summary>
        /// <param name="population">Population of returns.</param>
        /// <param name="resamples">Number of resamples.</param>
        /// <returns>Bootstrap statistics.</returns>
        public Stats Run(double[] population, int resamples)
        {
            if (population == null || population.Length == 0 || resamples < 1)
                return new Stats();

            int n = population.Length;
            double[] means = new double[resamples];
            for (int r = 0; r < resamples; r++)
            {
                double sum = 0.0;
                for (int i = 0; i < n; i++)
                {
                    int index = _rng.Next(n);
                    sum += population[index];
                }

                means[r] = sum / n;
            }

            Array.Sort(means);
            double mean = 0.0;
            double sq = 0.0;
            for (int i = 0; i < resamples; i++)
            {
                mean += means[i];
                sq += means[i] * means[i];
            }
            mean /= resamples;
            double variance = (sq / resamples) - (mean * mean);
            if (variance < 0) variance = 0.0;
            double std = Math.Sqrt(variance);
            int p5 = (int)((resamples - 1) * 0.05);
            int p95 = (int)((resamples - 1) * 0.95);
            return new Stats(mean, std, means[p5], means[p95]);
        }

        /// <summary>
        /// Bootstrap result metrics.
        /// </summary>
        public struct Stats
        {
            /// <summary>Mean of resampled means.</summary>
            public double Mean;

            /// <summary>Standard deviation of resampled means.</summary>
            public double StdDev;

            /// <summary>5th percentile of resampled means.</summary>
            public double Percentile5;

            /// <summary>95th percentile of resampled means.</summary>
            public double Percentile95;

            /// <summary>
            /// Initializes a new instance of the <see cref="Stats"/> struct.
            /// </summary>
            public Stats(double mean, double stdDev, double p5, double p95)
            {
                Mean = mean;
                StdDev = stdDev;
                Percentile5 = p5;
                Percentile95 = p95;
            }
        }
    }
}

#if DEBUG
namespace NT8.SDK.Harness
{
    internal static class MonteCarloBootstrapperDebug
    {
        public static void Smoke()
        {
            MonteCarloBootstrapper boot = new MonteCarloBootstrapper(1);
            double[] pop = new double[] { -1.0, 0.5, 0.2, 0.1 };
            MonteCarloBootstrapper.Stats stats = boot.Run(pop, 1000);
            System.Console.WriteLine("MC mean: " + stats.Mean + " std: " + stats.StdDev);
        }
    }
}
#endif
