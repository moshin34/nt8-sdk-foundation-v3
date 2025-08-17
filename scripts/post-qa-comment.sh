#!/usr/bin/env bash
set -euo pipefail
PR_NUMBER="${1:-}"
FILE="qa/summary.json"
if [ ! -f "$FILE" ] && [ -f "**/qa/summary.json" ]; then FILE="**/qa/summary.json"; fi
if ! ls $FILE >/dev/null 2>&1; then
  echo "No qa/summary.json found; skipping comment."
  exit 0
fi
MAR=$(jq -r '.MAR // .metrics.MAR // "n/a"' $FILE 2>/dev/null || echo "n/a")
HIT=$(jq -r '.HitRate // .metrics.HitRate // "n/a"' $FILE 2>/dev/null || echo "n/a")
MDD=$(jq -r '.MaxDrawdown // .metrics.MaxDrawdown // "n/a"' $FILE 2>/dev/null || echo "n/a")
cat > /tmp/qa.md <<EOF
**QA Summary**
- MAR: \`$MAR\`
- Hit Rate: \`$HIT\`
- Max DD: \`$MDD\`

_Source: \`qa/summary.json\`_
EOF
gh pr comment "$PR_NUMBER" -F /tmp/qa.md

