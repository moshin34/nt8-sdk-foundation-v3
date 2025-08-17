using System;
using NT8.SDK.Trailing;
using Xunit;

namespace NT8.SDK.QA.TestKit
{
    public class TrailingParityTests
    {
        [Fact]
        public void TrailingNeverLoosensStop()
        {
            var trailing = new TrailingEngine();
            trailing.Reset();

            double[] prices = { 100, 101, 102, 101.5, 103 };
            double stop = double.MinValue;

            foreach (var p in prices)
            {
                trailing.Update(p);
                var newStop = trailing.GetStopLoss(p);
                if (stop != double.MinValue)
                    Assert.True(newStop >= stop, "Trailing stop should not loosen");
                stop = newStop;
            }
        }
    }
}
