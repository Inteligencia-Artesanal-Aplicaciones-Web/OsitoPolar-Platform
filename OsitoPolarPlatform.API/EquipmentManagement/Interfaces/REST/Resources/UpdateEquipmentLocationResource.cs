namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Resources;

/// <summary>
/// Resource for updating equipment location.
/// </summary>
public record UpdateEquipmentLocationResource(
    string LocationName,
    string LocationAddress,
    decimal Latitude,
    decimal Longitude
);