using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.Shared.Domain.Repositories; 

namespace OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;

/// <summary>
/// Defines the contract for data access operations for WorkOrder aggregate.
/// </summary>
public interface IWorkOrderRepository : IBaseRepository<WorkOrder> 
{
    Task<WorkOrder?> FindByWorkOrderNumberAsync(string workOrderNumber);
    Task<WorkOrder?> FindByServiceRequestIdAsync(int serviceRequestId);
}