## Summary
- Add/modify NinjaScript strategies or related docs.

## Checklist
- [ ] Read `docs/ninjascript_contract.md`
- [ ] Strategy compiles in **NinjaTrader 8 Editor**
- [ ] Includes required `using` lines and correct namespace
- [ ] `OnOrderUpdate` signature matches contract (has `int filled`)
- [ ] `OnBarUpdate` includes `BarsRequiredToTrade` guard
- [ ] No async/await, records, init-only, or LINQ query syntax

## Notes
Attach NT8 compile confirmation (optional screenshot).
