using NUnit.Framework;
using NT8.SDK.TestKit;

namespace NT8.SDK.QA.TestKit
{
    public class BacktestGoldenPath
    {
        [Test]
        public void StrategyShouldMeetBaselinePerformance()
        {
            var sdk = new SdkBuilder().WithDefaults().Build();
            var harness = new BacktestHarness(sdk);

            var result = harness.Run("ES", "2025-01-01", "2025-01-15");

            Assert.LessOrEqual(result.MaxDrawdown, 1000, "MaxDrawdown exceeded");
            Assert.GreaterOrEqual(result.HitRate, 50.0, "HitRate below 50%");
        }
    }
}
