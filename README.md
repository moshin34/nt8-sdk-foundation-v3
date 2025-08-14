# NT8 Core SDK – Prop Firm & Micro Account Trading

Institutional-grade, risk-compliant NinjaTrader 8 SDK for automated futures trading.  The codebase is organized around the **Universal Strategy Interface (USI)** with clear layers: low-level abstractions, reusable components, and harness utilities.

## Quick Start
1. Clone the repository
   ```bash
   git clone https://github.com/moshin34/nt8-sdk-foundation-v3.git
   cd nt8-sdk-foundation-v3
   ```
2. Run the guard script to enforce repo rules
   ```bash
   python tools/nt8_guard.py --fail-on-warn
   ```
3. Build the SDK with Mono
   ```bash
   mcs -target:library Abstractions/*.cs Common/*.cs Facade/*.cs Strategies/*.cs Harness/*.cs Risk/*.cs Sizing/*.cs Session/*.cs Trailing/*.cs Telemetry/*.cs Diagnostics/*.cs QA.TestKit/*.cs Config/*.cs -r:/usr/lib/mono/4.5/System.Web.Extensions.dll -out:build/sdk.dll
   ```

## Build
- `python tools/nt8_guard.py --fail-on-warn`
- `mcs -target:library Abstractions/*.cs Common/*.cs Facade/*.cs Strategies/*.cs Harness/*.cs Risk/*.cs Sizing/*.cs Session/*.cs Trailing/*.cs Telemetry/*.cs Diagnostics/*.cs QA.TestKit/*.cs Config/*.cs -r:/usr/lib/mono/4.5/System.Web.Extensions.dll -out:build/sdk.dll`
- Optional debug runner:
  - `mcs -define:DEBUG -out:build/quickstart.exe Abstractions/*.cs Common/*.cs Facade/*.cs Strategies/*.cs Harness/*.cs Risk/*.cs Sizing/*.cs Session/*.cs Trailing/*.cs Telemetry/*.cs Diagnostics/*.cs QA.TestKit/*.cs Config/*.cs -r:/usr/lib/mono/4.5/System.Web.Extensions.dll -main:NT8.SDK.Harness.QuickStartProgram`
  - `mono build/quickstart.exe`

## Repo Map
- `Abstractions/` – USI interfaces and minimal contracts
- `Common/` – shared utilities and constants
- `Facade/` – SDK façade wiring all components together
- `Strategies/` – strategy templates using the façade
- `Harness/` – local runners and debug harnesses
- `Risk/` – risk rules and evaluation
- `Sizing/` – contract sizing logic
- `Session/` – exchange session handling
- `Trailing/` – trailing drawdown and stop logic
- `Telemetry/` – instrumentation hooks
- `Diagnostics/` – troubleshooting helpers
- `QA.TestKit/` – synthetic data and testing helpers
- `Config/` – calendars and seed data
- `Docs/` – specifications and runbooks
- `tools/` – development scripts including nt8_guard

## Contributing
- Target **.NET Framework 4.8** and **C# 7.3**; no `record`, `init`, advanced pattern matching, `async/await`, tuples beyond C# 7.3, or file-scoped namespaces.
- One public class per file and no NinjaTrader types in portable layers.
- Every public type and member requires an XML documentation comment.
- `IRisk.EvaluateEntry` implementations must accept an empty string without throwing.
- Trailing stop logic must never loosen stops once tightened.
- Run `python tools/nt8_guard.py --fail-on-warn` and build with `mcs` before sending a pull request.
