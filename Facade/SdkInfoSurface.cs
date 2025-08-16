using System;

namespace NT8.SDK.Facade
{
    /// <summary>
    /// Guard-safe smoke surface for quick E2E checks without touching NT types.
    /// </summary>
    public static class SdkInfoSurface
    {
        /// <summary>Returns a human-readable banner with version and last-tick presence.</summary>
        public static string Describe(ISdk sdk)
        {
            if (sdk == null) return ""(null sdk)"";
            var banner = sdk.StartupBanner ?? ""(no banner)"";
            // Keep it pure/read-only — we won't pull the last tick here until we add a query to the interface.
            return banner;
        }
    }
}