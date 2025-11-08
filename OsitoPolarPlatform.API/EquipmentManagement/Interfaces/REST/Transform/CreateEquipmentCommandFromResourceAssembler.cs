using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;
using OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Transform;

/// <summary>
/// Assembles a CreateEquipmentCommand from a CreateEquipmentResource.
/// OwnerId and OwnerType are injected from the authenticated owner for security.
/// </summary>
public static class CreateEquipmentCommandFromResourceAssembler
{
    public static CreateEquipmentCommand ToCommandFromResource(CreateEquipmentResource resource, int ownerId, string ownerType)
    {
        return new CreateEquipmentCommand(
            resource.Name,
            resource.Type,
            resource.Model,
            resource.Manufacturer,
            resource.SerialNumber,
            resource.Code,
            resource.Cost,
            resource.TechnicalDetails,
            resource.CurrentTemperature,
            resource.SetTemperature,
            resource.OptimalTemperatureMin,
            resource.OptimalTemperatureMax,
            resource.LocationName,
            resource.LocationAddress,
            resource.LocationLatitude,
            resource.LocationLongitude,
            resource.EnergyConsumptionCurrent,
            resource.EnergyConsumptionUnit,
            resource.EnergyConsumptionAverage,
            ownerId,
            ownerType,
            resource.OwnershipType,
            resource.Notes
        );
    }
}