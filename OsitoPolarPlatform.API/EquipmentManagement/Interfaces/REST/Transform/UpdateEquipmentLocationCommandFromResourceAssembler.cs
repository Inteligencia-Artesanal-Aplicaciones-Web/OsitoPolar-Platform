using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;
using OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Transform;

/// <summary>
/// Assembles an UpdateEquipmentLocationCommand from a resource.
/// </summary>
public static class UpdateEquipmentLocationCommandFromResourceAssembler
{
    public static UpdateEquipmentLocationCommand ToCommandFromResource(int equipmentId, UpdateEquipmentLocationResource resource)
    {
        return new UpdateEquipmentLocationCommand(
            equipmentId,
            resource.LocationName,
            resource.LocationAddress,
            resource.Latitude,
            resource.Longitude
        );
    }
}