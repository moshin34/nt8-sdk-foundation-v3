using System;
using NT8.SDK;                 // DTOs, enums, IClock
using NT8.SDK.Facade;         // SdkBuilder
using NT8.SDK.QA.TestKit;     // FixedClock

namespace NT8.SDK.Harness
{
    /// <summary>Debug-only smoke runner invoked by QuickStartProgram in DEBUG builds.</summary>
    public static class QuickStartRunner
    {
        public static void RunOnce()
        {
            // Start at a deterministic UTC time
            var clock = new FixedClock(new DateTime(2025, 1, 2, 14, 30, 00, DateTimeKind.Utc));

            // Build SDK with deterministic clock and a lockout threshold of 2 losses
            var sdk = new SdkBuilder()
                .WithMode(RiskMode.PCP)
                .WithLossStreakLockout(2)
                .WithClock(clock)
                .Build();

            // 1) Normal evaluation + sizing + trailing
            var intent = new PositionIntent("NQ", PositionSide.Long);
            var eval = sdk.Risk.EvaluateEntry(intent);
            var size = sdk.Sizing.Decide(sdk.Risk.Mode, intent);
            var stop = sdk.Trailing.ComputeStop(
                20000m, 20005m, true,
                new TrailingProfile(TrailingProfileType.FixedTicks, 10m, 0m),
                0m);

            Console.WriteLine("[{0:u}] Eval: {1} | Qty: {2} ({3}) | Stop: {4}",
                clock.UtcNow, string.IsNullOrEmpty(eval) ? "OK" : eval, size.Quantity, size.Reason, stop);

            
            // --- Entry gate demo using CME seed (assumes ET timestamps) ---
            intent = new PositionIntent("NQ", PositionSide.Long);

            // Inside settlement (should be blocked by session gate)
            var etInsideSettlement = new DateTime(2025, 8, 13, 16, 5, 0); // 16:05 ET
            var r1 = NT8.SDK.Facade.EntryGates.CheckEntry(sdk, etInsideSettlement, intent);
            Console.WriteLine("Settlement gate (16:05 ET): {0}", string.IsNullOrEmpty(r1) ? "OK" : r1);

            // Outside settlement (should be OK if not in risk lockout)
            var etOutsideSettlement = new DateTime(2025, 8, 13, 15, 0, 0); // 15:00 ET
            var r2 = NT8.SDK.Facade.EntryGates.CheckEntry(sdk, etOutsideSettlement, intent);
            Console.WriteLine("Settlement gate (15:00 ET): {0}", string.IsNullOrEmpty(r2) ? "OK" : r2);

            // 2) Trigger loss-streak lockout (2 losses)
            sdk.Risk.RecordWinLoss(false);
            sdk.Risk.RecordWinLoss(false);
            var canTradeAfterLosses = sdk.Risk.CanTradeNow();

            Console.WriteLine("[{0:u}] After 2 losses -> CanTradeNow={1}", clock.UtcNow, canTradeAfterLosses);

            // Show diagnostics if exposed (State/CooldownUntilUtc); harmless if not present
            try
            {
                var riskEngine = sdk.Risk as dynamic;
                Console.WriteLine("  State={0} LossStreak={1} CooldownUntilUtc={2:u}",
                    riskEngine.State, riskEngine.LossStreak, riskEngine.CooldownUntilUtc);
            }
            catch { /* diagnostics might not be present; ignore */ }

            // 3) Advance clock past cooldown and verify trading resumes
            //    Using RiskConfig defaults or the options inside the engine; safe to add a cushion second.
            clock.Advance(TimeSpan.FromMinutes(16));
            var canTradeAfterCooldown = sdk.Risk.CanTradeNow();
            Console.WriteLine("[{0:u}] After cooldown -> CanTradeNow={1}", clock.UtcNow, canTradeAfterCooldown);

            // --- Entry planner demo (outside settlement, should be OK) ---
            var trail = new TrailingProfile(TrailingProfileType.FixedTicks, 10m, 0m);
            var plan = NT8.SDK.Facade.EntryPlanner.Build(
                sdk: sdk,
                etNow: new DateTime(2025, 8, 13, 15, 0, 0), // 15:00 ET: outside settlement in seed
                symbol: "NQ",
                side: PositionSide.Long,
                entryType: OrderIntentType.Market,
                entryPrice: 20000m,
                trailingProfile: trail);

            Console.WriteLine(
                "EntryPlan: accepted={0} reason='{1}' entryType={2} stopType={3} stop={4}",
                plan.Accepted,
                plan.Reason ?? "",
                plan.Entry.Type,
                plan.Stop.Type,
                plan.Stop.Price);

            // Assert a stable numeric in log for CI (tickSize=1)
            Console.WriteLine("EntryPlan stop numeric check: stop={0}", plan.Stop.Price);

            // --- Entry planner bad-price demo (should be rejected) ---
            var bad = NT8.SDK.Facade.EntryPlanner.Build(
                sdk: sdk,
                etNow: new DateTime(2025, 8, 13, 15, 0, 0),
                symbol: "NQ",
                side: PositionSide.Long,
                entryType: OrderIntentType.Limit,
                entryPrice: 0m, // invalid for limit
                trailingProfile: trail,
                tickSize: 1m);
            Console.WriteLine("EntryPlan(bad price): accepted={0} reason='{1}'", bad.Accepted, bad.Reason ?? "");
        }
    }
}
