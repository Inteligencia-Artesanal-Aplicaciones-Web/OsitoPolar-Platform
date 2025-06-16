using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.WorkOrders.Domain.Repositories;
using OsitoPolarPlatform.API.WorkOrders.Domain.Services; 
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Queries; 

namespace OsitoPolarPlatform.API.WorkOrders.Application.Internal.QueryServices;

/// <summary>
/// Concrete implementation of IWorkOrderQueryService. Handles all query operations for Work Orders.
/// </summary>
public class WorkOrderQueryService(IWorkOrderRepository workOrderRepository) : IWorkOrderQueryService
{
    public async Task<WorkOrder?> Handle(GetWorkOrderByIdQuery query)
    {
        return await workOrderRepository.FindByIdAsync(query.WorkOrderId);
    }

    public async Task<IEnumerable<WorkOrder>> Handle(GetAllWorkOrdersQuery query)
    {
        return await workOrderRepository.ListAsync();
    }

    public async Task<WorkOrder?> Handle(GetWorkOrderByWorkOrderNumberQuery query)
    {
        return await workOrderRepository.FindByWorkOrderNumberAsync(query.WorkOrderNumber);
    }

    public async Task<WorkOrder?> Handle(GetWorkOrderByServiceRequestIdQuery query)
    {
        return await workOrderRepository.FindByServiceRequestIdAsync(query.ServiceRequestId);
    }

    public async Task<IEnumerable<WorkOrder>> Handle(GetWorkOrdersByStatusQuery query)
    {
        var allWorkOrders = await workOrderRepository.ListAsync();
        return allWorkOrders.Where(wo => wo.Status == query.Status);
    }

    public async Task<IEnumerable<WorkOrder>> Handle(GetWorkOrdersByTechnicianIdQuery query)
    {
        var allWorkOrders = await workOrderRepository.ListAsync();
        return allWorkOrders.Where(wo => wo.AssignedTechnicianId == query.TechnicianId);
    }

    public async Task<IEnumerable<WorkOrder>> Handle(GetWorkOrdersByEquipmentIdQuery query)
    {
        var allWorkOrders = await workOrderRepository.ListAsync();
        return allWorkOrders.Where(wo => wo.EquipmentId == query.EquipmentId);
    }
}