namespace NT8.SDK.Abstractions.Config
{
    using System;
    using System.IO;
    using System.Globalization;

    public static class RiskPresetLoader
    {
        // Minimal JSON loader (expects flat keys as provided above); no external deps.
        public static RiskPreset LoadFromFile(string path)
        {
            var text = File.ReadAllText(path);
            var rp = new RiskPreset();
            rp.Symbol = ExtractString(text, "\"symbol\"");
            rp.MaxContracts = ExtractInt(text, "\"maxContracts\"");
            rp.DailyLossLimit = ExtractDecimal(text, "\"dailyLossLimit\"");
            rp.WeeklyLossLimit = ExtractDecimal(text, "\"weeklyLossLimit\"");
            rp.TrailingDrawdown = ExtractDecimal(text, "\"trailingDrawdown\"");
            return rp;
        }

        private static string ExtractString(string json, string key)
        {
            int i = json.IndexOf(key); if (i < 0) return null;
            i = json.IndexOf(':', i) + 1; if (i <= 0) return null;
            int q1 = json.IndexOf('"', i) + 1; if (q1 <= 0) return null;
            int q2 = json.IndexOf('"', q1); if (q2 <= 0) return null;
            return json.Substring(q1, q2 - q1);
        }
        private static int ExtractInt(string json, string key) { return (int)ExtractDecimal(json, key); }
        private static decimal ExtractDecimal(string json, string key)
        {
            int i = json.IndexOf(key); if (i < 0) return 0m;
            i = json.IndexOf(':', i) + 1; if (i <= 0) return 0m;
            int j = i;
            while (j < json.Length && (char.IsDigit(json[j]) || json[j]=='.' || json[j]=='-' )) j++;
            var s = json.Substring(i, j - i).Trim();
            decimal d; if (!decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out d)) d = 0m;
            return d;
        }
    }
}
