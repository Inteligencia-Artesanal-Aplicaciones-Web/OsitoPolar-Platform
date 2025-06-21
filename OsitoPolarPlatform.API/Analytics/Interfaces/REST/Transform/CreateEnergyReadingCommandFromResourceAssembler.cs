using OsitoPolarPlatform.API.Analytics.Domain.Model.Commands;
using OsitoPolarPlatform.API.Analytics.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST.Transform;

/// <summary>
/// Assembles a RecordEnergyReadingCommand from a CreateEnergyReadingResource
/// </summary>
public static class CreateEnergyReadingCommandFromResourceAssembler
{
    public static RecordEnergyReadingCommand ToCommandFromResource(CreateEnergyReadingResource resource)
    {
        return new RecordEnergyReadingCommand(
            resource.EquipmentId,
            resource.Consumption,
            resource.Unit
        );
    }
}