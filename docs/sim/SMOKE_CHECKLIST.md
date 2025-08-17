# NT8 SIM Smoke Checklist
- [ ] Startup banner printed with instrument
- [ ] Risk caps printed on load (Daily/Weekly/Trailing)
- [ ] Orders blocked when caps breached (verify log event)
- [ ] Telemetry files present (orders + risk events)
- [ ] No unhandled exceptions in session

## Evidence to capture
- `logs/strategy.log`
- `logs/risk.log`
- `qa/summary.json` (if generated)
