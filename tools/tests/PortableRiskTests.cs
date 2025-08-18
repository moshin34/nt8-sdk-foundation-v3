using System;
using NT8.SDK.Abstractions.Risk;

public static class PortableRiskTests
{
    private static int fails = 0;

    public static void Main()
    {
        Test_MaxContracts();
        Test_DailyLoss();
        Test_WeeklyLoss();
        Test_TrailingDD();
        Test_Allow();

        if (fails > 0)
        {
            Console.WriteLine("FAILURES=" + fails);
            Environment.Exit(1);
        }
        Console.WriteLine("ALL TESTS PASSED");
        Environment.Exit(0);
    }

    private static void Assert(bool cond, string name)
    {
        if (!cond) { fails++; Console.WriteLine("FAIL: " + name); }
        else Console.WriteLine("OK: " + name);
    }

    private static RiskResult Eval(RiskCaps caps, RiskSnapshot snap)
    {
        var rm = new PortableRiskManager();
        return rm.Evaluate(caps, snap);
    }

    private static void Test_MaxContracts()
    {
        var caps = new RiskCaps { MaxContracts = 1 };
        var snap = new RiskSnapshot { AccountQuantity = 2 };
        var r = Eval(caps, snap);
        Assert(r.Decision == RiskDecision.BlockMaxContracts, "MaxContracts blocks when qty>max");
    }

    private static void Test_DailyLoss()
    {
        var caps = new RiskCaps { DailyLossLimit = 500m };
        var snap = new RiskSnapshot { DailyPnL = -500m };
        var r = Eval(caps, snap);
        Assert(r.Decision == RiskDecision.BlockDailyLoss, "Daily loss blocks at limit");
    }

    private static void Test_WeeklyLoss()
    {
        var caps = new RiskCaps { WeeklyLossLimit = 1000m };
        var snap = new RiskSnapshot { WeeklyPnL = -1000m };
        var r = Eval(caps, snap);
        Assert(r.Decision == RiskDecision.BlockWeeklyLoss, "Weekly loss blocks at limit");
    }

    private static void Test_TrailingDD()
    {
        var caps = new RiskCaps { TrailingDrawdown = 1000m };
        var snap = new RiskSnapshot { Equity = 0m, PeakEquity = 1500m };
        var r = Eval(caps, snap);
        Assert(r.Decision == RiskDecision.BlockTrailingDD, "TDD blocks when drawdown >= limit");
    }

    private static void Test_Allow()
    {
        var caps = new RiskCaps { MaxContracts = 3, DailyLossLimit = 500m, WeeklyLossLimit = 1500m, TrailingDrawdown = 2000m };
        var snap = new RiskSnapshot { AccountQuantity = 1, DailyPnL = -100m, WeeklyPnL = -200m, Equity = 100m, PeakEquity = 300m };
        var r = Eval(caps, snap);
        Assert(r.Decision == RiskDecision.Allow, "Allow under all limits");
    }
}
