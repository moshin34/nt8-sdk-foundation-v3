#region Using declarations
using System;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.StrategyGenerator;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// Canonical compile-safe strategy template for NinjaTrader 8.
    /// </summary>
    public class MyStrategyTemplate : Strategy
    {
        private int period = 14;
        private bool debugMode = false;

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period", Order = 1, GroupName = "Parameters")]
        public int Period
        {
            get { return period; }
            set { period = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Debug Mode", Order = 2, GroupName = "Parameters")]
        public bool DebugMode
        {
            get { return debugMode; }
            set { debugMode = value; }
        }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "MyStrategyTemplate";
                Description = "Canonical compile-safe NinjaScript template.";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                BarsRequiredToTrade = 20;

                Period = 14;
                DebugMode = false;
            }
            else if (State == State.Configure)
            {
                SetStopLoss(CalculationMode.Ticks, 10);
            }
            else if (State == State.DataLoaded)
            {
                if (DebugMode)
                    Print("=== MyStrategyTemplate initialized ===");
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade)
                return;

            if (Position.MarketPosition == MarketPosition.Flat)
            {
                EnterLong("LongEntry");

                if (DebugMode)
                    Print(Time[0] + " -> EnterLong executed @ " + Close[0]);
            }
            else if (DebugMode)
            {
                Print(Time[0] + " -> Current position: " + Position.MarketPosition);
            }
        }

        protected override void OnOrderUpdate(
            Order order, double limitPrice, double stopPrice, int quantity,
            int filled, double averageFillPrice, OrderState orderState,
            DateTime time, ErrorCode error, string nativeError)
        {
            if (DebugMode && order != null)
                Print($"[OnOrderUpdate] {time:u} {order.Name} {orderState} qty={quantity} filled={filled} avg={averageFillPrice}");
        }

        protected override void OnExecutionUpdate(
            Execution execution, string executionId, double price, int quantity,
            MarketPosition marketPosition, string orderId, DateTime time)
        {
            if (DebugMode && execution != null)
                Print($"[OnExecutionUpdate] {time:u} {execution.Order?.Name} {marketPosition} qty={quantity} price={price}");
        }
    }
}
