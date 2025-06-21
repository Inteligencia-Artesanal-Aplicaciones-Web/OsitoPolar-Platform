namespace OsitoPolarPlatform.API.Analytics.Domain.Model.ValueObjects;

/// <summary>
/// Represents a temperature range with min and max values
/// </summary>
/// <param name="Min">Minimum temperature</param>
/// <param name="Max">Maximum temperature</param>
public record TemperatureRange(decimal Min, decimal Max)
{
    public TemperatureRange() : this(0, 0) { }
    
    public bool IsWithinRange(decimal temperature) => temperature >= Min && temperature <= Max;
    
    public decimal GetRangeWidth() => Max - Min;
    
    public bool IsValid() => Max > Min;
}