// Minimal, print-only IOrders bridge for NinjaTrader.
// Uses fully-qualified names to avoid any import issues.

namespace NT8.SDK.NT8Bridge
{
    public sealed class Nt8Orders : NT8.SDK.IOrders
    {
        private readonly NinjaTrader.NinjaScript.Strategies.Strategy _host;

        public Nt8Orders(NinjaTrader.NinjaScript.Strategies.Strategy host)
        {
            _host = host ?? throw new System.ArgumentNullException(nameof(host));
        }

        public NT8.SDK.OrderIds Submit(NT8.SDK.OrderIntent intent)
        {
            Print("[Nt8Orders] Submit called");
            var stamp = System.DateTime.UtcNow.Ticks.ToString();
            var baseId = "NT8-" + stamp;
            return new NT8.SDK.OrderIds(baseId + "-E", baseId + "-S", baseId + "-T");
        }

        public void Cancel(NT8.SDK.OrderIds ids)
        {
            Print("[Nt8Orders] Cancel called");
        }

        public void Modify(NT8.SDK.OrderIds ids, NT8.SDK.OrderIntent intent)
        {
            Print("[Nt8Orders] Modify called");
        }

        private void Print(string message)
        {
            try { _host?.Print(message); } catch { /* ignore */ }
        }
    }
}
