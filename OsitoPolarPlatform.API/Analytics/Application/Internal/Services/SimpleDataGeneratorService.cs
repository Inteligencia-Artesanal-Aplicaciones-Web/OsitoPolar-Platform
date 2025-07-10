using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OsitoPolarPlatform.API.Analytics.Domain.Repositories;
using OsitoPolarPlatform.API.Analytics.Domain.Model;
using OsitoPolarPlatform.API.Analytics.Domain.Services;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Commands;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Services;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Queries;
using OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Aggregates; 
using OsitoPolarPlatform.API.Shared.Domain.Repositories;

namespace OsitoPolarPlatform.API.Analytics.Application.Internal.Services;

public interface IAutomaticDataGeneratorService
{
    Task GenerateInitialDataForEquipmentAsync(int equipmentId);
}

public class SimpleDataGeneratorService : IAutomaticDataGeneratorService
{
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly IAnalyticsCommandService _analyticsCommandService;
    private readonly IEquipmentQueryService _equipmentQueryService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SimpleDataGeneratorService> _logger;
    private readonly Random _random = new();

    public SimpleDataGeneratorService(
        IAnalyticsRepository analyticsRepository,
        IAnalyticsCommandService analyticsCommandService,
        IEquipmentQueryService equipmentQueryService,
        IUnitOfWork unitOfWork,
        ILogger<SimpleDataGeneratorService> logger)
    {
        _analyticsRepository = analyticsRepository;
        _analyticsCommandService = analyticsCommandService;
        _equipmentQueryService = equipmentQueryService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task GenerateInitialDataForEquipmentAsync(int equipmentId)
    {
        try
        {
            var existingReadings = await _analyticsRepository
                .FindTemperatureReadingsByEquipmentIdAsync(equipmentId, 1);

            if (existingReadings.Any())
            {
                _logger.LogDebug($"Equipment {equipmentId} already has data. Skipping generation.");
                return;
            }

            var equipment = await _equipmentQueryService.Handle(new GetEquipmentByIdQuery(equipmentId));
            if (equipment == null)
            {
                _logger.LogWarning($"Equipment {equipmentId} not found");
                return;
            }

            _logger.LogInformation($"Generating 7 days of historical data for {equipment.Name}...");

            // Generar datos para 7 días
            await GenerateTemperatureHistory(equipment, 7);
            await GenerateEnergyHistory(equipment, 7);
            await GenerateDailyAverages(equipmentId, 7);

            await _unitOfWork.CompleteAsync();
            
            _logger.LogInformation($"Successfully generated analytics data for equipment {equipmentId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating data for equipment {equipmentId}");
            throw;
        }
    }

    // CAMBIO: Usar Equipment tipado en lugar de dynamic
    private async Task GenerateTemperatureHistory(Equipment equipment, int days)
    {
        var startDate = DateTimeOffset.UtcNow.AddDays(-days);
        var optimalMin = equipment.OptimalTemperatureMin;
        var optimalMax = equipment.OptimalTemperatureMax;
        var optimalMid = (optimalMin + optimalMax) / 2;
        var range = optimalMax - optimalMin;

        for (int day = 0; day < days; day++)
        {
            var currentDate = startDate.AddDays(day);
            for (int hour = 0; hour < 24; hour++)
            {
                var timestamp = currentDate.AddHours(hour);
                var baseTemp = optimalMid;
                var hourOfDay = hour;
                var dailyVariation = Math.Sin((hourOfDay - 6) * Math.PI / 12) * (double)range * 0.1;
                var randomVariation = (decimal)(_random.NextDouble() - 0.5) * range * 0.15m;
                if (_random.Next(100) < 3)
                {
                    randomVariation = _random.Next(2) == 0 
                        ? range * 0.4m  
                        : -range * 0.4m; 
                }
                var temperature = baseTemp + (decimal)dailyVariation + randomVariation;
                temperature = Math.Max(optimalMin - range * 0.5m, Math.Min(optimalMax + range * 0.5m, temperature));
                var command = new RecordTemperatureReadingCommand(
                    equipment.Id,
                    temperature,
                    timestamp
                );
                await _analyticsCommandService.Handle(command);
            }
        }
    }
    
    private async Task GenerateEnergyHistory(Equipment equipment, int days)
    {
        var startDate = DateTimeOffset.UtcNow.AddDays(-days);
        var baseConsumption = equipment.Type.ToString() switch
        {
            "Freezer" => 180m,
            "ColdRoom" => 500m,
            "Refrigerator" => 120m,
            "AirConditioner" => 800m,
            "Cooler" => 150m,
            _ => 200m
        };
        
        if (equipment.EnergyConsumption?.Average > 0)
        {
            baseConsumption = equipment.EnergyConsumption.Average;
        }

        var unit = equipment.EnergyConsumption?.Unit ?? "watts";

        for (int day = 0; day < days; day++)
        {
            var currentDate = startDate.AddDays(day);
            for (int reading = 0; reading < 4; reading++)
            {
                var timestamp = currentDate.AddHours(reading * 6);
                var consumption = baseConsumption;
                var hour = reading * 6;
                
                
                if (hour >= 8 && hour <= 20)
                {
                    consumption *= 1.2m;
                }
                
                
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || 
                    currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    consumption *= 0.85m;
                }
                
                consumption += consumption * (decimal)(_random.NextDouble() - 0.5) * 0.1m;
                
                var energyReading = new EnergyReading(
                    equipment.Id,
                    Math.Max(0, consumption),
                    unit
                );
                await _analyticsRepository.AddEnergyReadingAsync(energyReading);
            }
        }
    }

private async Task GenerateDailyAverages(int equipmentId, int days)
{
    var startDate = DateTime.UtcNow.AddDays(-days).Date; 
    for (int day = 0; day < days; day++)
    {
        var currentDate = startDate.AddDays(day);
        
        try
        {
            var existingAverage = await _analyticsRepository.FindDailyAverageByEquipmentAndDateAsync(
                equipmentId, 
                DateOnly.FromDateTime(currentDate) 
            );
            
            if (existingAverage != null)
            {
                _logger.LogDebug($"Daily average already exists for equipment {equipmentId} on {currentDate:yyyy-MM-dd}. Skipping.");
                continue;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Could not check existing daily average for equipment {equipmentId} on {currentDate:yyyy-MM-dd}: {ex.Message}");

        }
        
        var dayStart = currentDate;
        var dayEnd = currentDate.AddDays(1);
        var readings = await _analyticsRepository.FindTemperatureReadingsByDateRangeAsync(
            equipmentId, 
            new DateTimeOffset(dayStart, TimeSpan.Zero),
            new DateTimeOffset(dayEnd, TimeSpan.Zero)
        );
        
        if (readings.Any())
        {
            try
            {
                var temperatures = readings.Select(r => r.Temperature).ToList();
                var dailyAverage = new DailyTemperatureAverage(
                    equipmentId,
                    DateOnly.FromDateTime(currentDate), 
                    temperatures.Average(),
                    temperatures.Min(),
                    temperatures.Max()
                );
                await _analyticsRepository.AddDailyAverageAsync(dailyAverage);
                _logger.LogDebug($"Created daily average for equipment {equipmentId} on {currentDate:yyyy-MM-dd}");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message?.Contains("Duplicate entry") == true)
            {
                _logger.LogDebug($"Daily average already exists for equipment {equipmentId} on {currentDate:yyyy-MM-dd}. Skipping duplicate.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Could not create daily average for equipment {equipmentId} on {currentDate:yyyy-MM-dd}. Error: {ex.Message}");
            }
        }
    }
}
}