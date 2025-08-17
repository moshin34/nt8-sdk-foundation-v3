<h1 align="left">NT8 SDK Foundation v3</h1>

<p align="left">
  <a href="https://github.com/moshin34/nt8-sdk-foundation-v3/actions/workflows/nt8-guard.yml">
    <img alt="NT8 Guard" src="https://github.com/moshin34/nt8-sdk-foundation-v3/actions/workflows/nt8-guard.yml/badge.svg" />
  </a>
  <a href="https://github.com/moshin34/nt8-sdk-foundation-v3/actions/workflows/package-sdk.yml">
    <img alt="Package SDK" src="https://github.com/moshin34/nt8-sdk-foundation-v3/actions/workflows/package-sdk.yml/badge.svg" />
  </a>
  <a href="https://github.com/moshin34/nt8-sdk-foundation-v3/actions/workflows/release-on-tag.yml">
    <img alt="Release on Tag" src="https://github.com/moshin34/nt8-sdk-foundation-v3/actions/workflows/release-on-tag.yml/badge.svg" />
  </a>
  <a href="https://github.com/moshin34/nt8-sdk-foundation-v3/actions/workflows/codeql.yml">
    <img alt="CodeQL" src="https://github.com/moshin34/nt8-sdk-foundation-v3/actions/workflows/codeql.yml/badge.svg" />
  </a>
  <a href="https://github.com/moshin34/nt8-sdk-foundation-v3/releases">
    <img alt="Latest Release" src="https://img.shields.io/github/v/release/moshin34/nt8-sdk-foundation-v3?display_name=release" />
  </a>
  <a href="https://github.com/moshin34/nt8-sdk-foundation-v3/blob/main/LICENSE">
    <img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-blue.svg" />
  </a>
  <a href="https://github.com/moshin34/nt8-sdk-foundation-v3/issues">
    <img alt="Open Issues" src="https://img.shields.io/github/issues/moshin34/nt8-sdk-foundation-v3" />
  </a>
</p>

> Institutional-grade NT8 strategy shell with **risk gating** (daily/weekly/trailing), **telemetry**, **QA harness**, and **CI packaging**.  
> Target go-live: **Aug 18, 2025**.

---

## 🔎 At a Glance
- **Risk Enforcement:** Per-tick trade gating with daily/weekly caps + trailing drawdown  
- **Observability:** File telemetry + CI QA artifacts (`qa/summary.json`)  
- **CI/CD:** Guard workflow, packaging, release-on-tag, CodeQL  
- **Docs-as-Source-of-Truth:** Roadmap, Blueprint, Runbook, ADRs

---

## ⚡ Quickstart
```bash
# Requirements: .NET 8 SDK
dotnet restore SDK.sln
dotnet build SDK.sln -c Release

# (optional) run tests / harness scripts if present
pwsh ./tools/test.ps1  # if exists
```

### Sim Smoke (evidence capture)

```bash
# Run SIM in NT8, then collect logs:
./scripts/collect-sim-logs.sh     # creates sim_evidence_*.zip with logs + qa/summary.json
```

---

## 🧭 Project Navigation

* **Roadmap:** [/docs/roadmap/ROADMAP.md](docs/roadmap/ROADMAP.md)
* **Blueprint:** [/docs/blueprint/BLUEPRINT.md](docs/blueprint/BLUEPRINT.md)
* **Runbook:** [/docs/runbook.md](docs/runbook.md)
* **History Index:** [/docs/history/README.md](docs/history/README.md)
* **ADRs:** [/adr/](adr/)

---

## 🔒 CI Controls (Toggles)

* Toggle “CI must pass” locally:

  * macOS/Linux: `./scripts/toggle-ci.sh on|off`
  * Windows: `./scripts/toggle-ci.ps1 on|off`

---

## 🧰 Contributing & Security

* **Contributing:** See [CONTRIBUTING.md](CONTRIBUTING.md)
* **Security Policy:** See [SECURITY.md](SECURITY.md)

---

<!-- README_TOP_END -->

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

## Project Labels
This repo uses standardized labels: `decision`, `docs`, `ci`, `build`, `feature`, `fix`, `bug`, `risk`, `telemetry`, `task`.
