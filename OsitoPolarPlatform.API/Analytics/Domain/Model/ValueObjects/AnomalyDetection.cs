namespace OsitoPolarPlatform.API.Analytics.Domain.Model.ValueObjects;

/// <summary>
/// Represents detected temperature anomalies
/// </summary>
public record AnomalyDetection
{
    /// <summary>
    /// Whether an anomaly was detected
    /// </summary>
    public bool HasAnomaly { get; init; }

    /// <summary>
    /// Type of anomaly: door_open, compressor_failure, power_outage, normal
    /// </summary>
    public string Type { get; init; } = "normal";

    /// <summary>
    /// Severity level: normal, warning, critical
    /// </summary>
    public string Severity { get; init; } = "normal";

    /// <summary>
    /// Human-readable message describing the anomaly
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Recommended action to take
    /// </summary>
    public string Recommendation { get; init; } = string.Empty;

    /// <summary>
    /// Temperature change rate (°C per minute)
    /// </summary>
    public decimal ChangeRate { get; init; }

    /// <summary>
    /// Duration of the anomaly in minutes
    /// </summary>
    public decimal DurationMinutes { get; init; }

    /// <summary>
    /// Whether this anomaly should trigger an alert
    /// </summary>
    public bool ShouldAlert { get; init; }

    /// <summary>
    /// When the anomaly was detected
    /// </summary>
    public DateTimeOffset DetectedAt { get; init; }

    /// <summary>
    /// Confidence level (0-100)
    /// </summary>
    public int Confidence { get; init; }
}

/// <summary>
/// Anomaly types
/// </summary>
public static class AnomalyType
{
    public const string Normal = "normal";
    public const string DoorOpen = "door_open";
    public const string CompressorFailure = "compressor_failure";
    public const string PowerOutage = "power_outage";
    public const string RapidCooling = "rapid_cooling";
}

/// <summary>
/// Severity levels
/// </summary>
public static class AnomalySeverity
{
    public const string Normal = "normal";
    public const string Warning = "warning";
    public const string Critical = "critical";
}
