# NinjaTrader 8 Compile & Run Guide

> Target: **.NET Framework 4.8**, C# 7.x (NT8’s compiler).
>
> Goal: Compile the SDK inside NinjaTrader 8, wire the NT-managed orders adapter, and run the demo strategy (`SdkStrategyShell`) on a chart.

---

## 1) What you’ll build inside NT8

- A single **Custom** assembly that includes:
  - The portable SDK (abstractions + risk + sizing + session + trailing + façade).
  - The NT-managed orders adapter `Nt8Orders`.
  - The sample strategy `SdkStrategyShell`.

Everything compiles into **Documents\NinjaTrader 8\bin\Custom\** (NT’s default).

---

## 2) One-time NinjaTrader setup

1. **Open NinjaTrader 8** (Control Center).
2. Open the **NinjaScript Editor**: `New → NinjaScript Editor`.
3. In the editor’s **References** (tree at left), **right-click → Add** the following if not already present:
   - `System.Web.Extensions`  
     (needed for `JavaScriptSerializer` used by the CME calendar loader)
4. Optional but recommended: `Tools → Options → NinjaScript`  
   - Enable *“Break on exceptions”* during development.

---

## 3) File placement (drop-in layout)

> Copy the SDK source files from the repo into your local **Documents\NinjaTrader 8\bin\Custom** tree as shown.  
> You can create the folders if they don’t exist.

Documents\NinjaTrader 8\bin\Custom
│
├─ seeds
│ └─ cme_calendar_2025-08_to_2026-07.json
│
├─ AddOns
│ └─ NT8SDK
│ ├─ Abstractions\ (all files from repo /Abstractions)
│ ├─ Config\ (calendar models + loader)
│ ├─ Diagnostics\ (NoopDiagnostics)
│ ├─ Facade\ (Sdk.cs, SdkBuilder.cs, EntryGates, EntryPlan, EntryPlanner, PriceMath, etc.)
│ ├─ Orders\ (NullOrders – optional)
│ ├─ Risk\ (IClock/SystemClock, RiskConfig, RiskOptions, RiskEngine)
│ ├─ Session\ (CmeBlackoutService)
│ ├─ Sizing\ (SizeEngine)
│ ├─ Telemetry\ (NoopTelemetry)
│ └─ Trailing\ (TrailingEngine)
│
├─ AddOns
│ └─ NT8Bridge
│ └─ Nt8Orders.cs (NT-managed orders adapter)
│
└─ Strategies
└─ SdkStrategyShell.cs (demo strategy)

markdown
Copy
Edit

### Notes

- **Only the files above are needed** inside NT8. You do **not** need CI, harness, or GitHub workflow files.
- The **seed** JSON path is used at runtime by `CmeCalendarLoader`. The repo default expects a `seeds\...json` folder relative to the NT8 custom root, so place it exactly as shown.
- Namespaces are already `NT8.SDK.*` (one public class per file), which NT8’s compiler supports.

---

## 4) Compile inside NT8

1. With files in place, go to the **NinjaScript Editor**.
2. Press **F5** (or right-click → **Compile**).
3. You should see **“Compile successful”** in the editor’s status bar/output.

### Common compile fixes

- **`JavaScriptSerializer` not found**  
  Add the **System.Web.Extensions** reference (Section 2), then recompile.

- **Duplicate type / namespace**  
  Ensure there’s only one copy of each SDK file. Remove stale copies in other folders.

- **Path to seed file**  
  Make sure the `seeds\cme_calendar_2025-08_to_2026-07.json` file exists under the **Custom** folder.  
  (You can change the path in `Config/CmeCalendarLoader.cs` if you prefer a different location.)

---

## 5) Run the demo strategy

1. **New → Chart**, pick an instrument (e.g., **NQ**), and open a small time frame (e.g., 1-min).
2. Right-click the chart → **Strategies…** → add **`SdkStrategyShell`**.
3. Configure parameters (examples):
   - **Risk Mode**: `PCP`
   - **Loss Streak Lockout**: `2` (validated range)
   - **Lockout Duration (minutes)**: `15`
   - **Trailing Distance (ticks)**: `10`
   - **Target Distance (ticks)**: `20`
   - **Direction**: `Long` or `Short`
4. Click **Enable**.

### What the shell does

- Builds the SDK via `SdkBuilder`:
  - Injects **`Nt8Orders`** so entries route through NinjaTrader’s **Managed** methods.
  - Shares a clock across **Risk** and **Session** logic.
- **Gates** the entry (risk + session), **sizes** the order, **rounds** prices to tick size.
- Sets **OCO brackets first** (tick-aware stop & target with basic validation).
- Submits a **market** entry using the chosen direction.

You’ll see tracing in the **Output window** (and NT’s Control Center logs).

---

## 6) Troubleshooting runtime

- **“settlement window / blackout window”** in logs  
  Session gate is working. Try running outside those windows or tweak the seed file.

- **Stop rejected** (“stop must be below/above entry”)  
  The shell enforces non-loosening, side-correct protective stops and validates one-tick separation.

- **No orders placed**  
  Ensure you are **Flat**, the strategy is **Enabled**, and the **instrument** has a sensible tick size (e.g., NQ = 0.25).

---

## 7) Updating the SDK

- Replace the files in **Documents\NinjaTrader 8\bin\Custom\…** with newer versions from the repo.
- Press **F5** to recompile.
- If you add new dependencies, update **References** (Section 2) as needed.

---

## 8) Safety & live-trading disclaimer

This SDK and the demo shell are **for development**. Before any live use:
- Test on **Sim** / Market Replay.
- Verify bracket behavior, lockout timing, and session gates.
- Add robust error handling and persistence suitable for your workflow.

---

## 9) Quick checklist

- [ ] Files copied to **Custom** tree exactly as in Section 3  
- [ ] **System.Web.Extensions** reference added  
- [ ] `seeds\cme_calendar_2025-08_to_2026-07.json` present  
- [ ] **Compile success** (F5)  
- [ ] `SdkStrategyShell` enabled on a chart and trading as expected

---

## Appendix: Folder differences vs. repo

- CI/harness (`.github`, `Harness`, `QA.TestKit`, `tools`, etc.) are **not** needed inside NT8.
- The **NT8Bridge** folder only contains `Nt8Orders.cs` for managed order routing.
- If you later create your own strategies, place them under **Strategies** and reuse the SDK via `SdkBuilder`.

