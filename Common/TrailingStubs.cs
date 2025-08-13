using System;

namespace NT8.SDK.Trailing
{
    /// <summary>
    /// Placeholder trailing stop interface for compilation.
    /// </summary>
    public interface ITrailingStop
    {
        /// <summary>Computes next stop value.</summary>
        double Compute(double entry, double current, bool isLong, double priorStop);
    }

    /// <summary>
    /// Fixed ticks trailing stop placeholder.
    /// </summary>
    public sealed class FixedTicksTrailingStop : ITrailingStop
    {
        /// <summary>Initializes a new instance of the <see cref="FixedTicksTrailingStop"/> class.</summary>
        public FixedTicksTrailingStop(int ticks, double tickSize)
        {
        }

        /// <inheritdoc/>
        public double Compute(double entry, double current, bool isLong, double priorStop)
        {
            return priorStop;
        }
    }

    /// <summary>
    /// Adapter bridging <see cref="ITrailingStop"/> to <see cref="NT8.SDK.ITrailing"/>.
    /// </summary>
    public sealed class TrailingAdapter : NT8.SDK.ITrailing
    {
        /// <summary>Initializes a new instance of the <see cref="TrailingAdapter"/> class.</summary>
        public TrailingAdapter(ITrailingStop inner)
        {
        }

        /// <inheritdoc/>
        public decimal ComputeStop(decimal entry, decimal current, bool isLong, NT8.SDK.TrailingProfile profile, decimal priorStop)
        {
            return priorStop;
        }
    }
}
