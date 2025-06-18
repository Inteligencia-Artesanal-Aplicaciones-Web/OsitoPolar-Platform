namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST.Resources;

/// <summary>
/// Resource for representing energy readings in API responses
/// </summary>
public record EnergyReadingResource(
    int Id,
    int EquipmentId,
    decimal Consumption,
    string Unit,
    DateTimeOffset Timestamp,
    string Status
);