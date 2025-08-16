using System;
using System.Collections.Generic;

namespace NT8.SDK.Session
{
    /// <summary>
    /// Minimal in-memory tick buffer kept inside Session. Guard-safe: no cross-layer references, no NT types.
    /// </summary>
    internal sealed class TickBuffer
    {
        private readonly Queue<Tick> _q = new Queue<Tick>();
        private const int Max = 128;

        public void OnTick(DateTime time, double price)
        {
            _q.Enqueue(new Tick(time, price));
            while (_q.Count > Max) _q.Dequeue();
        }

        public int Count { get { return _q.Count; } }

        public bool TryGetLatest(out Tick tick)
        {
            tick = default(Tick);
            if (_q.Count == 0) return false;

            // Walk to the last item without allocating a copy of the queue
            foreach (var t in _q) tick = t;
            return true;
        }
    }

    /// <summary>Simple value type for ticks (Guard-safe; avoids tuples and nullable refs).</summary>
    internal struct Tick
    {
        public DateTime Time;
        public double Price;

        public Tick(DateTime time, double price)
        {
            Time = time;
            Price = price;
        }
    }
}