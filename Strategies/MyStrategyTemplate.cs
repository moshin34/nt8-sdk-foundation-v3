#region Strategy Template
using System;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.StrategyGenerator;

/// <summary>
/// Sample strategy template demonstrating basic structure and debug capabilities.
/// </summary>
public class MyStrategyTemplate : Strategy
{
    private int period;
    private bool debugMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyStrategyTemplate"/> class.
    /// </summary>
    public MyStrategyTemplate()
    {
    }

    /// <summary>
    /// Handles state transitions for the strategy.
    /// </summary>
    protected override void OnStateChange()
    {
        if (State == State.SetDefaults)
        {
            Description                                     = "My strategy template.";
            Name                                            = "MyStrategyTemplate";
            Calculate                                       = Calculate.OnBarClose;
            EntriesPerDirection                             = 1;
            EntryHandling                                   = EntryHandling.AllEntries;
            IsExitOnSessionCloseStrategy                    = true;
            ExitOnSessionCloseSeconds                       = 30;
            IsInstantiatedOnEachOptimizationIteration       = false;
            BarsRequiredToTrade                             = 20;

            Period      = 14;
            DebugMode   = false;
        }
        else if (State == State.Configure)
        {
            SetStopLoss(CalculationMode.Ticks, 10);
        }
        else if (State == State.DataLoaded)
        {
            Print("=== MyStrategyTemplate initialized ===");
        }
    }

    /// <summary>
    /// Called on each bar update event.
    /// </summary>
    protected override void OnBarUpdate()
    {
        if (CurrentBar < BarsRequiredToTrade)
            return;

        if (Position.MarketPosition == MarketPosition.Flat)
        {
            EnterLong();

            if (debugMode)
                Print(Time[0] + " EnterLong executed at " + Close[0]);
        }
        else if (debugMode)
        {
            Print(Time[0] + " Current position: " + Position.MarketPosition);
        }
    }

    /// <summary>
    /// Receives execution events for orders.
    /// </summary>
    /// <param name="execution">The execution information.</param>
    /// <param name="executionId">The execution identifier.</param>
    /// <param name="price">The fill price.</param>
    /// <param name="quantity">The fill quantity.</param>
    /// <param name="marketPosition">The resulting market position.</param>
    /// <param name="orderId">The related order identifier.</param>
    /// <param name="time">The execution time.</param>
    protected override void OnExecutionUpdate(
        Execution execution,
        string executionId,
        double price,
        int quantity,
        MarketPosition marketPosition,
        string orderId,
        DateTime time)
    {
        // Placeholder for execution handling logic
    }

    /// <summary>
    /// Receives order status updates.
    /// </summary>
    /// <param name="order">The order being updated.</param>
    /// <param name="limitPrice">The limit price.</param>
    /// <param name="stopPrice">The stop price.</param>
    /// <param name="quantity">The order quantity.</param>
    /// <param name="filled">The filled quantity.</param>
    /// <param name="averageFillPrice">The average fill price.</param>
    /// <param name="orderState">The current order state.</param>
    /// <param name="time">The time of the update.</param>
    /// <param name="error">The error code, if any.</param>
    /// <param name="nativeError">The native error message, if any.</param>
    protected override void OnOrderUpdate(
        Order order,
        double limitPrice,
        double stopPrice,
        int quantity,
        int filled,
        double averageFillPrice,
        OrderState orderState,
        DateTime time,
        ErrorCode error,
        string nativeError)
    {
        // Placeholder for order handling logic
    }

    #region Properties

    /// <summary>
    /// Gets or sets the indicator period.
    /// </summary>
    [NinjaScriptProperty]
    [Range(1, int.MaxValue)]
    [Display(Name = "Period", Order = 1, GroupName = "Parameters")]
    public int Period
    {
        get { return period; }
        set { period = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether debug mode is enabled.
    /// </summary>
    [NinjaScriptProperty]
    [Display(Name = "Debug Mode", Order = 2, GroupName = "Parameters")]
    public bool DebugMode
    {
        get { return debugMode; }
        set { debugMode = value; }
    }

    #endregion
}
#endregion
