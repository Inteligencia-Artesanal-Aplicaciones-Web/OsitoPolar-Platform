using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Commands;
using OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.REST.Transform;

/// <summary>
/// Assembles an UpdateEquipmentTemperatureCommand from a resource.
/// </summary>
public static class UpdateEquipmentTemperatureCommandFromResourceAssembler
{
    public static UpdateEquipmentTemperatureCommand ToCommandFromResource(int equipmentId, UpdateEquipmentTemperatureResource resource)
    {
        return new UpdateEquipmentTemperatureCommand(equipmentId, resource.Temperature);
    }
}