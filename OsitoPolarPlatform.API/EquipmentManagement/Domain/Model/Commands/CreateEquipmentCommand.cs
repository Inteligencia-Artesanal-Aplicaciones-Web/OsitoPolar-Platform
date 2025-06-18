namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;

public record CreateEquipmentCommand(
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
    int OwnerId,
    string OwnerType,
    string OwnershipType,
    string Notes
);