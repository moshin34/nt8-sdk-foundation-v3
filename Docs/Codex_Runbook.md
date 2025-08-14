# Codex Runbook

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