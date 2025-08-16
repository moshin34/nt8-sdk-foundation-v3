using System;

namespace NT8.SDK
{
    /// <summary>
    /// Trading session awareness services.
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// Determines if the current time is within a blackout window.
        /// </summary>
        /// <param name="etNow">Current eastern time.</param>
        /// <param name="symbol">Symbol of interest.</param>
        /// <returns>True if trading is blacked out.</returns>
        bool IsBlackout(DateTime etNow, string symbol);

        /// <summary>
        /// Determines if settlement restrictions apply.
        /// </summary>
        /// <param name="etNow">Current eastern time.</param>
        /// <param name="symbol">Symbol of interest.</param>
        /// <returns>True if within settlement window.</returns>
        bool IsSettlementWindow(DateTime etNow, string symbol);

        /// <summary>
        /// Gets the session open time for a key.
        /// </summary>
        /// <param name="key">Session key.</param>
        /// <returns>Opening time.</returns>
        DateTime SessionOpen(SessionKey key);

        /// <summary>
        /// Gets the session close time for a key.
        /// </summary>
        /// <param name="key">Session key.</param>
        /// <returns>Closing time.</returns>
        DateTime SessionClose(SessionKey key);
    }
}
