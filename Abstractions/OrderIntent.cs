namespace NT8.SDK.Abstractions
{
    /// <summary>High-level intents used by the portable SDK to describe order actions.</summary>
    public enum OrderIntent
    {
        None = 0,
        EnterLong,
        EnterShort,
        ExitLong,
        ExitShort,
        ModifyStop,
        ModifyTarget,
        CancelAll
    }
}