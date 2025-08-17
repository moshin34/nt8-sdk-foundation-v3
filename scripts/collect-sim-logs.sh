#!/usr/bin/env bash
set -euo pipefail
OUT="sim_evidence_$(date -u +%Y%m%dT%H%M%SZ).zip"
mkdir -p logs qa
zip -r "$OUT" logs qa/summary.json 2>/dev/null || true
echo "Collected SIM evidence -> $OUT"
