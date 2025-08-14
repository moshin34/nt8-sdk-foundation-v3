using System;
using NT8.SDK;

namespace NT8.SDK.Facade
{
    /// <summary>
    /// Describes which subsystems are present in a built SDK instance.
    /// </summary>
    public sealed class SdkCapabilities
    {
        /// <summary>True when the facade has a non-null risk engine.</summary>
        public bool HasRisk { get; set; }

        /// <summary>True when the facade has a non-null sizing engine.</summary>
        public bool HasSizing { get; set; }

        /// <summary>True when the facade has a non-null order router.</summary>
        public bool HasOrders { get; set; }

        /// <summary>True when the facade has a non-null session service.</summary>
        public bool HasSession { get; set; }

        /// <summary>True when the facade has a non-null trailing service.</summary>
        public bool HasTrailing { get; set; }

        /// <summary>True when the facade has a non-null telemetry sink.</summary>
        public bool HasTelemetry { get; set; }

        /// <summary>True when the facade has a non-null diagnostics sink.</summary>
        public bool HasDiagnostics { get; set; }

        /// <summary>True when the facade has a non-null backtest hooks object.</summary>
        public bool HasBacktest { get; set; }
    }
}
