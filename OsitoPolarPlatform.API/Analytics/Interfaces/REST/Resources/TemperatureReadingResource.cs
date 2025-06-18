namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST.Resources;

/// <summary>
/// Resource for representing temperature readings in API responses
/// </summary>
public record TemperatureReadingResource(
    int Id,
    int EquipmentId,
    decimal Temperature,
    DateTimeOffset Timestamp,
    string Status
);