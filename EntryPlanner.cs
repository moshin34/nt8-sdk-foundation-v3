using System;
using NT8.SDK.Session;

namespace NT8.SDK.Facade
{
    /// <summary>Builds an entry plan (entry + protective stop) using the SDK components.</summary>
    public static class EntryPlanner
    {
        /// <summary>
        /// Plans a simple entry + protective stop.
        /// Returns <see cref="EntryPlan.Accepted"/> = true with populated orders when allowed,
        /// otherwise <see cref="EntryPlan.Reason"/> explains the block.
        /// Caller passes ET in <paramref name="etNow"/> (no timezone conversions).
        /// Logs deterministic session decisions via <see cref="ISdk.Diagnostics"/>.
        /// </summary>
        public static EntryPlan Build(
            ISdk sdk,
            DateTime etNow,
            string symbol,
            PositionSide side,
            OrderIntentType entryType,
            decimal entryPrice,
            TrailingProfile trailingProfile,
            decimal tickSize = 1m)
        {
            if (sdk == null) return new EntryPlan { Accepted = false, Reason = "sdk missing" };
            if (string.IsNullOrEmpty(symbol)) return new EntryPlan { Accepted = false, Reason = "symbol missing" };
            if (side == PositionSide.Flat) return new EntryPlan { Accepted = false, Reason = "flat intent" };

            var intent = new PositionIntent(symbol, side);

            // Risk gate
            var riskReason = sdk.Risk != null ? sdk.Risk.EvaluateEntry(intent) : string.Empty;
            if (!string.IsNullOrEmpty(riskReason))
                return new EntryPlan { Accepted = false, Reason = riskReason };

            // Session gate
            TimeRange? settlement = null;
            TimeRange[] blackouts = new TimeRange[0];
            var cme = sdk.Session as CmeBlackoutService;
            if (cme != null)
            {
                settlement = cme.SettlementRange(etNow, symbol);
                blackouts = cme.BlackoutRanges(etNow, symbol);
            }

            var gate = new SettlementGate(settlement, blackouts);
            var block = gate.Blocking(etNow);
            if (sdk.Diagnostics != null && sdk.Diagnostics.Enabled)
            {
                var msg = block.HasValue ? "session BLOCKED: " + block.Value.ToString() : "session OK";
                sdk.Diagnostics.Capture(msg, "session");
            }
            if (block.HasValue)
                return new EntryPlan { Accepted = false, Reason = "session window" };

            // Sizing
            var size = sdk.Sizing.Decide(sdk.Risk.Mode, intent);
            if (size.Quantity <= 0)
                return new EntryPlan { Accepted = false, Reason = string.IsNullOrEmpty(size.Reason) ? "size <= 0" : size.Reason };

            // Validate entry price for price-sensitive types
            if ((entryType == OrderIntentType.Limit || entryType == OrderIntentType.StopLimit) && entryPrice <= 0m)
                return new EntryPlan { Accepted = false, Reason = "bad price" };

            // Build OCO group and orders
            var oco = Guid.NewGuid().ToString("N");
            var isLong = side == PositionSide.Long;

            // Normalize entry price to tick for price-based orders
            var normalizedEntryPrice = (entryType == OrderIntentType.Limit || entryType == OrderIntentType.StopLimit)
                ? PriceMath.RoundToTick(entryPrice, tickSize)
                : entryPrice;

            var entry = new OrderIntent(
                symbol: symbol,
                isLong: isLong,
                quantity: size.Quantity,
                type: entryType,
                price: normalizedEntryPrice,
                signal: "entry",
                ocoGroup: oco);

            // Initial protective stop at fixed/derived distance from entry price then tick-normalize
            var initialStopPrice = sdk.Trailing.ComputeStop(
                normalizedEntryPrice,
                normalizedEntryPrice,
                isLong,
                trailingProfile,
                0m);
            initialStopPrice = PriceMath.RoundToTick(initialStopPrice, tickSize);

            // Sanity: ensure stop on correct side
            if (isLong && initialStopPrice >= normalizedEntryPrice)
                return new EntryPlan { Accepted = false, Reason = "stop side invalid (long)" };
            if (!isLong && initialStopPrice <= normalizedEntryPrice)
                return new EntryPlan { Accepted = false, Reason = "stop side invalid (short)" };

            var stop = new OrderIntent(
                symbol: symbol,
                isLong: isLong,
                quantity: size.Quantity,
                type: OrderIntentType.StopMarket,
                price: initialStopPrice,
                signal: "protect",
                ocoGroup: oco);

            return new EntryPlan
            {
                Accepted = true,
                Reason = "",
                Entry = entry,
                Stop = stop,
                OcoGroup = oco
            };
        }
    }
}

