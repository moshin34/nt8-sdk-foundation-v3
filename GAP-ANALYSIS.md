# Full Gap Analysis – Latest SDK vs. v3.5 Spec

**C# files:** 73  
**Total LOC (approx):** 4361

## Coverage Matrix

| Area                           | Status   | Present Folders   | Symbols Found                                                                                                | Missing Must-Haves                      | Keyword Evidence (samples)                 |
|:-------------------------------|:---------|:------------------|:-------------------------------------------------------------------------------------------------------------|:----------------------------------------|:-------------------------------------------|
| Documentation / Compile Target | Missing  |                   |                                                                                                              | README.md, compile-target.md            |                                            |
| Tooling / CI / Guardrails      | Missing  |                   |                                                                                                              | nt8_guard.py                            |                                            |
| Acceptance Gates & Metrics     | Partial  |                   |                                                                                                              | acceptance.json, acceptance.schema.json | MAR                                        |
| QA Harness (Backtest/WF/MC)    | Partial  |                   | BacktestHarness, QuickStartRunner, WalkForwardRunner                                                         | MonteCarlo                              | bootstrap, walk-forward                    |
| SDK Facade & Defaults          | Partial  |                   | Defaults, SdkCapabilities, SdkFacade                                                                         | SdkAdapter                              |                                            |
| Session & CME Calendar         | Partial  |                   | ISession, SessionCalendar                                                                                    | Blackout, Cme                           | settlement                                 |
| Strategies (Thin Shells)       | Partial  |                   | CrabelORBStrategy, StrategyBase, TemplateStrategy                                                            | ORB                                     | attempts, cooldown, opening range, session |
| USI / Abstractions             | Partial  |                   | IBacktestHooks, IDiagnostics, IOrders, IRisk, ISdk, ISession, ISizing, ITelemetry, ITrailing                 | Dto                                     |                                            |
| Orders Router & NT8 Adapter    | Present  |                   | OCO, OrderIntent, OrderRouter                                                                                |                                         | OCO                                        |
| Risk Modes & Prop-Firm Rules   | Present  |                   | CompositeRisk, DCP, ECP, HR, LossStreakGuard, PCP, RiskConfig, RiskEngine, RiskManager, RiskTiers            |                                         | lockout                                    |
| Sizing (Mode/Volatility-aware) | Present  |                   | BracketedQuantitySizing, FixedQuantitySizing, ISizeRule, PositionSizer                                       |                                         | ATR                                        |
| Telemetry & Diagnostics        | Present  |                   | DiagnosticsSwitch, IDiagnostics, ITelemetry, InMemoryDiagnostics, InMemoryTelemetry, PnLTracker, TradeLogger |                                         | JSON, diagnostics, log, telemetry          |
| Trailing Profiles              | Present  |                   | CompositeTrailingStop, FixedTicksTrailingStop, ITrailing, PercentTrailingStop                                |                                         | runner, step                               |

## High-Priority Gaps

- **USI / Abstractions** → Missing: Dto, Status: Partial
- **SDK Facade & Defaults** → Missing: SdkAdapter, Status: Partial
- **Session & CME Calendar** → Missing: Blackout, Cme, Status: Partial
- **QA Harness (Backtest/WF/MC)** → Missing: MonteCarlo, Status: Partial
- **Strategies (Thin Shells)** → Missing: ORB, Status: Partial
- **Tooling / CI / Guardrails** → Missing: nt8_guard.py, Status: Missing
- **Documentation / Compile Target** → Missing: README.md, compile-target.md, Status: Missing
- **Acceptance Gates & Metrics** → Missing: acceptance.json, acceptance.schema.json, Status: Partial

## Area Details (evidence)

