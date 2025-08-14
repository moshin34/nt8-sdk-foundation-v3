using NT8.SDK;

namespace NT8.SDK.Telemetry
{
    /// <summary>
    /// No-op telemetry implementation that silently drops all events.
    /// </summary>
    public sealed class NullTelemetry : ITelemetry
    {
        /// <inheritdoc />
        public void Emit(TelemetryEvent evt)
        {
        }
    }
}

