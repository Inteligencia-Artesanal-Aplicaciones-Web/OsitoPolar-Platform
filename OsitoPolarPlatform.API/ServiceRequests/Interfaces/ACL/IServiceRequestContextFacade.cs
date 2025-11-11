namespace OsitoPolarPlatform.API.ServiceRequests.Interfaces.ACL;

/// <summary>
/// Facade for the Service Requests context
/// </summary>
public interface IServiceRequestContextFacade
{
    /// <summary>
    /// Check if service request exists
    /// </summary>
    /// <param name="serviceRequestId">Service Request ID</param>
    /// <returns>True if service request exists, false otherwise</returns>
    Task<bool> ServiceRequestExists(int serviceRequestId);

    /// <summary>
    /// Fetch service request status
    /// </summary>
    /// <param name="serviceRequestId">Service Request ID</param>
    /// <returns>Status if found, empty string otherwise</returns>
    Task<string> FetchServiceRequestStatus(int serviceRequestId);

    /// <summary>
    /// Fetch service request equipment ID
    /// </summary>
    /// <param name="serviceRequestId">Service Request ID</param>
    /// <returns>Equipment ID if found, 0 otherwise</returns>
    Task<int> FetchServiceRequestEquipmentId(int serviceRequestId);

    /// <summary>
    /// Fetch service request title
    /// </summary>
    /// <param name="serviceRequestId">Service Request ID</param>
    /// <returns>Title if found, empty string otherwise</returns>
    Task<string> FetchServiceRequestTitle(int serviceRequestId);

    /// <summary>
    /// Count active service requests by equipment ID
    /// </summary>
    /// <param name="equipmentId">Equipment ID</param>
    /// <returns>Number of active (Pending or InProgress) service requests</returns>
    Task<int> CountActiveServiceRequestsByEquipmentId(int equipmentId);

    /// <summary>
    /// Count all active service requests
    /// </summary>
    /// <returns>Total number of active (Pending or InProgress) service requests</returns>
    Task<int> CountAllActiveServiceRequests();

    /// <summary>
    /// Get service request data for payment processing
    /// </summary>
    /// <param name="serviceRequestId">Service Request ID</param>
    /// <returns>Tuple with (id, clientId, companyId) or null if not found</returns>
    Task<(int id, int clientId, int companyId)?> GetServiceRequestData(int serviceRequestId);
}
