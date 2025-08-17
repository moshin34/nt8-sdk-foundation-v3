using System;

namespace NT8.SDK
{
    /// <summary>
    /// Build metadata surfaced to the SDK facade. Values are populated from env vars in CI if present,
    /// with safe defaults for local/dev usage.
    /// </summary>
    public static class SdkInfo
    {
        public static string Name => "NT8.SDK.Foundation";
        public static string Version =>
            Environment.GetEnvironmentVariable("SDK_VERSION") ?? "0.0.0-dev";
        public static string Commit =>
            Environment.GetEnvironmentVariable("GIT_COMMIT") ?? "dev";
        public static string BuildDateUtc =>
            Environment.GetEnvironmentVariable("BUILD_DATE_UTC") ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}
