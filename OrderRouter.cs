using System;
using System.Collections.Generic; // for EqualityComparer<T>
using NT8.SDK; // IOrders, OrderIds, OrderIntent

namespace NT8.SDK.Common
{
    /// <summary>
    /// Pure C# in-memory order router implementing <see cref="IOrders"/>.
    /// Generates synthetic order IDs and keeps a small log for diagnostics.
    /// Placeholder for platforms that do not provide a broker bridge.
    /// </summary>
    public sealed class OrderRouter : IOrders
    {
        private readonly List<OrderIntent> _log = new List<OrderIntent>();
        private readonly object _sync = new object();

        /// <summary>Maximum number of intents to retain in memory.</summary>
        public int Capacity { get; private set; }

        public OrderRouter(int capacity = 512)
        {
            if (capacity < 1) capacity = 1;
            Capacity = capacity;
        }

        /// <inheritdoc/>
        public OrderIds Submit(OrderIntent intent)
        {
            if (IsDefault(intent)) throw new ArgumentNullException(nameof(intent));
            lock (_sync) Enqueue(intent);

            string stamp = DateTime.UtcNow.Ticks.ToString();
            string baseId = (intent.Signal ?? "ORD") + "-" + stamp;
            return new OrderIds(baseId + "-E", baseId + "-S", baseId + "-T");
        }

        /// <inheritdoc/>
        public void Cancel(OrderIds ids)
        {
            // No-op for now; a real bridge would forward cancel to broker.
            if (IsDefault(ids)) return;
        }

        /// <inheritdoc/>
        public void Modify(OrderIds ids, OrderIntent intent)
        {
            // Log the modify intent; a real bridge would forward a replace request.
            if (IsDefault(ids) || IsDefault(intent)) return;
            lock (_sync) Enqueue(intent);
        }

        /// <summary>Snapshot of the current intent log.</summary>
        public OrderIntent[] Snapshot()
        {
            lock (_sync) return _log.ToArray();
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

        // Works for both classes (null) and structs (default)
        private static bool IsDefault<T>(T value)
            => EqualityComparer<T>.Default.Equals(value, default);
    }
}
