using Microsoft.EntityFrameworkCore;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;
using OsitoPolarPlatform.API.Analytics.Domain.Repositories;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using OsitoPolarPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace OsitoPolarPlatform.API.Analytics.Infrastructure.Persistence.EFC.Repositories;

public class AnalyticsRepository(AppDbContext context) : BaseRepository<TemperatureReading>(context), IAnalyticsRepository
{
    public async Task<IEnumerable<TemperatureReading>> FindTemperatureReadingsByEquipmentIdAsync(int equipmentId, int hours = 24)
    {
        var cutoff = DateTimeOffset.UtcNow.AddHours(-hours);
        return await Context.Set<TemperatureReading>()
            .Where(t => t.EquipmentId == equipmentId && t.Timestamp >= cutoff)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<TemperatureReading>> FindTemperatureReadingsByDateRangeAsync(int equipmentId, DateTimeOffset start, DateTimeOffset end)
    {
        return await Context.Set<TemperatureReading>()
            .Where(t => t.EquipmentId == equipmentId && t.Timestamp >= start && t.Timestamp <= end)
            .OrderBy(t => t.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<DailyTemperatureAverage>> FindDailyAveragesByEquipmentIdAsync(int equipmentId, int days = 7)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));
        return await Context.Set<DailyTemperatureAverage>()
            .Where(d => d.EquipmentId == equipmentId && d.Date >= cutoff)
            .OrderByDescending(d => d.Date)
            .ToListAsync();
    }

    public async Task<DailyTemperatureAverage?> FindDailyAverageByEquipmentAndDateAsync(int equipmentId, DateOnly date)
    {
        return await Context.Set<DailyTemperatureAverage>()
            .FirstOrDefaultAsync(d => d.EquipmentId == equipmentId && d.Date == date);
    }

    public async Task AddDailyAverageAsync(DailyTemperatureAverage dailyAverage)
    {
        await Context.Set<DailyTemperatureAverage>().AddAsync(dailyAverage);
    }

    public async Task<IEnumerable<EnergyReading>> FindEnergyReadingsByEquipmentIdAsync(int equipmentId, int hours = 24)
    {
        var cutoff = DateTimeOffset.UtcNow.AddHours(-hours);
        return await Context.Set<EnergyReading>()
            .Where(e => e.EquipmentId == equipmentId && e.Timestamp >= cutoff)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task AddEnergyReadingAsync(EnergyReading energyReading)
    {
        await Context.Set<EnergyReading>().AddAsync(energyReading);
    }

    public async Task<decimal> GetAverageEnergyConsumptionAsync(int equipmentId, int days = 30)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-days);
        var readings = await Context.Set<EnergyReading>()
            .Where(e => e.EquipmentId == equipmentId && e.Timestamp >= cutoff)
            .ToListAsync();

        return readings.Any() ? readings.Average(e => e.Consumption) : 0;
    }
}