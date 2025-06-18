namespace OsitoPolarPlatform.API.Analytics.Domain.Model.Entities;

/// <summary>
/// Represents daily temperature statistics for equipment
/// </summary>
public class DailyTemperatureAverage
{
    public int Id { get; private set; }
    public int EquipmentId { get; private set; }
    public DateOnly Date { get; private set; }
    public decimal AverageTemperature { get; private set; }
    public decimal MinTemperature { get; private set; }
    public decimal MaxTemperature { get; private set; }

    protected DailyTemperatureAverage()
    {
        Date = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    public DailyTemperatureAverage(int equipmentId, DateOnly date, decimal average, decimal min, decimal max) : this()
    {
        EquipmentId = equipmentId;
        Date = date;
        AverageTemperature = average;
        MinTemperature = min;
        MaxTemperature = max;
    }
}