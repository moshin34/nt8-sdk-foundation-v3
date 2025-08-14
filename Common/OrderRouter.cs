using System;
using System.Collections.Generic;
using NT8.SDK;

namespace NT8.SDK.Common
{
    /// <summary>
    /// Pure C# in-memory order router implementing <see cref="IOrders"/>.
    /// Generates synthetic order IDs and keeps a small log for diagnostics.
    /// This is a placeholder for platforms that do not provide a broker bridge.
    /// </summary>
    public sealed class OrderRouter : IOrders
    {
        private readonly List<OrderIntent> _log = new List<OrderIntent>();
        private readonly object _sync = new object();

        /// <summary>Maximum number of intents to retain in memory.</summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderRouter"/> class.
        /// </summary>
        /// <param name="capacity">Maximum number of intents kept in memory.</param>
        public OrderRouter(int capacity = 512)
        {
            if (capacity < 1) capacity = 1;
            Capacity = capacity;
        }

        /// <inheritdoc/>
        public OrderIds Submit(OrderIntent intent)
        {
            lock (_sync) Enqueue(intent);
            string stamp = DateTime.UtcNow.Ticks.ToString();
            string baseId = (intent.Signal ?? "ORD") + "-" + stamp;
            return new OrderIds(baseId + "-E", baseId + "-S", baseId + "-T");
        }

        /// <inheritdoc/>
        public bool Cancel(OrderIds ids)
        {
            return true;
        }

        /// <inheritdoc/>
        public bool Modify(OrderIds ids, OrderIntent intent)
        {
            lock (_sync) Enqueue(intent);
            return false;
        }

        /// <summary>
        /// Returns a snapshot copy of the current intent log.
        /// </summary>
        public OrderIntent[] Snapshot()
        {
            lock (_sync)
            {
                return _log.ToArray();
            }
        }

        private void Enqueue(OrderIntent intent)
        {
            _log.Add(intent);
            if (_log.Count > Capacity)
            {
                int remove = _log.Count - Capacity;
                _log.RemoveRange(0, remove);
            }
        }
    }
}
