using System;

namespace NT8.SDK.Diagnostics
{
    /// <summary>Toggleable no-op diagnostics collector.</summary>
    public sealed class NoopDiagnostics : IDiagnostics
    {
        public bool Enabled { get; set; }

        public void Capture(object snapshot, string tag)
        {
            // swallow
        }
    }
}

