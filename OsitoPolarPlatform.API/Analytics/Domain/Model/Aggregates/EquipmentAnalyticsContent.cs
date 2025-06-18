using OsitoPolarPlatform.API.Analytics.Domain.Model.Commands;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;
using OsitoPolarPlatform.API.Analytics.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.Analytics.Domain.Model.Aggregates;

public partial class EquipmentAnalytics
{
    public void RecordTemperature(decimal temperature, DateTimeOffset? timestamp = null)
    {
        var reading = new TemperatureReading(EquipmentId, temperature, timestamp ?? DateTimeOffset.UtcNow);
        TemperatureReadings.Add(reading);
    }

    public void RecordEnergyConsumption(decimal consumption, string unit = "watts")
    {
        var reading = new EnergyReading(EquipmentId, consumption, unit);
        EnergyReadings.Add(reading);
    }

    // MÉTODO ELIMINADO: CalculateDailyAverage
    // Los daily averages ahora son independientes y se calculan por separado

    public EReadingStatus GetTemperatureStatus(decimal temperature, decimal minTemp, decimal maxTemp)
    {
        if (temperature >= minTemp && temperature <= maxTemp) 
            return EReadingStatus.Normal;

        var rangeWidth = maxTemp - minTemp;
        var threshold = rangeWidth * 0.2m;

        var minDiff = Math.Abs(temperature - minTemp);
        var maxDiff = Math.Abs(temperature - maxTemp);

        if (minDiff > threshold || maxDiff > threshold)
            return EReadingStatus.Critical;

        return EReadingStatus.Warning;
    }

    public EReadingStatus GetEnergyStatus(decimal consumption, decimal averageConsumption)
    {
        if (averageConsumption <= 0) return EReadingStatus.Normal;

        var ratio = consumption / averageConsumption;
        
        if (ratio > 1.5m) return EReadingStatus.Critical;
        if (ratio > 1.2m) return EReadingStatus.Warning;
        return EReadingStatus.Normal;
    }

    public IEnumerable<TemperatureReading> GetRecentTemperatureReadings(int hours = 24)
    {
        var cutoff = DateTimeOffset.UtcNow.AddHours(-hours);
        return TemperatureReadings.Where(r => r.Timestamp >= cutoff).OrderByDescending(r => r.Timestamp);
    }

    public IEnumerable<EnergyReading> GetRecentEnergyReadings(int hours = 24)
    {
        var cutoff = DateTimeOffset.UtcNow.AddHours(-hours);
        return EnergyReadings.Where(r => r.Timestamp >= cutoff).OrderByDescending(r => r.Timestamp);
    }

    public void Handle(RecordTemperatureReadingCommand command)
    {
        if (command.EquipmentId == EquipmentId)
            RecordTemperature(command.Temperature, command.Timestamp);
    }

    public void Handle(RecordEnergyReadingCommand command)
    {
        if (command.EquipmentId == EquipmentId)
            RecordEnergyConsumption(command.Consumption, command.Unit);
    }
}