using Microsoft.EntityFrameworkCore;

namespace OsitoPolarPlatform.API.EquipmentManagement.Domain.Model.Entities;

[Owned]
public class EnergyConsumption
{
    public decimal Current { get; private set; }
    public string Unit { get; private set; }
    public decimal Average { get; private set; }

    public EnergyConsumption()
    {
        Current = 0;
        Unit = "watts";
        Average = 0;
    }

    public EnergyConsumption(decimal current, string unit, decimal average)
    {
        if (current < 0) throw new ArgumentException("Current consumption cannot be negative", nameof(current));
        if (string.IsNullOrWhiteSpace(unit)) throw new ArgumentException("Unit cannot be empty", nameof(unit));
        if (average < 0) throw new ArgumentException("Average consumption cannot be negative", nameof(average));

        Current = current;
        Unit = unit;
        Average = average;
    }

    public void UpdateCurrent(decimal newCurrent)
    {
        if (newCurrent < 0)
            throw new ArgumentException("Current consumption cannot be negative", nameof(newCurrent));
        
        Current = newCurrent;
    }

    public void UpdateAverage(decimal newAverage)
    {
        if (newAverage < 0)
            throw new ArgumentException("Average consumption cannot be negative", nameof(newAverage));
        
        Average = newAverage;
    }
}