### USI / Abstractions
- Folders detected: _none_
- Symbols found:
  - `IBacktestHooks` in `nt8-sdk-foundation-v3-main/Abstractions/IBacktestHooks.cs`
  - `IDiagnostics` in `nt8-sdk-foundation-v3-main/Abstractions/IDiagnostics.cs`
  - `IOrders` in `nt8-sdk-foundation-v3-main/Abstractions/IOrders.cs`
  - `IRisk` in `nt8-sdk-foundation-v3-main/Abstractions/IRisk.cs`
  - `IOrders` in `nt8-sdk-foundation-v3-main/Abstractions/ISdk.cs`
  - `IRisk` in `nt8-sdk-foundation-v3-main/Abstractions/ISdk.cs`
  - `ISizing` in `nt8-sdk-foundation-v3-main/Abstractions/ISdk.cs`
  - `ISession` in `nt8-sdk-foundation-v3-main/Abstractions/ISdk.cs`
  - `ITrailing` in `nt8-sdk-foundation-v3-main/Abstractions/ISdk.cs`
  - `ITelemetry` in `nt8-sdk-foundation-v3-main/Abstractions/ISdk.cs`

### SDK Facade & Defaults
- Folders detected: _none_
- Symbols found:
  - `SdkFacade` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `Defaults` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `SdkCapabilities` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `Defaults` in `nt8-sdk-foundation-v3-main/Common/RiskTiers.cs`
  - `SdkFacade` in `nt8-sdk-foundation-v3-main/Facade/SdkAdapterV1.cs`
  - `SdkCapabilities` in `nt8-sdk-foundation-v3-main/Facade/SdkAdapterV1.cs`
  - `SdkCapabilities` in `nt8-sdk-foundation-v3-main/Facade/SdkCapabilities.cs`
  - `SdkFacade` in `nt8-sdk-foundation-v3-main/Facade/SdkFacade.cs`
  - `Defaults` in `nt8-sdk-foundation-v3-main/Risk/RiskConfig.cs`

### Risk Modes & Prop-Firm Rules
- Folders detected: _none_
- Symbols found:
  - `ECP` in `nt8-sdk-foundation-v3-main/Abstractions/Dto.cs`
  - `PCP` in `nt8-sdk-foundation-v3-main/Abstractions/Dto.cs`
  - `DCP` in `nt8-sdk-foundation-v3-main/Abstractions/Dto.cs`
  - `HR` in `nt8-sdk-foundation-v3-main/Abstractions/Dto.cs`
  - `RiskManager` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `CompositeRisk` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `LossStreakGuard` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `RiskTiers` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `DCP` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `LossStreakGuard` in `nt8-sdk-foundation-v3-main/Common/LossStreakGuard.cs`
- Keyword evidence:
  - `lockout` → `nt8-sdk-foundation-v3-main/Abstractions/Dto.cs`
  - `lockout` → `nt8-sdk-foundation-v3-main/Abstractions/IRisk.cs`
  - `lockout` → `nt8-sdk-foundation-v3-main/Common/LossStreakGuard.cs`
  - `lockout` → `nt8-sdk-foundation-v3-main/Common/RiskManager.cs`
  - `lockout` → `nt8-sdk-foundation-v3-main/Facade/EntryGates.cs`
  - `lockout` → `nt8-sdk-foundation-v3-main/Facade/SdkBuilder.cs`
  - `lockout` → `nt8-sdk-foundation-v3-main/Harness/QuickStartRunner.cs`
  - `lockout` → `nt8-sdk-foundation-v3-main/Risk/BaseRisk.cs`
  - `lockout` → `nt8-sdk-foundation-v3-main/Risk/CompositeRisk.cs`
  - `lockout` → `nt8-sdk-foundation-v3-main/Risk/RiskConfig.cs`

### Sizing (Mode/Volatility-aware)
- Folders detected: _none_
- Symbols found:
  - `ISizeRule` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `PositionSizer` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `BracketedQuantitySizing` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `PositionSizer` in `nt8-sdk-foundation-v3-main/Common/PositionSizer.cs`
  - `PositionSizer` in `nt8-sdk-foundation-v3-main/Common/RiskTiers.cs`
  - `BracketedQuantitySizing` in `nt8-sdk-foundation-v3-main/Sizing/BracketedQuantitySizing.cs`
  - `FixedQuantitySizing` in `nt8-sdk-foundation-v3-main/Sizing/FixedQuantitySizing.cs`
  - `ISizeRule` in `nt8-sdk-foundation-v3-main/Sizing/ISizeRule.cs`
  - `ISizeRule` in `nt8-sdk-foundation-v3-main/Sizing/RuleBasedSizing.cs`
  - `FixedQuantitySizing` in `nt8-sdk-foundation-v3-main/Sizing/RuleBasedSizing.cs`
