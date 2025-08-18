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
    /// Risk-capped strategy shell for live/sim trading.
    /// Enforces MaxContracts, DailyLossLimit, WeeklyLossLimit, and TrailingDrawdown.
    /// Uses only NinjaScript-safe calls and follows the contract in docs/ninjascript_contract.md.
    /// </summary>
    public class RiskCappedStrategy : Strategy
    {
        // ==== Private fields ====
        private double startEquity;          // Equity baseline at strategy start (CumProfit at start)
        private double weekPnLBase;          // CumProfit baseline at start of week
        private DateTime weekAnchorDate;     // Monday anchor for weekly baseline
        private double highestEquity;        // Peak equity for trailing DD
        private bool printedStartup;

        // ==== Parameters (UI) ====
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
        [Range(1, int.MaxValue)]
        [Display(Name = "BarsRequiredToTrade", Order = 6, GroupName = "Diagnostics")]
        public int BarsRequiredToTradeParam { get; set; }

        // ==== State ====
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "RiskCappedStrategy";
                Description = "Live strategy shell with risk caps (max contracts, daily/weekly loss, trailing DD).";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;

                BarsRequiredToTrade = 20;     // engine guard
                BarsRequiredToTradeParam = 20; // user-visible param (mirrors guard)

                // Sensible defaults (tune in UI)
                MaxContracts = 1;
                DailyLossLimit = 500.0;
                WeeklyLossLimit = 1500.0;
                TrailingDrawdown = 1500.0;
                DebugMode = false;

                printedStartup = false;
            }
            else if (State == State.Configure)
            {
                // Optional safety: small static stop to prevent runaway in testing
                SetStopLoss(CalculationMode.Ticks, 10);
            }
            else if (State == State.DataLoaded)
            {
                // Baselines at strategy start
                startEquity = GetCumProfit();
                weekPnLBase = GetCumProfit();
                weekAnchorDate = GetWeekAnchor(Time[0].Date);
                highestEquity = GetCumProfit();

                if (DebugMode && !printedStartup)
                {
                    Print("=== RiskCappedStrategy initialized ===");
                    Print("Caps: MaxContracts=" + MaxContracts
                        + " DailyLossLimit=" + DailyLossLimit
                        + " WeeklyLossLimit=" + WeeklyLossLimit
                        + " TrailingDrawdown=" + TrailingDrawdown);
                    printedStartup = true;
                }
            }
        }

        // ==== Core bar loop ====
        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade) return;
            if (CurrentBar < BarsRequiredToTradeParam) return;

            // Maintain weekly baseline when new week starts
            if (IsNewSession() && Time[0].Date == GetWeekAnchor(Time[0].Date))
            {
                weekPnLBase = GetCumProfit();
                weekAnchorDate = GetWeekAnchor(Time[0].Date);
                if (DebugMode) Print("[WeeklyReset] weekPnLBase=" + weekPnLBase + " anchor=" + weekAnchorDate.ToShortDateString());
            }

            // Update trailing equity peak
            double equity = GetCumProfit();
            if (equity > highestEquity)
                highestEquity = equity;

            bool breached = IsMaxContractsBreached()
                            || IsDailyBreached()
                            || IsWeeklyBreached()
                            || IsTrailingDrawdownBreached();

            if (DebugMode)
                Print($"[RiskCheck] eq={equity:0.00} peak={highestEquity:0.00} dailyPnL={GetDailyPnL():0.00} weeklyPnL={GetWeeklyPnL():0.00} breached={breached}");

            if (breached)
            {
                // Flatten/avoid new entries
                if (Position.MarketPosition != MarketPosition.Flat)
                    ExitLong("RiskExit"); // flat-only shell; extend for shorts if needed

                return;
            }

            // ---- Minimal demonstration entry logic ----
            // Long-on-green bar; purely to show entries under caps; replace with real signals.
            if (Position.MarketPosition == MarketPosition.Flat)
            {
                if (Close[0] > Open[0] && !IsMaxContractsBreached())
                    EnterLong("LongEntry");
            }
        }

        // ==== Helpers (NT8-safe, no external deps) ====

        private bool IsMaxContractsBreached()
        {
            // Gates only new entries; existing position may be > MaxContracts due to fills/slippage in real NT routing.
            return Position.Quantity >= MaxContracts;
        }

        private bool IsDailyBreached()
        {
            // Daily PnL since midnight/session (approx via date change). For high fidelity use SessionIterator or account events.
            double dailyPnL = GetDailyPnL();
            return (-dailyPnL) >= DailyLossLimit; // loss is negative; breach when abs loss >= limit
        }

        private bool IsWeeklyBreached()
        {
            double weeklyPnL = GetWeeklyPnL();
            return (-weeklyPnL) >= WeeklyLossLimit;
        }

        private bool IsTrailingDrawdownBreached()
        {
            // Trailing DD measured from equity peak
            double equity = GetCumProfit();
            double dd = highestEquity - equity;
            return dd >= TrailingDrawdown;
        }

        private double GetCumProfit()
        {
            // NT8-safe cumulative currency PnL
            return SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
        }

        private double GetDailyPnL()
        {
            // Approximate: difference in cumulative profit from start of trading day
            // We'll anchor to midnight of current day by capturing the value on first bar of the day.
            // Use Tag storage via SessionIterator alternative: simple reset when date changes.
            // For robustness, compute baseline on first bar of day.
            DateTime currentDate = Time[0].Date;
            // Use State object to store per-day baseline via BarsSinceNewTradingDay
            // Simpler: derive dailyPnL from cumulative changes since first bar of current date:
            // We cannot access prior day baseline unless tracked; here we compute by scanning bars of current date’s trades,
            // but SystemPerformance lacks per-day breakdown. Use a lightweight approximation: daily = CumProfit - dayStartEquity.
            // We store dayStartEquity in a series keyed by date using static field per strategy instance (safe enough).
            // To keep things deterministic and compile-safe, approximate dailyPnL as CumProfit - startEquity when date == strategy start;
            // and when date changed (new session), reset startEquity to current CumProfit.
            if (Bars.IsFirstBarOfSession)
            {
                // Reset daily baseline at start of each session
                startEquity = GetCumProfit();
            }
            return GetCumProfit() - startEquity;
        }

        private double GetWeeklyPnL()
        {
            // Weekly baseline captured at Monday (or first bar of Monday session)
            if (Bars.IsFirstBarOfSession && Time[0].Date == GetWeekAnchor(Time[0].Date))
            {
                weekPnLBase = GetCumProfit();
            }
            return GetCumProfit() - weekPnLBase;
        }

        private static DateTime GetWeekAnchor(DateTime date)
        {
            // Monday as start-of-week
            int diff = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            return date.AddDays(-diff);
        }

        private bool IsNewSession()
        {
            // True on first bar of a session
            return Bars.IsFirstBarOfSession;
        }

        // ==== Order/Execution events (exact signatures per contract) ====
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

