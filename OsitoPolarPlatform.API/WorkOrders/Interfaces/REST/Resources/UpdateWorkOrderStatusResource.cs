namespace OsitoPolarPlatform.API.WorkOrders.Interfaces.REST.Resources;

/// <summary>
/// Resource for updating only the status of a WorkOrder.
/// </summary>
public record UpdateWorkOrderStatusResource(
    string NewStatus 
);