using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;
using OsitoPolarPlatform.API.WorkOrders.Interfaces.ACL;

namespace OsitoPolarPlatform.API.WorkOrders.Application.ACL;

/// <summary>
/// Facade implementation for the Work Orders context
/// </summary>
/// <param name="workOrderRepository">The work order repository</param>
public class WorkOrderContextFacade(IWorkOrderRepository workOrderRepository) : IWorkOrderContextFacade
{
    public async Task<bool> WorkOrderExists(int workOrderId)
    {
        var workOrder = await workOrderRepository.FindByIdAsync(workOrderId);
        return workOrder != null;
    }

    public async Task<string> FetchWorkOrderStatus(int workOrderId)
    {
        var workOrder = await workOrderRepository.FindByIdAsync(workOrderId);
        return workOrder?.Status.ToString() ?? string.Empty;
    }

    public async Task<int> FetchWorkOrderIdByServiceRequestId(int serviceRequestId)
    {
        var workOrder = await workOrderRepository.FindByServiceRequestIdAsync(serviceRequestId);
        return workOrder?.Id ?? 0;
    }

    public async Task<(int id, string workOrderNumber, string title, string status, decimal? cost, int? serviceRequestId)?> GetWorkOrderData(int workOrderId)
    {
        var workOrder = await workOrderRepository.FindByIdAsync(workOrderId);
        if (workOrder == null) return null;

        return (workOrder.Id, workOrder.WorkOrderNumber, workOrder.Title,
                workOrder.Status.ToString(), workOrder.Cost, workOrder.ServiceRequestId);
    }
}
