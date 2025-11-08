namespace OsitoPolarPlatform.API.Analytics.Domain.Model.ValueObjects;

/// <summary>
/// Represents equipment health metrics based on temperature stability
/// </summary>
public record EquipmentHealth
{
    /// <summary>
    /// Overall health score (0-100)
    /// Higher is better
    /// </summary>
    public decimal HealthScore { get; init; }

    /// <summary>
    /// Mean temperature over the period
    /// </summary>
    public decimal Mean { get; init; }

    /// <summary>
    /// Standard deviation (measure of variance)
    /// </summary>
    public decimal StandardDeviation { get; init; }

    /// <summary>
    /// Variance of temperature readings
    /// </summary>
    public decimal Variance { get; init; }

    /// <summary>
    /// Temperature range (max - min)
    /// </summary>
    public decimal Range { get; init; }

    /// <summary>
    /// Minimum temperature recorded
    /// </summary>
    public decimal MinTemperature { get; init; }

    /// <summary>
    /// Maximum temperature recorded
    /// </summary>
    public decimal MaxTemperature { get; init; }

    /// <summary>
    /// Whether the equipment is maintaining stable temperatures
    /// </summary>
    public bool IsStable { get; init; }

    /// <summary>
    /// Temperature trend information
    /// </summary>
    public TemperatureTrend Trend { get; init; }

    /// <summary>
    /// Number of readings analyzed
    /// </summary>
    public int ReadingsAnalyzed { get; init; }

    public EquipmentHealth()
    {
        Trend = new TemperatureTrend();
    }
}

/// <summary>
/// Represents temperature trend analysis
/// </summary>
public record TemperatureTrend
{
    /// <summary>
    /// Trend direction: rising, falling, stable
    /// </summary>
    public string Direction { get; init; } = "unknown";

    /// <summary>
    /// Slope of temperature change (°C per reading)
    /// </summary>
    public decimal Slope { get; init; }

    /// <summary>
    /// Projected temperature change in next 24 hours if trend continues
    /// </summary>
    public decimal ProjectedChange24h { get; init; }
}
