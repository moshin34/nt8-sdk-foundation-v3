using System;

namespace NT8.SDK.Common
{
    /// <summary>
    /// Builds a startup banner string using SdkVersion.
    /// </summary>
    public static class StartupBanner
    {
        /// <summary>Returns a human-readable SDK banner.</summary>
        public static string Get()
        {
            // Keep it simple for maximum compatibility.
            return "NT8.SDK v" + SdkVersion.Version;
        }
    }
}