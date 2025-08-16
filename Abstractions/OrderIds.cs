namespace NT8.SDK.Abstractions
{
    /// <summary>Identifiers for an entry order and its OCO stop/target (if any).</summary>
    public struct OrderIds
    {
        public string Entry;
        public string Stop;
        public string Target;

        public OrderIds(string entry, string stop = null, string target = null)
        {
            Entry = entry;
            Stop = stop;
            Target = target;
        }

        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(Entry) && string.IsNullOrEmpty(Stop) && string.IsNullOrEmpty(Target); }
        }
    }
}