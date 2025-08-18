# Using the Existing NT8 SDK DLL in NinjaTrader 8

1) Download the DLL from **Releases** or the **Actions → Package SDK** run (assets).
2) NinjaTrader 8 → Tools → NinjaScript → References → **Add** → select the DLL → OK → **Compile**.
3) Strategies can `using NT8.SDK.Abstractions.*` and will run against SDK implementations.
Notes:
- If Windows warns about the file coming from the internet, right-click the DLL → Properties → “Unblock”, then re-add.
- Keep RiskCappedStrategy for tonight’s enforcement. Switch to the SDK-delegated strategy after cutover.
