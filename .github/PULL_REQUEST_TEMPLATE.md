## Summary
Describe what this PR adds/changes.

## Compile Target (must pass)
- [ ] **NinjaTrader 8**, **.NET Framework 4.8**, **C# 7.3** only
- [ ] One public class per file
- [ ] No modern C# constructs (`record`, `init`, advanced `switch` patterns, `async`)

## NinjaTrader Signatures (if Strategy code)
- [ ] Uses exact overrides:
  - `protected override void OnOrderUpdate(Order order)`
  - `protected override void OnExecutionUpdate(Execution execution, Order order)`

## Layering & Hygiene
- [ ] Pure C# layers (Abstractions/Config/Session/Risk/Sizing/Trailing/Diag/Telemetry) compile without NT8 references
- [ ] Bridge is NT8-aware but has **no indicators**
- [ ] Template strategy compiles and runs on SIM

## Diagnostics/Logs
- [ ] Diagnostics path is auto-created on first write (no exceptions)
- [ ] JSONL events are compact and redact PII/account numbers (if applicable)

## Testing
- [ ] Happy path: submits entry, attaches OCO stop/target, trails without widening
- [ ] Rejection path: protective flatten triggers

## Docs
- [ ] README updated if folder structure or usage changed
