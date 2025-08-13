#!/usr/bin/env python3
import os, sys, re
from pathlib import Path

ROOT = Path(sys.argv[1]) if len(sys.argv) > 1 and not sys.argv[1].startswith("--") else Path(".")
FAIL_ON_WARN = "--fail-on-warn" in sys.argv

BANNED_TOKENS = [
    r"\brecord\b", r"\binit\b", r"\basync\b", r"\bawait\b",
    r"\byield\s+return\b", r"\bIAsyncEnumerable\b",
    r"\bswitch\s*\(.*\)\s*{[^}]*when\b",
]
CS_FILE_PATTERN = re.compile(r".*\.cs$", re.IGNORECASE)

def scan_file(path: Path):
    text = path.read_text(encoding="utf-8", errors="ignore")
    issues = []
    for pat in BANNED_TOKENS:
        if re.search(pat, text):
            issues.append(f"[BANNED] {pat} in {path}")
    pub_classes = re.findall(r"\bpublic\s+class\s+\w+", text)
    if len(pub_classes) > 1:
        issues.append(f"[STRUCTURE] More than one public class in {path}")
    if re.search(r"^\s*namespace\s+\w+(\.\w+)*\s*;\s*$", text, re.MULTILINE):
        issues.append(f"[LANG] File-scoped namespace in {path} (C# 10). Use block-style.")
    return issues

def main():
    problems = []
    for base, _, files in os.walk(ROOT):
        for f in files:
            if CS_FILE_PATTERN.match(f):
                problems += scan_file(Path(base) / f)
    if problems:
        print("NT8 Guard found issues:")
        for p in problems: print(" -", p)
        if FAIL_ON_WARN: sys.exit(1)
    else:
        print("NT8 Guard: no issues found.")

if __name__ == "__main__":
    main()
