using OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Queries;

namespace OsitoPolarPlatform.API.Analytics.Domain.Services;

/// <summary>
/// Query service interface for analytics operations
/// </summary>
public interface IAnalyticsQueryService
{
    Task<IEnumerable<TemperatureReading>> Handle(GetTemperatureReadingsQuery query);
    Task<IEnumerable<DailyTemperatureAverage>> Handle(GetDailyTemperatureAveragesQuery query);
    Task<IEnumerable<EnergyReading>> Handle(GetEnergyReadingsQuery query);
}