#if DEBUG
namespace NT8.SDK.Harness
{
    /// <summary>
    /// Entry point for debug builds that executes a single quick start run.
    /// </summary>
    public static class QuickStartProgram
    {
        /// <summary>
        /// Application entry point used when compiling the optional debug runner.
        /// </summary>
        public static void Main()
        {
            QuickStartRunner.RunOnce();
        }
    }
}
#endif
