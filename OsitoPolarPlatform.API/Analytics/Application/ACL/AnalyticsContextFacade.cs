using OsitoPolarPlatform.API.Analytics.Domain.Repositories;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;
using OsitoPolarPlatform.API.Analytics.Interfaces.ACL;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.Analytics.Application.ACL;

/// <summary>
/// Facade implementation for the Analytics context
/// </summary>
/// <param name="analyticsRepository">The analytics repository</param>
/// <param name="unitOfWork">The unit of work</param>
public class AnalyticsContextFacade(
    IAnalyticsRepository analyticsRepository,
    IUnitOfWork unitOfWork) : IAnalyticsContextFacade
{
    public async Task<int> RecordTemperatureReading(int equipmentId, decimal temperature, DateTime timestamp)
    {
        var reading = new TemperatureReading(equipmentId, temperature, timestamp);
        await analyticsRepository.AddAsync(reading);
        await unitOfWork.CompleteAsync();
        return reading.Id;
    }

    public async Task<int> RecordEnergyReading(int equipmentId, decimal consumption, DateTime timestamp)
    {
        var reading = new EnergyReading(equipmentId, consumption);
        await analyticsRepository.AddEnergyReadingAsync(reading);
        await unitOfWork.CompleteAsync();
        return reading.Id;
    }

    public async Task<bool> HasTemperatureAnomaly(int equipmentId, decimal optimalMin, decimal optimalMax)
    {
        var readings = await analyticsRepository.FindTemperatureReadingsByEquipmentIdAsync(equipmentId, 1);
        var latestReading = readings.FirstOrDefault();
        if (latestReading == null) return false;

        return latestReading.Temperature < optimalMin || latestReading.Temperature > optimalMax;
    }
}
