namespace OsitoPolarPlatform.API.Analytics.Domain.Model.ValueObjects;

/// <summary>
/// Represents a timeframe for analytics queries
/// </summary>
/// <param name="Hours">Number of hours to look back</param>
public record AnalyticsTimeframe(int Hours)
{
    public AnalyticsTimeframe() : this(24) { }
    
    public DateTimeOffset GetStartTime() => DateTimeOffset.UtcNow.AddHours(-Hours);
    public DateTimeOffset GetEndTime() => DateTimeOffset.UtcNow;
    
    public bool IsValid() => Hours > 0 && Hours <= 8760; // Max 1 year
}