using System;

namespace NT8.SDK
{
    /// <summary>
    /// Build metadata for the SDK facade. Values come from CI env vars when present,
    /// with safe local defaults so builds never fail on missing metadata.
    /// </summary>
    public static class SdkInfo
    {
        public static string Name         => "NT8.SDK.Foundation";
        public static string Version      => Environment.GetEnvironmentVariable("SDK_VERSION")     ?? "0.0.0-dev";
        public static string Commit       => Environment.GetEnvironmentVariable("GIT_COMMIT")      ?? "dev";
        public static string BuildDateUtc => Environment.GetEnvironmentVariable("BUILD_DATE_UTC")
                                             ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}
