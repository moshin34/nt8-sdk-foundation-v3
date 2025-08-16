namespace NT8.SDK.Common
{
    /// <summary>Builds a startup banner without side effects (Guard-safe).</summary>
    internal static class StartupBanner
    {
        public static string Get() => $""NT8.SDK v{SdkInfo.Version} ({SdkInfo.BuildStamp}) ready"";
    }
}