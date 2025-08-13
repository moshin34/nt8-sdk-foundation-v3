using System;

namespace NT8.SDK
{
    /// <summary>Enumerates capital protection modes.</summary>
    public enum RiskMode
    {
        /// <summary>Extreme Capital Protection.</summary>
        ECP,
        /// <summary>Protective Capital Preservation.</summary>
        PCP,
        /// <summary>Default Capital Protection.</summary>
        DCP,
        /// <summary>High-Risk mode.</summary>
        HR
    }

    /// <summary>Indicates lockout state from the risk engine.</summary>
    public enum RiskLockoutState
    {
        /// <summary>No lockout is active.</summary>
        None,
        /// <summary>Cooling down temporarily.</summary>
        CoolingDown,
        /// <summary>Trading fully locked out.</summary>
        LockedOut
    }

    /// <summary>Side of a position (or flat).</summary>
    public enum PositionSide
    {
        /// <summary>No position.</summary>
        Flat,
        /// <summary>Long position.</summary>
        Long,
        /// <summary>Short position.</summary>
        Short
    }

    /// <summary>Stable identifier for a session (symbol + named session).</summary>
    [Serializable]
    public struct SessionKey
    {
        public SessionKey(string symbol, string name)
        {
            Symbol = symbol;
            Name = name;
        }
        public string Symbol { get; set; }
        public string Name { get; set; }
    }

    /// <summary>Intent to hold a position in a given direction for a symbol.</summary>
    [Serializable]
    public struct PositionIntent
    {
        public PositionIntent(string symbol, PositionSide side)
        {
            Symbol = symbol;
            Side = side;
        }
        public string Symbol { get; set; }
        public PositionSide Side { get; set; }
    }

    /// <summary>Sizing decision output including quantity and rationale.</summary>
    [Serializable]
    public struct SizeDecision
    {
        public SizeDecision(int quantity, string reason, RiskMode riskModeUsed)
        {
            Quantity = quantity;
            Reason = reason ?? string.Empty;
            RiskModeUsed = riskModeUsed;
        }
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public RiskMode RiskModeUsed { get; set; }
    }

    /// <summary>Trailing stop profile kinds supported in Step 1.</summary>
    public enum TrailingProfileType
    {
        FixedTicks,
        AtrMultiple,
        OpeningRangeWidth
    }

    /// <summary>Generic trailing stop profile with two decimal parameters.</summary>
    [Serializable]
    public struct TrailingProfile
    {
        public TrailingProfile(TrailingProfileType type, decimal param1, decimal param2)
        {
            Type = type;
            Param1 = param1;
            Param2 = param2;
        }
        public TrailingProfileType Type { get; set; }
        public decimal Param1 { get; set; }
        public decimal Param2 { get; set; }
    }

    /// <summary>Supported order intent types.</summary>
    public enum OrderIntentType
    {
        Market,
        Limit,
        StopMarket,
        StopLimit
    }

    /// <summary>Pure DTO describing an intended order without NT8 types.</summary>
    [Serializable]
    public struct OrderIntent
    {
        public OrderIntent(string symbol, bool isLong, int quantity, OrderIntentType type, decimal price, string signal, string ocoGroup = null)
        {
            Symbol = symbol;
            IsLong = isLong;
            Quantity = quantity;
            Type = type;
            Price = price;
            Signal = signal;
            OcoGroup = ocoGroup;
        }
        public string Symbol { get; set; }
        public bool IsLong { get; set; }
        public int Quantity { get; set; }
        public OrderIntentType Type { get; set; }
        public decimal Price { get; set; }
        public string Signal { get; set; }
        public string OcoGroup { get; set; }
    }

    /// <summary>Triplet of system-generated order identifiers.</summary>
    [Serializable]
    public struct OrderIds
    {
        public OrderIds(string entryId, string stopId, string targetId)
        {
            EntryId = entryId;
            StopId = stopId;
            TargetId = targetId;
        }
        public string EntryId { get; set; }
        public string StopId { get; set; }
        public string TargetId { get; set; }
    }

    /// <summary>Lightweight diagnostics event for structured logs.</summary>
    [Serializable]
    public struct DiagnosticsEvent
    {
        public DiagnosticsEvent(string tag, string message)
        {
            Tag = tag;
            Message = message;
        }
        public string Tag { get; set; }
        public string Message { get; set; }
    }

    /// <summary>Telemetry event for counters, timings, and labeled values.</summary>
    [Serializable]
    public struct TelemetryEvent
    {
        public TelemetryEvent(string category, string action, string label, string value)
        {
            Category = category;
            Action = action;
            Label = label;
            Value = value;
        }
        public string Category { get; set; }
        public string Action { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
    }
}