- Keyword evidence:
  - `ATR` → `nt8-sdk-foundation-v3-main/Abstractions/Dto.cs`
  - `ATR` → `nt8-sdk-foundation-v3-main/Trailing/TrailingEngine.cs`

### Orders Router & NT8 Adapter
- Folders detected: _none_
- Symbols found:
  - `OrderIntent` in `nt8-sdk-foundation-v3-main/Abstractions/Dto.cs`
  - `OCO` in `nt8-sdk-foundation-v3-main/Abstractions/Dto.cs`
  - `OrderIntent` in `nt8-sdk-foundation-v3-main/Abstractions/IOrders.cs`
  - `OrderRouter` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `OrderRouter` in `nt8-sdk-foundation-v3-main/Common/OrderRouter.cs`
  - `OrderIntent` in `nt8-sdk-foundation-v3-main/Common/OrderRouter.cs`
  - `OrderIntent` in `nt8-sdk-foundation-v3-main/Common/TradeLogger.cs`
  - `OrderIntent` in `nt8-sdk-foundation-v3-main/Facade/EntryPlan.cs`
  - `OCO` in `nt8-sdk-foundation-v3-main/Facade/EntryPlan.cs`
  - `OrderIntent` in `nt8-sdk-foundation-v3-main/Facade/EntryPlanner.cs`
- Keyword evidence:
  - `OCO` → `nt8-sdk-foundation-v3-main/Abstractions/Dto.cs`
  - `OCO` → `nt8-sdk-foundation-v3-main/Facade/EntryPlan.cs`
  - `OCO` → `nt8-sdk-foundation-v3-main/Facade/EntryPlanner.cs`
  - `OCO` → `nt8-sdk-foundation-v3-main/Strategies/StrategyBase.cs`

### Session & CME Calendar
- Folders detected: _none_
- Symbols found:
  - `ISession` in `nt8-sdk-foundation-v3-main/Abstractions/ISdk.cs`
  - `ISession` in `nt8-sdk-foundation-v3-main/Abstractions/ISession.cs`
  - `SessionCalendar` in `nt8-sdk-foundation-v3-main/Common/SessionCalendar.cs`
  - `ISession` in `nt8-sdk-foundation-v3-main/Facade/Sdk.cs`
  - `ISession` in `nt8-sdk-foundation-v3-main/Facade/SdkAdapterV1.cs`
  - `ISession` in `nt8-sdk-foundation-v3-main/Facade/SdkFacade.cs`
  - `ISession` in `nt8-sdk-foundation-v3-main/Session/CmeBlackoutService.cs`
  - `ISession` in `nt8-sdk-foundation-v3-main/Strategies/CrabelORBStrategy.cs`
- Keyword evidence:
  - `settlement` → `nt8-sdk-foundation-v3-main/Abstractions/ISession.cs`
  - `settlement` → `nt8-sdk-foundation-v3-main/Common/SessionCalendar.cs`
  - `settlement` → `nt8-sdk-foundation-v3-main/Config/CmeCalendarLoader.cs`
  - `settlement` → `nt8-sdk-foundation-v3-main/Config/CmeDay.cs`
  - `settlement` → `nt8-sdk-foundation-v3-main/Facade/EntryGates.cs`
  - `settlement` → `nt8-sdk-foundation-v3-main/Harness/QuickStartRunner.cs`
  - `settlement` → `nt8-sdk-foundation-v3-main/Session/CmeBlackoutService.cs`
  - `settlement` → `nt8-sdk-foundation-v3-main/Strategies/CrabelORBStrategy.cs`

