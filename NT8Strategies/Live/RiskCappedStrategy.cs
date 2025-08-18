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
    /// Risk-capped strategy with ACCOUNT-LEVEL enforcement.
    /// Caps: MaxContracts, DailyLossLimit, WeeklyLossLimit, TrailingDrawdown.
    /// Enforcement: continuous (OnMarketData) + OnBarUpdate.
    /// Breach: cancel all working orders; optionally flatten account (UseAccountFlatten).
    /// </summary>
    public class RiskCappedStrategy : Strategy
    {
        // Baselines / State
        private double dayPnLBase;
        private double weekPnLBase;
        private DateTime weekAnchorDate;
        private double highestEquity;
        private bool printedStartup;

        // ===== Parameters =====
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "MaxContracts", Order = 1, GroupName = "Risk Caps")]
        public int MaxContracts { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "DailyLossLimit", Order = 2, GroupName = "Risk Caps")]
        public double DailyLossLimit { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "WeeklyLossLimit", Order = 3, GroupName = "Risk Caps")]
        public double WeeklyLossLimit { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "TrailingDrawdown", Order = 4, GroupName = "Risk Caps")]
        public double TrailingDrawdown { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Debug Mode", Order = 5, GroupName = "Diagnostics")]
        public bool DebugMode { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "UseAccountFlatten", Order = 6, GroupName = "Diagnostics")]
        public bool UseAccountFlatten { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "BarsRequiredToTrade", Order = 7, GroupName = "Diagnostics")]
        public int BarsRequiredToTradeParam { get; set; }

        // ===== Lifecycle =====
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "RiskCappedStrategy";
                Description = "Account-level risk caps with continuous enforcement on market data.";
                Calculate = Calculate.OnBarClose; // keep per contract/linter
                IsOverlay = false;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                BarsRequiredToTrade = 20;

                // Defaults
                MaxContracts = 1;
                DailyLossLimit = 500.0;
                WeeklyLossLimit = 1500.0;
                TrailingDrawdown = 1500.0;
                DebugMode = false;
                UseAccountFlatten = true;
                BarsRequiredToTradeParam = 20;

                printedStartup = false;
            }
            else if (State == State.Configure)
            {
                SetStopLoss(CalculationMode.Ticks, 10);
            }
            else if (State == State.DataLoaded)
            {
                dayPnLBase = GetCumProfit();
                weekPnLBase = GetCumProfit();
                weekAnchorDate = GetWeekAnchor(Time[0].Date);
                highestEquity = GetCumProfit();

                if (DebugMode && !printedStartup)
                {
                    Print("=== RiskCappedStrategy initialized ===");
                    Print("Caps: Max=" + MaxContracts +
                          " Daily=" + DailyLossLimit +
                          " Weekly=" + WeeklyLossLimit +
                          " TDD=" + TrailingDrawdown);
                    printedStartup = true;
                }
            }
        }

        // Bar loop (entries + periodic enforcement)
        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade) return;
            if (CurrentBar < BarsRequiredToTradeParam) return;

            MaintainBaselines();
            UpdatePeak();
            if (EnforceIfBreached()) return; // hard-block entries

            // Demo entry logic (replace with production signals)
            int acctQty = Math.Abs(PositionAccount != null ? PositionAccount.Quantity : 0);
            if (PositionAccount != null && PositionAccount.MarketPosition == MarketPosition.Flat && acctQty < MaxContracts)
            {
                if (Close[0] > Open[0]) EnterLong("LongEntry");
            }
        }

        // Tick loop (realtime continuous enforcement regardless of Calculate)
        protected override void OnMarketData(MarketDataEventArgs e)
        {
            if (State != State.Realtime) return; // ignore historical
            MaintainBaselines();
            UpdatePeak();
            EnforceIfBreached();
        }

        // ===== Enforcement =====
        private bool EnforceIfBreached()
        {
            int acctQty = Math.Abs(PositionAccount != null ? PositionAccount.Quantity : 0);

            bool breached =
                (acctQty > MaxContracts) ||
                IsDailyBreached() ||
                IsWeeklyBreached() ||
                IsTrailingDrawdownBreached();

            if (DebugMode)
                Print($"[RiskCheck] acctQty={acctQty} eq={GetCumProfit():0.00} peak={highestEquity:0.00} dailyPnL={GetDailyPnL():0.00} weeklyPnL={GetWeeklyPnL():0.00} breached={breached}");

            if (!breached) return false;

            try
            {
                if (Account != null)
                    Account.CancelAllOrders();

                if (UseAccountFlatten && Account != null && PositionAccount != null && PositionAccount.MarketPosition != MarketPosition.Flat)
                    Account.FlattenEverything();
            }
            catch (Exception ex)
            {
                if (DebugMode) Print("[RiskEnforceError] " + ex.Message);
            }
            return true;
        }

        // ===== Baselines / PnL helpers =====
        private void MaintainBaselines()
        {
            if (Bars == null || CurrentBar < 0) return;
            if (Bars.IsFirstBarOfSession) dayPnLBase = GetCumProfit();
            DateTime anchor = GetWeekAnchor(Time[0].Date);
            if (Bars.IsFirstBarOfSession && Time[0].Date == anchor) weekPnLBase = GetCumProfit();
        }

        private void UpdatePeak()
        {
            double eq = GetCumProfit();
            if (eq > highestEquity) highestEquity = eq;
        }

        private double GetCumProfit()
        {
            return SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
        }

        private double GetDailyPnL()
        {
            return GetCumProfit() - dayPnLBase;
        }

        private double GetWeeklyPnL()
        {
            return GetCumProfit() - weekPnLBase;
        }

        private bool IsDailyBreached()
        {
            double daily = GetDailyPnL();
            return (-daily) >= DailyLossLimit;
        }

        private bool IsWeeklyBreached()
        {
            double weekly = GetWeeklyPnL();
            return (-weekly) >= WeeklyLossLimit;
        }

        private bool IsTrailingDrawdownBreached()
        {
            double dd = highestEquity - GetCumProfit();
            return dd >= TrailingDrawdown;
        }

        private static DateTime GetWeekAnchor(DateTime date)
        {
            int diff = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            return date.AddDays(-diff);
        }

        // Contract signatures
        protected override void OnOrderUpdate(
            Order order, double limitPrice, double stopPrice, int quantity,
            int filled, double averageFillPrice, OrderState orderState,
            DateTime time, ErrorCode error, string nativeError)
        {
            if (DebugMode && order != null)
                Print($"[OnOrderUpdate] {time:u} {order.Name} {orderState} qty={quantity} filled={filled} avg={averageFillPrice:0.00}");
        }

        protected override void OnExecutionUpdate(
            Execution execution, string executionId, double price, int quantity,
            MarketPosition marketPosition, string orderId, DateTime time)
        {
            if (DebugMode && execution != null)
                Print($"[OnExecutionUpdate] {time:u} {execution.Order?.Name} {marketPosition} qty={quantity} price={price:0.00}");
        }
    }
}

