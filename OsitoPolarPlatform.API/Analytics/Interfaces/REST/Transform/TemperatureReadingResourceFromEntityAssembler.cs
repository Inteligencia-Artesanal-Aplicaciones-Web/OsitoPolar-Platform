using OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;
using OsitoPolarPlatform.API.Analytics.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST.Transform;

/// <summary>
/// Assembles a TemperatureReadingResource from a TemperatureReading entity
/// </summary>
public static class TemperatureReadingResourceFromEntityAssembler
{
    public static TemperatureReadingResource ToResourceFromEntity(TemperatureReading entity)
    {
        return new TemperatureReadingResource(
            entity.Id,
            entity.EquipmentId,
            entity.Temperature,
            entity.Timestamp,
            entity.Status.ToString()
        );
    }
}