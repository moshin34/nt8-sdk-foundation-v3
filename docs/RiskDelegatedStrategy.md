# RiskDelegatedStrategy

- Compiles in NT8 now with an internal evaluator (no DLL required).
- If you add your existing NT8 SDK DLL to **Tools → NinjaScript → References**, it auto-hooks `PortableRiskManager` via reflection and uses the SDK.
- Behavior is unchanged if the DLL is absent.

How to use SDK:
1) Add your SDK DLL to NT8 References → **Compile**.
2) Enable `RiskDelegatedStrategy` (SIM first). Output window should show “[SDK] Hooked …” when referenced successfully.
3) After cutover, you can retire the internal fallback and keep the SDK path only.
