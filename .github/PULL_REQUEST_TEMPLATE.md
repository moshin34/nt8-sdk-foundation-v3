## Summary
Describe what this PR adds/changes.

## Compile Target (must pass)
- [ ] NinjaTrader 8, .NET Framework 4.8, C# 7.3 only
- [ ] One public class per file
- [ ] No modern C# constructs (`record`, `init`, advanced `switch` patterns, `async`)

## NinjaTrader Signatures (if Strategy code)
- [ ] Uses exact overrides:
  - protected override void OnOrderUpdate(Order order)
  - protected override void OnExecutionUpdate(Execution execution, Order order)

## Layering & Hygiene
- [ ] Pure C# layers compile without NT8 refs
- [ ] Bridge has no indicators
- [ ] Template strategy compiles and runs on SIM

## Diagnostics/Logs
- [ ] Diagnostics path auto-created; no exceptions
- [ ] JSONL compact; no PII

## Testing
- [ ] Entry+OCO+trailing happy path
- [ ] Rejection → protective flatten

## Docs
- [ ] README updated if needed
