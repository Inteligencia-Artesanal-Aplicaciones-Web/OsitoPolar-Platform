namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;

/// <summary>
/// Command to assign a technician to a WorkOrder.
/// </summary>
public record AssignTechnicianToWorkOrderCommand(
    int WorkOrderId,
    int TechnicianId
);