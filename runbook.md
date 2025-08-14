Operator Runbook (non-technical)
Open Codex (your usual ChatGPT/Codex coding session).

For each step below:

Paste the Universal Preamble first.

Paste that step’s Codex Prompt.

Copy each returned file into your repo under the shown path.

Commit with the step name (e.g., step-1-abstractions).

After Step 5, open NinjaTrader 8 → New NinjaScript → Compile.

Enable _SdkTemplateStrategy on a 1-minute sim chart to sanity-check fills.

If compile fails, re-run that step’s prompt and paste the “Fix & Re-emit” line included in the prompt.

Target: NinjaTrader 8 / .NET 4.8 / C# 7.3. No init, records, modern switch patterns, tuples beyond C# 7.3, or async.

Universal Preamble (paste before every Codex prompt)
You are generating NinjaTrader 8 (NT8) NinjaScript code. Target .NET Framework 4.8 and C# 7.3. Do not use init, record, advanced pattern matching, or async. One public class per file. No external packages.
Use these exact NT8 override signatures when needed:

protected override void OnOrderUpdate(Order order)

protected override void OnExecutionUpdate(Execution execution, Order order)
Namespaces: NT8.SDK for core, with sub-namespaces (e.g., NT8.SDK.Risk).
Add XML doc comments on public types. If helpful, include tiny #if DEBUG asserts.
Output: full file contents for each requested file, nothing else.

