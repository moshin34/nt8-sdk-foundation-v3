using System;

namespace NT8.SDK
{
    /// <summary>Risk modes used across the SDK and strategy layer.</summary>
    public enum RiskMode { ECP, PCP, DCP, HR }

    /// <summary>Indicates the current lockout state from risk controls.</summary>
    public enum RiskLockoutState { None, CoolingDown, LockedOut }

    /// <summary>Side of a position or trade intent.</summary>
    public enum PositionSide { Flat, Long, Short }

    /// <summary>Stable identifier for a session (symbol + named session).</summary>
    [Serializable]
    public struct SessionKey
    {
        public SessionKey(string symbol, string name) { Symbol = symbol; Name = name; }
        /// <summary>Instrument symbol, e.g., "NQ".</summary>
        public string Symbol { get; set; }
        /// <summary>Session template name, e.g., "CME US Index Futures RTH".</summary>
        public string Name { get; set; }
    }

    /// <summary>Intent to hold a position on a given symbol.</summary>
    [Serializable]
    public struct PositionIntent
    {
        public PositionIntent(string symbol, PositionSide side) { Symbol = symbol; Side = side; }
        /// <summary>Instrument symbol.</summary>
        public string Symbol { get; set; }
        /// <summary>Desired position side (Long/Short/Flat).</summary>
        public PositionSide Side { get; set; }
    }

    /// <summary>Sizing decision output including quantity and rationale.</summary>
    [Serializable]
    public struct SizeDecision
    {
        public SizeDecision(int quantity, string reason, RiskMode riskModeUsed)
        { Quantity = quantity; Reason = reason ?? string.Empty; RiskModeUsed = riskModeUsed; }
        /// <summary>Suggested order quantity.</summary>
        public int Quantity { get; set; }
        /// <summary>Diagnostic reason for the chosen size (e.g., "PCP degrade -1").</summary>
        public string Reason { get; set; }
        /// <summary>The risk mode that determined this decision.</summary>
        public RiskMode RiskModeUsed { get; set; }
    }

    /// <summary>Trailing stop profile kinds.</summary>
    public enum TrailingProfileType { FixedTicks, AtrMultiple, OpeningRangeWidth }

    /// <summary>Parameters for a trailing stop profile (non-loosening).</summary>
    [Serializable]
    public struct TrailingProfile
    {
        public TrailingProfile(TrailingProfileType type, decimal param1, decimal param2)
        { Type = type; Param1 = param1; Param2 = param2; }
        /// <summary>Type of profile.</summary>
        public TrailingProfileType Type { get; set; }
        /// <summary>Primary parameter (meaning depends on Type).</summary>
        public decimal Param1 { get; set; }
        /// <summary>Secondary parameter (meaning depends on Type).</summary>
        public decimal Param2 { get; set; }
    }

    /// <summary>Order intent type expected by the bridge.</summary>
    public enum OrderIntentType { Market, Limit, StopMarket, StopLimit }

    /// <summary>Pure DTO describing an intended order without NT8 types.</summary>
    [Serializable]
    public struct OrderIntent
    {
        public OrderIntent(string symbol, bool isLong, int quantity, OrderIntentType type, decimal price, string signal, string ocoGroup = null)
        { Symbol = symbol; IsLong = isLong; Quantity = quantity; Type = type; Price = price; Signal = signal; OcoGroup = ocoGroup; }
        /// <summary>Instrument symbol.</summary>
        public string Symbol { get; set; }
        /// <summary>True for long; false for short.</summary>
        public bool IsLong { get; set; }
        /// <summary>Order quantity.</summary>
        public int Quantity { get; set; }
        /// <summary>Order type (Limit/Stop/etc.).</summary>
        public OrderIntentType Type { get; set; }
        /// <summary>Limit or stop trigger price when applicable.</summary>
        public decimal Price { get; set; }
        /// <summary>Signal tag (strategy-generated identifier).</summary>
        public string Signal { get; set; }
        /// <summary>Optional OCO group ID for child links.</summary>
        public string OcoGroup { get; set; }
    }

    /// <summary>Identifiers for entry and related orders.</summary>
    [Serializable]
    public struct OrderIds
    {
        public OrderIds(string entryId, string stopId, string targetId)
        { EntryId = entryId; StopId = stopId; TargetId = targetId; }
        public string EntryId { get; set; }
        public string StopId { get; set; }
        public string TargetId { get; set; }
    }

    /// <summary>Lightweight diagnostics event for structured logs.</summary>
    [Serializable]
    public struct DiagnosticsEvent
    {
        public DiagnosticsEvent(string tag, string message) { Tag = tag; Message = message; }
        public string Tag { get; set; }
        public string Message { get; set; }
    }

    /// <summary>Telemetry event for analytic emission.</summary>
    [Serializable]
    public struct TelemetryEvent
    {
        public TelemetryEvent(string category, string action, string label, string value)
        { Category = category; Action = action; Label = label; Value = value; }
        public string Category { get; set; }
        public string Action { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
    }
}
