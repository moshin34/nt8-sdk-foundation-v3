# Branch Protection — main
To (re)enable:
```bash
OWNER=moshin34 REPO=nt8-sdk-foundation-v3 BRANCH=main GITHUB_TOKEN=*** \
bash scripts/enable-branch-protection.sh
```

Required checks (current):

* NT8 Guard (layout-aware) / guard
* NT8 Guard / build-test   (temporary)
* Guard & Build / guard
* Guard & Build / build
* QA Validate / validate
* Code scanning results / CodeQL
* CodeQL / analyze
