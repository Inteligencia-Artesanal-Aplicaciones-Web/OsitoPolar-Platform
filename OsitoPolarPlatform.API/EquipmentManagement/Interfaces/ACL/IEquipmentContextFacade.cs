namespace OsitoPolarPlatform.API.EquipmentManagement.Interfaces.ACL;

/// <summary>
/// Facade for the Equipment Management context
/// </summary>
public interface IEquipmentContextFacade
{
    /// <summary>
    /// Check if equipment exists
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <returns>True if equipment exists, false otherwise</returns>
    Task<bool> EquipmentExists(int equipmentId);

    /// <summary>
    /// Fetch equipment owner type (Owner or RenterProvider)
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <returns>Owner type if found, empty string otherwise</returns>
    Task<string> FetchEquipmentOwnerType(int equipmentId);

    /// <summary>
    /// Fetch equipment owner ID
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <returns>Owner ID if found, 0 otherwise</returns>
    Task<int> FetchEquipmentOwnerId(int equipmentId);

    /// <summary>
    /// Check if equipment is owned by a specific owner
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <param name="ownerId">Owner ID</param>
    /// <returns>True if equipment is owned by the specified owner, false otherwise</returns>
    Task<bool> IsEquipmentOwnedBy(int equipmentId, int ownerId);

    /// <summary>
    /// Fetch all equipment IDs owned by an owner
    /// </summary>
    /// <param name="ownerId">Owner ID</param>
    /// <returns>List of equipment IDs</returns>
    Task<IEnumerable<int>> FetchEquipmentIdsByOwnerId(int ownerId);

    /// <summary>
    /// Count equipment owned by an owner
    /// </summary>
    /// <param name="ownerId">Owner ID</param>
    /// <returns>Number of equipment items owned</returns>
    Task<int> CountEquipmentByOwnerId(int ownerId);

    /// <summary>
    /// Get equipment optimal temperature range
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <returns>Tuple with (minTemp, maxTemp) or null if not found</returns>
    Task<(decimal minTemp, decimal maxTemp)?> GetEquipmentOptimalTemperatureRange(int equipmentId);

    /// <summary>
    /// Get equipment maintenance data for forecasting
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <returns>Tuple with (installationDate, minTemp, maxTemp) or null if not found</returns>
    Task<(DateTimeOffset installationDate, decimal minTemp, decimal maxTemp)?> GetEquipmentMaintenanceData(int equipmentId);

    /// <summary>
    /// Get equipment rental data for payment processing
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <returns>Tuple with (id, name, type, hasRentalInfo) or null if not found</returns>
    Task<(int id, string name, string type, bool hasRentalInfo)?> GetEquipmentRentalData(int equipmentId);

    /// <summary>
    /// Process equipment rental assignment
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <param name="ownerId">Owner ID to assign the equipment to</param>
    /// <param name="startDate">Rental start date</param>
    /// <param name="endDate">Rental end date</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> ProcessEquipmentRental(int equipmentId, int ownerId, DateTimeOffset startDate, DateTimeOffset endDate);
}
