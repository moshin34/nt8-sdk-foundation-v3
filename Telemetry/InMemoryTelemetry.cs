using System;
using NT8.SDK;

namespace NT8.SDK.Telemetry
{
    /// <summary>
    /// In-memory telemetry service that stores a bounded log of recent telemetry events.
    /// </summary>
    public sealed class InMemoryTelemetry : ITelemetry
    {
        private readonly TelemetryEvent[] _buffer;
        private readonly object _sync = new object();
        private int _next;
        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryTelemetry"/> class.
        /// </summary>
        /// <param name="capacity">Maximum number of events to retain.</param>
        public InMemoryTelemetry(int capacity = 512)
        {
            if (capacity < 1) capacity = 1;
            _buffer = new TelemetryEvent[capacity];
        }

        /// <inheritdoc />
        public void Emit(TelemetryEvent evt)
        {
            lock (_sync)
            {
                _buffer[_next] = evt;
                _next = (_next + 1) % _buffer.Length;
                if (_count < _buffer.Length) _count++;
            }
        }

        /// <summary>
        /// Gets a snapshot of the currently stored telemetry events.
        /// </summary>
        /// <returns>Events in chronological order (most recent last).</returns>
        public TelemetryEvent[] Snapshot()
        {
            lock (_sync)
            {
                TelemetryEvent[] result = new TelemetryEvent[_count];
                int cap = _buffer.Length;
                int start = (_next - _count + cap) % cap;
                for (int i = 0; i < _count; i++)
                {
                    int idx = (start + i) % cap;
                    result[i] = _buffer[idx];
                }
                return result;
            }
        }
    }
}