Step 1 — Abstractions & DTOs (pure C#; no NT8 types)
Codex Prompt — Step 1

Create these files and nothing else:

Abstractions/Dto.cs — DTOs: RiskMode, RiskLockoutState, PositionSide, PositionIntent, SizeDecision, TrailingProfile, OrderIntent, OrderIds, DiagnosticsEvent, TelemetryEvent, SessionKey.

Abstractions/IOrders.cs — submit/cancel/modify intents (no NT types; use DTOs).

Abstractions/IRisk.cs — RiskMode Mode {get;}, RiskLockoutState Lockout(), bool CanTradeNow(), string EvaluateEntry(PositionIntent intent), void RecordWinLoss(bool win).

Abstractions/ISizing.cs — SizeDecision Decide(RiskMode mode, PositionIntent intent).

Abstractions/ISession.cs — bool IsBlackout(DateTime etNow, string symbol), bool IsSettlementWindow(DateTime etNow, string symbol), DateTime SessionOpen(SessionKey key), DateTime SessionClose(SessionKey key).

Abstractions/ITrailing.cs — non-loosening trailing: decimal ComputeStop(decimal entry, decimal current, bool isLong, TrailingProfile profile, decimal priorStop).

Abstractions/ITelemetry.cs — void Emit(TelemetryEvent evt).

Abstractions/IDiagnostics.cs — toggleable void Capture(object snapshot, string tag).

Abstractions/IBacktestHooks.cs — optional void Stamp(string key, string value).

Abstractions/ISdk.cs — facade exposing the above interfaces.

Acceptance: All files compile standalone (no NT references).
If anything fails: Fix and re-emit the minimal files until compile-clean.

Step 2 — Session & CME Calendar Loader (still pure C#)
Codex Prompt — Step 2

Create:

Config/CmeCalendarModels.cs — POCOs matching a seed file shaped like:
{"symbol":"NQ","days":[{"date":"2025-08-13","settlement":"16:00-16:45","blackouts":["16:00-16:05"]}]}

Config/CmeCalendarLoader.cs — static loader that can:

load from ./seeds/cme_calendar_2025-08_to_2026-07.json if present, else

return safe defaults.

Session/CmeBlackoutService.cs — implements ISession using loader.
Rules: If JSON missing, all methods return safe defaults; times are US/Eastern; no DST gymnastics beyond assuming input times are ET ranges.

Acceptance: Pure C# compile; if file missing, no exceptions; methods return safe defaults.

Step 3 — Risk Modes & Loss-Streak Guard (pure C#)
Codex Prompt — Step 3

Create:

Risk/RiskMode.cs — enum: ECP, PCP, DCP, HR.

Risk/LossStreakGuard.cs — track consecutive losses, daily loss cap, cooldown.

Risk/RiskManager.cs — implements IRisk with props for:

Balance, High-Water Mark, Trailing Drawdown buffer (Bulenox style), Daily loss cap, Max consecutive losses, Cooldown-until, Disable-after-X trades/day.

EvaluateEntry(PositionIntent) returns empty string if OK, else reason text.

RecordWinLoss(bool) updates streaks and daily running PnL.

Acceptance: Pure C# compile. Include #if DEBUG boundary asserts (e.g., buffer ≤ 500 → force ECP).

Step 4 — Sizing & Trailing (pure C#)
Codex Prompt — Step 4

Create:

Sizing/SizingEngine.cs — implements ISizing. Inputs: RiskMode, account caps (e.g., max micros), per-mode base size, loss-streak degrade (e.g., minus 1 micro after each loss, floor at 1). Output: SizeDecision(Qty, Reason, RiskMode).

Trailing/Profiles.cs — model types for: FixedTicks, ATRxMultiplier, and a simple “OR-width based” profile.

Trailing/TrailingEngine.cs — non-loosening rule; ComputeStop(...) must never widen; re-arm on partials handled by passing prior stop.

Acceptance: Pure C# compile. Deterministic outputs for fixed inputs.

Step 5 — NT8 Bridge & Facade (first NT8-touching code)
Codex Prompt — Step 5

Create NinjaTrader-aware glue:

Bridge/OrderBridge.cs — implements IOrders using NT8 types. Provide public methods Codex can call from a Strategy:

SubmitEntryLimit(string signal, bool isLong, int qty, double price)

SubmitEntryStop(string signal, bool isLong, int qty, double price)

AttachOcoStopTarget(string fromSignal, double stopPrice, double targetPrice)

UpdateStop(string fromSignal, double newStopPrice)

CancelAll(string reason)
Internally handle: OCO IDs, child rejection → protective flatten.

Bridge/SdkFacade.cs — implements ISdk by composing: RiskManager, SizingEngine, TrailingEngine, CmeBlackoutService, DiagnosticsSwitch, TelemetrySink, and the OrderBridge. Expose them as properties.

Critical Requirements:

Use exact NT8 overrides in comments for the Strategy to forward to:

OnOrderUpdate(Order order)

OnExecutionUpdate(Execution execution, Order order)
(The bridge should provide methods the Strategy calls from these overrides.)

Do not create indicators in this step. Keep it glue-only.

Acceptance: Drops into bin/Custom/ and compiles once Step 6 adds the template Strategy. If any NT8 signature mismatch is found, fix and re-emit.

Step 6 — Template Strategy for Wiring (NT8 Strategy)
Codex Prompt — Step 6

Create:

Strategies/_SdkTemplateStrategy.cs — minimal Strategy to prove the SDK wiring. Behavior:

User inputs: UseLong, UseShort, StopTicks=10, TargetTicks=15, RiskMode=PCP.

On each bar when flat and a trivial condition is true (e.g., Close[0] > Open[0] for long, Close[0] < Open[0] for short), do:

if (!sdk.Risk.CanTradeNow()) return;

var size = sdk.Sizing.Decide(sdk.Risk.Mode, new PositionIntent(...));

Submit entry via sdk.Orders.*; on fill, call trailing compute and update stop.

Wire OnOrderUpdate and OnExecutionUpdate to call into OrderBridge helpers.

Add an input EnableDiagnostics to toggle JSONL diagnostics to ..\bin\Custom\Logs\SDK\.

Acceptance: Compiles in NT8 and will submit/flatten on SIM without exceptions. No indicators beyond SMA(1) if you need any trivial condition.

Final “Make It Foolproof” Add-Ons (optional but recommended)
A) PR Checklist (paste into your repo as .github/PULL_REQUEST_TEMPLATE.md)
 Target NT8 / .NET 4.8 / C# 7.3 only

 One public class per file

 No modern C# features (records, init, advanced patterns)

 Exact NT8 method signatures where applicable

 Pure C# layers compile without NT references

 Bridge compiles in NT8 with Template Strategy

 Diagnostics path created on first write

B) Lint Guard (text for Codex to add later)
Add a tiny script tools/nt8_guard.py that greps for banned tokens: init, record, switch when, IAsyncEnumerable, etc. (Optional—keeps Codex in bounds.)

Tips for a truly hands-off run
One step = one Codex run (don’t let it “touch everything”).

If Codex drifts, paste:
“Fix & Re-emit only the files listed in this step. Keep names, namespaces, and C# 7.3 constraints exactly as specified.”

After Step 6 compiles, you already have a production-grade foundation: risk modes, sizing, trailing, session/blackouts, diagnostics/telemetry, and a bridge that any real strategy can call.
