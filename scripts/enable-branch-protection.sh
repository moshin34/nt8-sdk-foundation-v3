#!/usr/bin/env bash
set -euo pipefail
: "${GITHUB_TOKEN:?}"; : "${OWNER:?}"; : "${REPO:?}"; : "${BRANCH:=main}"

api="https://api.github.com/repos/${OWNER}/${REPO}/branches/${BRANCH}/protection"
payload=$(cat <<'JSON'
{
  "required_status_checks": {
    "strict": false,
    "contexts": [
      "NT8 Guard (layout-aware) / guard",
      "NT8 Guard / build-test",        // temporary shim — remove after hardening
      "Guard & Build / guard",
      "Guard & Build / build",
      "QA Validate / validate",
      "Code scanning results / CodeQL",
      "CodeQL / analyze"
    ]
  },
  "enforce_admins": true,
  "required_pull_request_reviews": {
    "required_approving_review_count": 1,
    "dismiss_stale_reviews": false
  },
  "restrictions": null
}
JSON
)

curl -sS -X PUT -H "Authorization: token ${GITHUB_TOKEN}" \
     -H "Accept: application/vnd.github+json" \
     --data "${payload}" "${api}"

echo "✅ Branch protection restored for ${OWNER}/${REPO}@${BRANCH}"
echo "NOTE: 'NT8 Guard / build-test' is a temporary shim; remove after guard hardening."
