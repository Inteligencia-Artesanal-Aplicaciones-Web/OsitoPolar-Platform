namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST.Resources;

/// <summary>
/// Unified resource for analytics summaries (daily averages, trends, etc.)
/// Replaces separate DailyTemperatureAverageResource
/// </summary>
public class AnalyticsSummaryResource
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "daily-average", "weekly-trend"
    public decimal? AverageTemperature { get; set; }
    public decimal? MinTemperature { get; set; }
    public decimal? MaxTemperature { get; set; }
    public decimal? AverageEnergy { get; set; }
    public decimal? PeakEnergy { get; set; }
}

/// <summary>
/// Response wrapper for analytics summaries
/// </summary>
public class AnalyticsSummaryResponse
{
    public List<AnalyticsSummaryResource> Data { get; set; } = new();
    public int Total { get; set; }
    public int EquipmentId { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Days { get; set; }
}