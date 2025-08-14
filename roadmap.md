# ROADMAP

## Phase 1 — Portable SDK Core ✅
- [x] Abstractions (DTOs, IRisk, ISizing, ITrailing, IOrders, clock)
- [x] Risk engine + options + deterministic clock
- [x] CME session + blackout/settlement seed + gating
- [x] Sizing (base) and trailing (fixed ticks)
- [x] Planner + OCO, tick rounding, price-side validation
- [x] Façade + builder + no-op adapters
- [x] CI builds + QuickStart harness log assertions

## Phase 2 — NT8 Packaging & Compile (in progress)
- [ ] NT8 Orders adapter (managed) — **code in repo**
- [ ] Strategy shell compiles in NT8, places bracketed demo trade
- [ ] “How to compile in NT8” doc + screenshots
- [ ] CI: publish artifact bundle + quickstart log
- **Exit Criteria:** Strategy compiles & runs in NT8 with bracketed demo, README updated

## Phase 3 — Strategies & QA
- [ ] ORB reference strategy (RTH)
- [ ] ATR/OR trailing profiles
- [ ] Vol-/mode-aware sizing knobs
- [ ] Structured trade logs + enhanced telemetry
- [ ] Walk-forward / Monte Carlo harness (QA.TestKit)
- **Exit Criteria:** Two reference strategies, backtests reproducible, CI gates pass

## Parking Lot
- [ ] Selective cancel/modify (if/when we go Unmanaged)
- [ ] Additional CME seeds + holiday rules
- [ ] Export packaging for distribution
