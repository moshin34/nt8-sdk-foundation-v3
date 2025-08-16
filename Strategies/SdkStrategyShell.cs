#region Using declarations
using System;
using System.ComponentModel.DataAnnotations;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NT8.SDK;
using NT8.SDK.Facade;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    public enum EntryDirection
    {
        Long,
        Short
    }

    /// <summary>SDK shell that submits a single demo trade with tick-aware stop/target brackets.</summary>
    public class SdkStrategyShell : Strategy
    {
        private ISdk _sdk;
        private bool _submitted;

        [NinjaScriptProperty]
        [Display(Name = "Risk Mode", Order = 1, GroupName = "Parameters")]
        public RiskMode RiskMode { get; set; }

        [NinjaScriptProperty]
        [Range(1, 10)]
        [Display(Name = "Loss Streak Lockout", Order = 2, GroupName = "Parameters")]
        public int LossStreakLockout { get; set; }

        [NinjaScriptProperty]
        [Range(1, 120)]
        [Display(Name = "Lockout Duration (minutes)", Order = 3, GroupName = "Parameters")]
        public int LockoutDurationMinutes { get; set; }

        [NinjaScriptProperty]
        [Range(1, 5000)]
        [Display(Name = "Trailing Distance (ticks)", Order = 4, GroupName = "Parameters")]
        public int TrailingDistanceTicks { get; set; }

        [NinjaScriptProperty]
        [Range(1, 10000)]
        [Display(Name = "Target Distance (ticks)", Order = 5, GroupName = "Parameters")]
        public int TargetDistanceTicks { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Direction", Order = 6, GroupName = "Parameters")]
        public EntryDirection Direction { get; set; }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "SdkStrategyShell";
                Calculate = Calculate.OnEachTick;
                EntriesPerDirection = 1;
                BarsRequiredToTrade = 20;

                RiskMode = RiskMode.PCP;
                LossStreakLockout = 2;
                LockoutDurationMinutes = 15;

                TrailingDistanceTicks = 10; // protective stop distance
                TargetDistanceTicks   = 20; // profit target distance
                Direction = EntryDirection.Long;
            }
            else if (State == State.Configure)
            {
                var builder = new SdkBuilder()
                    .WithMode(RiskMode)
                    .WithLossStreakLockout(LossStreakLockout)
                    .WithLockoutDuration(TimeSpan.FromMinutes(LockoutDurationMinutes))
                    .WithClock(NT8.SDK.Common.SystemClock.Instance)
                    .WithOrders(new NT8.SDK.NT8Bridge.Nt8Orders(this)); // Managed-orders adapter

                _sdk = builder.Build();
            }
            else if (State == State.DataLoaded)
            {
                _submitted = false;
                Print($"SDK loaded v{NT8.SDK.SdkVersion.Informational} ({NT8.SDK.SdkVersion.AssemblyVersion})");
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0) return;
            if (CurrentBar < BarsRequiredToTrade) return;
            if (_submitted) return; // demo: fire once
            if (Position.MarketPosition != MarketPosition.Flat) return;

            string symbol = Instrument.MasterInstrument.Name;
            bool isLong = Direction == EntryDirection.Long;

            // Risk gate
            var reason = _sdk.Risk.EvaluateEntry(new PositionIntent(symbol, isLong ? PositionSide.Long : PositionSide.Short));
            if (!string.IsNullOrEmpty(reason))
            {
                Print("Risk gate: " + reason);
                return;
            }

            // Sizing
            var size = _sdk.Sizing.Decide(_sdk.Risk.Mode, new PositionIntent(symbol, isLong ? PositionSide.Long : PositionSide.Short));
            if (size.Quantity <= 0)
            {
                Print("Sizing gate: " + size.Reason);
                return;
            }

            // Tick sanity
            double tick = Instrument.MasterInstrument.TickSize;
            if (tick <= 0)
            {
                Print("Tick size not available for instrument; aborting order submit.");
                return;
            }

            string signal = "SDK-Entry";
            double entryPrice = Close[0];

            // --- Stop (symmetric, tick-aware) ---
            double rawStop = isLong
                ? entryPrice - TrailingDistanceTicks * tick   // long: below entry
                : entryPrice + TrailingDistanceTicks * tick;  // short: above entry

            double stopRounded = Instrument.MasterInstrument.RoundToTickSize(rawStop);

            // Enforce at least 1-tick separation after rounding
            if (isLong)
            {
                if (stopRounded >= entryPrice - tick)
                {
                    Print(string.Format(
                        "Rejected protective stop: must be at least 1 tick below entry. entry={0:0.00} stop={1:0.00} tick={2:0.00}",
                        entryPrice, stopRounded, tick));
                    return;
                }
            }
            else
            {
                if (stopRounded <= entryPrice + tick)
                {
                    Print(string.Format(
                        "Rejected protective stop: must be at least 1 tick above entry. entry={0:0.00} stop={1:0.00} tick={2:0.00}",
                        entryPrice, stopRounded, tick));
                    return;
                }
            }

            // --- Target (symmetric, tick-aware) ---
            double rawTarget = isLong
                ? entryPrice + TargetDistanceTicks * tick     // long: above entry
                : entryPrice - TargetDistanceTicks * tick;    // short: below entry

            double targetRounded = Instrument.MasterInstrument.RoundToTickSize(rawTarget);

            // Enforce at least 1-tick separation after rounding (with FP tolerance)
            if (isLong)
            {
                if (targetRounded <= entryPrice + tick * 0.9999)
                {
                    Print(string.Format(
                        "Rejected profit target: must be at least 1 tick above entry. entry={0:0.00} target={1:0.00} tick={2:0.00}",
                        entryPrice, targetRounded, tick));
                    return;
                }
            }
            else
            {
                if (targetRounded >= entryPrice - tick * 0.9999)
                {
                    Print(string.Format(
                        "Rejected profit target: must be at least 1 tick below entry. entry={0:0.00} target={1:0.00} tick={2:0.00}",
                        entryPrice, targetRounded, tick));
                    return;
                }
            }

            // Place bracket BEFORE entry so NT8 links by signal (OCO behavior)
            SetStopLoss(signal, CalculationMode.Price, stopRounded, false);
            SetProfitTarget(signal, CalculationMode.Price, targetRounded);

            // Submit entry (market for demo)
            var entryIntent = new OrderIntent(symbol, isLong, size.Quantity, OrderIntentType.Market, 0m, signal, null);
            _sdk.Orders.Submit(entryIntent);

            Print(string.Format("Submitted: dir={0} qty={1} entry={2:0.00} stop={3:0.00} target={4:0.00} mode={5}",
                isLong ? "LONG" : "SHORT", size.Quantity, entryPrice, stopRounded, targetRounded, _sdk.Risk.Mode));

            _submitted = true;
        }

        // --- Optional: rich tracing for diagnostics ---

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled,
                                              double averageFillPrice, OrderState orderState, DateTime time,
                                              ErrorCode error, string nativeError)
        {
            Print(string.Format(
                "OrderUpdate: name={0} action={1} state={2} qty={3} filled={4} avg={5:0.00} limit={6:0.00} stop={7:0.00} time={8:HH:mm:ss} err={9} native={10}",
                order != null ? order.Name : "(null)",
                order != null ? order.OrderAction.ToString() : "(null)",
                orderState, quantity, filled, averageFillPrice, limitPrice, stopPrice, time, error, nativeError));
        }

        protected override void OnExecutionUpdate(Execution execution, Order order)
        {
            if (execution == null)
            {
                Print("Execution: (null)");
                return;
            }

            Print(string.Format(
                "Execution: name={0} action={1} qty={2} price={3:0.00} pos={4} time={5:HH:mm:ss}",
                execution.Name,
                order != null ? order.OrderAction.ToString() : "(n/a)",
                execution.Quantity,
                execution.Price,
                execution.MarketPosition,
                execution.Time));
        }
    }
}

