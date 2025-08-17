using System;

namespace NT8.SDK.Risk
{
    /// <summary>
    /// Tracks the highest equity value and determines when the trailing drawdown limit is breached.
    /// </summary>
    public class TrailingDrawdown
    {
        private readonly double _trailingLimit;
        private double _maxEquity;

        /// <summary>Initializes a new instance of the <see cref="TrailingDrawdown"/> class.</summary>
        /// <param name="initialEquity">Initial net equity value.</param>
        /// <param name="trailingLimit">Maximum allowed drawdown from the highest equity value.</param>
        public TrailingDrawdown(double initialEquity, double trailingLimit)
        {
            _maxEquity = initialEquity;
            _trailingLimit = trailingLimit;
        }

        /// <summary>Updates the internal maximum equity value.</summary>
        /// <param name="netEquity">Current net equity value.</param>
        public void ApplyPnL(double netEquity)
        {
            if (netEquity > _maxEquity)
            {
                _maxEquity = netEquity;
            }
        }

        /// <summary>Determines whether the trailing drawdown has been breached.</summary>
        /// <param name="netEquity">Current net equity value.</param>
        /// <returns>True if the account is locked out; otherwise, false.</returns>
        public bool IsLocked(double netEquity)
        {
            return netEquity < (_maxEquity - _trailingLimit);
        }
    }
}

