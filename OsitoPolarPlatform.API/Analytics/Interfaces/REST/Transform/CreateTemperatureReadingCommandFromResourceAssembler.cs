using OsitoPolarPlatform.API.Analytics.Domain.Model.Commands;
using OsitoPolarPlatform.API.Analytics.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST.Transform;

/// <summary>
/// Assembles a RecordTemperatureReadingCommand from a CreateTemperatureReadingResource
/// </summary>
public static class CreateTemperatureReadingCommandFromResourceAssembler
{
    public static RecordTemperatureReadingCommand ToCommandFromResource(CreateTemperatureReadingResource resource)
    {
        return new RecordTemperatureReadingCommand(
            resource.EquipmentId,
            resource.Temperature,
            resource.Timestamp
        );
    }
}