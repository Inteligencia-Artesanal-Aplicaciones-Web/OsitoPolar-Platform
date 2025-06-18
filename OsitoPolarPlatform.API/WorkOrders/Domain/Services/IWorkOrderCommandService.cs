using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Aggregates;
using OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;

namespace OsitoPolarPlatform.API.WorkOrders.Domain.Services;

/// <summary>
/// Defines the contract for command-based operations on Work Orders.
/// </summary>
public interface IWorkOrderCommandService
{
    Task<WorkOrder?> Handle(CreateWorkOrderCommand command);
    Task<WorkOrder?> Handle(UpdateWorkOrderStatusCommand command); 
    Task<WorkOrder?> Handle(AssignTechnicianToWorkOrderCommand command); 
    Task<WorkOrder?> Handle(AddWorkOrderResolutionDetailsCommand command); 
    //Task<WorkOrder?> Handle(UpdateWorkOrderDetailsCommand command);
}