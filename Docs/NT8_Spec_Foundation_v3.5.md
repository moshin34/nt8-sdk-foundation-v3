# NT8 Spec Foundation v3.5

Institutional NT8 SDK — Foundation v3.5 (Codex‑Ready, Developer Handoff)
Scope: CME futures (MES, MNQ, ES, NQ, CL, GC). 23h×5 ETH with RTH support. Prop‑firm compatible (Apex, Bulenox).
Purpose: Greenfield SDK foundation with a frozen Unified Strategy API (USI) and replaceable internals. Strategies are thin and call USI only.
Contract: Design only (no code). Deterministic, testable, capital‑protection first. First‑compile success when Codex/new devs generate exactly the manifest in §17 using the rules in §16.

1) System Identity
Field	Spec
SDK Name	Institutional NT8 SDK — Foundation v3.5
Target	NinjaTrader 8 / .NET 4.8 / C# 7.3
Calculate Mode	OnEachTick (live); OnBarClose allowed in harness only
Determinism	Identical inputs ⇒ identical trades/logs/exports (byte‑for‑byte)
Compile Rules	One public class per file; filename == class name; allowed NT8 namespaces only
Markets	MES, MNQ, ES, NQ, CL, GC
Sessions	ETH canonical; optional RTH (NY 09:30–16:00 ET). Daily break 17:00–18:00 ET (no orders).
Settlement Blackout	Never trade during CME settlement windows (instrument‑specific; driven by calendar JSON); always black out maintenance 16:15–17:00 CT and equity settle micro‑window (e.g., 15:14:30–15:15:00 CT for equity index).
Strategies	Thin adapters over USI only (no direct access to internals)
Unknowns Policy	Unconfirmed firm figures marked UNKNOWN with parameter stubs + verification notes

2) Architecture Overview (Stable USI + Replaceable Internals)
Stable/Abstractions (USI – frozen surface)
ISdk, IOrders, IRisk, ISizing, ISession, ITrailing, ITelemetry, IDiagnostics, IBacktestHooks, Dto.cs (immutable DTOs; append‑only).

Facade & Adapter
/Facade/SdkFacade.cs is the only entry used by strategies → SdkAdapterV1 maps USI ⇄ internals. Future internal changes ship as new adapters (e.g., SdkAdapterV1_1) without changing USI.

Internal Modules (final merged set — 17)
SDKBootstrap, IndicatorHelper, SessionCalendar, TimeFilter, TradeLogger, DiagnosticsSwitch, RiskTiers, PnLTracker, RiskManager, LossStreakGuard, PositionSizer, TrailProfiles, OrderRouter, BacktestHarness, WalkForwardRunner, MonteCarloBootstrapper, QA.TestKit