### Trailing Profiles
- Folders detected: _none_
- Symbols found:
  - `ITrailing` in `nt8-sdk-foundation-v3-main/Abstractions/ISdk.cs`
  - `ITrailing` in `nt8-sdk-foundation-v3-main/Abstractions/ITrailing.cs`
  - `FixedTicksTrailingStop` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `ITrailing` in `nt8-sdk-foundation-v3-main/Common/TrailProfiles.cs`
  - `FixedTicksTrailingStop` in `nt8-sdk-foundation-v3-main/Common/TrailProfiles.cs`
  - `ITrailing` in `nt8-sdk-foundation-v3-main/Facade/Sdk.cs`
  - `ITrailing` in `nt8-sdk-foundation-v3-main/Facade/SdkAdapterV1.cs`
  - `ITrailing` in `nt8-sdk-foundation-v3-main/Facade/SdkFacade.cs`
  - `ITrailing` in `nt8-sdk-foundation-v3-main/Strategies/CrabelORBStrategy.cs`
  - `FixedTicksTrailingStop` in `nt8-sdk-foundation-v3-main/Trailing/CompositeTrailingStop.cs`
- Keyword evidence:
  - `step` → `nt8-sdk-foundation-v3-main/Common/TrailProfiles.cs`
  - `runner` → `nt8-sdk-foundation-v3-main/Harness/QuickStartProgram.cs`
  - `runner` → `nt8-sdk-foundation-v3-main/Harness/QuickStartRunner.cs`
  - `runner` → `nt8-sdk-foundation-v3-main/Harness/WalkForwardRunner.cs`
  - `step` → `nt8-sdk-foundation-v3-main/QA.TestKit/SyntheticData.cs`
  - `step` → `nt8-sdk-foundation-v3-main/Risk/RiskEngine.cs`
  - `step` → `nt8-sdk-foundation-v3-main/Trailing/TrailingEngine.cs`

### Telemetry & Diagnostics
- Folders detected: _none_
- Symbols found:
  - `IDiagnostics` in `nt8-sdk-foundation-v3-main/Abstractions/IDiagnostics.cs`
  - `ITelemetry` in `nt8-sdk-foundation-v3-main/Abstractions/ISdk.cs`
  - `IDiagnostics` in `nt8-sdk-foundation-v3-main/Abstractions/ISdk.cs`
  - `ITelemetry` in `nt8-sdk-foundation-v3-main/Abstractions/ITelemetry.cs`
  - `InMemoryTelemetry` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `InMemoryDiagnostics` in `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `DiagnosticsSwitch` in `nt8-sdk-foundation-v3-main/Common/DiagnosticsSwitch.cs`
  - `PnLTracker` in `nt8-sdk-foundation-v3-main/Common/PnLTracker.cs`
  - `TradeLogger` in `nt8-sdk-foundation-v3-main/Common/TradeLogger.cs`
  - `IDiagnostics` in `nt8-sdk-foundation-v3-main/Diagnostics/InMemoryDiagnostics.cs`
- Keyword evidence:
  - `log` → `nt8-sdk-foundation-v3-main/Abstractions/Dto.cs`
  - `telemetry` → `nt8-sdk-foundation-v3-main/Abstractions/Dto.cs`
  - `diagnostics` → `nt8-sdk-foundation-v3-main/Abstractions/Dto.cs`
  - `diagnostics` → `nt8-sdk-foundation-v3-main/Abstractions/IDiagnostics.cs`
  - `telemetry` → `nt8-sdk-foundation-v3-main/Abstractions/ISdk.cs`
  - `diagnostics` → `nt8-sdk-foundation-v3-main/Abstractions/ISdk.cs`
  - `log` → `nt8-sdk-foundation-v3-main/Abstractions/ITelemetry.cs`
  - `telemetry` → `nt8-sdk-foundation-v3-main/Abstractions/ITelemetry.cs`
  - `telemetry` → `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `diagnostics` → `nt8-sdk-foundation-v3-main/Common/Defaults.cs`

