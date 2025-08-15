using System;

namespace NT8.SDK
{
    /// <summary>
    /// Telemetry emission services.
    /// </summary>
    public interface ITelemetry
    {
        /// <summary>
        /// Emits a telemetry event.
        /// </summary>
        /// <param name="evt">Event to log.</param>
        void Emit(TelemetryEvent evt);
    }
}
