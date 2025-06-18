using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;
using OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Transform;

/// <summary>
/// Assembles an UpdateEquipmentPowerStateCommand from a resource.
/// </summary>
public static class UpdateEquipmentPowerStateCommandFromResourceAssembler
{
    public static UpdateEquipmentPowerStateCommand ToCommandFromResource(int equipmentId, UpdateEquipmentPowerStateResource resource)
    {
        return new UpdateEquipmentPowerStateCommand(equipmentId, resource.IsPoweredOn);
    }
}