### QA Harness (Backtest/WF/MC)
- Folders detected: _none_
- Symbols found:
  - `BacktestHarness` in `nt8-sdk-foundation-v3-main/Harness/BacktestHarness.cs`
  - `QuickStartRunner` in `nt8-sdk-foundation-v3-main/Harness/QuickStartProgram.cs`
  - `QuickStartRunner` in `nt8-sdk-foundation-v3-main/Harness/QuickStartRunner.cs`
  - `BacktestHarness` in `nt8-sdk-foundation-v3-main/Harness/WalkForwardRunner.cs`
  - `WalkForwardRunner` in `nt8-sdk-foundation-v3-main/Harness/WalkForwardRunner.cs`
- Keyword evidence:
  - `bootstrap` → `nt8-sdk-foundation-v3-main/Harness/MonteCarloBootstrapper.cs`
  - `walk-forward` → `nt8-sdk-foundation-v3-main/Harness/WalkForwardRunner.cs`

### Strategies (Thin Shells)
- Folders detected: _none_
- Symbols found:
  - `StrategyBase` in `nt8-sdk-foundation-v3-main/Harness/BacktestHarness.cs`
  - `TemplateStrategy` in `nt8-sdk-foundation-v3-main/Harness/BacktestHarness.cs`
  - `StrategyBase` in `nt8-sdk-foundation-v3-main/Harness/WalkForwardRunner.cs`
  - `TemplateStrategy` in `nt8-sdk-foundation-v3-main/Harness/WalkForwardRunner.cs`
  - `StrategyBase` in `nt8-sdk-foundation-v3-main/Strategies/CrabelORBStrategy.cs`
  - `CrabelORBStrategy` in `nt8-sdk-foundation-v3-main/Strategies/CrabelORBStrategy.cs`
  - `StrategyBase` in `nt8-sdk-foundation-v3-main/Strategies/StrategyBase.cs`
  - `StrategyBase` in `nt8-sdk-foundation-v3-main/Strategies/TemplateStrategy.cs`
  - `TemplateStrategy` in `nt8-sdk-foundation-v3-main/Strategies/TemplateStrategy.cs`
- Keyword evidence:
  - `session` → `nt8-sdk-foundation-v3-main/Abstractions/Dto.cs`
  - `session` → `nt8-sdk-foundation-v3-main/Abstractions/ISdk.cs`
  - `session` → `nt8-sdk-foundation-v3-main/Abstractions/ISession.cs`
  - `session` → `nt8-sdk-foundation-v3-main/Common/Defaults.cs`
  - `session` → `nt8-sdk-foundation-v3-main/Common/SessionCalendar.cs`
  - `session` → `nt8-sdk-foundation-v3-main/Facade/EntryGates.cs`
  - `session` → `nt8-sdk-foundation-v3-main/Facade/EntryPlan.cs`
  - `session` → `nt8-sdk-foundation-v3-main/Facade/Sdk.cs`
  - `session` → `nt8-sdk-foundation-v3-main/Facade/SdkAdapterV1.cs`
  - `session` → `nt8-sdk-foundation-v3-main/Facade/SdkBuilder.cs`

### Tooling / CI / Guardrails
- Folders detected: _none_

### Documentation / Compile Target
- Folders detected: _none_

### Acceptance Gates & Metrics
- Folders detected: _none_
- Keyword evidence:
  - `MAR` → `nt8-sdk-foundation-v3-main/Abstractions/Dto.cs`
  - `MAR` → `nt8-sdk-foundation-v3-main/Abstractions/IBacktestHooks.cs`
  - `MAR` → `nt8-sdk-foundation-v3-main/Abstractions/IClock.cs`
  - `MAR` → `nt8-sdk-foundation-v3-main/Abstractions/IDiagnostics.cs`
  - `MAR` → `nt8-sdk-foundation-v3-main/Abstractions/IOrders.cs`
  - `MAR` → `nt8-sdk-foundation-v3-main/Abstractions/IRisk.cs`
  - `MAR` → `nt8-sdk-foundation-v3-main/Abstractions/ISdk.cs`
  - `MAR` → `nt8-sdk-foundation-v3-main/Abstractions/ISession.cs`
  - `MAR` → `nt8-sdk-foundation-v3-main/Abstractions/ISizing.cs`
  - `MAR` → `nt8-sdk-foundation-v3-main/Abstractions/ITelemetry.cs`
