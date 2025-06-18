using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Transform;

/// <summary>
/// Assembles an EquipmentResource from an Equipment entity.
/// </summary>
public static class EquipmentResourceFromEntityAssembler
{
    public static EquipmentResource ToResourceFromEntity(Equipment entity)
    {
        return new EquipmentResource(
            entity.Id,
            entity.Name,
            entity.Type.ToString(),
            entity.Model,
            entity.Manufacturer,
            entity.SerialNumber,
            entity.Code,
            entity.Cost,
            entity.TechnicalDetails,
            entity.Status.ToString(),
            entity.IsPoweredOn,
            entity.InstallationDate,
            entity.CurrentTemperature,
            entity.SetTemperature,
            entity.OptimalTemperatureMin,
            entity.OptimalTemperatureMax,
            entity.Location.Name,
            entity.Location.Address,
            entity.Location.Coordinates.Latitude,
            entity.Location.Coordinates.Longitude,
            entity.EnergyConsumption.Current,
            entity.EnergyConsumption.Unit,
            entity.EnergyConsumption.Average,
            entity.OwnerId,
            entity.OwnerType,
            entity.OwnershipType.ToString(),
            entity.Notes
        );
    }
}