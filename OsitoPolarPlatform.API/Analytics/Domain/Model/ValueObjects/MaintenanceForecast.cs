namespace OsitoPolarPlatform.API.Analytics.Domain.Model.ValueObjects;

/// <summary>
/// Represents a simple rule-based maintenance forecast
/// No ML - just temperature variance and trend analysis
/// </summary>
public record MaintenanceForecast
{
    /// <summary>
    /// Overall risk score (0-100)
    /// Higher means more likely to need maintenance soon
    /// </summary>
    public int RiskScore { get; init; }

    /// <summary>
    /// Risk level: low, moderate, high, critical
    /// </summary>
    public string RiskLevel { get; init; } = "low";

    /// <summary>
    /// Estimated days until maintenance recommended
    /// null = no maintenance needed
    /// </summary>
    public int? EstimatedDaysToMaintenance { get; init; }

    /// <summary>
    /// Confidence level: low, medium, high
    /// </summary>
    public string Confidence { get; init; } = "medium";

    /// <summary>
    /// Breakdown of risk indicators
    /// </summary>
    public RiskIndicators Indicators { get; init; } = new();

    /// <summary>
    /// Recommended action
    /// </summary>
    public string Recommendation { get; init; } = string.Empty;

    /// <summary>
    /// Estimated maintenance cost
    /// </summary>
    public decimal? EstimatedCost { get; init; }

    /// <summary>
    /// Reasons for the forecast
    /// </summary>
    public List<string> Reasons { get; init; } = new();
}

/// <summary>
/// Individual risk indicators that contribute to maintenance forecast
/// </summary>
public record RiskIndicators
{
    /// <summary>
    /// Risk from temperature variance (0-40 points)
    /// High variance = equipment struggling to maintain temp
    /// </summary>
    public int VarianceScore { get; init; }

    /// <summary>
    /// Risk from efficiency decline (0-30 points)
    /// Calculated from energy consumption vs temperature maintenance
    /// </summary>
    public int EfficiencyScore { get; init; }

    /// <summary>
    /// Risk from age (0-20 points)
    /// Based on installation date
    /// </summary>
    public int AgeScore { get; init; }

    /// <summary>
    /// Risk from temperature trend (0-10 points)
    /// Gradual rise = compressor failing
    /// </summary>
    public int TrendScore { get; init; }
}

/// <summary>
/// Risk levels
/// </summary>
public static class RiskLevel
{
    public const string Low = "low";
    public const string Moderate = "moderate";
    public const string High = "high";
    public const string Critical = "critical";
}
