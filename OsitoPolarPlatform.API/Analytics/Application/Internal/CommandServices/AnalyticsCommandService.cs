using OsitoPolarPlatform.API.Analytics.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Commands;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;
using OsitoPolarPlatform.API.Analytics.Domain.Repositories;
using OsitoPolarPlatform.API.Analytics.Domain.Services;
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.Analytics.Application.Internal.CommandServices;

/// <summary>
/// Implementation of analytics command service
/// </summary>
public class AnalyticsCommandService(
    IAnalyticsRepository analyticsRepository,
    IUnitOfWork unitOfWork) : IAnalyticsCommandService
{
    public async Task<TemperatureReading?> Handle(RecordTemperatureReadingCommand command)
    {
        if (command.Temperature < -50 || command.Temperature > 100)
            throw new ArgumentException("Temperature reading is out of valid range (-50°C to 100°C)");

        var reading = new TemperatureReading(command.EquipmentId, command.Temperature, 
            command.Timestamp ?? DateTimeOffset.UtcNow);
        
        await analyticsRepository.AddAsync(reading);
        await unitOfWork.CompleteAsync();
        
        return reading;
    }

    public async Task<EnergyReading?> Handle(RecordEnergyReadingCommand command)
    {
        if (command.Consumption < 0)
            throw new ArgumentException("Energy consumption cannot be negative");

        var reading = new EnergyReading(command.EquipmentId, command.Consumption, command.Unit);
        
        await analyticsRepository.AddEnergyReadingAsync(reading);
        await unitOfWork.CompleteAsync();
        
        return reading;
    }
}