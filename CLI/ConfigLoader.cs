using System;
using System.IO;
using Newtonsoft.Json;

namespace NT8.SDK.CLI
{
    public class RiskCapsConfig
    {
        public double DailyCap { get; set; }
        public double WeeklyCap { get; set; }

        public static RiskCapsConfig Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Config file not found", path);

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<RiskCapsConfig>(json);
        }
    }
}
