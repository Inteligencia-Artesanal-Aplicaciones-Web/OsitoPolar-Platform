namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST.Resources;

/// <summary>
/// Resource for creating temperature readings
/// </summary>
public record CreateTemperatureReadingResource(
    int EquipmentId,
    decimal Temperature,
    DateTimeOffset? Timestamp = null
);