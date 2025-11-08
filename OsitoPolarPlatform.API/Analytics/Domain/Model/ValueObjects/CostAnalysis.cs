namespace OsitoPolarPlatform.API.Analytics.Domain.Model.ValueObjects;

/// <summary>
/// Represents energy cost analysis for equipment
/// </summary>
public record CostAnalysis
{
    /// <summary>
    /// Current month's energy cost
    /// </summary>
    public decimal CurrentMonthCost { get; init; }

    /// <summary>
    /// Previous month's energy cost
    /// </summary>
    public decimal PreviousMonthCost { get; init; }

    /// <summary>
    /// Cost difference (current - previous)
    /// </summary>
    public decimal Difference { get; init; }

    /// <summary>
    /// Percentage change
    /// </summary>
    public decimal PercentChange { get; init; }

    /// <summary>
    /// Trend: increasing, decreasing, stable
    /// </summary>
    public string Trend { get; init; } = "stable";

    /// <summary>
    /// Projected annual cost based on current usage
    /// </summary>
    public decimal ProjectedAnnualCost { get; init; }

    /// <summary>
    /// Daily average cost
    /// </summary>
    public decimal DailyAverageCost { get; init; }

    /// <summary>
    /// Total kWh consumed this month
    /// </summary>
    public decimal TotalKwh { get; init; }

    /// <summary>
    /// Electricity rate used for calculation (per kWh)
    /// </summary>
    public decimal ElectricityRate { get; init; }

    /// <summary>
    /// Days analyzed for current month
    /// </summary>
    public int DaysAnalyzed { get; init; }

    /// <summary>
    /// Potential monthly savings if efficiency improves
    /// </summary>
    public decimal? PotentialSavings { get; init; }

    /// <summary>
    /// Recommendations for cost reduction
    /// </summary>
    public List<string> Recommendations { get; init; } = new();
}
