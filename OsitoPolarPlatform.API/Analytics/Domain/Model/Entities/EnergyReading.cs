using OsitoPolarPlatform.API.Analytics.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;

/// <summary>
/// Represents an energy consumption reading from equipment
/// </summary>
public partial class EnergyReading
{
    public int Id { get; private set; }
    public int EquipmentId { get; private set; }
    public decimal Consumption { get; private set; }
    public string Unit { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public EReadingStatus Status { get; private set; }

    protected EnergyReading()
    {
        Unit = "watts";
        Timestamp = DateTimeOffset.UtcNow;
        Status = EReadingStatus.Normal;
    }

    public EnergyReading(int equipmentId, decimal consumption, string unit = "watts") : this()
    {
        EquipmentId = equipmentId;
        Consumption = consumption;
        Unit = unit;
    }
}