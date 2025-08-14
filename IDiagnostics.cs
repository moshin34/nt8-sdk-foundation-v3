using System;

namespace NT8.SDK
{
    /// <summary>
    /// Diagnostics capture services.
    /// </summary>
    public interface IDiagnostics
    {
        /// <summary>
        /// Gets or sets whether diagnostics are enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Captures a diagnostic snapshot.
        /// </summary>
        /// <param name="snapshot">Object to capture.</param>
        /// <param name="tag">Tag describing the snapshot.</param>
        void Capture(object snapshot, string tag);
    }
}
