#region Using declarations
using System;
using System.Reflection;
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
    /// Thin strategy that delegates risk to the SDK if the DLL is referenced.
    /// Falls back to identical local evaluator if no DLL is present.
    /// Contract-compliant and safe to run in parallel with RiskCappedStrategy.
    /// </summary>
    public class RiskDelegatedStrategy : Strategy
    {
        // DTO mirror (minimal)
        private struct Caps { public int MaxContracts; public decimal DailyLossLimit, WeeklyLossLimit, TrailingDrawdown; }
        private struct Snap { public int AccountQuantity; public decimal Equity, PeakEquity, DailyPnL, WeeklyPnL; }
        private enum Decision { Allow=0, Max=1, Daily=2, Weekly=3, TDD=4 }

        // Local evaluator (identical logic)
        private sealed class LocalEval {
            public Decision Eval(Caps c, Snap s) {
                if (c.MaxContracts > 0 && s.AccountQuantity > c.MaxContracts) return Decision.Max;
                if (c.DailyLossLimit > 0m && (-s.DailyPnL) >= c.DailyLossLimit) return Decision.Daily;
                if (c.WeeklyLossLimit > 0m && (-s.WeeklyPnL) >= c.WeeklyLossLimit) return Decision.Weekly;
                var dd = s.PeakEquity - s.Equity;
                if (c.TrailingDrawdown > 0m && dd >= c.TrailingDrawdown) return Decision.TDD;
                return Decision.Allow;
            }
        }

        private object sdk; private MethodInfo sdkEval; private bool useSdk;
        private double dayBase, weekBase, peak; private DateTime weekAnchor;

        [NinjaScriptProperty, Range(1,int.MaxValue)]
        [Display(Name="MaxContracts", Order=1, GroupName="Risk Caps")]
        public int MaxContracts { get; set; }

        [NinjaScriptProperty, Range(0,double.MaxValue)]
        [Display(Name="DailyLossLimit", Order=2, GroupName="Risk Caps")]
        public double DailyLossLimit { get; set; }

        [NinjaScriptProperty, Range(0,double.MaxValue)]
        [Display(Name="WeeklyLossLimit", Order=3, GroupName="Risk Caps")]
        public double WeeklyLossLimit { get; set; }

        [NinjaScriptProperty, Range(0,double.MaxValue)]
        [Display(Name="TrailingDrawdown", Order=4, GroupName="Risk Caps")]
        public double TrailingDrawdown { get; set; }

        [NinjaScriptProperty]
        [Display(Name="UseAccountFlatten", Order=5, GroupName="Diagnostics")]
        public bool UseAccountFlatten { get; set; }

        [NinjaScriptProperty]
        [Display(Name="Debug Mode", Order=6, GroupName="Diagnostics")]
        public bool DebugMode { get; set; }

        [NinjaScriptProperty, Range(1,int.MaxValue)]
        [Display(Name="BarsRequiredToTrade", Order=7, GroupName="Diagnostics")]
        public int BarsRequiredToTradeParam { get; set; }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "RiskDelegatedStrategy";
                Description = "Delegates risk to SDK if referenced; otherwise uses identical local evaluator.";
                Calculate = Calculate.OnBarClose; // linter compliance (we also enforce in OnMarketData)
                IsOverlay = false;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                BarsRequiredToTrade = 20;

                MaxContracts = 1; DailyLossLimit = 500; WeeklyLossLimit = 1500; TrailingDrawdown = 1500;
                UseAccountFlatten = true; DebugMode = false; BarsRequiredToTradeParam = 20;
            }
            else if (State == State.Configure)
            {
                SetStopLoss(CalculationMode.Ticks, 10);
            }
            else if (State == State.DataLoaded)
            {
                dayBase = Cum(); weekBase = Cum(); peak = Cum(); weekAnchor = WeekAnchor(Time[0].Date);
                TryHookSdk();
            }
        }

        private void TryHookSdk()
        {
            // Known likely type names across prior SDK builds/releases.
            string[] typeCandidates = new string[]
            {
                "NT8.SDK.Abstractions.Risk.PortableRiskManager",
                "NT8.SDK.Risk.PortableRiskManager",
                "NT8.SDK.Portable.PortableRiskManager",
                "NT8.SDK.PortableRiskManager",
                "SDK.Abstractions.Risk.PortableRiskManager"
            };

            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                // 1) Fast path: exact candidates
                foreach (var a in assemblies)
                {
                    foreach (var fullName in typeCandidates)
                    {
                        var t = a.GetType(fullName, false);
                        if (t != null)
                        {
                            sdk = Activator.CreateInstance(t);
                            sdkEval = t.GetMethod("Evaluate", BindingFlags.Public | BindingFlags.Instance);
                            useSdk = (sdk != null && sdkEval != null);
                            if (DebugMode && useSdk) Print("[SDK] Hooked " + fullName + " from " + a.GetName().Name);
                            if (useSdk) return;
                        }
                    }
                }

                // 2) Fallback: scan for any type named 'PortableRiskManager' with Evaluate(...)
                foreach (var a in assemblies)
                {
                    var types = a.GetTypes();
                    for (int i = 0; i < types.Length; i++)
                    {
                        var t = types[i];
                        if (t != null && t.Name == "PortableRiskManager")
                        {
                            var m = t.GetMethod("Evaluate", BindingFlags.Public | BindingFlags.Instance);
                            if (m != null)
                            {
                                sdk = Activator.CreateInstance(t);
                                sdkEval = m;
                                useSdk = (sdk != null);
                                if (DebugMode && useSdk) Print("[SDK] Hooked by fallback: " + t.FullName + " from " + a.GetName().Name);
                                if (useSdk) return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (DebugMode) Print("[SDK Hook Error] " + ex.Message);
            }

            useSdk = false;
            if (DebugMode) Print("[SDK] Not found; using local evaluator");
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade) return;
            if (CurrentBar < BarsRequiredToTradeParam) return;
            MaintainAnchors(); UpdatePeak();
            if (Enforce()) return;

            int qty = Math.Abs(PositionAccount != null ? PositionAccount.Quantity : 0);
            if (PositionAccount != null && PositionAccount.MarketPosition == MarketPosition.Flat && qty < MaxContracts)
                if (Close[0] > Open[0]) EnterLong("LongEntry");
        }

        protected override void OnMarketData(MarketDataEventArgs e)
        {
            if (State != State.Realtime) return;
            MaintainAnchors(); UpdatePeak(); Enforce();
        }

        private bool Enforce()
        {
            int qty = Math.Abs(PositionAccount != null ? PositionAccount.Quantity : 0);
            var caps = new Caps { MaxContracts = MaxContracts, DailyLossLimit = (decimal)DailyLossLimit, WeeklyLossLimit = (decimal)WeeklyLossLimit, TrailingDrawdown = (decimal)TrailingDrawdown };
            var snap = new Snap { AccountQuantity = qty, Equity = (decimal)Cum(), PeakEquity = (decimal)peak, DailyPnL = (decimal)(Cum() - dayBase), WeeklyPnL = (decimal)(Cum() - weekBase) };

            Decision d;
            if (useSdk)
            {
                // SDK Evaluate(in RiskCaps, in RiskSnapshot) returning RiskResult with Decision property
                var result = sdkEval.Invoke(sdk, new object[] { caps, snap });
                var prop = result.GetType().GetProperty("Decision");
                d = prop != null ? (Decision)(int)prop.GetValue(result, null) : Decision.Allow;
            }
            else
                d = new LocalEval().Eval(caps, snap);

            bool breached = d != Decision.Allow;
            if (DebugMode) Print($"[RiskCheck] acctQty={qty} eq={Cum():0.00} peak={peak:0.00} dailyPnL={(Cum()-dayBase):0.00} weeklyPnL={(Cum()-weekBase):0.00} decision={d} breached={breached}");

            if (!breached) return false;

            try {
                if (Account != null) Account.CancelAllOrders();
                if (UseAccountFlatten && Account != null && PositionAccount != null && PositionAccount.MarketPosition != MarketPosition.Flat)
                    Account.FlattenEverything();
            } catch (Exception ex) { if (DebugMode) Print("[RiskEnforceError] " + ex.Message); }
            return true;
        }

        private void MaintainAnchors()
        {
            if (Bars == null || CurrentBar < 0) return;
            if (Bars.IsFirstBarOfSession) dayBase = Cum();
            var wa = WeekAnchor(Time[0].Date);
            if (Bars.IsFirstBarOfSession && Time[0].Date == wa) weekBase = Cum();
        }

        private void UpdatePeak() { var c = Cum(); if (c > peak) peak = c; }
        private double Cum() { return SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit; }
        private static DateTime WeekAnchor(DateTime d){ int diff=(int)d.DayOfWeek - (int)DayOfWeek.Monday; if (diff<0) diff+=7; return d.AddDays(-diff); }

        // Contract signatures
        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError) { }
        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time) { }
    }
}