Dependency Rules
Strategies → USI only → Facade/Adapter → Internal modules. No lateral coupling. No strategy may reference /Common/*.

3) Session Logic & 24×5 Trading Windows
SessionCalendar

Default ETH; optional RTH; all time math exchange‑local.

Holiday JSON + manual DISABLE_WINDOWS (timestamp spans).

Daily break: 17:00–18:00 ET (no orders routed).

Settlement Blackouts (per calendar JSON):

Equity index settlement micro‑window blocked (e.g., 15:14:30–15:15:00 CT).

CME maintenance 16:15–17:00 CT blocked.

Calendar‑driven fields are authoritative per instrument group; map to instrument family at load.

EOD flatten (when RTH enabled): indices flat by 15:25:00 ET; ETH EOD flat 17:00 ET.

TimeFilter Entry Slots (disabled until a strategy opts in; unchanged)

NY1 09:35–10:30, NY2 10:30–12:00, NY3 13:00–15:00 ET; LDN 03:00–05:00 ET; ASIA 20:00–22:00 ET.

One‑trade‑per‑direction per slot (independent locks), reset at session boundary.

Deterministic Constraints

No orders during daily break and calendar blackouts.

Flatten T−5s before any configured RTH close.

4) Risk Management (Prop‑Aware; Modes; TTD; Loss‑Streak Guard)
4.1 Canonical Equity & TTD Definitions (PnLTracker)
START_BAL_USD: session‑start account balance snapshot.

BAL_USD: current equity (realized + unrealized).

NET_PNL_USD: cumulative session realized + unrealized PnL.

HIGH_WATER_EQUITY_USD: max equity observed since account inception or last manual reset.

Trailing Drawdown: TTD_LIMIT_USD = 2,500 per account. Includes open & unrealized; does not auto‑reset; manual reset supported.

Remaining TTD Buffer: REMAINING_DD_BUFFER_USD = TTD_LIMIT_USD − (HIGH_WATER_EQUITY_USD − BAL_USD) (floor at 0).

4.2 Capital‑Protection Modes (with STOP_TRADING overlay)
Modes: ECP (Extreme), PCP (Protective), DCP (Default), HR (High‑Risk; manual enable).
Overlay: STOP_TRADING (blocks new entries; does not flatten by itself).

Priority & Triggers (evaluate every OnExecution + OnBarUpdate)

Eval STOP_TRADING overlay: if NET_PNL_USD ≥ 3,020 (Evaluation mode only) ⇒ entries blocked until reset/upgrade.

Circuit Breaker (CB) overlay: if REMAINING_DD_BUFFER_USD ≤ 2,250 and > 2,000 ⇒ CB_ON.

Action: Block new entries; manage open positions normally; persists until REMAINING_DD_BUFFER_USD ≥ 2,400 (CB_RELEASE_BUFFER) or session reset.

ECP: if REMAINING_DD_BUFFER_USD ≤ 2,000 ⇒ Mode=ECP (even if CB already on).

PCP: if BAL_USD < PCP_BAL_THRESHOLD_USD (default 57,100).

DCP: if 57,100 ≤ BAL_USD < START_BAL_USD + 10,000.

HR: if BAL_USD ≥ START_BAL_USD + 10,000 AND HR_FEATURE_ENABLED=true.

Per‑Mode Controls

Mode	CONF_MIN	SIZE_MULT	DAILY_LOSS_MULT_EFF	OPEN_RISK_MULT_EFF
ECP	90	0.20×	0.25×	0.25×
PCP	75	0.50×	0.50×	0.50×
DCP	65	1.00×	1.00×	1.00×
HR	55	1.50×	1.25×	1.25×

Dynamic Caps (tier‑based, equity‑scaled)

BASE_DAILY_LOSS_CAP_USD = DAILY_LOSS_MULT[tier] × EquityBasis

BASE_MAX_OPEN_RISK_USD = OPEN_RISK_MULT[tier] × EquityBasis

EquityBasis = BAL_USD (Evaluation may use fixed account size if configured).

Effective Caps

DAILY_LOSS_CAP_EFFECTIVE = BASE_DAILY_LOSS_CAP_USD × DAILY_LOSS_MULT_EFF

MAX_OPEN_RISK_EFFECTIVE = BASE_MAX_OPEN_RISK_USD × OPEN_RISK_MULT_EFF

No Weekly Loss Cap

Weekly cap disabled (explicitly off for prop evaluation & funded modes).

4.3 Loss‑Streak Guard (day‑scoped)
Rule A (2‑loss day lockout): If two consecutive losing closed trades occur within the same trading day, lock out new entries for the remainder of that day. (Event: DAILY_LOCK_AFTER_2_LOSSES.)

Rule B (carryover sensitivity): If the previous day ended due to Rule A, and on the next day the first closed trade is a loss, immediately lock out that day’s entries. (Event: NEXTDAY_FIRST_LOSS_LOCKOUT.)

Counting resets at session roll (ETH daily start 18:00 ET). Loss streak counter uses realized PnL at close of each trade.

4.4 Hard/Soft Gates (RiskManager deterministic actions)
Gate	Deterministic Check	Action	ReasonCode
Eval Target (overlay)	NET_PNL_USD ≥ 3,020 (Eval)	Block new entries; manage opens per normal	StopTrading
Circuit Breaker (overlay)	REMAINING_DD_BUFFER_USD ≤ 2,250 and > 2,000	Block new entries until recovery ≥2,400 or session reset	CircuitBreak
ECP trigger	REMAINING_DD_BUFFER_USD ≤ 2,000	Mode=ECP immediately	ModeChanged
Trailing‑DD	REMAINING_DD_BUFFER_USD == 0	Flatten + block	TrailingDD
Daily Loss	Realized + Unrealized ≤ −DAILY_LOSS_CAP_EFFECTIVE	Flatten + block (session)	DailyStop
Max Contracts	RequestedQty > MaxContracts[symbol]	Reject	MaxContracts
Open Risk	ProjectedOpenRiskUSD > MAX_OPEN_RISK_EFFECTIVE	Reject	OpenRiskExceeded
Margin Buffer	Projected margin > (1−buffer) × FreeMargin	Reject	Margin
Time/Holiday/Settlement	Outside tradable window or in blackout	Reject	TimeBlocked
Config Safety	Missing/invalid tier config	Reject	ConfigError

Telemetry (required): MODE_CHANGED, STOP_TRADING_ON/OFF, CIRCUIT_BREAK_ON/OFF, DAILY_LOCK_AFTER_2_LOSSES, NEXTDAY_FIRST_LOSS_LOCKOUT, TTD_STOP.

5) Position Sizing (Risk‑Per‑Trade; Unit Weights; Firm Caps)
Sizer Formula
risk_per_contract = INIT_STOP_ATR × ATR(Period) × DollarPerPoint
base_contracts = floor( (RISK_PCT × EquityBasis) / risk_per_contract )
Apply Mode: contracts_mode = floor(base_contracts × SIZE_MULT[Mode])
Clip to MAX_CONTRACTS_BY_SYMBOL[symbol] and firm/account caps (§5.1).
Allocate per UNIT_WEIGHTS over POS_UNITS.

5.1 Firm / Account Caps (global)

Max contracts: 7 minis (ES, NQ, CL, GC) or 70 micros (MES, MNQ) — hard cap.

Final per‑symbol cap = min(FirmCap, TierCap, AccountCap).

Deterministic Constraints

If contracts_mode < 1 ⇒ reject (SizeInsufficient).

Integer‑lots only; no averaging down against unrealized loss.

Trails never widen after armed (see §6).

6) Trailing & Exit Profiles (Reusable; Non‑Loosening Invariant)
TrailProfiles (selectable)
FixedATR(mult) · Chandelier(mult, lookback) · BreakEvenThenATR(armAtTP1=true, mult) · StepLock(steps:[(profit,offset)…])

Invariant: Once armed, stop level never loosens. Any violation is a QA failure.

7) Indicators & Data Hygiene
ATR(14), ADX(14), EMA(n), SessionVWAP = Σ(P×V)/ΣV since session open.

Warmup ≥ 2× max lookback.

Data template: ETH default; RTH when strategy requests.

Bad‑tick filter; continuous contract handling in harness.

8) Diagnostics, Logging, and Standardized Output
DiagnosticsSwitch (global): if true, export a parameter manifest (JSON) once per run: instrument, SDK version, capabilities, tier limits, runtime params, cost model, seeds, slots enabled, Mode, CircuitBreaker, effective caps, loss‑streak state.

TradeLogger: standard line schema (one row per event)

css
Copy
Edit
[ts][symbol][strategy][state][event][dir][qty][px][atr][adx][vwap][pnlR][tier][reason]
Required events: LONG_ENTRY, SHORT_ENTRY, TP1_HIT, TP2_HIT, TP3_TRAIL_UPDATE, CAT_STOP, TIME_STOP, EOD_FLAT, DAILY_STOP, TTD_STOP, MODE_CHANGED, STOP_TRADING_ON, STOP_TRADING_OFF, CIRCUIT_BREAK_ON, CIRCUIT_BREAK_OFF, DAILY_LOCK_AFTER_2_LOSSES, NEXTDAY_FIRST_LOSS_LOCKOUT, LOCK_LONG, LOCK_SHORT, PARAM_EXPORT_OK.

Determinism: Two identical runs ⇒ byte‑identical logs and exports.

9) Backtest / Walk‑Forward / Monte Carlo (Acceptance Gates)
Costs: Commissions ON; SLIP_TICKS per symbol (ES/NQ/MES/MNQ:1; CL/GC:2 default).

Latency: 100–250 ms decision‑to‑route (backtest model).

Walk‑Forward: 6m IS / 2m OOS ×4 folds; accept if median OOS CAR/MDD ≥ 0.60 × median IS.

Monte Carlo: 5,000 reshuffles; go‑live if 5th‑pctile CAR/MDD ≥ 0.40 × IS and 5th‑pctile MaxDD within prop limits.

Determinism: Fixed random seed.

10) Parameters (Central Registry; Surfaced; Bounds)
Key	Default	Bounds	Notes
RISK_TIER	Conservative	enum	Binds RiskTiers
RISK_PCT	0.25%	0.10–0.75%	Tier‑driven; editable if Custom
DAILY_LOSS_MULT[tier]	1.0	0.25–2.0	Scales EquityBasis for base daily cap
OPEN_RISK_MULT[tier]	0.5	0.25–2.0	Scales EquityBasis for base open risk
WEEKLY_LOSS_CAP_ENABLED	false	bool	Explicitly off
TTD_LIMIT_USD	2,500	fixed	Per account; includes open & unrealized; manual reset supported
CB_TRIGGER_BUFFER_USD	2,250	≥0	Circuit‑break onset (entries off)
CB_RELEASE_BUFFER_USD	2,400	≥CB_TRIGGER	Entries allowed again
ECP_TRIGGER_BUFFER_USD	2,000	≥0	Switch to ECP mode
MAX_CONTRACTS_BY_SYMBOL	minis:7; micros:70	map	Hard caps, all modes
INIT_STOP_ATR	1.25	0.5–3.0	Drives sizer & catastrophic stop
ATR_PERIOD	14	5–50	–
ADX_PERIOD	14	5–50	–
MIN_ATR	inst‑scaled	≥0	Vol floor
MAX_ADX	30	≥5	Chop gate
POS_UNITS	3	1–10	–
UNIT_WEIGHTS	[1,1,1]	sum>0	Unit allocation
TRAIL_PROFILE	BreakEvenThenATR(1.0)	enum+args	–
SLIP_TICKS	by symbol	0–3	Backtest only
DIAGNOSTICS_ENABLED	false	bool	–
PARAM_EXPORT_PATH	~/NT8_Exports	path	–
ENTRY_SLOTS	NY1,NY2,NY3,LDN,ASIA	editable	Per §3
HR_FEATURE_ENABLED	false	bool	Manual enable for HR
EVAL_TARGET_PNL_USD	3,020	≥0	Evaluation overlay
CONF_SCORE_SOURCE	EntryModule	enum	Must yield 0–100
CONF_SCORE_MIN_OVERRIDE	null	0–100 or null	Overrides per‑mode CONF_MIN
CALENDAR_JSON_PATH	required	path	CME calendar file used by SessionCalendar

11) Failure‑Safes
Connectivity Drop: Maintain protective stop; on reconnect, resync OCO; remain flat if uncertainty.

Order Rejects: One retry max; else pause entries 5 min and log reason.

Clock Skew: If local vs exchange > 500 ms → halt entries.

Restart Idempotency: Reconcile orphan orders; no position unless explicit recovery flag set.

Parameter Guards: Invalid OR windows or slot overlaps → block and emit ConfigError.

12) USI (Unified Strategy API) — Frozen Surface (Names Only)
ISdk: Version, Capabilities, Initialize(ctx), OnBar(bar), OnExecution(exec), Shutdown(); Orders(), Risk(), Sizer(), Session(), Trail(), Telemetry(), Diag(), Backtest()

IOrders: SubmitEntry(intent)→OrderAck; AttachBrackets(b); ModifyStop(adj); FlattenAll(reason)

IRisk: Evaluate(snapshot)→RiskStatus { CanTrade, LastBreach, Mode }; Context()→RiskContextView { Mode, StopTradingActive, CircuitBreakerActive, LimitsView, ConfMin, SizeMult, LossStreakState }

ISizing: Size(request)→SizingResult { Contracts, Reason } (applies SIZE_MULT after base calc; caps per §5.1)

ISession: IsTradableNow(t); IsSettlementBlackout(t); ORBWindow(day); EODCutoff(day); IsBreak(t)

ITrailing: Arm(profile, ctx); Update(ctx)→StopLevel (non‑loosening invariant)

ITelemetry: Log(event, tradeCtx) — standardized schema in §8

IDiagnostics: Enabled; ExportRunParameters(path, runCtx)

IBacktestHooks: Costs(commissions, slippage); Seed; Report(slice)

DTOs (append‑only): OrderIntent, Brackets, StopAdjust, SizingRequest/Result, RiskSnapshot/Status, BarInfo, ExecutionInfo, TrailContext, StopLevel, TradeContext, RunContext, BacktestCosts, CapabilityFlags, ReasonCode, OrderAck, LossStreakState

13) Spec → Code Compliance Matrix
Spec Item	NT8 Module/File	Path (target)	Harness/Test Action
USI stability	/Abstractions/*.cs	/Abstractions/	Build contracts in isolation; reflection snapshot stable
Adapter wiring	SdkFacade.cs, SdkAdapterV1.cs	/Facade/	Strategy compiles via USI only; all flows pass
Calendar/settlement/breaks	SessionCalendar.cs	/Common/Session/SessionCalendar.cs	Load JSON; enforce blackouts; daily break
Entry locks per slot	TimeFilter.cs	/Common/Session/TimeFilter.cs	One‑trade‑per‑direction per slot; resets at session
Indicators & warmup	IndicatorHelper.cs	/Common/Indicators/IndicatorHelper.cs	VWAP=ΣPV/ΣV; warmup gate
PnL & high‑water & TTD	PnLTracker.cs	/Common/Risk/PnLTracker.cs	HWM math; TTD buffer calc; manual reset
Modes/overlays (Eval/CB/ECP)	RiskManager.cs	/Common/Risk/RiskManager.cs	Hit each threshold; verify actions & reasons
Loss‑Streak Guard	LossStreakGuard.cs	/Common/Risk/LossStreakGuard.cs	2‑loss lockout; next‑day first‑loss lockout
Sizing & caps	PositionSizer.cs	/Common/Sizing/PositionSizer.cs	Closed‑form tests; integer lots; cap clipping; SIZE_MULT applied
Order safety & OCO	OrderRouter.cs	/Common/Exec/OrderRouter.cs	Brackets armed; reconnect preserves stop
Trails invariant	TrailProfiles.cs	/Common/Exits/TrailProfiles.cs	Synthetic paths; non‑loosening asserted
Diagnostics & export	DiagnosticsSwitch.cs	/Common/Diag/DiagnosticsSwitch.cs	Manifest includes Mode, CB, caps, loss‑streak
Standard logs	TradeLogger.cs	/Common/Log/TradeLogger.cs	Regex schema + deterministic ordering
Backtest costs	BacktestHarness.cs	/Harness/Backtest/	Slippage to mkt/stop only; fees on; latency model
Walk‑Forward gate	WalkForwardRunner.cs	/Harness/WFA/	6m/2m×4; accept rule enforced
Monte Carlo bands	MonteCarloBootstrapper.cs	/Harness/MonteCarlo/	5k runs; 5th‑pctile gates enforced

14) Open Unknowns (Blocking for CI “green”)
Stub	Default	Verification Note
PCP_BAL_THRESHOLD_USD	57,100	Confirm account policy or make tier‑dependent
DAILY_LOSS_MULT[tier]	1.0	Confirm per tier for eval vs funded
OPEN_RISK_MULT[tier]	0.5	Confirm percent‑of‑equity policy
Holiday JSON coverage	Provided months	Ensure full 12‑month CME calendar in JSON; keep updated

15) Roadmap (Foundation‑First; Strategies Later)
Sprint	Deliverable	Exit Criteria
3.5.1	/Abstractions + Facade/Adapter	Contracts compile alone; adapter smoke tests pass
3.5.2	Indicators, SessionCalendar (+settlement blackouts), TimeFilter	Blackouts & EOD/break enforced
3.5.3	PnLTracker, RiskManager (TTD/CB/ECP), LossStreakGuard, PositionSizer	Thresholds & gates pass unit tests
3.5.4	TrailProfiles, OrderRouter, TradeLogger, Diagnostics	Non‑loosening invariant; param export OK
3.5.5	Harness (Backtest, WFA, MC) + QA.TestKit	Acceptance gates automated; determinism proven
3.5.6	Strategy shells (NY×3, LDN×1, ASIA×1) over USI	Shells compile; slots validated

16) LLM Target Profile — NinjaTrader 8 (Embed for Codex/LLM)
Target & Rules

Target: NT8, .NET 4.8, C# 7.3

Calculate: OnEachTick (live); OnBarClose allowed in harness

One public class per file; filename == class name

Allowed namespaces:
System, NinjaTrader.Cbi, NinjaTrader.Data, NinjaTrader.Gui.Tools, NinjaTrader.NinjaScript, NinjaTrader.NinjaScript.Strategies, NinjaTrader.NinjaScript.Indicators

No external deps; no extra files beyond manifest.

Determinism: same inputs ⇒ same trades/logs/exports.

Prop safety: Eval overlay (3,020), TTD=2,500, CB=2,250/2,400, ECP=2,000, loss‑streak lockout, 7 minis/70 micros hard caps.

Strategy constraint: strategies call USI only.

Forbidden: files/namespaces not listed in §17.

17) File Manifest (exact; generate only these)
Abstractions (USI)
/Abstractions/ISdk.cs
/Abstractions/IOrders.cs
/Abstractions/IRisk.cs
/Abstractions/ISizing.cs
/Abstractions/ISession.cs
/Abstractions/ITrailing.cs
/Abstractions/ITelemetry.cs
/Abstractions/IDiagnostics.cs
/Abstractions/IBacktestHooks.cs
/Abstractions/Dto.cs

Facade & Adapter
/Facade/SdkFacade.cs
/Facade/SdkCapabilities.cs
/Facade/SdkAdapterV1.cs

Core SDK
/Common/SDKBootstrap.cs
/Common/Indicators/IndicatorHelper.cs
/Common/Session/SessionCalendar.cs
/Common/Session/TimeFilter.cs
/Common/Log/TradeLogger.cs
/Common/Diag/DiagnosticsSwitch.cs
/Common/Risk/RiskTiers.cs
/Common/Risk/PnLTracker.cs
/Common/Risk/RiskManager.cs
/Common/Risk/LossStreakGuard.cs
/Common/Exits/TrailProfiles.cs
/Common/Exec/OrderRouter.cs
/Common/Sizing/PositionSizer.cs

Harness / QA
/Harness/Backtest/BacktestHarness.cs
/Harness/WFA/WalkForwardRunner.cs
/Harness/MonteCarlo/MonteCarloBootstrapper.cs
/Harness/QA.TestKit/SyntheticData.cs

Strategy Shell (USI‑only)
/Strategies/Shells/CrabelORBStrategy.cs

Total files: 31 (adds LossStreakGuard.cs). Do not add or rename.

18) Codex Prompt Contract (Paste‑Ready for Generation)
Instruction to Codex:
Generate exactly the 31 files in §17 with the exact paths/filenames. Use only the allowed namespaces in §16. Implement the public surfaces named in §12; DTOs are append‑only. Surface all parameters with Display(...) and bounds from §10. Enforce: determinism; standardized logging (§8); diagnostics export; session/break/holiday+settlement blackouts (§3); risk modes, TTD=2,500, Circuit Breaker 2,250/2,400, ECP at 2,000, Eval overlay at 3,020; Loss‑Streak Guard (§4.3); dynamic sizing + 7 minis/70 micros hard caps (§5); trailing non‑loosening (§6); and harness acceptance gates (§9). Do not add files/helpers/namespaces beyond the manifest. Strategies must call USI only.

Acceptance Gates (must pass)

Compiles clean in NT8 on first pass (one public class per file; namespaces allowed).

Determinism: identical runs → identical logs + identical param export.

Sessions: holiday JSON loaded; settlement & maintenance blackouts; daily break 17:00–18:00 ET; slot locks.

Risk: Eval, CB, ECP, TTD_STOP, MaxContracts, OpenRisk, Margin, TimeBlocked actions & reasons correct.

Loss‑Streak Guard: 2‑loss day lockout; next‑day first‑loss lockout.

Sizer: integer lots; SIZE_MULT; 7/70 caps enforced.

Trails: all profiles satisfy non‑loosening after arming.

WFA/MC: OOS CAR/MDD ≥ 0.60× IS (median); MC 5th‑pctile CAR/MDD ≥ 0.40× IS and 5th‑pctile MaxDD within prop limits.

19) Run‑Ready Verification Plan (QA)
A. Static & Compile

Place files per §17; NT8 compile (F5) → 0 errors.

Reflection test: /Abstractions/* interfaces unchanged vs baseline snapshot.

B. Calendar & Sessions
3. Load CME calendar JSON; validate parsing.
4. Settlement micro‑window (e.g., equity index 15:14:30–15:15:00 CT): assert no orders (TimeBlocked).
5. Maintenance 16:15–17:00 CT: assert no orders.
6. Daily break 17:00–18:00 ET: assert no orders.
7. EOD flatten checks: RTH 15:25 and ETH 17:00.

C. Indicators & Warmup
8. VWAP truth test: feed known ΣPV/ΣV; assert equality.
9. Warmup gate: READY only after ≥ 2× max lookback.

D. Risk, Modes, TTD, Loss‑Streak
10. Eval overlay: set EVAL_TARGET_PNL_USD=50; inject +60 PnL; STOP_TRADING_ON and entries blocked; manage opens; STOP_TRADING_OFF on reset.
11. Circuit Breaker: set buffer to 2,230; expect CIRCUIT_BREAK_ON; entries blocked; recover buffer to ≥2,400 → CIRCUIT_BREAK_OFF.
12. ECP: set buffer to 1,980; expect Mode=ECP (SIZE_MULT=0.20, CONF_MIN=90).
13. TTD_STOP: set buffer to 0; flatten + block, reason TrailingDD.
14. Loss‑Streak Guard A: 2 consecutive losing trades in day ⇒ day lockout; verify event & block.
15. Loss‑Streak Guard B: prior day ended via A; next day first closed trade loss ⇒ immediate lockout.
16. Daily loss cap disabled? (Base daily cap still enforced via tier math; verify weekly cap disabled).

E. Sizing & Caps
17. Closed‑form checks (MES/ES/NQ/CL/GC): integer lots; SIZE_MULT applied; 7 minis / 70 micros cap enforced; <1 ⇒ SizeInsufficient.

F. Trailing & Exits
18. Arm each TrailProfile; synthetic path asserts non‑loosening.
19. Time stop: flatten at configured TIME_STOP_HHMM if no TP.

G. Diagnostics & Logs
20. Diagnostics ON: run twice; param manifest and logs byte‑identical.
21. Logs include MODE_CHANGED, STOP_TRADING_*, CIRCUIT_BREAK_*, DAILY_LOCK_AFTER_2_LOSSES, NEXTDAY_FIRST_LOSS_LOCKOUT.

Pass/Fail: Any failed step blocks release; defect logged with NDJSON excerpt and step ID.

20) Prop‑Firm Specific Notes (Deterministic, Testable)
Evaluation overlay applies only in evaluation risk mode; park at NET_PNL_USD ≥ 3,020.

TTD: 2,500 USD per account; includes open & unrealized; no auto reset; manual reset supported and audited.

Circuit Breaker: at 2,250 buffer, entries off until 2,400 or session reset.

ECP: at 2,000 buffer, enforce strictest mode controls.

No weekly loss cap; no daily trade limit; daily 2‑loss lockout and next‑day first‑loss lockout enforced.

Max size caps: 7 minis or 70 micros (hard, all modes).

21) Determinism, Seeds & Cost Model
Fixed seed for any stochastic components (e.g., MC reshuffles).

Backtest slippage: per‑symbol constant; applied only to market/stop touches (not passive limits).

Decision‑to‑route latency model: 100–250 ms (fixed seed).

22) Deliverables & Handoff
This spec is the single source of truth for the greenfield SDK v3.5.

Codex/new team must generate only the files in §17 and satisfy acceptance gates in §18.

Any change to USI requires a versioned adapter; do not mutate USI contracts in place.

End of Spec
Verification Notes

Provide/point CALENDAR_JSON_PATH to the attached CME calendar file for 24×5 settlement/maintenance enforcement.

Keep a frozen reflection snapshot of /Abstractions/* to enforce API stability across releases.








Ask ChatGPT
