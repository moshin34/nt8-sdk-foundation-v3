using System;

namespace NT8.SDK.Common
{
    /// <summary>Central SDK identity (Guard-safe: Common-only).</summary>
    internal static class SdkInfo
    {
        public const string Version = "0.1.0";
        public static readonly string BuildStamp =
            (Environment.GetEnvironmentVariable("GITHUB_RUN_ID") ?? "local") + "-" +
            DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    }
}