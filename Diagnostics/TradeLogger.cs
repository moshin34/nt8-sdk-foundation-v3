using System;
using NT8.SDK;

namespace NT8.SDK.Diagnostics
{
    /// <summary>
    /// Bounded in-memory log for trade lifecycle events.
    /// </summary>
    public sealed class TradeLogger
    {
        /// <summary>
        /// Describes a trade-related event.
        /// </summary>
        [Serializable]
        public struct TradeEvent
        {
            /// <summary>Timestamp in ET.</summary>
            public DateTime EtTimestamp;
            /// <summary>Instrument symbol.</summary>
            public string Symbol;
            /// <summary>Event tag (Submit, Fill, etc.).</summary>
            public string Event;
            /// <summary>Additional detail text.</summary>
            public string Detail;
        }

        private readonly TradeEvent[] _buffer;
        private readonly object _lock = new object();
        private int _next;
        private int _count;

        /// <summary>
        /// Initializes a new logger with the specified capacity.
        /// </summary>
        /// <param name="capacity">Maximum number of events stored.</param>
        public TradeLogger(int capacity = 1024)
        {
            if (capacity < 1) capacity = 1;
            _buffer = new TradeEvent[capacity];
            _next = 0;
            _count = 0;
        }

        /// <summary>
        /// Gets the maximum number of events stored in the log.
        /// </summary>
        public int Capacity { get { return _buffer.Length; } }

        /// <summary>
        /// Appends an event to the log.
        /// </summary>
        /// <param name="evt">Trade event to append.</param>
        public void Append(TradeEvent evt)
        {
            evt.Symbol = evt.Symbol ?? string.Empty;
            evt.Event = evt.Event ?? string.Empty;
            evt.Detail = evt.Detail ?? string.Empty;
            lock (_lock)
            {
                _buffer[_next] = evt;
                _next = (_next + 1) % _buffer.Length;
                if (_count < _buffer.Length) _count++;
            }
        }

        /// <summary>
        /// Records a submission of a new order intent.
        /// </summary>
        /// <param name="intent">Order intent.</param>
        public void OnSubmit(OrderIntent intent)
        {
            var detail = string.Format("{0}|{1}|{2}|{3}|{4}",
                intent.IsLong ? "Buy" : "Sell",
                intent.Quantity,
                intent.Price,
                intent.Type,
                intent.Signal ?? string.Empty);
            var evt = new TradeEvent
            {
                EtTimestamp = DateTime.UtcNow,
                Symbol = intent.Symbol ?? string.Empty,
                Event = "Submit",
                Detail = detail
            };
            Append(evt);
        }

        /// <summary>
        /// Records a modification of existing orders.
        /// </summary>
        /// <param name="ids">Order identifiers.</param>
        /// <param name="intent">New order intent values.</param>
        public void OnModify(OrderIds ids, OrderIntent intent)
        {
            var detail = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                ids.EntryId ?? string.Empty,
                ids.StopId ?? string.Empty,
                ids.TargetId ?? string.Empty,
                intent.IsLong ? "Buy" : "Sell",
                intent.Quantity,
                intent.Price,
                intent.Type);
            var evt = new TradeEvent
            {
                EtTimestamp = DateTime.UtcNow,
                Symbol = intent.Symbol ?? string.Empty,
                Event = "Modify",
                Detail = detail
            };
            Append(evt);
        }

        /// <summary>
        /// Records cancellation of existing orders.
        /// </summary>
        /// <param name="ids">Order identifiers.</param>
        /// <param name="symbol">Instrument symbol.</param>
        public void OnCancel(OrderIds ids, string symbol)
        {
            var detail = string.Format("{0}|{1}|{2}",
                ids.EntryId ?? string.Empty,
                ids.StopId ?? string.Empty,
                ids.TargetId ?? string.Empty);
            var evt = new TradeEvent
            {
                EtTimestamp = DateTime.UtcNow,
                Symbol = symbol ?? string.Empty,
                Event = "Cancel",
                Detail = detail
            };
            Append(evt);
        }

        /// <summary>
        /// Records a fill event.
        /// </summary>
        /// <param name="symbol">Instrument symbol.</param>
        /// <param name="isBuy">True if a buy fill.</param>
        /// <param name="quantity">Fill quantity.</param>
        /// <param name="price">Fill price.</param>
        public void OnFill(string symbol, bool isBuy, int quantity, decimal price)
        {
            var detail = string.Format("{0} {1} @ {2}", isBuy ? "Buy" : "Sell", quantity, price);
            var evt = new TradeEvent
            {
                EtTimestamp = DateTime.UtcNow,
                Symbol = symbol ?? string.Empty,
                Event = "Fill",
                Detail = detail
            };
            Append(evt);
        }

        /// <summary>
        /// Records a stop being hit.
        /// </summary>
        /// <param name="symbol">Instrument symbol.</param>
        /// <param name="price">Stop price.</param>
        public void OnStop(string symbol, decimal price)
        {
            var evt = new TradeEvent
            {
                EtTimestamp = DateTime.UtcNow,
                Symbol = symbol ?? string.Empty,
                Event = "Stop",
                Detail = price.ToString()
            };
            Append(evt);
        }

        /// <summary>
        /// Records a target fill.
        /// </summary>
        /// <param name="symbol">Instrument symbol.</param>
        /// <param name="price">Target price.</param>
        public void OnTarget(string symbol, decimal price)
        {
            var evt = new TradeEvent
            {
                EtTimestamp = DateTime.UtcNow,
                Symbol = symbol ?? string.Empty,
                Event = "Target",
                Detail = price.ToString()
            };
            Append(evt);
        }

        /// <summary>
        /// Returns a chronological snapshot of the log contents.
        /// </summary>
        /// <returns>Array of events from oldest to newest.</returns>
        public TradeEvent[] Snapshot()
        {
            lock (_lock)
            {
                var result = new TradeEvent[_count];
                var idx = (_next - _count + _buffer.Length) % _buffer.Length;
                for (int i = 0; i < _count; i++)
                {
                    result[i] = _buffer[idx];
                    idx = (idx + 1) % _buffer.Length;
                }
                return result;
            }
        }

#if DEBUG
        internal static class TradeLoggerTests
        {
            internal static void Smoke()
            {
                var log = new TradeLogger(2);
                log.Append(new TradeEvent { EtTimestamp = DateTime.UtcNow, Symbol = "ES", Event = "A", Detail = "" });
                log.Append(new TradeEvent { EtTimestamp = DateTime.UtcNow, Symbol = "ES", Event = "B", Detail = "" });
                log.Append(new TradeEvent { EtTimestamp = DateTime.UtcNow, Symbol = "ES", Event = "C", Detail = "" });
                var snap = log.Snapshot();
                System.Diagnostics.Debug.Assert(snap.Length == 2);
                System.Diagnostics.Debug.Assert(snap[0].Event == "B");
                System.Diagnostics.Debug.Assert(snap[1].Event == "C");
            }
        }
#endif
    }
}

