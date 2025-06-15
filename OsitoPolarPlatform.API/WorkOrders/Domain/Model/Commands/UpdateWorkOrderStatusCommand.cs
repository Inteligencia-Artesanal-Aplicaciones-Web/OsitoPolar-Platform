using OsitoPolarPlatform.API.WorkOrders.Domain.Model.ValueObjects;

namespace OsitoPolarPlatform.API.WorkOrders.Domain.Model.Commands;  

/// <summary>
/// Command to update the status of an existing WorkOrder.
/// </summary>
public record UpdateWorkOrderStatusCommand(
    int WorkOrderId,
    EWorkOrderStatus NewStatus
);