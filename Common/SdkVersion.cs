using System;

namespace NT8.SDK
{
    /// <summary>Provides version metadata for the SDK assembly.</summary>
    public static class SdkVersion
    {
        /// <summary>Assembly version string, e.g. "1.0.0.0".</summary>
        public static string AssemblyVersion =>
            typeof(SdkVersion).Assembly.GetName().Version?.ToString() ?? "unknown";

        /// <summary>Informational version if present; falls back to AssemblyVersion.</summary>
        public static string Informational
        {
            get
            {
                var attr = Attribute.GetCustomAttribute(
                    typeof(SdkVersion).Assembly,
                    typeof(System.Reflection.AssemblyInformationalVersionAttribute)
                ) as System.Reflection.AssemblyInformationalVersionAttribute;

                return !string.IsNullOrWhiteSpace(attr?.InformationalVersion)
                    ? attr.InformationalVersion
                    : AssemblyVersion;
            }
        }
    }
}
