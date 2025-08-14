using System;
using System.Collections.Generic;

namespace NT8.SDK.Common
{
    /// <summary>
    /// Simple per-symbol P&amp;L tracker with average price accounting.
    /// Positive position = long, negative = short. Pure math (no broker/NT8 types).
    /// </summary>
    public sealed class PnLTracker
    {
        private sealed class State
        {
            public int Qty;            // signed quantity; >0 long, <0 short
            public decimal AvgPrice;   // average price of open position
            public decimal Realized;   // cumulative realized PnL
        }

        private readonly Dictionary<string, State> _bySymbol = new Dictionary<string, State>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Resets all symbols.</summary>
        public void Reset() { _bySymbol.Clear(); }

        /// <summary>Returns the signed position quantity for a symbol (0 if none).</summary>
        public int Position(string symbol) { return Get(symbol).Qty; }

        /// <summary>Returns the average price of the open position (0 if flat).</summary>
        public decimal AvgPrice(string symbol) { return Get(symbol).Qty == 0 ? 0m : Get(symbol).AvgPrice; }

        /// <summary>Returns cumulative realized PnL for the symbol.</summary>
        public decimal Realized(string symbol) { return Get(symbol).Realized; }

        /// <summary>
        /// Returns unrealized PnL given a current price and an optional point value (multiplier).
        /// </summary>
        public decimal Unrealized(string symbol, decimal currentPrice, decimal pointValue)
        {
            var s = Get(symbol);
            if (s.Qty == 0) return 0m;
            var diff = (currentPrice - s.AvgPrice);
            if (s.Qty < 0) diff = -diff; // short gains when price falls
            return diff * Math.Abs(s.Qty) * pointValue;
        }

        /// <summary>
        /// Processes a fill. isBuy=true for buy, false for sell. Quantity must be &gt; 0.
        /// Uses FIFO-like average price accounting.
        /// </summary>
        public void OnFill(string symbol, bool isBuy, int quantity, decimal price, decimal pointValue)
        {
            if (quantity <= 0) return;
            var s = Get(symbol);
            int signed = isBuy ? quantity : -quantity;

            // If adding to same direction, update weighted average.
            if ((s.Qty >= 0 && signed > 0) || (s.Qty <= 0 && signed < 0))
            {
                int newQty = s.Qty + signed;
                if (s.Qty == 0)
                {
                    s.AvgPrice = price;
                }
                else
                {
                    decimal notional = (s.AvgPrice * Math.Abs(s.Qty)) + (price * Math.Abs(signed));
                    s.AvgPrice = notional / (Math.Abs(newQty));
                }
                s.Qty = newQty;
                return;
            }

            // Opposite direction: realize PnL up to flatten or flip
            int closeQty = Math.Min(Math.Abs(s.Qty), Math.Abs(signed));
            if (closeQty > 0)
            {
                decimal pnlPerUnit = (price - s.AvgPrice);
                if (s.Qty < 0) pnlPerUnit = -pnlPerUnit; // short: profits when price < avg
                s.Realized += pnlPerUnit * closeQty * pointValue;
                s.Qty += isBuy ? closeQty : -closeQty; // closing portion reduces magnitude
            }

            // If flip remains, reset avg on the new side
            int remaining = signed + (isBuy ? -closeQty : closeQty);
            if (remaining != 0)
            {
                s.AvgPrice = price;
                s.Qty += remaining;
            }
            else if (s.Qty == 0)
            {
                s.AvgPrice = 0m;
            }
        }

        private State Get(string symbol)
        {
            State s;
            if (!_bySymbol.TryGetValue(symbol, out s))
            {
                s = new State();
                _bySymbol[symbol] = s;
            }
            return s;
        }

#if DEBUG
        internal static class PnLTrackerTests
        {
            internal static void Smoke()
            {
                var t = new PnLTracker();
                t.OnFill("ES", true, 2, 100m, 1m);     // long 2 @100
                t.OnFill("ES", false, 1, 105m, 1m);    // close 1 @105 → +5
                System.Diagnostics.Debug.Assert(t.Realized("ES") == 5m);
                System.Diagnostics.Debug.Assert(t.Position("ES") == 1);
                System.Diagnostics.Debug.Assert(t.Unrealized("ES", 104m, 1m) == 4m);
            }
        }
#endif
    }
}

