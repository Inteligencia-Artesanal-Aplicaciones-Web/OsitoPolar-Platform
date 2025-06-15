using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Aggregates;

namespace OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;

/// <summary>
/// Defines the contract for data access operations for WorkOrder aggregate.
/// </summary>
public interface IWorkOrderRepository
{
    /// <summary>
    /// Adds a new WorkOrder to the repository.
    /// </summary>
    /// <param name="workOrder">The WorkOrder entity to add.</param>
    Task AddAsync(WorkOrder workOrder);

    /// <summary>
    /// Finds a WorkOrder by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the WorkOrder.</param>
    /// <returns>The WorkOrder if found, otherwise null.</returns>
    Task<WorkOrder?> FindByIdAsync(int id);

    /// <summary>
    /// Finds a WorkOrder by its WorkOrderNumber.
    /// </summary>
    /// <param name="workOrderNumber">The WorkOrderNumber to find.</param>
    /// <returns>The WorkOrder if found, otherwise null.</returns>
    Task<WorkOrder?> FindByWorkOrderNumberAsync(string workOrderNumber);

    /// <summary>
    /// Finds a WorkOrder by its associated ServiceRequestId.
    /// </summary>
    /// <param name="serviceRequestId">The ID of the ServiceRequest.</param>
    /// <returns>The WorkOrder if found, otherwise null.</returns>
    Task<WorkOrder?> FindByServiceRequestIdAsync(int serviceRequestId);

    /// <summary>
    /// Finds all WorkOrders.
    /// </summary>
    /// <returns>A list of all WorkOrders.</returns>
    Task<IEnumerable<WorkOrder>> FindAllAsync();

    /// <summary>
    /// Updates an existing WorkOrder.
    /// </summary>
    /// <param name="workOrder">The WorkOrder entity to update.</param>
    void Update(WorkOrder workOrder);

    /// <summary>
    /// Deletes a WorkOrder.
    /// </summary>
    /// <param name="workOrder">The WorkOrder entity to delete.</param>
    void Remove(WorkOrder workOrder);

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    Task<int> SaveChangesAsync();
}