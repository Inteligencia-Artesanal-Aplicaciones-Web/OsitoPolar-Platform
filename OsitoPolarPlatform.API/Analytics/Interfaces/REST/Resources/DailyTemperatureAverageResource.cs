namespace OsitoPolarPlatform.API.Analytics.Interfaces.REST.Resources;

/// <summary>
/// Resource for representing daily temperature averages in API responses
/// </summary>
public record DailyTemperatureAverageResource(
    int Id,
    int EquipmentId,
    string Date,
    decimal AverageTemperature,
    decimal MinTemperature,
    decimal MaxTemperature
);