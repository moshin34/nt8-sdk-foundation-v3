# INSTALL

1) Unzip this bundle into your repo root (same folder as `SDK.sln`). Allow overwrite.
2) In Windows PowerShell:
   Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned -Force
   .\tools\guard.ps1
   .\tools\build.ps1
   .\tools\test.ps1
3) Commit & push a PR, and ensure the Guard & Build workflow is green. Enable branch protection on `main`.
