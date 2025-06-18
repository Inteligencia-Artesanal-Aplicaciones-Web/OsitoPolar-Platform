using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Queries;

namespace OsitoPolarPlatform.API.WorkOrders.Domain.Services;

/// <summary>
/// Defines the contract for query-based operations on Work Orders.
/// </summary>
public interface IWorkOrderQueryService
{
    Task<WorkOrder?> Handle(GetWorkOrderByIdQuery query);
    Task<IEnumerable<WorkOrder>> Handle(GetAllWorkOrdersQuery query);
    Task<WorkOrder?> Handle(GetWorkOrderByWorkOrderNumberQuery query);
    Task<WorkOrder?> Handle(GetWorkOrderByServiceRequestIdQuery query);
    Task<IEnumerable<WorkOrder>> Handle(GetWorkOrdersByStatusQuery query);
    Task<IEnumerable<WorkOrder>> Handle(GetWorkOrdersByTechnicianIdQuery query);
    Task<IEnumerable<WorkOrder>> Handle(GetWorkOrdersByEquipmentIdQuery query);
}