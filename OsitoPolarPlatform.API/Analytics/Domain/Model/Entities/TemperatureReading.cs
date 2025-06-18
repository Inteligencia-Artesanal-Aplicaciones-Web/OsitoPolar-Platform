using OsitoPolarPlatform.API.Analytics.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;

/// <summary>
/// Represents a temperature reading from equipment
/// </summary>
public partial class TemperatureReading
{
    public int Id { get; private set; }
    public int EquipmentId { get; private set; }
    public decimal Temperature { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public EReadingStatus Status { get; private set; }

    protected TemperatureReading()
    {
        Timestamp = DateTimeOffset.UtcNow;
        Status = EReadingStatus.Normal;
    }

    public TemperatureReading(int equipmentId, decimal temperature) : this()
    {
        EquipmentId = equipmentId;
        Temperature = temperature;
    }

    public TemperatureReading(int equipmentId, decimal temperature, DateTimeOffset timestamp) : this(equipmentId, temperature)
    {
        Timestamp = timestamp;
    }
}