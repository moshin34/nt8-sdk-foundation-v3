using System;

namespace NT8.SDK.Facade
{
    /// <summary>Result of planning an entry using SDK risk/session gates, sizing, and trailing.</summary>
    public sealed class EntryPlan
    {
        /// <summary>True when an entry is allowed and orders are populated.</summary>
        public bool Accepted { get; set; }

        /// <summary>Empty string when accepted; otherwise the rejection reason (never null).</summary>
        public string Reason { get; set; }

        /// <summary>Planned entry order when <see cref="Accepted"/> is true.</summary>
        public OrderIntent Entry { get; set; }

        /// <summary>Planned protective stop when <see cref="Accepted"/> is true.</summary>
        public OrderIntent Stop { get; set; }

        /// <summary>Shared OCO group for planned orders (may be empty).</summary>
        public string OcoGroup { get; set; }
    }
}
