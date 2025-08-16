using System;
using NT8.SDK.Abstractions;

namespace NT8.SDK.Facade
{
    /// <summary>
    /// Guard-safe helper that returns the SDK banner from an ISdk instance.
    /// </summary>
    public static class SdkInfoSurface
    {
        public static string Describe(ISdk sdk)
        {
            if (sdk == null)
            {
                return "(null sdk)";
            }

            string banner = sdk.StartupBanner;
            if (string.IsNullOrEmpty(banner))
            {
                return "(no banner)";
            }

            return banner;
        }
    }
}