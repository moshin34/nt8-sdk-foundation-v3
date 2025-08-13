# NT8 Core SDK – Prop Firm & Micro Account Trading

## Overview
Institutional-grade, risk-compliant NinjaTrader 8 SDK for automated futures trading.
Designed for prop firm challenges and micro capital growth, fully aligned with [NT8_Spec_Foundation_v3.5.md](Docs/NT8_Spec_Foundation_v3.5.md).

## Features
- Risk-tier aware contract sizing
- Trailing drawdown & capital protection
- Loss lockout logic
- CME settlement blackout logic
- Strategy-agnostic core for reusability

## Repo Layout
- `/Docs/` – Core specs & Codex runbooks
- `/config/` – Calendars & seed data
- `/QA/Trace/` – Test trace YAMLs

## Environment
- NinjaTrader 8, .NET 4.8, C# 7.3
- Strategy Calculate = OnEachTick (live)
- Windows dev workstation

## Getting Started
1. Clone repo
2. Review `/Docs/NT8_Spec_Foundation_v3.5.md`
3. Build in NinjaScript Editor
4. Run QA verification plan

## NinjaTrader Documentation Reference (for Codex)
This repository targets **NinjaTrader 8**, **.NET Framework 4.8**, and **C# 7.3**.  
Codex and contributors should consult the official NinjaScript documentation to confirm method signatures and platform behavior:

- https://developer.ninjatrader.com/docs/desktop

**Key Strategy overrides required by NT8:**
- `protected override void OnOrderUpdate(Order order)`
- `protected override void OnExecutionUpdate(Execution execution, Order order)`

**Language constraints:** No `record`, `init`, advanced pattern matching (`switch` with `when`), `async`/`await`, file-scoped namespaces, or multiple public classes per `.cs` file.

