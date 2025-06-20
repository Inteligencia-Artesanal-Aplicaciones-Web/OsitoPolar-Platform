namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST.Resources;

/// <summary>
/// Unified resource for all types of equipment readings (temperature, energy, etc.)
/// Replaces separate TemperatureReadingResource and EnergyReadingResource
/// </summary>
public class UnifiedReadingResource
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string Type { get; set; } = string.Empty; // "temperature", "energy"
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty; // "celsius", "watts"
    public DateTimeOffset Timestamp { get; set; }
    public string Status { get; set; } = "normal"; // "normal", "warning", "critical"
}

/// <summary>
/// Response wrapper for unified readings
/// </summary>
public class UnifiedReadingResponse
{
    public List<UnifiedReadingResource> Data { get; set; } = new();
    public int Total { get; set; }
    public int EquipmentId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
}