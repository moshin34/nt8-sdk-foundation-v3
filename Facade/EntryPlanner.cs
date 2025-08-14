using System;

namespace NT8.SDK.Facade
{
    /// <summary>Builds an entry plan (entry + protective stop) using the SDK components.</summary>
    public static class EntryPlanner
    {
        /// <summary>
        /// Plans a simple entry + protective stop.
        /// Returns <see cref="EntryPlan.Accepted"/> = true with populated orders when allowed,
        /// otherwise <see cref="EntryPlan.Reason"/> explains the block.
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
            // Basic gate
            var intent = new PositionIntent(symbol, side);
            var reason = EntryGates.CheckEntry(sdk, etNow, intent);
            if (!string.IsNullOrEmpty(reason))
                return new EntryPlan { Accepted = false, Reason = reason };

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
