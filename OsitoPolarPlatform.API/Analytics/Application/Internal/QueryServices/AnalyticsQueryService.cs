using OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Queries;
using OsitoPolarPlatform.API.Analytics.Domain.Repositories;
using OsitoPolarPlatform.API.Analytics.Domain.Services;

namespace OsitoPolarPlatform.API.Analytics.Application.Internal.QueryServices;

/// <summary>
/// Implementation of analytics query service
/// </summary>
public class AnalyticsQueryService(IAnalyticsRepository analyticsRepository) : IAnalyticsQueryService
{
    public async Task<IEnumerable<TemperatureReading>> Handle(GetTemperatureReadingsQuery query)
    {
        return await analyticsRepository.FindTemperatureReadingsByEquipmentIdAsync(query.EquipmentId, query.Hours);
    }

    public async Task<IEnumerable<DailyTemperatureAverage>> Handle(GetDailyTemperatureAveragesQuery query)
    {
        return await analyticsRepository.FindDailyAveragesByEquipmentIdAsync(query.EquipmentId, query.Days);
    }

    public async Task<IEnumerable<EnergyReading>> Handle(GetEnergyReadingsQuery query)
    {
        return await analyticsRepository.FindEnergyReadingsByEquipmentIdAsync(query.EquipmentId, query.Hours);
    }
}