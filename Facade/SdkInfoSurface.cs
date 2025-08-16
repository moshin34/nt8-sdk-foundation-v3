using NT8.SDK.Abstractions;

namespace NT8.SDK.Facade
{
    public static class SdkInfoSurface
    {
        public static string Describe(ISdk sdk)
        {
            if (sdk == null) return "(null sdk)";
            var banner = sdk.StartupBanner;
            return string.IsNullOrEmpty(banner) ? "(no banner)" : banner;
        }
    }
}