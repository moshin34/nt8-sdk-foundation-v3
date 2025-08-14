using NT8.SDK;

namespace NT8.SDK.Diagnostics
{
    /// <summary>
    /// No-op diagnostics capture that discards all input snapshots.
    /// </summary>
    public sealed class NullDiagnostics : IDiagnostics
    {
        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public void Capture(object snapshot, string tag)
        {
        }
    }
}

