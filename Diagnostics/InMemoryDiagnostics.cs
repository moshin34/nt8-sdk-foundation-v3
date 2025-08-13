using System;
using NT8.SDK;

namespace NT8.SDK.Diagnostics
{
    /// <summary>
    /// In-memory diagnostics capture that stores recent events in a bounded ring buffer.
    /// </summary>
    public sealed class InMemoryDiagnostics : IDiagnostics
    {
        private readonly DiagnosticsEvent[] _buffer;
        private readonly object _sync = new object();
        private int _next;
        private int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDiagnostics"/> class.
        /// </summary>
        /// <param name="capacity">Maximum number of events to retain.</param>
        public InMemoryDiagnostics(int capacity = 512)
        {
            if (capacity < 1) capacity = 1;
            _buffer = new DiagnosticsEvent[capacity];
        }

        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public void Capture(object snapshot, string tag)
        {
            if (!Enabled) return;

            DiagnosticsEvent evt = new DiagnosticsEvent(tag ?? string.Empty, snapshot != null ? snapshot.ToString() : string.Empty);
            lock (_sync)
            {
                _buffer[_next] = evt;
                _next = (_next + 1) % _buffer.Length;
                if (_count < _buffer.Length) _count++;
            }
        }

        /// <summary>
        /// Gets a snapshot of the captured diagnostic events.
        /// </summary>
        /// <returns>Events in chronological order (most recent last).</returns>
        public DiagnosticsEvent[] Snapshot()
        {
            lock (_sync)
            {
                DiagnosticsEvent[] result = new DiagnosticsEvent[_count];
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

