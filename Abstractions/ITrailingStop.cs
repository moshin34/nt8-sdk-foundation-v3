using System;

namespace NT8.SDK.Abstractions
{
    /// <summary>Portable position side for SDK abstractions.</summary>
    

    /// <summary>
    /// Minimal trailing stop abstraction for portable layers (no NinjaTrader types).
    /// Implementations should update StopPrice monotonically in the direction of profit.
    /// </summary>
    public interface ITrailingStop
    {
        /// <summary>The side (Long/Short/Flat) this stop protects.</summary>
        PositionSide Side { get; }

        /// <summary>The current stop price.</summary>
        double StopPrice { get; }

        /// <summary>Reset the trailing stop for a new position.</summary>
        /// <param name="side">Position side.</param>
        /// <param name="entryPrice">Entry price for the position.</param>
        void Reset(PositionSide side, double entryPrice);

        /// <summary>Update the trailing stop with a new tick.</summary>
        /// <param name="time">UTC time of the tick.</param>
        /// <param name="lastPrice">Last traded price.</param>
        void OnPriceTick(DateTime time, double lastPrice);
    }
}