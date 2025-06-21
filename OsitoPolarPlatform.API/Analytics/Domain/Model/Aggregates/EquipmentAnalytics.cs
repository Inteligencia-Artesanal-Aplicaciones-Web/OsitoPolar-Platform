using OsitoPolarPlatform.API.Analytics.Domain.Model.Commands;
using OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;
using OsitoPolarPlatform.API.Analytics.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.Analytics.Domain.Model.Aggregates;

/// <summary>
/// Aggregate root for equipment analytics data
/// </summary>
public partial class EquipmentAnalytics
{
    public int Id { get; private set; }
    public int EquipmentId { get; private set; }
    public ICollection<TemperatureReading> TemperatureReadings { get; private set; }
    public ICollection<EnergyReading> EnergyReadings { get; private set; }

    protected EquipmentAnalytics()
    {
        TemperatureReadings = new List<TemperatureReading>();
        EnergyReadings = new List<EnergyReading>();
    }

    public EquipmentAnalytics(int equipmentId) : this()
    {
        EquipmentId = equipmentId;
    }

    public EquipmentAnalytics(RecordTemperatureReadingCommand command) : this(command.EquipmentId)
    {
        RecordTemperature(command.Temperature, command.Timestamp);
    }
}