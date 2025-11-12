namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.ACL;

/// <summary>
/// Facade for the Work Orders context
/// </summary>
public interface IWorkOrderContextFacade
{
    /// <summary>
    /// Check if work order exists
    /// </summary>
    /// <param name="workOrderId">Work Order ID</param>
    /// <returns>True if work order exists, false otherwise</returns>
    Task<bool> WorkOrderExists(int workOrderId);

    /// <summary>
    /// Fetch work order status
    /// </summary>
    /// <param name="workOrderId">Work Order ID</param>
    /// <returns>Status if found, empty string otherwise</returns>
    Task<string> FetchWorkOrderStatus(int workOrderId);

    /// <summary>
    /// Fetch work order by service request ID
    /// </summary>
    /// <param name="serviceRequestId">Service Request ID</param>
    /// <returns>Work Order ID if found, 0 otherwise</returns>
    Task<int> FetchWorkOrderIdByServiceRequestId(int serviceRequestId);

    /// <summary>
    /// Get work order data for payment processing
    /// </summary>
    /// <param name="workOrderId">Work Order ID</param>
    /// <returns>Tuple with (id, workOrderNumber, title, status, cost, serviceRequestId) or null if not found</returns>
    Task<(int id, string workOrderNumber, string title, string status, decimal? cost, int? serviceRequestId)?> GetWorkOrderData(int workOrderId);

    /// <summary>
    /// Get technician average rating from work order feedback
    /// </summary>
    /// <param name="technicianId">Technician ID</param>
    /// <returns>Average rating (0.0 if no ratings found)</returns>
    Task<double> GetTechnicianAverageRating(int technicianId);
}
