# NinjaScript Contract (NinjaTrader 8) — Ground Truth

Purpose: eliminate compile errors by standardizing imports, namespace, lifecycle, and exact override signatures for NinjaTrader 8 strategies.

## Namespace (must be exact)
    namespace NinjaTrader.NinjaScript.Strategies
    {
        // ...
    }

## Required using lines
    using System;
    using NinjaTrader.Cbi;
    using NinjaTrader.Data;
    using NinjaTrader.Gui.Tools;
    using NinjaTrader.NinjaScript;
    using NinjaTrader.NinjaScript.Strategies;
    using NinjaTrader.NinjaScript.StrategyGenerator;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

## Required lifecycle blocks
    protected override void OnStateChange()
    {
        if (State == State.SetDefaults)
        {
            Name = "StrategyName";
            Description = "Strategy description.";
            Calculate = Calculate.OnBarClose; // exact enum
            IsOverlay = false;
            EntriesPerDirection = 1;
            EntryHandling = EntryHandling.AllEntries;
            IsExitOnSessionCloseStrategy = true;
            ExitOnSessionCloseSeconds = 30;
            BarsRequiredToTrade = 20;
        }
        else if (State == State.Configure)
        {
            // e.g., SetStopLoss(CalculationMode.Ticks, 10);
        }
        else if (State == State.DataLoaded)
        {
            // allocate indicators/fields if needed
        }
    }

    protected override void OnBarUpdate()
    {
        if (CurrentBar < BarsRequiredToTrade)
            return;

        // your trading logic
    }

## Exact override signatures (do not change order or types)
    protected override void OnOrderUpdate(
        Order order, double limitPrice, double stopPrice, int quantity,
        int filled, double averageFillPrice, OrderState orderState,
        DateTime time, ErrorCode error, string nativeError)
    {
        // optional
    }

    protected override void OnExecutionUpdate(
        Execution execution, string executionId, double price, int quantity,
        MarketPosition marketPosition, string orderId, DateTime time)
    {
        // optional
    }

## Property attributes
Use these attributes exactly for Strategy UI params:
    [NinjaScriptProperty]
    [Range(1, int.MaxValue)]
    [Display(Name = "Period", Order = 1, GroupName = "Parameters")]
    public int Period { get; set; }

## Don’t use (will fail or drift in NT8)
- async/await
- LINQ query syntax in hot paths
- record types, init-only setters, top-level statements
- Unqualified or wrong enums (e.g., do not use MarketCalculate)

## Self-Checklist (must be true)
- [ ] File in `NinjaTrader.NinjaScript.Strategies` namespace
- [ ] All required `using` lines present
- [ ] `Calculate = Calculate.OnBarClose;`
- [ ] `OnOrderUpdate` includes `int filled` with exact signature
- [ ] `OnBarUpdate` has `if (CurrentBar < BarsRequiredToTrade) return;`
- [ ] No async/await, records, init, or LINQ query syntax

See `NT8Strategies/Templates/MyStrategyTemplate.cs` for the canonical template.
