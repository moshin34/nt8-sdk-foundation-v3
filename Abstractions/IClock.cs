using System;

namespace NT8.SDK
{
    /// <summary>Abstraction for a UTC time source.</summary>
    public interface IClock
    {
        /// <summary>Current UTC timestamp.</summary>
        DateTime UtcNow { get; }
    }
}
