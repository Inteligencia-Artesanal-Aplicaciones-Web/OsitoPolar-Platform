namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Resources;

/// <summary>
/// Resource for creating a new Equipment.
/// OwnerId is automatically set from the authenticated user - do not send it from the client.
/// </summary>
public record CreateEquipmentResource(
    string Name,
    string Type,
    string Model,
    string Manufacturer,
    string SerialNumber,
    string Code,
    decimal Cost,
    string TechnicalDetails,
    decimal CurrentTemperature,
    decimal SetTemperature,
    decimal OptimalTemperatureMin,
    decimal OptimalTemperatureMax,
    string LocationName,
    string LocationAddress,
    decimal LocationLatitude,
    decimal LocationLongitude,
    decimal EnergyConsumptionCurrent,
    string EnergyConsumptionUnit,
    decimal EnergyConsumptionAverage,
    string OwnershipType,
    string Notes
);