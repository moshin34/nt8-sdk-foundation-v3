namespace NT8.SDK.Common
{
    public static class StartupBanner
    {
        public static string Get()
        {
            return "NT8.SDK v" + SdkVersionInfo.Version;
        }
    }
}