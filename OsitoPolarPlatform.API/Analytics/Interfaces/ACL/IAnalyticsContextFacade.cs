namespace OsitoPolarPlatform.API.Analytics.Interfaces.ACL;

/// <summary>
/// Facade for the Analytics context
/// </summary>
public interface IAnalyticsContextFacade
{
    /// <summary>
    /// Record temperature reading for equipment
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <param name="temperature">Temperature value</param>
    /// <param name="timestamp">Reading timestamp</param>
    /// <returns>Reading ID if created, 0 otherwise</returns>
    Task<int> RecordTemperatureReading(int equipmentId, decimal temperature, DateTime timestamp);

    /// <summary>
    /// Record energy consumption reading for equipment
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <param name="consumption">Energy consumption value</param>
    /// <param name="timestamp">Reading timestamp</param>
    /// <returns>Reading ID if created, 0 otherwise</returns>
    Task<int> RecordEnergyReading(int equipmentId, decimal consumption, DateTime timestamp);

    /// <summary>
    /// Check if equipment has temperature anomaly
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <param name="optimalMin">Optimal temperature minimum</param>
    /// <param name="optimalMax">Optimal temperature maximum</param>
    /// <returns>True if there's an anomaly, false otherwise</returns>
    Task<bool> HasTemperatureAnomaly(int equipmentId, decimal optimalMin, decimal optimalMax);
}
