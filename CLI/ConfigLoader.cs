using System;
using System.IO;
using Newtonsoft.Json;

namespace NT8.SDK.CLI
{
    public class RiskCapsConfig
    {
        public double DailyCap { get; set; }
        public double WeeklyCap { get; set; }
        public double TrailingLimit { get; set; }

        public static RiskCapsConfig Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Config file not found", path);

            var json = File.ReadAllText(path);
            var config = JsonConvert.DeserializeObject<RiskCapsConfig>(json);
            Console.WriteLine($"✅ Loaded risk caps: Daily={config.DailyCap}, Weekly={config.WeeklyCap}, Trailing={config.TrailingLimit}");
            return config;
        }
    }
}
