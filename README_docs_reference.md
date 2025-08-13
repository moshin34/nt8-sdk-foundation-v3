## NinjaTrader Documentation Reference (for Codex)
This repository targets **NinjaTrader 8**, **.NET Framework 4.8**, and **C# 7.3**.  
Codex and contributors should consult the official NinjaScript documentation to confirm method signatures and platform behavior:

- https://developer.ninjatrader.com/docs/desktop

**Key Strategy overrides required by NT8:**
- `protected override void OnOrderUpdate(Order order)`
- `protected override void OnExecutionUpdate(Execution execution, Order order)`

**Language constraints:** No `record`, `init`, advanced pattern matching (`switch` with `when`), `async`/`await`, file-scoped namespaces, or multiple public classes per `.cs` file.
