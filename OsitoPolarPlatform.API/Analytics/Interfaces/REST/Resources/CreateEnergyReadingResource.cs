namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST.Resources;

/// <summary>
/// Resource for creating energy readings
/// </summary>
public record CreateEnergyReadingResource(
    int EquipmentId,
    decimal Consumption,
    string Unit = "watts"
);