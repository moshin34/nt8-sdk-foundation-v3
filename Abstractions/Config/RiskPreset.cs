namespace NT8.SDK.Abstractions.Config
{
    public struct RiskPreset { public string Symbol; public int MaxContracts; public decimal DailyLossLimit; public decimal WeeklyLossLimit; public decimal TrailingDrawdown; }
}
