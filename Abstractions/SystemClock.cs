using System;

namespace NT8.SDK
{
    /// <summary>Default system-backed clock singleton.</summary>
    public sealed class SystemClock : IClock
    {
        private SystemClock() { }
        public static readonly SystemClock Instance = new SystemClock();
        public DateTime UtcNow { get { return DateTime.UtcNow; } }
    }
}
