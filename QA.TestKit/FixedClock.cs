using System;

namespace NT8.SDK.QA.TestKit
{
    /// <summary>Deterministic clock for tests and demos.</summary>
    public sealed class FixedClock : IClock
    {
        private DateTime _utcNow;

        /// <summary>Creates a clock pinned at <paramref name="utcStart"/>.</summary>
        public FixedClock(DateTime utcStart)
        {
            // Ensure it's UTC; if not, assume it's already UTC semantics for simplicity.
            _utcNow = utcStart.Kind == DateTimeKind.Utc
                ? utcStart
                : DateTime.SpecifyKind(utcStart, DateTimeKind.Utc);
        }

        /// <summary>Current UTC time.</summary>
        public DateTime UtcNow { get { return _utcNow; } }

        /// <summary>Advances the clock by <paramref name="delta"/>.</summary>
        public void Advance(TimeSpan delta)
        {
            _utcNow = _utcNow + delta;
        }
    }
}
