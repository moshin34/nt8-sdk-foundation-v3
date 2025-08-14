using System;

namespace NT8.SDK.Telemetry
{
    /// <summary>No-op telemetry sink.</summary>
    public sealed class NoopTelemetry : ITelemetry
    {
        public void Emit(TelemetryEvent evt)
        {
            // swallow
        }
    }
}

