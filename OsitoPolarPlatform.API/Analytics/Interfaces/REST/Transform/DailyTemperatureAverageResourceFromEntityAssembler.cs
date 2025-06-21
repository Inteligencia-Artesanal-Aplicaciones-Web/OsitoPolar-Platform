using OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;
using OsitoPolarPlatform.API.Analytics.Interfaces.REST.Resources;

namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST.Transform;

/// <summary>
/// Assembles a DailyTemperatureAverageResource from a DailyTemperatureAverage entity
/// </summary>
public static class DailyTemperatureAverageResourceFromEntityAssembler
{
    public static DailyTemperatureAverageResource ToResourceFromEntity(DailyTemperatureAverage entity)
    {
        return new DailyTemperatureAverageResource(
            entity.Id,
            entity.EquipmentId,
            entity.Date.ToString("yyyy-MM-dd"),
            entity.AverageTemperature,
            entity.MinTemperature,
            entity.MaxTemperature
        );
    }
}